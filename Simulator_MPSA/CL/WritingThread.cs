using Modbus.Device;
using Simulator_MPSA.CL;
using System;
using System.Diagnostics; // for DEBUG 
using System.Net.Sockets;
using System.Threading;

namespace Simulator_MPSA
{
    public class WritingThread
    {
        bool paused = false;
        public bool Paused
        {
            get { return paused; }
            set { paused = value; }
        }
        public MainWindow refMainWindow=null;

        DateTime prevCycleTime;

        float dt_sec;


        ModbusIpMaster[] mbMasterW;
        Thread[] wrThread;
        TcpClient[] tcp;

        /// <summary>
        /// количество регистров в блоке
        /// </summary>
        int NReg;
        /// <summary>
        /// количество блоков по 120 рег
        /// </summary>
        int CoilCount;

        /// <summary>
        /// количество блоков на один поток
        /// </summary>
        int CoilPerTask;

        /// <summary>
        /// количество потоков
        /// </summary>
        int NJob=4;
        int NMasters = 6;
        /// <summary>
        /// адрес назначения в ПЛК
        /// </summary>
        

        public WritingThread(string hostname, int port)
        {
            tcp = new TcpClient[NMasters];
            for(int i=0; i< NMasters; i++)
                tcp[i] = new TcpClient(hostname, port);

            mbMasterW = new ModbusIpMaster[NMasters];
            for (int i = 0; i < NMasters; i++)
                mbMasterW[i] = ModbusIpMaster.CreateIp(tcp[i]);

            // masterLoopW0 = Task.Factory.StartNew(new Action(WritingJob));
            wrThread = new Thread[NMasters];
            // for (int i = 0; i < NJob; i++)
            //    wrThread[i] = new Thread(WriteToPLC);
            wrThread[0] = new Thread(WriteToPLC1);
            wrThread[1] = new Thread(WriteToPLC2);
            wrThread[2] = new Thread(WriteToPLC3);
            wrThread[3] = new Thread(WriteToPLC4);
            wrThread[4] = new Thread(WriteA3);
         //   wrThread[5] = new Thread(WriteA4);
           // wrThread[6] = new Thread(WriteToPLC7);
           // wrThread[7] = new Thread(WriteToPLC8);

            NReg = 120; // количество регистров на запись не более 120
            CoilCount = WB.W.Length / NReg; /* / Sett.Instance.NWrTask;*/   //количество бочек по 120 регистров которые записываются в данном потоке
            CoilPerTask = (int)Math.Ceiling((double)CoilCount/(double)NJob);
          //  destStartAddr = Sett.Instance.BegAddrW; //адрес в ПЛК 
        }

        public void Start()
        {
            for (int i = 0; i < NMasters; i++)
            {
                if (wrThread[i] != null)
                    wrThread[i].Start();
            }
            
        }

        public void Stop()
        {
            refMainWindow = null;
 

            for (int i = 0; i < NMasters; i++)
            {
                if (wrThread[i] != null)
                    wrThread[i].Abort();
            }

            for (int i = 0; i < NMasters; i++)
            {
                tcp[i].Close();
            }
        }

        public void WritingJob(object data)
        {
            int jobnum = (int)data;
            prevCycleTime = DateTime.Now;

            dt_sec = 0f;
            while (true)
            {                       
              //  Debug.WriteLine("W0 time = " + dt_sec.ToString());
                System.Threading.Thread.Sleep(Sett.Instance.TPause);
            }
        }
        void WriteToPLC1()
        {
            int destStartAddr;
            int jobnum = 0;
            ushort[] data = new ushort[NReg];   //буфер для записи одной бочки



            int nCoilFirst = (jobnum * CoilPerTask);
            int nCoilLast = (jobnum + 1) * CoilPerTask - 1;
            if (nCoilLast >= CoilCount)
                nCoilLast = CoilCount - 1;
            bool isFirstCycle = true;
            while (true)
            {
                //----------------------------- запись в буфер УСО ЦП ---------------------------------------------
                destStartAddr = Sett.Instance.BegAddrW;
                for (int Coil_i = nCoilFirst; Coil_i <= nCoilLast; Coil_i++)
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
                        mbMasterW[(int)jobnum].WriteMultipleRegisters(1, (ushort)(destStartAddr + NReg * Coil_i + Sett.Instance.IncAddr), data);
                    }
                    else
                        Debug.WriteLine("thread 1 - skip");
                }
                
             
                isFirstCycle = false;

                //----------------------- Вызов делегата --------------------------------------------------------
                if (refMainWindow != null)
                    refMainWindow.Dispatcher.Invoke(refMainWindow.EndCycle);

                System.Threading.Thread.Sleep(Sett.Instance.TPause);
            }
        }
        void WriteToPLC2()
        {
            int destStartAddr;

            int jobnum = 1;
            ushort[] data = new ushort[NReg];   //буфер для записи одной бочки



            int nCoilFirst = (jobnum * CoilPerTask);
            int nCoilLast = (jobnum + 1) * CoilPerTask - 1;
            if (nCoilLast >= CoilCount)
                nCoilLast = CoilCount - 1;

            bool isFirstCycle = true;
            while (true)
            {
                //----------------------------- запись в буфер УСО ЦП ---------------------------------------------
                destStartAddr = Sett.Instance.BegAddrW;

                for (int Coil_i = nCoilFirst; Coil_i <= nCoilLast; Coil_i++)
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
                        mbMasterW[(int)jobnum].WriteMultipleRegisters(1, (ushort)(destStartAddr + NReg * Coil_i + Sett.Instance.IncAddr), data);
                    }
                    else
                        Debug.WriteLine("thread 2 - skip");
                }
              
                isFirstCycle = false;

                if (refMainWindow != null)
                    refMainWindow.Dispatcher.Invoke(refMainWindow.EndCycle2);
                System.Threading.Thread.Sleep(Sett.Instance.TPause);
            }
        }
        void WriteToPLC3()
        {
            int destStartAddr;
            int jobnum = 2;
            ushort[] data = new ushort[NReg];   //буфер для записи одной бочки



            int nCoilFirst = (jobnum * CoilPerTask);
            int nCoilLast = (jobnum + 1) * CoilPerTask - 1;
            if (nCoilLast >= CoilCount)
                nCoilLast = CoilCount - 1;
            bool isFirstCycle = true;
            while (true)
            {
                //----------------------------- запись в буфер УСО ЦП ---------------------------------------------
                destStartAddr = Sett.Instance.BegAddrW;

                for (int Coil_i = nCoilFirst; Coil_i <= nCoilLast; Coil_i++)
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
                        mbMasterW[(int)jobnum].WriteMultipleRegisters(1, (ushort)(destStartAddr + NReg * Coil_i + Sett.Instance.IncAddr), data);
                    }
                    else
                        Debug.WriteLine("thread 3 - skip");
                }

                isFirstCycle = false;

                if (refMainWindow != null)
                    refMainWindow.Dispatcher.Invoke(refMainWindow.EndCycle3);
                System.Threading.Thread.Sleep(Sett.Instance.TPause);
            }
        }
        void WriteToPLC4()
        {
            int destStartAddr;
            int jobnum = 3;
            ushort[] data = new ushort[NReg];   //буфер для записи одной бочки



            int nCoilFirst = (jobnum * CoilPerTask);
            int nCoilLast = (jobnum + 1) * CoilPerTask - 1;
            if (nCoilLast >= CoilCount)
                nCoilLast = CoilCount - 1;
            bool isFirstCycle = true;
            while (true)
            {
                //----------------------------- запись в буфер УСО ЦП ---------------------------------------------
                destStartAddr = Sett.Instance.BegAddrW;

                for (int Coil_i = nCoilFirst; Coil_i <= nCoilLast; Coil_i++)
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
                        mbMasterW[(int)jobnum].WriteMultipleRegisters(1, (ushort)(destStartAddr + NReg * Coil_i + Sett.Instance.IncAddr), data);
                    }
                    else
                        Debug.WriteLine("thread 4 - skip");
                }
                
                isFirstCycle = false;

                if (refMainWindow != null)
                    refMainWindow.Dispatcher.Invoke(refMainWindow.EndCycle4);

                System.Threading.Thread.Sleep(Sett.Instance.TPause);
            }
        }

        void WriteA3()
        {

            int destStartAddr;
            ushort[] data = new ushort[NReg];   //буфер для записи одной бочки

            bool isFirstCycle = true;
            while (true)
            {
                destStartAddr = Sett.Instance.iBegAddrA3;
                CoilCount = WB.W_a3.Length / NReg;
                for (int Coil_i = 0; Coil_i < CoilCount; Coil_i++)
                {
                    bool isChanged = false;
                    for (int i_reg = NReg * Coil_i; i_reg < NReg * (Coil_i + 1); i_reg++)
                    {
                        if (WB.W_a3[i_reg] != WB.W_a3_prev[i_reg])
                        {
                            isChanged = true;
                            //  WB.W_a3_prev[i_reg] = WB.W_a3[i_reg];
                            break;
                        }
                    }

                    if (isChanged || isFirstCycle)
                    {

                        Array.Copy(WB.W_a3, NReg * Coil_i, WB.W_a3_prev, NReg * Coil_i, NReg);
                        Array.Copy(WB.W_a3, NReg * Coil_i, data, (0), NReg);

                       mbMasterW[4].WriteMultipleRegisters(1, (ushort)(destStartAddr + NReg * Coil_i + Sett.Instance.IncAddr), data);
                    }
                    else
                        Debug.WriteLine("skip - A3");
                }

                isFirstCycle = false;

                destStartAddr = Sett.Instance.iBegAddrA4;
                CoilCount = WB.W_a4.Length / NReg;
                for (int Coil_i = 0; Coil_i < CoilCount; Coil_i++)
                {
                    bool isChanged = false;
                    for (int i_reg = NReg * Coil_i; i_reg < NReg * (Coil_i + 1); i_reg++)
                    {
                        if (WB.W_a4[i_reg] != WB.W_a4_prev[i_reg])
                        {
                            isChanged = true;
                            //  WB.W_a3_prev[i_reg] = WB.W_a3[i_reg];
                            break;
                        }
                    }

                    if (isChanged || isFirstCycle)
                    {

                        Array.Copy(WB.W_a4, NReg * Coil_i, WB.W_a4_prev, NReg * Coil_i, NReg);
                        Array.Copy(WB.W_a4, NReg * Coil_i, data, (0), NReg);

                        mbMasterW[4].WriteMultipleRegisters(1, (ushort)(destStartAddr + NReg * Coil_i + Sett.Instance.IncAddr), data);
                    }
                    else
                        Debug.WriteLine("skip - A4");
                }
                System.Threading.Thread.Sleep(Sett.Instance.TPause);

                if (refMainWindow != null)
                    refMainWindow.Dispatcher.Invoke(refMainWindow.EndCycle5);
            }
        }
  
        /* void WriteToPLC(object jobnum)
         {


             ushort[] data = new ushort[NReg];   //буфер для записи одной бочки



             int nCoilFirst = (int)jobnum * CoilPerTask;
             int nCoilLast = ((int)jobnum + 1) * CoilPerTask - 1;
             if (nCoilLast >= CoilCount)
                 nCoilLast = CoilCount - 1;

             for (int Coil_i = nCoilFirst; Coil_i <= nCoilLast; Coil_i++)
             {

                 mbMasterW[(int)jobnum].WriteMultipleRegisters(1, (ushort)(destStartAddr + NReg * Coil_i), data);
             }
             //------------------------ Запись буфера А3 -------------------------------------------------------
               destStartAddr = Sett.Instance.iBegAddrA3;
               CoilCount = WB.W_a3.Length / NReg;
               for (int Coil_i = 0; Coil_i < CoilCount; Coil_i++)
               {
                   bool isChanged = false;
                   for (int i_reg = NReg * Coil_i; i_reg < NReg * (Coil_i + 1); i_reg++)
                   {
                       if (WB.W_a3[i_reg] != WB.W_a3_prev[i_reg])
                       {
                           isChanged = true;
                         //  WB.W_a3_prev[i_reg] = WB.W_a3[i_reg];
                           break;
                       }
                   }

                   if (isChanged || isFirstCycle)
                   {

                       Array.Copy(WB.W_a3, NReg * Coil_i, WB.W_a3_prev, NReg * Coil_i, NReg);
                       Array.Copy(WB.W_a3, NReg * Coil_i, data, (0), NReg);

                       mbMasterW0.WriteMultipleRegisters(1, (ushort)(destStartAddr + NReg * Coil_i), data);
                   }
                   else
                       Debug.WriteLine("skip");
               }

             //------------------- сигнализируем о завершении цикла ---------------------------------
             if (refMainWindow != null && (int)jobnum==0)
                 refMainWindow.Dispatcher.Invoke(refMainWindow.EndCycle);

         }*/


    }
}
