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
        ModbusIpMaster[] mbMasterR = new ModbusIpMaster[2] ;

        Thread[] thread = new Thread[2];
        TcpClient[] tcp = new TcpClient[2];
        ushort NReg = 125;
        int coilCount;
        public ReadThread(string hostname, int port)
        {
            coilCount = RB.R.Length / NReg + 1;
            for (int i = 0; i < 2; i++)
            {
                tcp[i] = new TcpClient(hostname, port);
                mbMasterR[i] = ModbusIpMaster.CreateIp(tcp[i]);

                if (i==0)
                thread[i] = new Thread(new ThreadStart(ThreadJob1));
                else
                    thread[i] = new Thread(new ThreadStart(ThreadJob2));

                thread[i].Start();
            }

        }
        public void Stop()
        {
            if (thread[0] != null)
            thread[0].Abort();
            tcp[0].Close();

            if (thread[1] != null)
                thread[1].Abort();
            tcp[1].Close();


            refMainWindow = null;
        }

        private void ThreadJob1()
        {
            
            ushort tbStartAdress = (ushort)Sett.Instance.BegAddrR;

            ushort[] data;

            while (true)
            {
                for (ushort i = 0; i < coilCount/2; i++)
                {
                    data = mbMasterR[0].ReadHoldingRegisters(1, (ushort)(tbStartAdress + NReg * i + Sett.Instance.IncAddr), NReg);
                    data.CopyTo(RB.R, NReg*i);
                }
                GetDOfromR();

                if (refMainWindow != null)
                    refMainWindow.Dispatcher.Invoke(refMainWindow.delegateEndRead);
                System.Threading.Thread.Sleep(Sett.Instance.TPause);
            }
        }

        private void ThreadJob2()
        {
             ushort NReg = 125;
        ushort tbStartAdress = (ushort)Sett.Instance.BegAddrR;

        ushort[] data;

            while (true)
            {
                for (int i = coilCount / 2; i< coilCount-1; i++)
                {
                    data = mbMasterR[1].ReadHoldingRegisters(1, (ushort) (tbStartAdress + NReg* i + Sett.Instance.IncAddr), NReg);
                    data.CopyTo(RB.R, NReg* i);
                }
                GetDOfromR();

                /*  if (refMainWindow != null)
                      refMainWindow.Dispatcher.Invoke(refMainWindow.delegateEndRead);*/
                System.Threading.Thread.Sleep(Sett.Instance.TPause);
            }
        }

        void GetDOfromR() // копирование значения сигналов DO из массива для чтения ЦПУ
        {
            foreach(DOStruct _do in DOStruct.items)
            {
                int indx = _do.PLCAddr - Sett.Instance.BegAddrR;
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
    }
}
