using System.Configuration; // xml
using System.Collections.Specialized; //xml
using Modbus.Device;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
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

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Sett settings;
        ZDTableViewModel zdmodel = new ZDTableViewModel();
        public MainWindow()
        {
            settings=new Sett();
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

            dataGridDI.DataContext = new DITableViewModel();
           // for (int i = 0; i < DIs.Length; i++)
           //     DIs[i] = new DIStruct();
            dataGridAI.DataContext = new AITableViewModel();
            //for (int i = 0; i < AIs.Length; i++)
            //      AIs[i] = new AIStruct();
            dataGridSettings.DataContext = new SettingsTableViewModel(settings);
            dataGridZD.DataContext = new ZDTableViewModel();
            dataGridVS.DataContext = new VSTableViewModel();
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
            while (true)   
            {
                GetDOfromR(); // записываем значение DO из массива для чтения CPU
                SendAItoW(); // записываем значение АЦП в массив для записи CPU
                SendDItoW(); // записываем значение DI в массив для записи CPU
                Debug.WriteLine("Update() = 1000ms "); // + NReg + " " + tbStartAdress);
                System.Threading.Thread.Sleep(1000 /*Sett.TPause*/ );
            }
        }
        #region UpdateReaders
        private void UpdateR0()
        {
            while (true)
            {
                //  int AreaR = (29 - 3 + 1) * 50; //    2000; // Convert.ToInt32(textBoxAreaR.Text); // 
                int NReg = 125; // Convert.ToInt32(textBoxNReg.Text);
                //int tbStartAdress = settings.iBegAddrR; // (Convert.ToUInt16(textBoxStartAdress.Text))
                int tbStartAdress = (int)settings.items["iBegAddR"].value;
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
                System.Threading.Thread.Sleep((int)settings.items["TPause"].value);
            }
        }
        private void UpdateR1()
        {
            while (true)
            {
                //  int AreaR = (29 - 3 + 1) * 50; //    2000; // Convert.ToInt32(textBoxAreaR.Text); // 
                int NReg = 125; // Convert.ToInt32(textBoxNReg.Text);
                //int tbStartAdress = settings.iBegAddrR; // (Convert.ToUInt16(textBoxStartAdress.Text))
                int tbStartAdress = (int)settings.items["iBegAddrR"].value;
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
                System.Threading.Thread.Sleep((int)settings.items["TPause"].value);
            }
        }
        private void UpdateR2()
        {
            while (true)
            {
                //  int AreaR = (29 - 3 + 1) * 50; //    2000; // Convert.ToInt32(textBoxAreaR.Text); // 
                int NReg = 125; // Convert.ToInt32(textBoxNReg.Text);
               // int tbStartAdress = settings.iBegAddrR; // (Convert.ToUInt16(textBoxStartAdress.Text))
                int tbStartAdress = (int)settings.items["iBegAddrR"].value;
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
                System.Threading.Thread.Sleep((int)settings.items["TPause"].value);
            }
        }
        private void UpdateR3()
        {
            while (true)
            {
                //  int AreaR = (29 - 3 + 1) * 50; //    2000; // Convert.ToInt32(textBoxAreaR.Text); // 
                int NReg = 125; // Convert.ToInt32(textBoxNReg.Text);
                //int tbStartAdress = settings.iBegAddrR; // (Convert.ToUInt16(textBoxStartAdress.Text))
                int tbStartAdress = (int)settings.items["iBegAddrR"].value;
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
                System.Threading.Thread.Sleep((int)settings.items["TPause"].value);
            }
        }
        #endregion
        #region UpdateWriters
        private void UpdateW0()
        {
            while (true)
            {
                int n = 0; // W0
                int AreaW = WB.W.Length; //  (29 - 3 + 1) * 126]; // 3402 
                int NReg = 120; // Convert.ToInt32(textBoxNReg.Text);
                int tbStartAdress = settings.BegAddrW + n * NReg * settings.NWrTask; // (Convert.ToUInt16(textBoxStartAdress.Text))
                ushort[] data = new ushort[NReg];
                int c = AreaW / NReg / settings.NWrTask;
                for (int i = settings.NWrTask * n; i < (settings.NWrTask * n + c); i++)
                {
                    Array.Copy(WB.W, (NReg * i), data, (0), NReg);
                    mbMasterW0.WriteMultipleRegisters(1, (ushort)(tbStartAdress + NReg * i), data);
                    //TSW.Wr(mbMasterW.WriteMultipleRegisters(1, (ushort)(tbStartAdress + 125 * 4), data )); 
                    //System.Threading.Thread.Sleep(Sett.TPause);
                }
                DeBag.WW++;
                Debug.WriteLine("W0()   WW= " + DeBag.WW + " /n");
                System.Threading.Thread.Sleep(settings.TPause);
            }
        }
        private void UpdateW1()
        {
            while (true)
            {
                int n = 1; // W0
                int AreaW = WB.W.Length; //  (29 - 3 + 1) * 126]; // 3402 
                int NReg = 120; // Convert.ToInt32(textBoxNReg.Text);
                int tbStartAdress = settings.BegAddrW + n * NReg * settings.NWrTask; // (Convert.ToUInt16(textBoxStartAdress.Text))
                ushort[] data = new ushort[NReg];
                int c = AreaW / NReg / settings.NWrTask;
                for (int i = settings.NWrTask * n; i < (settings.NWrTask * n + c); i++)
                {
                    Array.Copy(WB.W, (NReg * i), data, (0), NReg);
                    mbMasterW1.WriteMultipleRegisters(1, (ushort)(tbStartAdress + NReg * i), data);
                    //TSW.Wr(mbMasterW.WriteMultipleRegisters(1, (ushort)(tbStartAdress + 125 * 4), data )); 
                    //System.Threading.Thread.Sleep(Sett.TPause);
                }
                //DeBag.WW++;
                Debug.WriteLine("W1()");
                System.Threading.Thread.Sleep(settings.TPause);
            }
        }
        private void UpdateW2()
        {
            while (true)
            {
                int n = 2; // W0
                int AreaW = WB.W.Length; //  (29 - 3 + 1) * 126]; // 3402 
                int NReg = 120; // Convert.ToInt32(textBoxNReg.Text);
                int tbStartAdress = settings.BegAddrW + n * NReg * settings.NWrTask; // (Convert.ToUInt16(textBoxStartAdress.Text))
                ushort[] data = new ushort[NReg];
                int c = AreaW / NReg / settings.NWrTask;
                for (int i = settings.NWrTask * n; i < (settings.NWrTask * n + c); i++)
                {
                    Array.Copy(WB.W, (NReg * i), data, (0), NReg);
                    mbMasterW2.WriteMultipleRegisters(1, (ushort)(tbStartAdress + NReg * i), data);
                    //TSW.Wr(mbMasterW.WriteMultipleRegisters(1, (ushort)(tbStartAdress + 125 * 4), data )); 
                    //System.Threading.Thread.Sleep(Sett.TPause);
                }
                //DeBag.WW++;
                Debug.WriteLine("W2()   ");
                System.Threading.Thread.Sleep(settings.TPause);
            }
        }
        private void UpdateW3()
        {
            while (true)
            {
                int n = 3; // W0
                int AreaW = WB.W.Length; //  (29 - 3 + 1) * 126]; // 3402 
                int NReg = 120; // Convert.ToInt32(textBoxNReg.Text);
                int tbStartAdress = settings.BegAddrW + n * NReg * settings.NWrTask; // (Convert.ToUInt16(textBoxStartAdress.Text))
                ushort[] data = new ushort[NReg];
                int c = AreaW / NReg / settings.NWrTask;
                for (int i = settings.NWrTask * n; i < (settings.NWrTask * n + c); i++)
                {
                    Array.Copy(WB.W, (NReg * i), data, (0), NReg);
                    mbMasterW3.WriteMultipleRegisters(1, (ushort)(tbStartAdress + NReg * i), data);
                    //TSW.Wr(mbMasterW.WriteMultipleRegisters(1, (ushort)(tbStartAdress + 125 * 4), data )); 
                    //System.Threading.Thread.Sleep(Sett.TPause);
                }
                //DeBag.WW++;
                Debug.WriteLine("W3()   ");
                System.Threading.Thread.Sleep(settings.TPause);
            }
        }
        #endregion
        private void Grid_Loaded(object sender, RoutedEventArgs e) // выполняется при загрузке основной формы программы, то есть при Старте ПО
        {
           /* for (int i = 0; i < WB.W.Length; i++)   // for Debug !!!
            {
                WB.W[i] = (ushort)i; // заполняем массив для записи в ЦПУ значениями равными номеру элемента массива
            }*/

         /*   if (false) // !!! false == Disable Modbus for Debug !!!
            {
                // mbMaster = ModbusIpMaster.CreateIp(new TcpClient("192.168.201.1", 502));  
                mbMaster = ModbusIpMaster.CreateIp(new TcpClient(settings.HostName, settings.MBPort));
                mbMasterR0 = ModbusIpMaster.CreateIp(new TcpClient(settings.HostName, settings.MBPort));
                mbMasterR1 = ModbusIpMaster.CreateIp(new TcpClient(settings.HostName, settings.MBPort));
                mbMasterR2 = ModbusIpMaster.CreateIp(new TcpClient(settings.HostName, settings.MBPort));
                mbMasterR3 = ModbusIpMaster.CreateIp(new TcpClient(settings.HostName, settings.MBPort));
                mbMasterW0 = ModbusIpMaster.CreateIp(new TcpClient(settings.HostName, settings.MBPort));
                mbMasterW1 = ModbusIpMaster.CreateIp(new TcpClient(settings.HostName, settings.MBPort));
                mbMasterW2 = ModbusIpMaster.CreateIp(new TcpClient(settings.HostName, settings.MBPort));
                mbMasterW3 = ModbusIpMaster.CreateIp(new TcpClient(settings.HostName, settings.MBPort));

                masterLoop = Task.Factory.StartNew(new Action(Update));
                masterLoopR0 = Task.Factory.StartNew(new Action(UpdateR0));
                masterLoopR1 = Task.Factory.StartNew(new Action(UpdateR1));
                masterLoopR2 = Task.Factory.StartNew(new Action(UpdateR2));
                masterLoopR3 = Task.Factory.StartNew(new Action(UpdateR3));
                masterLoopW0 = Task.Factory.StartNew(new Action(UpdateW0));
                masterLoopW1 = Task.Factory.StartNew(new Action(UpdateW1));
                masterLoopW2 = Task.Factory.StartNew(new Action(UpdateW2));
                masterLoopW3 = Task.Factory.StartNew(new Action(UpdateW3));
            } else { 
            //mbMaster = ModbusIpMaster.CreateIp(new TcpClient(Sett.HostName, Sett.MBPort));
            masterLoop = Task.Factory.StartNew(new Action(Update));
            }*/
        }
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
            for (int i = 0; i < AIStruct.AIs.Length; i++)
            {
                if (AIStruct.AIs[i].En /* || true */)
                {
                    WB.W[(AIStruct.AIs[i].indxW)] = AIStruct.AIs[i].ValACD; // записываем значение АЦП в массив для записи CPU
                }
            }
        }
        void SendDItoW() // копирование значения сигналов DI в массив для записи в ЦПУ
        {
            for (int i = 0; i < DIStruct.DIs.Length; i++)
            {
                SetBit(ref (WB.W[(DIStruct.DIs[i].IndxW)]), (DIStruct.DIs[i].indxBitDI), (DIStruct.DIs[i].ValDI));
            }
        }
        void GetDOfromR() // копирование значения сигналов DO из массива для чтения ЦПУ
        {
            for (int i = 0; i < DOStruct.DOs.Length; i++)
            {
                DOStruct.DOs[i].ValDO = GetBit(RB.R[DOStruct.DOs[i].indxR], DOStruct.DOs[i].indxBitDO);
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
        #region settings.xml
        void SaveSettings(string Sxml= "XMLs//" + "settings.xml")
        {
            XmlSerializer xml = new XmlSerializer(typeof(Sett));
            System.IO.StreamWriter writeStream = new System.IO.StreamWriter(Sxml);
            xml.Serialize(writeStream, settings);
            writeStream.Dispose();
            System.Windows.MessageBox.Show("Файл " + Sxml + " сохранен ");
        }
        void LoadSettings(string Sxml = "XMLs//" + "settings.xml")
        {
            XmlSerializer xml = new XmlSerializer(typeof(Sett));
            System.IO.StreamReader reader = null;
            try
            {
                reader = new System.IO.StreamReader(Sxml);
                settings = (Sett)xml.Deserialize(reader);
                reader.Dispose();
                System.Windows.MessageBox.Show("Файл " + Sxml + " считан ");
            }
            catch
            {
                if (reader != null)
                    reader.Dispose();
                System.IO.StreamWriter writer = new System.IO.StreamWriter(Sxml);
                xml.Serialize(writer, settings);
                writer.Dispose();
                System.Windows.MessageBox.Show("Файл " + Sxml + " не считан !!! ");
            }
            RB.R = new ushort[(settings.NRackEnd) * 50];//[(29 - 3 + 1) * 50]    =1450   From IOScaner CPU
            WB.W = new ushort[(settings.NRackEnd - settings.NRackBeg + 1) * 126]; // =3402 From IOScaner CPU
            AIStruct.AIs = new AIStruct[settings.NAI];
            //ZDs = new ZDStruct[settings.NZD];
            DOStruct.DOs =new DOStruct[settings.NDO * 32];
            //ZDs = new ZDStruct[settings.NZD];
            KLs = new KLStruct[settings.NKL];
            VSs = new VSStruct[settings.NVS];
            MPNAs = new MPNAStruct[settings.NMPNA];
            DIStruct.DIs = new DIStruct[settings.NDI * 32];
            //TODO: вставить код активации кнопки
        }
        #endregion
        // -----------------------------------------------------------------

       // public AIStruct[] AIs;// = new AIStruct[settings.nAI];

        #region AIsettings.xml
        public void LoadSettAI(string Sxml = "XMLs//" + "AIsettings.xml")
        {
            XmlSerializer xml = new XmlSerializer(typeof(AIStruct[]));
            System.IO.StreamReader reader = null;
            try
            {
                reader = new System.IO.StreamReader(Sxml);
                AIStruct.AIs = (AIStruct[])xml.Deserialize(reader);
                reader.Dispose();

                System.Windows.Forms.MessageBox.Show("AIsettings.xml loaded.");
            }
            catch
            {
                System.IO.StreamWriter writer = new System.IO.StreamWriter(Sxml);
                xml.Serialize(writer, AIStruct.AIs);
                writer.Dispose();
            }
        }
        // ---------------------------------------------------------------------
        void SaveSettAI(string Sxml = "XMLs//" + "AIsettings.xml")
        {
            XmlSerializer xml = new XmlSerializer(typeof(AIStruct[]));
            System.IO.StreamWriter writeStream = new System.IO.StreamWriter(Sxml);
            xml.Serialize(writeStream, AIStruct.AIs);
            writeStream.Dispose();
            System.Windows.Forms.MessageBox.Show("AIsettings.xml saved.");
        }
        #endregion
        // ---------------------------------------------------------------------

       // public DIStruct[] DIs;// = new DIStruct[Sett.nDI * 32];

        #region DIsettings.xml
        public void LoadSettDI(string Sxml = "XMLs//" + "DIsettings.xml")
        {
            XmlSerializer xml = new XmlSerializer(typeof(DIStruct[]));
            System.IO.StreamReader reader = null;
            try
            {
                reader = new System.IO.StreamReader(Sxml);
                DIStruct.DIs = (DIStruct[])xml.Deserialize(reader);
                reader.Dispose();
                System.Windows.Forms.MessageBox.Show("DIsettings.xml loaded.");
            }
            catch
            {
                System.IO.StreamWriter writer = new System.IO.StreamWriter(Sxml);
                xml.Serialize(writer, DIStruct.DIs);
                writer.Dispose();
            }
        }
        // ---------------------------------------------------------------------
        void SaveSettDI(string Sxml = "XMLs//" + "DIsettings.xml")
        {
            XmlSerializer xml = new XmlSerializer(typeof(DIStruct[]));
            System.IO.StreamWriter writeStream = new System.IO.StreamWriter(Sxml);
            xml.Serialize(writeStream, DIStruct.DIs);
            writeStream.Dispose();
            System.Windows.Forms.MessageBox.Show("DIsettings.xml saved.");
        }
        #endregion
        // ---------------------------------------------------------------------

      //  public DOStruct[] DOs;// new DOStruct[Sett.nDO * 32];

        #region DOsettings.xml
        public void LoadSettDO(string Sxml = "XMLs//" + "DOsettings.xml")
        {
            XmlSerializer xml = new XmlSerializer(typeof(DOStruct[]));
            System.IO.StreamReader reader = null;
            try
            {
                reader = new System.IO.StreamReader(Sxml);
                DOStruct.DOs = (DOStruct[])xml.Deserialize(reader);
                reader.Dispose();
                System.Windows.Forms.MessageBox.Show("DOsettings.xml loaded.");
            }
            catch
            {
                System.IO.StreamWriter writer = new System.IO.StreamWriter( Sxml);
                xml.Serialize(writer, DOStruct.DOs);
                writer.Dispose();
            }
        }
        // ---------------------------------------------------------------------
        void SaveSettDO(string Sxml = "XMLs//" + "DOsettings.xml")
        {
            XmlSerializer xml = new XmlSerializer(typeof(DOStruct[]));
            System.IO.StreamWriter writeStream = new System.IO.StreamWriter(Sxml);
            xml.Serialize(writeStream, DOStruct.DOs);
            writeStream.Dispose();
            System.Windows.Forms.MessageBox.Show("DOsettings.xml saved.");
        }
        #endregion
        // ---------------------------------------------------------------------
        // ---------------------------------------------------------------------
        // = new ZDStruct[Sett.nZD];
        #region ZDsettings.xml
        public void LoadSettZD(string Sxml = "XMLs//" + "ZDsettings.xml")
        {
            ZDStruct[] ZDs;
        XmlSerializer xml = new XmlSerializer(typeof(ZDStruct[]));
            System.IO.StreamReader reader = null;
            try
            {
                reader = new System.IO.StreamReader(Sxml);
                ZDs = (ZDStruct[])xml.Deserialize(reader);
                reader.Dispose();

                zdmodel = new ZDTableViewModel(ZDs);
                dataGridZD.DataContext = zdmodel;

                System.Windows.Forms.MessageBox.Show("ZDsettings.xml loaded.");
            }
            catch
            {
                /*
                System.IO.StreamWriter writer = new System.IO.StreamWriter(Sxml);
                xml.Serialize(writer, ZDs);
                writer.Dispose();*/
            }
        
        }
        // ---------------------------------------------------------------------
        void SaveSettZD(string Sxml = "XMLs//" + "ZDsettings.xml")
        {
            ZDStruct[] ZDs = new ZDStruct[zdmodel.Count];
            zdmodel.ZDs.CopyTo(ZDs, 0);
            XmlSerializer xml = new XmlSerializer(typeof(ZDStruct[]));
            System.IO.StreamWriter writeStream = new System.IO.StreamWriter(Sxml);

            xml.Serialize(writeStream, ZDs);
            writeStream.Dispose();
            System.Windows.Forms.MessageBox.Show("ZDsettings.xml saved.");
        }
        #endregion
        // ---------------------------------------------------------------------
        // ---------------------------------------------------------------------
        public KLStruct[] KLs;// = new KLStruct[Sett.nKL];
        #region KLsettings.xml
        public void LoadSettKL(string Sxml = "XMLs//" + "KLsettings.xml")
        {
            XmlSerializer xml = new XmlSerializer(typeof(KLStruct[]));
            System.IO.StreamReader reader = null;
            try
            {
                reader = new System.IO.StreamReader(Sxml);
                KLs = (KLStruct[])xml.Deserialize(reader);
                reader.Dispose();
                System.Windows.Forms.MessageBox.Show("KLsettings.xml loaded.");
            }
            catch
            {
                System.IO.StreamWriter writer = new System.IO.StreamWriter(Sxml);
                xml.Serialize(writer, KLs);
                writer.Dispose();
            }
        }
        // ---------------------------------------------------------------------
        void SaveSettKL(string Sxml = "XMLs//" + "KLsettings.xml")
        {
            XmlSerializer xml = new XmlSerializer(typeof(KLStruct[]));
            System.IO.StreamWriter writeStream = new System.IO.StreamWriter(Sxml);
            xml.Serialize(writeStream, KLs);
            writeStream.Dispose();
            System.Windows.Forms.MessageBox.Show("KLsettings.xml saved.");
        }
        #endregion
        // ---------------------------------------------------------------------
        // ---------------------------------------------------------------------
        public VSStruct[] VSs;// = new VSStruct[Sett.nVS];
        #region VSsettings.xml
        public void LoadSettVS(string Sxml = "XMLs//" + "VSsettings.xml")
        {
            XmlSerializer xml = new XmlSerializer(typeof(VSStruct[]));
            System.IO.StreamReader reader = null;
            try
            {
                reader = new System.IO.StreamReader(Sxml);
                VSs = (VSStruct[])xml.Deserialize(reader);
                reader.Dispose();
                System.Windows.Forms.MessageBox.Show("VSsettings.xml loaded.");
            }
            catch
            {
                System.IO.StreamWriter writer = new System.IO.StreamWriter(Sxml);
                xml.Serialize(writer, VSs);
                writer.Dispose();
            }
        }
        // ---------------------------------------------------------------------
        void SaveSettVS(string Sxml = "XMLs//" + "VSsettings.xml")
        {
            XmlSerializer xml = new XmlSerializer(typeof(VSStruct[]));
            System.IO.StreamWriter writeStream = new System.IO.StreamWriter(Sxml);
            xml.Serialize(writeStream, VSTableViewModel.GetArray());
            writeStream.Dispose();
            System.Windows.Forms.MessageBox.Show("VSsettings.xml saved.");
        }
        #endregion
        // ---------------------------------------------------------------------
        // ---------------------------------------------------------------------
        public MPNAStruct[] MPNAs;// = new MPNAStruct[Sett.nMPNA];
        #region MPNAsettings.xml
        public void LoadSettMPNA(string Sxml = "XMLs//" + "MPNAsettings.xml")
        {
            XmlSerializer xml = new XmlSerializer(typeof(MPNAStruct[]));
            System.IO.StreamReader reader = null;
            try
            {
                reader = new System.IO.StreamReader( Sxml);
                MPNAs = (MPNAStruct[])xml.Deserialize(reader);
                reader.Dispose();
                System.Windows.Forms.MessageBox.Show("MPNAsettings.xml loaded.");
            }
            catch
            {
                System.IO.StreamWriter writer = new System.IO.StreamWriter(Sxml);
                xml.Serialize(writer, MPNAs);
                writer.Dispose();
            }
        }
        // ---------------------------------------------------------------------
        void SaveSettMPNA(string Sxml = "XMLs//" + "MPNAsettings.xml")
        {
            XmlSerializer xml = new XmlSerializer(typeof(MPNAStruct[]));
            System.IO.StreamWriter writeStream = new System.IO.StreamWriter(Sxml);
            xml.Serialize(writeStream, MPNAs);
            writeStream.Dispose();
            System.Windows.Forms.MessageBox.Show(Sxml + " saved.");
        }
        #endregion
        // ---------------------------------------------------------------------
        // ---------------------------------------------------------------------
        void OpenXMLs()
        {
            LoadSettings();
            LoadSettAI();
            LoadSettDI(); 
            LoadSettDO();
            LoadSettZD();
            LoadSettKL();
            LoadSettVS();

            dataGridAI.DataContext = new AITableViewModel(AIStruct.AIs);
            dataGridDI.DataContext = new DITableViewModel(DIStruct.DIs);
            dataGridDO.DataContext = new DOTableViewModel(DOStruct.DOs);
            dataGridSettings.DataContext = new SettingsTableViewModel(settings);
            dataGridVS.DataContext = new VSTableViewModel(VSs);
            dataGridKL.DataContext = new KLTableViewModel(KLs);

        }
        // ---------------------------------------------------------------------


        private void MenuItem_Click(object sender, RoutedEventArgs e) // open xml
        {
            OpenXMLs();
            // OpenFileDialog ofd = new OpenFileDialog();
            // ofd.Filter = "XML Files (*.xml)|*.xml";
            // ofd.FilterIndex = 0;
            // ofd.DefaultExt = "xml";
            // if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            // {
            //     if (!String.Equals(System.IO.Path.GetExtension(ofd.FileName),
            //                        ".xml",
            //                        StringComparison.OrdinalIgnoreCase))
            //     {
            //         // Invalid file type selected; display an error.
            //         System.Windows.Forms.MessageBox.Show("The type of the selected file is not supported by this application. You must select an XML file.",
            //                         "Invalid File Type",
            //                         MessageBoxButtons.OK,
            //                         MessageBoxIcon.Error);
            //
            //         // Optionally, force the user to select another file.
            //         // ...
            //     }
            //     else
            //     {
            //         LoadSettings(ofd.FileName);
            // The selected file is good; do something with it.
            // ...
            //     }
            // }
        }
        void CloseApp()
        {
            System.Windows.Application curApp = System.Windows.Application.Current;
            curApp.Shutdown();
        }
        private void MenuItem_Click_1(object sender, RoutedEventArgs e) // Close App
        {
            CloseApp();
        }

        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
             OpenXMLs();   
        }
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog
            {
                InitialDirectory = Directory.GetCurrentDirectory() + "\\XMLs",
                OverwritePrompt = true,
                Filter = "xml-файл|*.xml"
            };

            string activeTabHeader = ((TabItem)tabControl.SelectedItem).Header.ToString();
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
            }
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
        private void btnSaveAll_Click(object sender, RoutedEventArgs e)
        {
            if (System.Windows.MessageBox.Show("Внимание! Все таблицы будут сохранены в файлы по умолчанию.", "Предупреждение", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.OK)
            {
                SaveSettings();
                SaveSettAI();
                SaveSettDI();
                SaveSettDO();
                SaveSettZD();
                SaveSettKL();
                SaveSettVS();
                SaveSettMPNA();
            }
        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            CloseApp();
        }
        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (true) // !!! false == Disable Modbus for Debug !!!
            {
                // mbMaster = ModbusIpMaster.CreateIp(new TcpClient("192.168.201.1", 502));  
                mbMaster = ModbusIpMaster.CreateIp(new TcpClient(settings.HostName, settings.MBPort));
                mbMasterR0 = ModbusIpMaster.CreateIp(new TcpClient(settings.HostName, settings.MBPort));
                mbMasterR1 = ModbusIpMaster.CreateIp(new TcpClient(settings.HostName, settings.MBPort));
                mbMasterR2 = ModbusIpMaster.CreateIp(new TcpClient(settings.HostName, settings.MBPort));
                mbMasterR3 = ModbusIpMaster.CreateIp(new TcpClient(settings.HostName, settings.MBPort));
                mbMasterW0 = ModbusIpMaster.CreateIp(new TcpClient(settings.HostName, settings.MBPort));
                mbMasterW1 = ModbusIpMaster.CreateIp(new TcpClient(settings.HostName, settings.MBPort));
                mbMasterW2 = ModbusIpMaster.CreateIp(new TcpClient(settings.HostName, settings.MBPort));
                mbMasterW3 = ModbusIpMaster.CreateIp(new TcpClient(settings.HostName, settings.MBPort));

                masterLoop = Task.Factory.StartNew(new Action(Update));
                masterLoopR0 = Task.Factory.StartNew(new Action(UpdateR0));
                masterLoopR1 = Task.Factory.StartNew(new Action(UpdateR1));
                masterLoopR2 = Task.Factory.StartNew(new Action(UpdateR2));
                masterLoopR3 = Task.Factory.StartNew(new Action(UpdateR3));
                masterLoopW0 = Task.Factory.StartNew(new Action(UpdateW0));
                masterLoopW1 = Task.Factory.StartNew(new Action(UpdateW1));
                masterLoopW2 = Task.Factory.StartNew(new Action(UpdateW2));
                masterLoopW3 = Task.Factory.StartNew(new Action(UpdateW3));
            }
            else
            {
                //mbMaster = ModbusIpMaster.CreateIp(new TcpClient(Sett.HostName, Sett.MBPort));
                masterLoop = Task.Factory.StartNew(new Action(Update));
            }
        }
        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
        }
        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
        }

        private void MenuItem_Click_save(object sender, RoutedEventArgs e)
        {

        }

        private void ZDTable_keydown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                if (System.Windows.MessageBox.Show("Удалить выбранные строки?","Подтверждение",System.Windows.MessageBoxButton.YesNo,MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    ZDStruct[] selectedZDs = new ZDStruct[dataGridZD.SelectedItems.Count];
                    dataGridZD.SelectedItems.CopyTo(selectedZDs, 0);

                    foreach (ZDStruct zd in selectedZDs)
                        zdmodel.ZDs.Remove(zd);
                }
            }
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
    }
}
