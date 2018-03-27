using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Simulator_MPSA.CL;
using System.Diagnostics; // for DEBUG 
using Modbus.Device;

namespace Simulator_MPSA
{
    class WritingThread
    {
        bool paused = false;
        public bool Paused
        {
            get { return paused; }
            set { paused = value; }
        }
        public MainWindow refMainWindow;

        float prevCycleTime;

        float endCycleTime;
        float dt_sec;


        ModbusIpMaster mbMasterW0;
        Task masterLoopW0;
        Thread wrThread;
        public TcpClient tcp;
        public WritingThread(string hostname, int port)
        {
            tcp = new TcpClient(hostname, port);
            
            mbMasterW0 = ModbusIpMaster.CreateIp(tcp);

            // masterLoopW0 = Task.Factory.StartNew(new Action(WritingJob));
            wrThread = new Thread(new ThreadStart(WritingJob));
            wrThread.Start();
        }

      /*  public void Start(Sett settings=null)
        {
            if (settings != null)
            {
                tcp = new TcpClient(settings.HostName, settings.MBPort);
            }

            if (wrThread != null)
                wrThread.Start();
        }*/

        public void Stop()
        {
            refMainWindow = null;
            if (wrThread != null)
                wrThread.Abort();
            tcp.Close();
        }

        //первая итерация цикла следует записывать все включая нули
        bool isFirstCycle = true;
        public void WritingJob()
        {
            prevCycleTime = DateTime.Now.Second + ((float)DateTime.Now.Millisecond) / 1000f;
            endCycleTime = prevCycleTime;
            
            while (true)
            {
                if (paused) continue;

                if (!tcp.Connected)
                {
                    if (refMainWindow!=null)
                    refMainWindow.Dispatcher.Invoke(refMainWindow.delegateDisconnected);
                }
                
                prevCycleTime = endCycleTime;
                //-------- обновление структур --------------------------
                foreach (ZDStruct zd in ZDTableViewModel.ZDs)
                    zd.UpdateZD(dt_sec);

                foreach (KLStruct kl in KLTableViewModel.KL)
                    kl.UpdateKL(dt_sec);

                foreach (MPNAStruct mpna in MPNATableViewModel.MPNAs)
                    mpna.UpdateMPNA(dt_sec);

                foreach (VSStruct vs in VSTableViewModel.VS)
                    vs.UpdateVS(dt_sec);

                //--------------- формирование массивов для передачи в ПЛК ---------------------
                //for (int i = 0; i < AIStruct.items.Length; i++)
                foreach(AIStruct ai in AIStruct.items)
                {
                    if (ai.En /* || true */)
                    {

                        if (ai.PLCDestType == EPLCDestType.ADC)
                        {

                            if (ai.Buffer == BufType.USO)
                                WB.W[(ai.PLCAddr - Sett.Instance.BegAddrW - 1)] = ai.ValACD; // записываем значение АЦП в массив для записи CPU

                            if (ai.Buffer == BufType.A3)
                                WB.W_a3[(ai.PLCAddr - Sett.Instance.iBegAddrA3 - 1)] = ai.ValACD;

                            if (ai.Buffer == BufType.A4)
                                WB.W_a4[(ai.PLCAddr - Sett.Instance.iBegAddrA4 - 1)] = ai.ValACD;
                        }
                        else
                        if (ai.PLCDestType == EPLCDestType.Float)
                        {
                            //разибиваем float на 2 word'a
                            byte[] bytes = BitConverter.GetBytes(ai.fValAI);
                            ushort w1 = BitConverter.ToUInt16(bytes, 0);
                            ushort w2 = BitConverter.ToUInt16(bytes, 2);

                            if (ai.Buffer == BufType.USO)
                            {
                                WB.W[ai.PLCAddr - Sett.Instance.BegAddrW - 1] = w1;
                                WB.W[ai.PLCAddr - Sett.Instance.BegAddrW] = w2;
                            }

                            if (ai.Buffer == BufType.A3)
                            {
                                WB.W_a3[ai.PLCAddr - Sett.Instance.iBegAddrA3 - 1] = w1;
                                WB.W_a3[ai.PLCAddr - Sett.Instance.iBegAddrA3] = w2;
                            }

                            if (ai.Buffer == BufType.A4)
                            {
                                WB.W_a4[ ai.PLCAddr - Sett.Instance.iBegAddrA4 - 1 ] = w1;
                                WB.W_a4[ ai.PLCAddr - Sett.Instance.iBegAddrA4 ] = w2;
                            }
                        }
                    }//ai.en
                }//foreach

                /*for (int i = 0; i < DIStruct.items.Length; i++)
                {
                   
                    SetBit(ref (WB.W[(DIStruct.items[i].indxW)]), (DIStruct.items[i].indxBitDI), (DIStruct.items[i].ValDI));
                }*/
                foreach (DIStruct di in DIStruct.items)
                {
                    if (di.En)
                    {
                        if (di.InvertDI)
                            SetBit(ref (WB.W[(di.indxW)]), (di.indxBitDI), !(di.ValDI));
                        else 
                            SetBit(ref (WB.W[(di.indxW)]), (di.indxBitDI), (di.ValDI));
                    }
                }

                //записываем счетчики УСО
                foreach (USOCounter c in CountersTableViewModel.Counters)
                {
                    c.Update(dt_sec);
                    if (c.buffer == BufType.USO)
                        if (c.PLCAddr >= Sett.Instance.BegAddrW + 1)
                        {
                            WB.W[c.PLCAddr - Sett.Instance.BegAddrW - 1] = c.Value;
                        }

                    if (c.buffer == BufType.A3)
                        if ((c.PLCAddr >= Sett.Instance.iBegAddrA3 +1) && ((c.PLCAddr - Sett.Instance.iBegAddrA3 - 1)<WB.W_a3.Length))
                        {
                            WB.W_a3[c.PLCAddr - Sett.Instance.iBegAddrA3 - 1] = c.Value;
                        }
                }

                //----- время на момент формирования массивов --------------------------
                float formingTime = DateTime.Now.Second + ((float)DateTime.Now.Millisecond) / 1000f;
                Debug.WriteLine("время перекладки: " + (formingTime - prevCycleTime).ToString());
                //------------------ запись в ПЛК ----------------------------------------
                WriteToPLC();

                //---------- вычисление время с момента предыдущей итерации ----------------
                endCycleTime = DateTime.Now.Second + ((float)DateTime.Now.Millisecond) / 1000f;
                
                if ((endCycleTime - prevCycleTime) < 0)
                    dt_sec = 60f - prevCycleTime + endCycleTime;
                else
                    dt_sec = endCycleTime - prevCycleTime;

                if (dt_sec > 60)
                    dt_sec = 0;

                //------------------- сигнализируем о завершении цикла ---------------------------------
                if (refMainWindow != null)
                    refMainWindow.Dispatcher.Invoke(refMainWindow.EndCycle, dt_sec);

                Debug.WriteLine("W0 time = " + dt_sec.ToString());
                System.Threading.Thread.Sleep(Sett.Instance.TPause);

                isFirstCycle = false;
            }
        }

        void WriteToPLC()
        {
            //--------------------- Запись буферов УСО ------------------------------------------------------
           // int AreaW = ; //  (29 - 3 + 1) * 126]; // 3402 
            int NReg = 120; // количество регистров на запись не более 120

            ushort[] data = new ushort[NReg];   //буфер для записи одной бочки
            int TaskCoilCount = WB.W.Length / NReg; /* / Sett.Instance.NWrTask;*/   //количество бочек по 120 регистров которые записываются в данном потоке

            int destStartAddr = Sett.Instance.BegAddrW; //адрес в ПЛК 

            
            for (int Coil_i = 0; Coil_i < TaskCoilCount; Coil_i++)
            {
                bool isChanged = false;
                for (int i_reg = NReg * Coil_i; i_reg < NReg * (Coil_i + 1); i_reg++)
                {
                    if (WB.W[i_reg] != WB.WB_old[i_reg])
                    {
                        isChanged = true;
                        break;
                    }
                }

                if (isChanged || isFirstCycle)
                {
                    Array.Copy(WB.W, NReg * Coil_i, WB.WB_old, NReg * Coil_i, NReg);
                    Array.Copy(WB.W, NReg * Coil_i, data, (0), NReg);
                    try
                    {
                        mbMasterW0.WriteMultipleRegisters(1, (ushort)(destStartAddr + NReg * Coil_i), data);
                    }
                    catch (Exception ex)
                    {
                        if (refMainWindow!=null)
                        refMainWindow.Dispatcher.Invoke(refMainWindow.delegateDisconnected);
                    }
                }
                else
                    Debug.WriteLine("skip");
            }
            //------------------------ Запись буфера А3 -------------------------------------------------------
            destStartAddr = Sett.Instance.iBegAddrA3;
            TaskCoilCount = WB.W_a3.Length / NReg;
            for (int Coil_i = 0; Coil_i < TaskCoilCount; Coil_i++)
            {
                bool isChanged = false;
                for (int i_reg = NReg * Coil_i; i_reg < NReg * (Coil_i + 1); i_reg++)
                {
                    if (WB.W_a3[i_reg] != WB.W_a3_prev[i_reg])
                    {
                        isChanged = true;
                        WB.W_a3_prev[i_reg] = WB.W_a3[i_reg];
                       // break;
                    }
                }

                if (isChanged || isFirstCycle)
                {

                    Array.Copy(WB.W_a3, NReg * Coil_i, data, (0), NReg);
                    try
                    {
                        mbMasterW0.WriteMultipleRegisters(1, (ushort)(destStartAddr + NReg * Coil_i), data);
                    }
                    catch (Exception ex)
                    {
                        if (refMainWindow!=null)
                        refMainWindow.Dispatcher.Invoke(refMainWindow.delegateDisconnected);
                    }
                }
                else
                    Debug.WriteLine("skip");
            }

        }
        void SetBit(ref ushort b, int bitNumber, bool state)
        {
            if (bitNumber < 0 || bitNumber > 15)
                bitNumber = 0; //throw an Exception or return
            if (state)
            {
                b |= (ushort)(1 << bitNumber);
            }
            else
            {
                int i = b;
                i &= ~(1 << bitNumber);
                b = (ushort)i;
            }
        }      
       
    }
}
