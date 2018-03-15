using System.Configuration; // xml
using System.Collections.Specialized; //xml
using Modbus.Device;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics; // for DEBAG 
using System.Xml;
//using System.Windows.Forms;
using System.IO;
using System.Xml.Serialization;
using System.Data;
using Simulator_MPSA.CL;
using System.Windows.Forms;
using System.ComponentModel;

namespace Simulator_MPSA
{
   
    /*public  class clS
    {
        public string HostName = "192.168.201.1"; // IP adress CPU
        public  int MBPort = 502; // Modbus TCP port adress
        public  int TPause = 50; // задержка между циклами чтения
        public  int nWrTask = 4; // потоков на запись в CPU
        public  int iBegAddrR = 23170 - 1; // начальный адрес для чтения выходов CPU
        public  int iBegAddrW = 15100 - 1; // начальный адрес для записи входов CPU
        public  int iNRackBeg = 3; // номер начальной корзины
        public  int iNRackEnd = 29; // номер конечной корзины
        public  int nAI = 1000; // count of AI 
        public static int nDI = 128; // count of DI 
        public static int nDO = 64; // count of DO 
        public static int nZD = 64; // count of ZD  200
        public static int nKL = 64; // count of KL 100
        public static int nVS = 256; // count of VS 200
        public static int nMPNA = 16; // count of MPNA
    }*/

    public static class RB
    {
        public static ushort[] R;// = new ushort[(Sett.iNRackEnd) * 50];//[(29 - 3 + 1) * 50]    =1450   From IOScaner CPU
    }
    public static class WB
    {
        public static ushort[] W;// = new ushort[(Sett.iNRackEnd - Sett.iNRackBeg + 1) * 126]; // =3402 From IOScaner CPU
    }
    public static class DeBag
    {
        public static ulong RR;
        public static ulong WW;
        public static ushort usR;
        public static ushort usW;

    }
    enum SimState { Stop, Work};
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SimState simState;
        public MainWindow()
        {
          
            InitializeComponent();
            TagSource = new Depoller(Dispatcher);
            TSR0 = new Depoller(Dispatcher);
            TSR1 = new Depoller(Dispatcher);
            TSR2 = new Depoller(Dispatcher);
            TSR3 = new Depoller(Dispatcher);
            TSR4 = new Depoller(Dispatcher);
            TSR5 = new Depoller(Dispatcher);
            TSR6 = new Depoller(Dispatcher);
            TSR7 = new Depoller(Dispatcher);
            TSR8 = new Depoller(Dispatcher);
            TSR9 = new Depoller(Dispatcher);
            TSR10 = new Depoller(Dispatcher);
            TSW = new Depoller(Dispatcher);


            //Debug.WriteLine("------------------------------------------------------------------");
            //string sAttr, sAllAtr;
            //sAttr = ConfigurationManager.AppSettings.Get("Key0");
            //Debug.WriteLine("Key0 = " + sAttr);
            //ConfigurationManager.AppSettings.Set("Key0", "777");
            //sAllAtr = ConfigurationManager.AppSettings.ToString();
            //Debug.Write("AllAtr = " + sAllAtr + " \n");
            //sAttr = ConfigurationManager.AppSettings.Get("Key0");
            //Debug.WriteLine("Key0 = " + sAttr);
            //Debug.WriteLine("------------------------------------------------------------------");
            //NameValueCollection sAll; sAll = ConfigurationManager.AppSettings;
            //foreach (string s in sAll.AllKeys) Debug.WriteLine("Key: " + s + " Value: " + sAll.Get(s));
            //Debug.WriteLine("------------------------------------------------------------------");

            //начальная инициализация структур и моделей
            dataGridDO.DataContext = new DOTableViewModel();
            //  for (int i = 0; i < DOs.Length; i++)
            //    DOs[i] = new DOStruct();

            // dataGridDI.DataContext = new DITableViewModel();
           dataGridDI.ItemsSource=  DITableViewModel.Instance.viewSource.View;
            // for (int i = 0; i < DIs.Length; i++)
            //     DIs[i] = new DIStruct();
            dataGridAI.ItemsSource = AITableViewModel.Instance.viewSource.View;
            //for (int i = 0; i < AIs.Length; i++)
            //      AIs[i] = new AIStruct();
            dataGridSettings.DataContext = new SettingsTableViewModel(Sett.Instance);
            dataGridZD.DataContext = ZDTableViewModel.Instance;
            dataGridVS.DataContext = VSTableViewModel.Instance;
            dataGridKL.DataContext = KLTableViewModel.Instance;
            dataGridMPNA.DataContext = MPNATableViewModel.Instance;

            statusText.Content = "Остановлен";
            statusText.Background = System.Windows.Media.Brushes.Yellow;

        }
        #region IPMasters
        ModbusIpMaster mbMaster;
        ModbusIpMaster mbMasterR0;
        ModbusIpMaster mbMasterR1;
        ModbusIpMaster mbMasterR2;
        ModbusIpMaster mbMasterR3;
        ModbusIpMaster mbMasterW0;
        ModbusIpMaster mbMasterW1;
        ModbusIpMaster mbMasterW2;
        ModbusIpMaster mbMasterW3;
        #endregion
        #region Loops
        Task masterLoop;
        Task masterLoopR0;
        Task masterLoopR1;
        Task masterLoopR2;
        Task masterLoopR3;
        Task masterLoopW0;
        Task masterLoopW1;
        Task masterLoopW2;
        Task masterLoopW3;
        #endregion
        private void Update()
        {
            while (!cancelTokenSrc.IsCancellationRequested)   
            {

                foreach (ZDStruct zd in ZDTableViewModel.ZDs)
                    zd.UpdateZD((float)Sett.Instance.TPause/1000f);

                foreach (KLStruct kl in KLTableViewModel.KL)
                    kl.UpdateKL((float)Sett.Instance.TPause / 1000f);

                foreach (MPNAStruct mpna in MPNATableViewModel.MPNAs)
                    mpna.UpdateMPNA((float)Sett.Instance.TPause / 1000f);

                foreach (VSStruct vs in VSTableViewModel.VS)
                    vs.UpdateVS((float)Sett.Instance.TPause / 1000f);
               // ct1.ThrowIfCancellationRequested();                
                GetDOfromR(); // записываем значение DO из массива для чтения CPU
                SendAItoW(); // записываем значение АЦП в массив для записи CPU
                SendDItoW(); // записываем значение DI в массив для записи CPU
                Debug.WriteLine("Update()"); // + NReg + " " + tbStartAdress);
                System.Threading.Thread.Sleep(Sett.Instance.TPause);
            }
        }
        #region UpdateReaders
        private void UpdateR0()
        {
            while (!cancelTokenSrc.IsCancellationRequested)
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
                Debug.WriteLine("UpdateR0() ");
                //System.Threading.Thread.Sleep(settings.TPause);
                System.Threading.Thread.Sleep((int)Sett.Instance.TPause);
            }
        }
        private void UpdateR1()
        {
            while (!cancelTokenSrc.IsCancellationRequested)
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
                Debug.WriteLine("UpdateR1() ");
                //System.Threading.Thread.Sleep(settings.TPause);
                System.Threading.Thread.Sleep((int)Sett.Instance.TPause);
            }
        }
        private void UpdateR2()
        {
            while (!cancelTokenSrc.IsCancellationRequested)
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
                Debug.WriteLine("UpdateR2() ");
                //System.Threading.Thread.Sleep(settings.TPause);
                System.Threading.Thread.Sleep((int)Sett.Instance.TPause);
            }
        }
        private void UpdateR3()
        {
            while (!cancelTokenSrc.IsCancellationRequested)
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
                Debug.WriteLine("UpdateR3() ");
                //System.Threading.Thread.Sleep(settings.TPause);
                System.Threading.Thread.Sleep((int)Sett.Instance.TPause);
            }
        }
        #endregion
        #region UpdateWriters
        private void UpdateW0()
        {
            while (!cancelTokenSrc.IsCancellationRequested)
            {
                int n = 0; // W0
                int AreaW = WB.W.Length; //  (29 - 3 + 1) * 126]; // 3402 
                int NReg = 120; // Convert.ToInt32(textBoxNReg.Text);
                int tbStartAdress = Sett.Instance.BegAddrW + n * NReg * Sett.Instance.NWrTask; // (Convert.ToUInt16(textBoxStartAdress.Text))
                ushort[] data = new ushort[NReg];
                int c = AreaW / NReg / Sett.Instance.NWrTask;
                for (int i = Sett.Instance.NWrTask * n; i < (Sett.Instance.NWrTask * n + c); i++)
                {
                    Array.Copy(WB.W, (NReg * i), data, (0), NReg);
                    mbMasterW0.WriteMultipleRegisters(1, (ushort)(tbStartAdress + NReg * i), data);
                    //TSW.Wr(mbMasterW.WriteMultipleRegisters(1, (ushort)(tbStartAdress + 125 * 4), data )); 
                    //System.Threading.Thread.Sleep(Sett.TPause);
                }
                DeBag.WW++;
                Debug.WriteLine("W0()   WW= " + DeBag.WW + " /n");
                System.Threading.Thread.Sleep(Sett.Instance.TPause);
            }
        }
        private void UpdateW1()
        {
            while (!cancelTokenSrc.IsCancellationRequested)
            {
                int n = 1; // W0
                int AreaW = WB.W.Length; //  (29 - 3 + 1) * 126]; // 3402 
                int NReg = 120; // Convert.ToInt32(textBoxNReg.Text);
                int tbStartAdress = Sett.Instance.BegAddrW + n * NReg * Sett.Instance.NWrTask; // (Convert.ToUInt16(textBoxStartAdress.Text))
                ushort[] data = new ushort[NReg];
                int c = AreaW / NReg / Sett.Instance.NWrTask;
                for (int i = Sett.Instance.NWrTask * n; i < (Sett.Instance.NWrTask * n + c); i++)
                {
                    Array.Copy(WB.W, (NReg * i), data, (0), NReg);
                    mbMasterW1.WriteMultipleRegisters(1, (ushort)(tbStartAdress + NReg * i), data);
                    //TSW.Wr(mbMasterW.WriteMultipleRegisters(1, (ushort)(tbStartAdress + 125 * 4), data )); 
                    //System.Threading.Thread.Sleep(Sett.TPause);
                }
                //DeBag.WW++;
                Debug.WriteLine("W1()");
                System.Threading.Thread.Sleep(Sett.Instance.TPause);
            }
        }
        private void UpdateW2()
        {
            while (!cancelTokenSrc.IsCancellationRequested)
            {
                int n = 2; // W0
                int AreaW = WB.W.Length; //  (29 - 3 + 1) * 126]; // 3402 
                int NReg = 120; // Convert.ToInt32(textBoxNReg.Text);
                int tbStartAdress = Sett.Instance.BegAddrW + n * NReg * Sett.Instance.NWrTask; // (Convert.ToUInt16(textBoxStartAdress.Text))
                ushort[] data = new ushort[NReg];
                int c = AreaW / NReg / Sett.Instance.NWrTask;
                for (int i = Sett.Instance.NWrTask * n; i < (Sett.Instance.NWrTask * n + c); i++)
                {
                    Array.Copy(WB.W, (NReg * i), data, (0), NReg);
                    mbMasterW2.WriteMultipleRegisters(1, (ushort)(tbStartAdress + NReg * i), data);
                    //TSW.Wr(mbMasterW.WriteMultipleRegisters(1, (ushort)(tbStartAdress + 125 * 4), data )); 
                    //System.Threading.Thread.Sleep(Sett.TPause);
                }
                //DeBag.WW++;
                Debug.WriteLine("W2()   ");
                System.Threading.Thread.Sleep(Sett.Instance.TPause);
            }
        }
        private void UpdateW3()
        {
            while (!cancelTokenSrc.IsCancellationRequested)
            {
                int n = 3; // W0
                int AreaW = WB.W.Length; //  (29 - 3 + 1) * 126]; // 3402 
                int NReg = 120; // Convert.ToInt32(textBoxNReg.Text);
                int tbStartAdress = Sett.Instance.BegAddrW + n * NReg * Sett.Instance.NWrTask; // (Convert.ToUInt16(textBoxStartAdress.Text))
                ushort[] data = new ushort[NReg];
                int c = AreaW / NReg / Sett.Instance.NWrTask;
                for (int i = Sett.Instance.NWrTask * n; i < (Sett.Instance.NWrTask * n + c); i++)
                {
                    Array.Copy(WB.W, (NReg * i), data, (0), NReg);
                    mbMasterW3.WriteMultipleRegisters(1, (ushort)(tbStartAdress + NReg * i), data);
                    //TSW.Wr(mbMasterW.WriteMultipleRegisters(1, (ushort)(tbStartAdress + 125 * 4), data )); 
                    //System.Threading.Thread.Sleep(Sett.TPause);
                }
                //DeBag.WW++;
                Debug.WriteLine("W3()   ");
                System.Threading.Thread.Sleep(Sett.Instance.TPause);
            }
        }
        #endregion
   
        #region Dedollers
        public Depoller TagSource
        {
            get;
            private set;
        }
        public Depoller TSR0
        {
            get;
            private set;
        }
        public Depoller TSR1
        {
            get;
            private set;
        }
        public Depoller TSR2
        {
            get;
            private set;
        }
        public Depoller TSR3
        {
            get;
            private set;
        }
        public Depoller TSR4
        {
            get;
            private set;
        }
        public Depoller TSR5
        {
            get;
            private set;
        }
        public Depoller TSR6
        {
            get;
            private set;
        }
        public Depoller TSR7
        {
            get;
            private set;
        }
        public Depoller TSR8
        {
            get;
            private set;
        }
        public Depoller TSR9
        {
            get;
            private set;
        }
        public Depoller TSR10
        {
            get;
            private set;
        }
        public Depoller TSW
        {
            get;
            private set;
        }
        #endregion
        void SendAItoW()  // записываем значение АЦП в массив для записи CPU
        {
            for (int i = 0; i < AIStruct.items.Length; i++)
            {
                if (AIStruct.items[i].En /* || true */)
                {
                    WB.W[(AIStruct.items[i].indxW)] = AIStruct.items[i].ValACD; // записываем значение АЦП в массив для записи CPU
                }
            }
        }
        void SendDItoW() // копирование значения сигналов DI в массив для записи в ЦПУ
        {
            for (int i = 0; i < DIStruct.items.Length; i++)
            {
                SetBit(ref (WB.W[(DIStruct.items[i].indxW)]), (DIStruct.items[i].indxBitDI), (DIStruct.items[i].ValDI));
            }
        }
        void GetDOfromR() // копирование значения сигналов DO из массива для чтения ЦПУ
        {
            for (int i = 0; i < DOStruct.items.Length; i++)
            {
                DOStruct.items[i].ValDO = GetBit(RB.R[DOStruct.items[i].indxR], DOStruct.items[i].indxBitDO);
            }
        }

        #region SetBit/GetBit
        // -----------------------------------------------------------------
        public void SetBit(ref ushort b, int bitNumber, bool state) 
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
        #endregion
        // -----------------------------------------------------------------
       // clS S = new clS(); // settings; на самом деле настройки в Sett !!!

        void CloseApp()
        {
            System.Windows.Application curApp = System.Windows.Application.Current;
            curApp.Shutdown();
        }
        private void MenuItem_Click_1(object sender, RoutedEventArgs e) // Close App
        {
            CloseApp();
        }

        private void Menu_OpenAll(object sender, RoutedEventArgs e)
        {
            Station station = new Station();

            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "XML Files (*.xml)|*.xml";
            dialog.FilterIndex = 0;
            dialog.DefaultExt = "xml";
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                if (station.Load(dialog.FileName) == StationLoadResult.OK)
                {

                    /* AIStruct.items = station.AIs;
                     DIStruct.items = station.DIs;
                     DOStruct.items = station.DOs;

                     VSStruct.VSs = station.VSs;
                     KLStruct.KLs = station.KLs;
                     MPNAStruct.MPNAs = station.MPNAs;*/
                    //Sett.Instance = station.settings;

                    RB.R = new ushort[(Sett.Instance.NRackEnd) * 50];//[(29 - 3 + 1) * 50]    =1450   From IOScaner CPU
                    WB.W = new ushort[(Sett.Instance.NRackEnd - Sett.Instance.NRackBeg + 1) * 126]; // =3402 From IOScaner CPU

                    AITableViewModel.Instance.Init(AIStruct.items);
                    dataGridAI.ItemsSource = AITableViewModel.Instance.viewSource.View;

                    DITableViewModel.Instance.Init(DIStruct.items);
                    dataGridDI.ItemsSource = DITableViewModel.Instance.viewSource.View;


                    // dataGridDI.DataContext = new DITableViewModel(DIStruct.items);

                    DOTableViewModel.Instance.Init(DOStruct.items);
                    dataGridDO.ItemsSource = DOTableViewModel.Instance.viewSource.View;  
                    
                    dataGridSettings.DataContext = new SettingsTableViewModel(Sett.Instance);

                    dataGridVS.DataContext = VSTableViewModel.Instance;
                    dataGridKL.DataContext = KLTableViewModel.Instance;
                    dataGridMPNA.DataContext = MPNATableViewModel.Instance;
                    dataGridZD.DataContext = ZDTableViewModel.Instance; 

                    
                }
             
             //старый способ загрузки
           /* LoadSettings();
            LoadSettDI();
            LoadSettDO();
            LoadSettAI();

            LoadSettKL();
            LoadSettVS();
            LoadSettZD();
            LoadSettMPNA();*/
        }
        private void Menu_SaveSingle(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog
            {
                InitialDirectory = Directory.GetCurrentDirectory() + "\\XMLs",
                OverwritePrompt = true,
                Filter = "xml-файл|*.xml"
            };
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Station s = new Station();                
                s.settings = Sett.Instance;
                
                s.Save(dialog.FileName);
            }
           /* string activeTabHeader = ((TabItem)tabControl.SelectedItem).Header.ToString();
            Debug.WriteLine("save tab: "+ activeTabHeader);
            if (activeTabHeader == "Settings")
            {
                dialog.Title = "Сохранение Settings";
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    SaveSettings(dialog.FileName);
                return;
            }

            if (activeTabHeader == "AI")
            {
                dialog.Title = "Сохранение таблицы AI";
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    SaveSettAI(dialog.FileName);
                return;
            }

            if (activeTabHeader == "DI")
            {
                dialog.Title = "Сохранение таблицы DI";
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    SaveSettDI(dialog.FileName);
                return;
            }

            if (activeTabHeader == "DO")
            {
                dialog.Title = "Сохранение таблицы DO";
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    SaveSettDO(dialog.FileName);
                return;
            }

            if (activeTabHeader == "ZD")
            {
                dialog.Title = "Сохранение таблицы ZD";
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    SaveSettZD(dialog.FileName);
                return;
            }

            if (activeTabHeader == "KL")
            {
                dialog.Title = "Сохранение таблицы KL";
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    SaveSettKL(dialog.FileName);
                return;
            }

            if (activeTabHeader == "VS")
            {
                dialog.Title = "Сохранение таблицы VS";
                
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    SaveSettVS(dialog.FileName);
                return;
            }
            if (activeTabHeader == "MPNA")
            {
                dialog.Title = "Сохранение таблицы MPNA";
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    SaveSettMPNA(dialog.FileName);
                return;
            }*/

            /*   if (MessageBox.Show("Сохранить настройки в файл?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                   SaveSettings();*/

            /*            
             SaveSettings();
             SaveSettAI();
             SaveSettDI();
             SaveSettDO();
             SaveSettZD();
             SaveSettKL();
             SaveSettVS();
             SaveSettMPNA();*/


        }
        private void Menu_SaveAll(object sender, RoutedEventArgs e)
        {
            if (System.Windows.MessageBox.Show("Внимание! Все таблицы будут сохранены в файлы по умолчанию.", "Предупреждение", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.OK)
            {
               Station.SaveSettings();
                Station.SaveSettAI();
                Station.SaveSettDI();
                Station.SaveSettDO();
                Station.SaveSettZD();
                Station.SaveSettKL();
                Station.SaveSettVS();
                Station.SaveSettMPNA();
            }
        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            CloseApp();
        }

        /// <summary>
        /// Токен отмены
        /// </summary>
        CancellationTokenSource cancelTokenSrc;
        CancellationToken cancellationToken;
        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            cancelTokenSrc = new CancellationTokenSource();
            cancellationToken = cancelTokenSrc.Token;
            try
            {
                // mbMaster = ModbusIpMaster.CreateIp(new TcpClient("192.168.201.1", 502));  
                mbMaster = ModbusIpMaster.CreateIp(new TcpClient(Sett.Instance.HostName, Sett.Instance.MBPort));
                mbMasterR0 = ModbusIpMaster.CreateIp(new TcpClient(Sett.Instance.HostName, Sett.Instance.MBPort));
                mbMasterR1 = ModbusIpMaster.CreateIp(new TcpClient(Sett.Instance.HostName, Sett.Instance.MBPort));
                mbMasterR2 = ModbusIpMaster.CreateIp(new TcpClient(Sett.Instance.HostName, Sett.Instance.MBPort));
                mbMasterR3 = ModbusIpMaster.CreateIp(new TcpClient(Sett.Instance.HostName, Sett.Instance.MBPort));
                mbMasterW0 = ModbusIpMaster.CreateIp(new TcpClient(Sett.Instance.HostName, Sett.Instance.MBPort));
                mbMasterW1 = ModbusIpMaster.CreateIp(new TcpClient(Sett.Instance.HostName, Sett.Instance.MBPort));
                mbMasterW2 = ModbusIpMaster.CreateIp(new TcpClient(Sett.Instance.HostName, Sett.Instance.MBPort));
                mbMasterW3 = ModbusIpMaster.CreateIp(new TcpClient(Sett.Instance.HostName, Sett.Instance.MBPort));

                masterLoop = Task.Factory.StartNew(new Action(Update), cancellationToken);
                masterLoopR0 = Task.Factory.StartNew(new Action(UpdateR0), cancellationToken);
                masterLoopR1 = Task.Factory.StartNew(new Action(UpdateR1), cancellationToken);
                masterLoopR2 = Task.Factory.StartNew(new Action(UpdateR2), cancellationToken);
                masterLoopR3 = Task.Factory.StartNew(new Action(UpdateR3), cancellationToken);
                masterLoopW0 = Task.Factory.StartNew(new Action(UpdateW0), cancellationToken);
                masterLoopW1 = Task.Factory.StartNew(new Action(UpdateW1), cancellationToken);
                masterLoopW2 = Task.Factory.StartNew(new Action(UpdateW2), cancellationToken);
                masterLoopW3 = Task.Factory.StartNew(new Action(UpdateW3), cancellationToken);
                statusText.Content = "Запущен";
                statusText.Background = System.Windows.Media.Brushes.Green;
                btnStart.IsEnabled = false;
                btnStop.IsEnabled = true;
            }
            catch(Exception ex)
            {
                System.Windows.MessageBox.Show("Ошибка: " +Environment.NewLine + ex.Message, "Ошибка");
            }
        }
        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            
        }
        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            statusText.Content = "Остановлен";
            statusText.Background = System.Windows.Media.Brushes.Yellow;

            btnStart.IsEnabled = true;
            btnStop.IsEnabled = false;
            try
            {
                cancelTokenSrc.Cancel();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message,"Ошибка",MessageBoxButton.OK,MessageBoxImage.Error);
            }
        }

        private void MenuItem_Click_save(object sender, RoutedEventArgs e)
        {

        }

        SetupDialog dialog;
        private void dataGridVS_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if ((dialog != null)&&(dialog.IsLoaded))
                {
                    dialog.Activate();
                    //         dialog.Close();
                }
                else
                {
                VSStruct temp = dataGridVS.SelectedItem as VSStruct;
                    if (temp != null)
                    {
                        dialog = new SetupDialog(temp);
                        dialog.Show();
                    }
                }

        }

        private void dataGridKL_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if ((dialog != null) && (dialog.IsLoaded))
            {
                dialog.Activate();
            }
            else
            {
                KLStruct temp = dataGridKL.SelectedItem as KLStruct;
                if (temp != null)
                {
                    dialog = new SetupDialog(temp);
                    dialog.Show();
                }
            }
        }

        private void dataGridZD_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if ((dialog != null) && (dialog.IsLoaded))
            {
                dialog.Activate();
            }
            else
            {
                ZDStruct temp = dataGridZD.SelectedItem as ZDStruct;
                if (temp != null)
                {
                    dialog = new SetupDialog(temp);
                    dialog.Show();
                }
            }
        }

        private void Menu_LoadSeq(object sender, RoutedEventArgs e)
        {
           Station.LoadSettings();
            Station.LoadSettDI();
            Station.LoadSettDO();
            Station.LoadSettAI();

            Station.LoadSettKL();
            Station.LoadSettVS();
            Station.LoadSettZD();
            Station.LoadSettMPNA();

            AITableViewModel.Instance.Init(AIStruct.items);
            dataGridAI.ItemsSource = AITableViewModel.Instance.viewSource.View;

            DITableViewModel.Instance.Init(DIStruct.items);
            dataGridDI.ItemsSource = DITableViewModel.Instance.viewSource.View;
            // dataGridDI.DataContext = new DITableViewModel(DIStruct.items);

            DOTableViewModel.Instance.Init(DOStruct.items);
            dataGridDO.ItemsSource = DOTableViewModel.Instance.viewSource.View;


            dataGridSettings.DataContext = new SettingsTableViewModel(Sett.Instance);
            dataGridVS.DataContext = VSTableViewModel.Instance;
            dataGridKL.DataContext = KLTableViewModel.Instance;

            dataGridZD.DataContext = ZDTableViewModel.Instance;
            dataGridMPNA.DataContext = MPNATableViewModel.Instance;
        }

        private void dataGridMPNA_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if ((dialog != null) && (dialog.IsLoaded))
            {
                dialog.Activate();
            }
            else
            {
                MPNAStruct temp = dataGridMPNA.SelectedItem as MPNAStruct;
                if (temp != null)
                {
                    dialog = new SetupDialog(temp);
                    dialog.Show();
                }
            }
        }

        private void MenuItem_about_Click(object sender, RoutedEventArgs e)
        {
            AboutWindow w = new AboutWindow();
            w.ShowDialog();
        }

        

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //  DIFilter = textBoxDIFilter.Text;
           
  //          dataGridDI.ItemsSource = null;
   //         dataGridDI.ItemsSource = DITableViewModel.Instance.viewSource.View;
        }


        private void textBoxDIFilter_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Return)
                DITableViewModel.Instance.NameFilter = textBoxDIFilter.Text;
        }

        private void textBoxAIFilter_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Return)
                AITableViewModel.Instance.NameFilter = textBoxAIFilter.Text;
        }

        private void textBoxDOFilter_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Return)
                DOTableViewModel.Instance.NameFilter = textBoxDOFilter.Text;
        }
    }
}
