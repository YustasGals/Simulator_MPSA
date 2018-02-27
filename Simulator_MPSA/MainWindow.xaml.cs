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
using System.Windows.Forms;
using System.IO;
using System.Xml.Serialization;


namespace Simulator_MPSA
{
    public static class Sett
    {
        public static string HostName = "192.168.201.1"; // IP adress CPU
        public static int MBPort = 502; // Modbus TCP port adress
        public static int TPause = 50; // задержка между циклами чтения
        public static int nWrTask = 4; // потоков на запись в CPU
        public static int iBegAddrR = 23170 - 1; // начальный адрес для чтения выходов CPU
        public static int iBegAddrW = 15100 - 1; // начальный адрес для записи входов CPU
        public static int iNRackBeg = 3; // номер начальной корзины
        public static int iNRackEnd = 29; // номер конечной корзины
        public static int nAI = 1000; // count of AI 
        public static int nDI = 70; // count of DI 
        public static int nDO = 100; // count of DO 
    }
    public  class clS
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

        //public CL_ANALOG[] AI = CL_ANALOG[1000]  ;
    }

    public static class RB
    {
        public static ushort[] R = new ushort[(Sett.iNRackEnd) * 50];//[(29 - 3 + 1) * 50]    =1450   From IOScaner CPU
    }
    public static class WB
    {
        public static ushort[] W = new ushort[(Sett.iNRackEnd - Sett.iNRackBeg + 1) * 126]; // =3402 From IOScaner CPU
    }


    public static class DeBag
    {
        public static ulong RR;
        public static ulong WW;
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //System.Data.DataTable gridData;

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

            //gridData = new System.Data.DataTable();
            

            Debug.WriteLine("------------------------------------------------------------------");
            string sAttr, sAllAtr;
            sAttr = ConfigurationManager.AppSettings.Get("Key0");
            Debug.WriteLine("Key0 = " + sAttr);
            ConfigurationManager.AppSettings.Set("Key0", "777");
            sAllAtr = ConfigurationManager.AppSettings.ToString();
            Debug.Write("AllAtr = " + sAllAtr + " \n");
            sAttr = ConfigurationManager.AppSettings.Get("Key0");
            Debug.WriteLine("Key0 = " + sAttr);
            Debug.WriteLine("------------------------------------------------------------------");
            NameValueCollection sAll; sAll = ConfigurationManager.AppSettings;
            foreach (string s in sAll.AllKeys) Debug.WriteLine("Key: " + s + " Value: " + sAll.Get(s));
            Debug.WriteLine("------------------------------------------------------------------");

            //dataGrid.ItemsSource = AIs;  
           // dataGrid.DataContext = AIs; 
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
            for (int i = 0; i < AIs.Length; i++)
            {
               // AIs[i] = new AIStruct();
                //clAI.AI[i].indxAI = i;
                //clAI.AI[i].fValAI = i;
               // string s = clAI.AI[i].PrintAI();
               // Debug.WriteLine(s);
            }

           // MainWindow.dataGrid.DataContext = MainWindow.AIs;

            while (true)   
            {
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
                int tbStartAdress = Sett.iBegAddrR; // (Convert.ToUInt16(textBoxStartAdress.Text))
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
                System.Threading.Thread.Sleep(Sett.TPause);

            }
        }
        private void UpdateR1()
        {
            while (true)
            {
                //  int AreaR = (29 - 3 + 1) * 50; //    2000; // Convert.ToInt32(textBoxAreaR.Text); // 
                int NReg = 125; // Convert.ToInt32(textBoxNReg.Text);
                int tbStartAdress = Sett.iBegAddrR; // (Convert.ToUInt16(textBoxStartAdress.Text))
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
                System.Threading.Thread.Sleep(Sett.TPause);

            }
        }
        private void UpdateR2()
        {
            while (true)
            {
                //  int AreaR = (29 - 3 + 1) * 50; //    2000; // Convert.ToInt32(textBoxAreaR.Text); // 
                int NReg = 125; // Convert.ToInt32(textBoxNReg.Text);
                int tbStartAdress = Sett.iBegAddrR; // (Convert.ToUInt16(textBoxStartAdress.Text))
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
                System.Threading.Thread.Sleep(Sett.TPause);

            }
        }
        private void UpdateR3()
        {
            while (true)
            {
                //  int AreaR = (29 - 3 + 1) * 50; //    2000; // Convert.ToInt32(textBoxAreaR.Text); // 
                int NReg = 125; // Convert.ToInt32(textBoxNReg.Text);
                int tbStartAdress = Sett.iBegAddrR; // (Convert.ToUInt16(textBoxStartAdress.Text))
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
                System.Threading.Thread.Sleep(Sett.TPause);

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
                int tbStartAdress = Sett.iBegAddrW + n * NReg * Sett.nWrTask; // (Convert.ToUInt16(textBoxStartAdress.Text))
                ushort[] data = new ushort[NReg];
                int c = AreaW / NReg / Sett.nWrTask;
                for (int i = Sett.nWrTask * n; i < (Sett.nWrTask * n + c); i++)
                {
                    Array.Copy(WB.W, (NReg * i), data, (0), NReg);
                    mbMasterW0.WriteMultipleRegisters(1, (ushort)(tbStartAdress + NReg * i), data);
                    //TSW.Wr(mbMasterW.WriteMultipleRegisters(1, (ushort)(tbStartAdress + 125 * 4), data )); 
                    //System.Threading.Thread.Sleep(Sett.TPause);
                }
                DeBag.WW++;
                Debug.WriteLine("W0()   WW= " + DeBag.WW + " /n");
                System.Threading.Thread.Sleep(Sett.TPause);
            }
        }
        private void UpdateW1()
        {
            while (true)
            {
                int n = 1; // W0
                int AreaW = WB.W.Length; //  (29 - 3 + 1) * 126]; // 3402 
                int NReg = 120; // Convert.ToInt32(textBoxNReg.Text);
                int tbStartAdress = Sett.iBegAddrW + n * NReg * Sett.nWrTask; // (Convert.ToUInt16(textBoxStartAdress.Text))
                ushort[] data = new ushort[NReg];
                int c = AreaW / NReg / Sett.nWrTask;
                for (int i = Sett.nWrTask * n; i < (Sett.nWrTask * n + c); i++)
                {
                    Array.Copy(WB.W, (NReg * i), data, (0), NReg);
                    mbMasterW1.WriteMultipleRegisters(1, (ushort)(tbStartAdress + NReg * i), data);
                    //TSW.Wr(mbMasterW.WriteMultipleRegisters(1, (ushort)(tbStartAdress + 125 * 4), data )); 
                    //System.Threading.Thread.Sleep(Sett.TPause);
                }
                //DeBag.WW++;
                Debug.WriteLine("W1()");
                System.Threading.Thread.Sleep(Sett.TPause);
            }
        }
        private void UpdateW2()
        {
            while (true)
            {
                int n = 2; // W0
                int AreaW = WB.W.Length; //  (29 - 3 + 1) * 126]; // 3402 
                int NReg = 120; // Convert.ToInt32(textBoxNReg.Text);
                int tbStartAdress = Sett.iBegAddrW + n * NReg * Sett.nWrTask; // (Convert.ToUInt16(textBoxStartAdress.Text))
                ushort[] data = new ushort[NReg];
                int c = AreaW / NReg / Sett.nWrTask;
                for (int i = Sett.nWrTask * n; i < (Sett.nWrTask * n + c); i++)
                {
                    Array.Copy(WB.W, (NReg * i), data, (0), NReg);
                    mbMasterW2.WriteMultipleRegisters(1, (ushort)(tbStartAdress + NReg * i), data);
                    //TSW.Wr(mbMasterW.WriteMultipleRegisters(1, (ushort)(tbStartAdress + 125 * 4), data )); 
                    //System.Threading.Thread.Sleep(Sett.TPause);
                }
                //DeBag.WW++;
                Debug.WriteLine("W2()   ");
                System.Threading.Thread.Sleep(Sett.TPause);
            }
        }
        private void UpdateW3()
        {
            while (true)
            {
                int n = 3; // W0
                int AreaW = WB.W.Length; //  (29 - 3 + 1) * 126]; // 3402 
                int NReg = 120; // Convert.ToInt32(textBoxNReg.Text);
                int tbStartAdress = Sett.iBegAddrW + n * NReg * Sett.nWrTask; // (Convert.ToUInt16(textBoxStartAdress.Text))
                ushort[] data = new ushort[NReg];
                int c = AreaW / NReg / Sett.nWrTask;
                for (int i = Sett.nWrTask * n; i < (Sett.nWrTask * n + c); i++)
                {
                    Array.Copy(WB.W, (NReg * i), data, (0), NReg);
                    mbMasterW3.WriteMultipleRegisters(1, (ushort)(tbStartAdress + NReg * i), data);
                    //TSW.Wr(mbMasterW.WriteMultipleRegisters(1, (ushort)(tbStartAdress + 125 * 4), data )); 
                    //System.Threading.Thread.Sleep(Sett.TPause);
                }
                //DeBag.WW++;
                Debug.WriteLine("W3()   ");
                System.Threading.Thread.Sleep(Sett.TPause);
            }
        }
        #endregion
        private void Grid_Loaded(object sender, RoutedEventArgs e) // выполняется при загрузке основной формы программы, то есть при Старте ПО
        {
            for (int i = 0; i < WB.W.Length; i++)   // for Debug !!!
            {
                WB.W[i] = (ushort)i; // заполняем массив для записи в ЦПУ значениями равными номеру элемента массива
            }

            if (false) // !!! false == Disable Modbus for Debug !!!
            {
                // mbMaster = ModbusIpMaster.CreateIp(new TcpClient("192.168.201.1", 502));  
                mbMaster = ModbusIpMaster.CreateIp(new TcpClient(Sett.HostName, Sett.MBPort));
                mbMasterR0 = ModbusIpMaster.CreateIp(new TcpClient(Sett.HostName, Sett.MBPort));
                mbMasterR1 = ModbusIpMaster.CreateIp(new TcpClient(Sett.HostName, Sett.MBPort));
                mbMasterR2 = ModbusIpMaster.CreateIp(new TcpClient(Sett.HostName, Sett.MBPort));
                mbMasterR3 = ModbusIpMaster.CreateIp(new TcpClient(Sett.HostName, Sett.MBPort));
                mbMasterW0 = ModbusIpMaster.CreateIp(new TcpClient(Sett.HostName, Sett.MBPort));
                mbMasterW1 = ModbusIpMaster.CreateIp(new TcpClient(Sett.HostName, Sett.MBPort));
                mbMasterW2 = ModbusIpMaster.CreateIp(new TcpClient(Sett.HostName, Sett.MBPort));
                mbMasterW3 = ModbusIpMaster.CreateIp(new TcpClient(Sett.HostName, Sett.MBPort));

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
            }
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
            for (int i = 0; i < AIs.Length; i++)
            {
                if (AIs[i].En /* || true */)
                {
//  !!!                   AIs[i].updateAI(AIs[i].fValAI + 0.01F); // имитируем изменение значения аналогового сигнвала. for Debug !!!
                    WB.W[(AIs[i].indxW)] = AIs[i].ValACD; // записываем значение АЦП в массив для записи CPU
    //                string s = AIs[i].PrintAI();
      //              s += ("ACD = " + WB.W[(AIs[i].indxW)] + " ; fValAI = " + AIs[i].fValAI);
        //            Debug.WriteLine(s);*/
                }
            }
        }
        void SendDItoW() // копирование значения сигналов DI в массив для записи в ЦПУ
        {
            for (int i = 0; i < DIs.Length; i++)
            {
                SetBit(ref (WB.W[(DIs[i].indxW)]), (DIs[i].indxBitDI), (DIs[i].ValDI));
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
        #endregion
        // -----------------------------------------------------------------
        clS S = new clS(); // settings;
        void SaveSettings(string Sxml= "settings.xml")
        {
            XmlSerializer xml = new XmlSerializer(typeof(clS));
            System.IO.StreamWriter writeStream = new System.IO.StreamWriter(Sxml);
            xml.Serialize(writeStream, S);
            writeStream.Dispose();
        }
        void LoadSettings(string Sxml = "settings.xml")
        {
            XmlSerializer xml = new XmlSerializer(typeof(clS));
            System.IO.StreamReader reader = null;
            try
            {
                reader = new System.IO.StreamReader(Sxml);
                S = (clS)xml.Deserialize(reader);
                reader.Dispose();
            }
            catch
            {
                System.IO.StreamWriter writer = new System.IO.StreamWriter(Sxml);
                xml.Serialize(writer, S);
                writer.Dispose();
            }
        }
        // -----------------------------------------------------------------
        public AIStruct[] AIs = new AIStruct[Sett.nAI];
        //void LoadSettAI(string Sxml = "AI_settings.xml")
        public void LoadSettAI(string Sxml = "AIsettings.xml")
        {
            //XmlSerializer xml = new XmlSerializer(typeof(classAI));  /* */
            XmlSerializer xml = new XmlSerializer(typeof(AIStruct[]));
            System.IO.StreamReader reader = null;
            try
            {
                reader = new System.IO.StreamReader(Sxml);
                // clAI = (classAI)xml.Deserialize(reader);  /* */
                 AIs = (AIStruct[])xml.Deserialize(reader);
                reader.Dispose();
                //for (int i = 0; i < Sett.nAI; i++)
                //{
                //    AIs[i].En = clAI.AI[i].En;
                //    AIs[i].indxAI = clAI.AI[i].indxAI;
                //    AIs[i].indxW = clAI.AI[i].indxW;
                //    AIs[i].TegAI = clAI.AI[i].TegAI;
                //    AIs[i].NameAI = clAI.AI[i].NameAI;
                //    AIs[i].NameAI = clAI.AI[i].NameAI;
                //    AIs[i].ValACD = clAI.AI[i].ValACD;
                //    AIs[i].minACD = clAI.AI[i].minACD;
                //    AIs[i].maxACD = clAI.AI[i].maxACD;
                //    AIs[i].minPhis = clAI.AI[i].minPhis;
                //    AIs[i].maxPhis = clAI.AI[i].maxPhis;
                //    AIs[i].fValAI = clAI.AI[i].fValAI;
                //    AIs[i].DelayAI = clAI.AI[i].DelayAI;
                //}
                // dataGrid.Columns.Add("title 1");
                System.Data.DataTable gridData = new System.Data.DataTable();
                
                gridData.Clear();
                for(int i=0;i<12;i++)
                {
                    gridData.Columns.Add("title" + i.ToString());
                }

                gridData.Rows.Add(AIs[0].GetType());
                gridData.Rows.Add(AIs[1].ToString());
                dataGrid.ItemsSource = gridData.DefaultView;

                System.Windows.Forms.MessageBox.Show("AIsettings.xml loaded.");
            }
            catch
            {
                System.IO.StreamWriter writer = new System.IO.StreamWriter(Sxml);
               // xml.Serialize(writer, clAI);  /* */
                xml.Serialize(writer, AIs);
                writer.Dispose();
            }
        }
        // ---------------------------------------------------------------------
        void SaveSettAI(string Sxml = "AIsettings.xml")
        {
            // clAI.InitArrAI();
           // XmlSerializer xml = new XmlSerializer(typeof(classAI));
            XmlSerializer xml = new XmlSerializer(typeof(AIStruct[]));
            System.IO.StreamWriter writeStream = new System.IO.StreamWriter(Sxml);
           // xml.Serialize(writeStream, clAI);
            xml.Serialize(writeStream, AIs);
            writeStream.Dispose();
            System.Windows.Forms.MessageBox.Show("AIsettings.xml saved.");
        }
        // ---------------------------------------------------------------------
        public DIStruct[] DIs = new DIStruct[Sett.nDI * 32];
        public void LoadSettDI(string Sxml = "DIsettings.xml")
        {
            XmlSerializer xml = new XmlSerializer(typeof(DIStruct[]));
            System.IO.StreamReader reader = null;
            try
            {
                reader = new System.IO.StreamReader(Sxml);
                DIs = (DIStruct[])xml.Deserialize(reader);
                reader.Dispose();

                //System.Data.DataTable gridData = new System.Data.DataTable();
                //gridData.Clear();
                //for (int i = 0; i < 12; i++)
                //{
                //    gridData.Columns.Add("title" + i.ToString());
                //}

                //gridData.Rows.Add(AIs[0].GetType());
                //gridData.Rows.Add(AIs[1].ToString());
                //dataGrid.ItemsSource = gridData.DefaultView;

                System.Windows.Forms.MessageBox.Show("DIsettings.xml loaded.");
            }
            catch
            {
                System.IO.StreamWriter writer = new System.IO.StreamWriter(Sxml);
                xml.Serialize(writer, DIs);
                writer.Dispose();
            }
        }
        // ---------------------------------------------------------------------
        void SaveSettDI(string Sxml = "DIsettings.xml")
        {
            XmlSerializer xml = new XmlSerializer(typeof(DIStruct[]));
            System.IO.StreamWriter writeStream = new System.IO.StreamWriter(Sxml);
            xml.Serialize(writeStream, DIs);
            writeStream.Dispose();
            System.Windows.Forms.MessageBox.Show("DIsettings.xml saved.");
        }
        // ---------------------------------------------------------------------
        private void MenuItem_Click(object sender, RoutedEventArgs e) // open xml
        {
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
                    LoadSettings();
                    LoadSettAI();
                    LoadSettDI();

            // The selected file is good; do something with it.
            // ...
            //     }
            // }
        }
        private void MenuItem_Click_1(object sender, RoutedEventArgs e) // Close App
        {
            System.Windows.Application curApp = System.Windows.Application.Current;
            curApp.Shutdown();
        }
        private void MenuItem_Click_2(object sender, RoutedEventArgs e) // Save Settings xml
        {
            SaveSettings();
            SaveSettAI();
            SaveSettDI();
        }

        private void dataGrid_MouseUp(object sender, MouseButtonEventArgs e)
        {
           // dataGrid.SelectedItem = 2;
            
          //  CL_ANALOG path = dataGrid.SelectedItem as CL_ANALOG;
            System.Windows.Forms.MessageBox.Show("dataGrid");
                //".En = " + path.En +
                //" .indxAI = " + path.indxAI +
                //" .indxW =  " + path.indxW +
                //" .TegAI =  " + path.TegAI +
                //" .NameAI = " + path.NameAI +
                //" .NameAI = " + path.NameAI +
                //" .ValACD = " + path.ValACD +
                //" .minACD = " + path.minACD +
                //" .maxACD = " + path.maxACD +
                //" .minPhis = " + path.minPhis +
                //" .maxPhis = " + path.maxPhis +
                //" .fValAI = " + path.fValAI +
                //" .DelayAI = " + path.DelayAI);

        }
    }
}
