using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using Modbus.Device;
using System.Threading;
using Simulator_MPSA.CL;
namespace Simulator_MPSA
{
    class ReadThread
    {
        public MainWindow refMainWindow;
       // ModbusIpMaster mbMaster;
        ModbusIpMaster mbMasterR0;

        Thread thread;
        TcpClient tcp;

        public ReadThread(string hostname, int port)
        {
            tcp = new TcpClient(hostname, port);

            mbMasterR0 = ModbusIpMaster.CreateIp(tcp);
            thread = new Thread(new ThreadStart(ThreadJob));
            thread.Start();

        }
        public void Stop()
        {
            if (thread != null)
            thread.Abort();
            tcp.Close();

            refMainWindow = null;
        }

        private void ThreadJob()
        {
            ushort NReg = 125;
            ushort tbStartAdress = (ushort)Sett.Instance.BegAddrR;

            ushort[] data;

            while (true)
            {
                for (ushort i = 0; i < 11; i++)
                {
                    data = mbMasterR0.ReadHoldingRegisters(1, (ushort)(tbStartAdress + NReg * i), NReg);
                    data.CopyTo(RB.R, NReg*i);
                }
                GetDOfromR();

                if (refMainWindow != null)
                    refMainWindow.Dispatcher.Invoke(refMainWindow.delegateEndRead);
            }
        }

        void GetDOfromR() // копирование значения сигналов DO из массива для чтения ЦПУ
        {
            foreach(DOStruct _do in DOStruct.items)
            {
                int indx = _do.PLCAddr - Sett.Instance.BegAddrR - 1;
                if (indx > 0 && indx < RB.R.Length)
                {
                    bool res = GetBit(RB.R[indx], _do.indxBitDO);
                    _do.ValDO = _do.InvertDO ? !res : res;
                }
            }
        }
        public bool GetBit(ushort b, int bitNumber)
        {
            if (bitNumber < 0 || bitNumber > 15)
                return false;//throw an Exception or just return false
            return (b & (1 << bitNumber)) > 0;
        }
        public ushort ReverseBytesUshort(ushort value)
        {
            uint V = value;
            V = ((V >> 8) & 0x00FFu | (V & 0x00FFu) << 8);
            V = ((V >> 4) & 0x0F0Fu | (V & 0x0F0Fu) << 4);
            V = ((V >> 2) & 0x3333u | (V & 0x3333u) << 2);
            V = ((V >> 1) & 0x5555u | (V & 0x5555u) << 1);
            return (ushort)V;
        }

        /*    private void UpdateR0()
            {
                while (true)
                {
                    //  int AreaR = (29 - 3 + 1) * 50; //    2000; // Convert.ToInt32(textBoxAreaR.Text); // 
                    int NReg = 125; // Convert.ToInt32(textBoxNReg.Text);
                    //int tbStartAdress = settings.iBegAddrR; // (Convert.ToUInt16(textBoxStartAdress.Text))
                    int tbStartAdress = (int)Sett.Instance.BegAddrR;

                    TSR0.Inp(mbMasterR0.ReadHoldingRegisters(1, (ushort)(tbStartAdress + 125 * 0), (ushort)NReg), 0);
                    TSR1.Inp(mbMasterR0.ReadHoldingRegisters(1, (ushort)(tbStartAdress + 125 * 1), (ushort)NReg), 1);
                    TSR2.Inp(mbMasterR0.ReadHoldingRegisters(1, (ushort)(tbStartAdress + 125 * 2), (ushort)NReg), 2);

                    //TSR3.Inp(mbMasterR0.ReadHoldingRegisters(1, (ushort)(tbStartAdress + 125 * 3), (ushort)NReg), 3);
                    //TSR4.Inp(mbMasterR0.ReadHoldingRegisters(1, (ushort)(tbStartAdress + 125 * 4), (ushort)NReg), 4);
                    //TSR5.Inp(mbMasterR0.ReadHoldingRegisters(1, (ushort)(tbStartAdress + 125 * 5), (ushort)NReg), 5);
                    //TSR6.Inp(mbMasterR0.ReadHoldingRegisters(1, (ushort)(tbStartAdress + 125 * 6), (ushort)NReg), 6);
                    //TSR7.Inp(mbMasterR0.ReadHoldingRegisters(1, (ushort)(tbStartAdress + 125 * 7), (ushort)NReg), 7);
                    //TSR8.Inp(mbMasterR0.ReadHoldingRegisters(1, (ushort)(tbStartAdress + 125 * 8), (ushort)NReg), 8);
                    //TSR9.Inp(mbMasterR0.ReadHoldingRegisters(1, (ushort)(tbStartAdress + 125 * 9), (ushort)NReg), 9);
                    //TSR10.Inp(mbMasterR0.ReadHoldingRegisters(1, (ushort)(tbStartAdress + 125 * 10), (ushort)NReg), 10);
                    //TagSource.Input(mbMasterR0.ReadHoldingRegisters(1, (ushort)(tbStartAdress + 125 * 0), (ushort)NReg));
                    //  Debug.WriteLine("UpdateR0() ");
                    //System.Threading.Thread.Sleep(settings.TPause);
                    System.Threading.Thread.Sleep((int)Sett.Instance.TPause);
                }
            }
            private void UpdateR1()
            {
                while (true)
                {
                    //  int AreaR = (29 - 3 + 1) * 50; //    2000; // Convert.ToInt32(textBoxAreaR.Text); // 
                    int NReg = 125; // Convert.ToInt32(textBoxNReg.Text);
                    //int tbStartAdress = settings.iBegAddrR; // (Convert.ToUInt16(textBoxStartAdress.Text))
                    int tbStartAdress = (int)Sett.Instance.BegAddrR;
                    //TSR0.Inp(mbMasterR1.ReadHoldingRegisters(1, (ushort)(tbStartAdress + 125 * 0), (ushort)NReg), 0);
                    //TSR1.Inp(mbMasterR1.ReadHoldingRegisters(1, (ushort)(tbStartAdress + 125 * 1), (ushort)NReg), 1);
                    //TSR2.Inp(mbMasterR1.ReadHoldingRegisters(1, (ushort)(tbStartAdress + 125 * 2), (ushort)NReg), 2);

                    TSR3.Inp(mbMasterR1.ReadHoldingRegisters(1, (ushort)(tbStartAdress + 125 * 3), (ushort)NReg), 3);
                    TSR4.Inp(mbMasterR1.ReadHoldingRegisters(1, (ushort)(tbStartAdress + 125 * 4), (ushort)NReg), 4);
                    TSR5.Inp(mbMasterR1.ReadHoldingRegisters(1, (ushort)(tbStartAdress + 125 * 5), (ushort)NReg), 5);

                    //TSR6.Inp(mbMasterR1.ReadHoldingRegisters(1, (ushort)(tbStartAdress + 125 * 6), (ushort)NReg), 6);
                    //TSR7.Inp(mbMasterR1.ReadHoldingRegisters(1, (ushort)(tbStartAdress + 125 * 7), (ushort)NReg), 7);
                    //TSR8.Inp(mbMasterR1.ReadHoldingRegisters(1, (ushort)(tbStartAdress + 125 * 8), (ushort)NReg), 8);
                    //TSR9.Inp(mbMasterR1.ReadHoldingRegisters(1, (ushort)(tbStartAdress + 125 * 9), (ushort)NReg), 9);
                    //TSR10.Inp(mbMasterR1.ReadHoldingRegisters(1, (ushort)(tbStartAdress + 125 * 10), (ushort)NReg), 10);
                    //TagSource.Input(mbMasterR1.ReadHoldingRegisters(1, (ushort)(tbStartAdress + 125 * 0), (ushort)NReg));
                    //  Debug.WriteLine("UpdateR1() ");
                    //System.Threading.Thread.Sleep(settings.TPause);
                    System.Threading.Thread.Sleep((int)Sett.Instance.TPause);
                }
            }
            private void UpdateR2()
            {
                while (true)
                {
                    //  int AreaR = (29 - 3 + 1) * 50; //    2000; // Convert.ToInt32(textBoxAreaR.Text); // 
                    int NReg = 125; // Convert.ToInt32(textBoxNReg.Text);
                                    // int tbStartAdress = settings.iBegAddrR; // (Convert.ToUInt16(textBoxStartAdress.Text))
                    int tbStartAdress = Sett.Instance.BegAddrR;
                    //TSR0.Inp(mbMasterR2.ReadHoldingRegisters(1, (ushort)(tbStartAdress + 125 * 0), (ushort)NReg), 0);
                    //TSR1.Inp(mbMasterR2.ReadHoldingRegisters(1, (ushort)(tbStartAdress + 125 * 1), (ushort)NReg), 1);
                    //TSR2.Inp(mbMasterR2.ReadHoldingRegisters(1, (ushort)(tbStartAdress + 125 * 2), (ushort)NReg), 2);
                    //TSR3.Inp(mbMasterR2.ReadHoldingRegisters(1, (ushort)(tbStartAdress + 125 * 3), (ushort)NReg), 3);
                    //TSR4.Inp(mbMasterR2.ReadHoldingRegisters(1, (ushort)(tbStartAdress + 125 * 4), (ushort)NReg), 4);
                    //TSR5.Inp(mbMasterR2.ReadHoldingRegisters(1, (ushort)(tbStartAdress + 125 * 5), (ushort)NReg), 5);
                    TSR6.Inp(mbMasterR2.ReadHoldingRegisters(1, (ushort)(tbStartAdress + 125 * 6), (ushort)NReg), 6);
                    TSR7.Inp(mbMasterR2.ReadHoldingRegisters(1, (ushort)(tbStartAdress + 125 * 7), (ushort)NReg), 7);
                    TSR8.Inp(mbMasterR2.ReadHoldingRegisters(1, (ushort)(tbStartAdress + 125 * 8), (ushort)NReg), 8);
                    //TSR9.Inp(mbMasterR2.ReadHoldingRegisters(1, (ushort)(tbStartAdress + 125 * 9), (ushort)NReg), 9);
                    //TSR10.Inp(mbMasterR2.ReadHoldingRegisters(1, (ushort)(tbStartAdress + 125 * 10), (ushort)NReg), 10);
                    //TagSource.Input(mbMasterR2.ReadHoldingRegisters(1, (ushort)(tbStartAdress + 125 * 0), (ushort)NReg));
                    //   Debug.WriteLine("UpdateR2() ");
                    //System.Threading.Thread.Sleep(settings.TPause);
                    System.Threading.Thread.Sleep((int)Sett.Instance.TPause);
                }
            }
            private void UpdateR3()
            {
                while (true)
                {
                    //  int AreaR = (29 - 3 + 1) * 50; //    2000; // Convert.ToInt32(textBoxAreaR.Text); // 
                    int NReg = 125; // Convert.ToInt32(textBoxNReg.Text);
                    //int tbStartAdress = settings.iBegAddrR; // (Convert.ToUInt16(textBoxStartAdress.Text))
                    int tbStartAdress = Sett.Instance.BegAddrR;
                    //TSR0.Inp(mbMasterR3.ReadHoldingRegisters(1, (ushort)(tbStartAdress + 125 * 0), (ushort)NReg), 0);
                    //TSR1.Inp(mbMasterR3.ReadHoldingRegisters(1, (ushort)(tbStartAdress + 125 * 1), (ushort)NReg), 1);
                    //TSR2.Inp(mbMasterR3.ReadHoldingRegisters(1, (ushort)(tbStartAdress + 125 * 2), (ushort)NReg), 2);
                    //TSR3.Inp(mbMasterR3.ReadHoldingRegisters(1, (ushort)(tbStartAdress + 125 * 3), (ushort)NReg), 3);
                    //TSR4.Inp(mbMasterR3.ReadHoldingRegisters(1, (ushort)(tbStartAdress + 125 * 4), (ushort)NReg), 4);
                    //TSR5.Inp(mbMasterR3.ReadHoldingRegisters(1, (ushort)(tbStartAdress + 125 * 5), (ushort)NReg), 5);
                    //TSR6.Inp(mbMasterR3.ReadHoldingRegisters(1, (ushort)(tbStartAdress + 125 * 6), (ushort)NReg), 6);
                    //TSR7.Inp(mbMasterR3.ReadHoldingRegisters(1, (ushort)(tbStartAdress + 125 * 7), (ushort)NReg), 7);
                    //TSR8.Inp(mbMasterR3.ReadHoldingRegisters(1, (ushort)(tbStartAdress + 125 * 8), (ushort)NReg), 8);
                    TSR9.Inp(mbMasterR3.ReadHoldingRegisters(1, (ushort)(tbStartAdress + 125 * 9), (ushort)NReg), 9);
                    TSR10.Inp(mbMasterR3.ReadHoldingRegisters(1, (ushort)(tbStartAdress + 125 * 10), (ushort)NReg), 10);
                    TagSource.Input(mbMasterR0.ReadHoldingRegisters(1, (ushort)(tbStartAdress + 125 * 0), (ushort)NReg));
                    //    Debug.WriteLine("UpdateR3() ");
                    //System.Threading.Thread.Sleep(settings.TPause);
                    System.Threading.Thread.Sleep((int)Sett.Instance.TPause);
                }
            }*/
    }
}
