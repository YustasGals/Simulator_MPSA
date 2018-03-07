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

namespace Simulator_MPSA
{
    public  class Sett
    {
        public  string HostName = "192.168.201.1"; // IP adress CPU
        public  int MBPort = 502; // Modbus TCP port adress
        public  int TPause = 50; // задержка между циклами чтения
        public  int nWrTask = 4; // потоков на запись в CPU
        public  int iBegAddrR = 23170 - 1; // начальный адрес для чтения выходов CPU
        public  int iBegAddrW = 15100 - 1; // начальный адрес для записи входов CPU
        public  int iNRackBeg = 3; // номер начальной корзины
        public  int iNRackEnd = 29; // номер конечной корзины
        public  int nAI = 1024; // count of AI 1000
        public  int nDI = 128; // count of DI 
        public  int nDO = 64; // count of DO 
        public  int nZD = 64; // count of ZD  200
        public  int nKL = 64; // count of KL 100
        public  int nVS = 256; // count of VS 200
        public  int nMPNA = 16; // count of MPNA 
                                    // public const string AI_file = "AIsettings.xml";
                                    // public static string DI_file = "";
                                    // public static string DO_file = "";
                                    // public static string ZD_file = "";
                                    // public static string MPNA_file = "";
                                    // public static string VS_file = "";
    }
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
                int tbStartAdress = settings.iBegAddrR; // (Convert.ToUInt16(textBoxStartAdress.Text))
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
                System.Threading.Thread.Sleep(settings.TPause);

            }
        }
        private void UpdateR1()
        {
            while (true)
            {
                //  int AreaR = (29 - 3 + 1) * 50; //    2000; // Convert.ToInt32(textBoxAreaR.Text); // 
                int NReg = 125; // Convert.ToInt32(textBoxNReg.Text);
                int tbStartAdress = settings.iBegAddrR; // (Convert.ToUInt16(textBoxStartAdress.Text))
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
                System.Threading.Thread.Sleep(settings.TPause);

            }
        }
        private void UpdateR2()
        {
            while (true)
            {
                //  int AreaR = (29 - 3 + 1) * 50; //    2000; // Convert.ToInt32(textBoxAreaR.Text); // 
                int NReg = 125; // Convert.ToInt32(textBoxNReg.Text);
                int tbStartAdress = settings.iBegAddrR; // (Convert.ToUInt16(textBoxStartAdress.Text))
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
                System.Threading.Thread.Sleep(settings.TPause);

            }
        }
        private void UpdateR3()
        {
            while (true)
            {
                //  int AreaR = (29 - 3 + 1) * 50; //    2000; // Convert.ToInt32(textBoxAreaR.Text); // 
                int NReg = 125; // Convert.ToInt32(textBoxNReg.Text);
                int tbStartAdress = settings.iBegAddrR; // (Convert.ToUInt16(textBoxStartAdress.Text))
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
                System.Threading.Thread.Sleep(settings.TPause);

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
                int tbStartAdress = settings.iBegAddrW + n * NReg * settings.nWrTask; // (Convert.ToUInt16(textBoxStartAdress.Text))
                ushort[] data = new ushort[NReg];
                int c = AreaW / NReg / settings.nWrTask;
                for (int i = settings.nWrTask * n; i < (settings.nWrTask * n + c); i++)
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
                int tbStartAdress = settings.iBegAddrW + n * NReg * settings.nWrTask; // (Convert.ToUInt16(textBoxStartAdress.Text))
                ushort[] data = new ushort[NReg];
                int c = AreaW / NReg / settings.nWrTask;
                for (int i = settings.nWrTask * n; i < (settings.nWrTask * n + c); i++)
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
                int tbStartAdress = settings.iBegAddrW + n * NReg * settings.nWrTask; // (Convert.ToUInt16(textBoxStartAdress.Text))
                ushort[] data = new ushort[NReg];
                int c = AreaW / NReg / settings.nWrTask;
                for (int i = settings.nWrTask * n; i < (settings.nWrTask * n + c); i++)
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
                int tbStartAdress = settings.iBegAddrW + n * NReg * settings.nWrTask; // (Convert.ToUInt16(textBoxStartAdress.Text))
                ushort[] data = new ushort[NReg];
                int c = AreaW / NReg / settings.nWrTask;
                for (int i = settings.nWrTask * n; i < (settings.nWrTask * n + c); i++)
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
            for (int i = 0; i < AIs.Length; i++)
            {
                if (AIs[i].En /* || true */)
                {
                    WB.W[(AIs[i].indxW)] = AIs[i].ValACD; // записываем значение АЦП в массив для записи CPU
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
        void GetDOfromR() // копирование значения сигналов DO из массива для чтения ЦПУ
        {
            for (int i = 0; i < DOs.Length; i++)
            {
                DOs[i].ValDO = GetBit(RB.R[DOs[i].indxR], DOs[i].indxBitDO);
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
        void SaveSettings(string Sxml= "settings.xml")
        {
            XmlSerializer xml = new XmlSerializer(typeof(Sett));
            System.IO.StreamWriter writeStream = new System.IO.StreamWriter("XMLs//" + Sxml);
            xml.Serialize(writeStream, settings);
            writeStream.Dispose();
            MessageBox.Show("Файл " + Sxml + " сохранен ");
        }
        void LoadSettings(string Sxml = "settings.xml")
        {
            XmlSerializer xml = new XmlSerializer(typeof(Sett));
            System.IO.StreamReader reader = null;
            try
            {
                reader = new System.IO.StreamReader("XMLs//" + Sxml);
                settings = (Sett)xml.Deserialize(reader);
                reader.Dispose();
                MessageBox.Show("Файл " + Sxml + " считан ");
            }
            catch
            {
                reader.Dispose();
                System.IO.StreamWriter writer = new System.IO.StreamWriter("XMLs//" + Sxml);
                xml.Serialize(writer, settings);
                writer.Dispose();
                MessageBox.Show("Файл " + Sxml + " не считан !!! ");
            }
            RB.R = new ushort[(settings.iNRackEnd) * 50];//[(29 - 3 + 1) * 50]    =1450   From IOScaner CPU
            WB.W = new ushort[(settings.iNRackEnd - settings.iNRackBeg + 1) * 126]; // =3402 From IOScaner CPU
            AIs = new AIStruct[settings.nAI];
            ZDs = new ZDStruct[settings.nZD];
            DOs =new DOStruct[settings.nDO * 32];
            ZDs = new ZDStruct[settings.nZD];
            KLs = new KLStruct[settings.nKL];
            VSs = new VSStruct[settings.nVS];
            MPNAs = new MPNAStruct[settings.nMPNA];
            DIs = new DIStruct[settings.nDI * 32];
            //TODO: вставить код активации кнопки
        }
        #endregion
        // -----------------------------------------------------------------
<<<<<<< HEAD
        public static AIStruct[] AIs = new AIStruct[Sett.nAI];
=======
        public AIStruct[] AIs;// = new AIStruct[settings.nAI];
>>>>>>> 6be81fe050c7780aa1f670b432bd4bc9ff1dafdc
        #region AIsettings.xml
        public void LoadSettAI(string Sxml = "AIsettings.xml")
        {
            XmlSerializer xml = new XmlSerializer(typeof(AIStruct[]));
            System.IO.StreamReader reader = null;
            try
            {
                reader = new System.IO.StreamReader("XMLs//" + Sxml);
                 AIs = (AIStruct[])xml.Deserialize(reader);
                reader.Dispose();

                System.Windows.Forms.MessageBox.Show("AIsettings.xml loaded.");
            }
            catch
            {
                System.IO.StreamWriter writer = new System.IO.StreamWriter("XMLs//" + Sxml);
                xml.Serialize(writer, AIs);
                writer.Dispose();
            }
        }
        // ---------------------------------------------------------------------
        void SaveSettAI(string Sxml = "AIsettings.xml")
        {
            XmlSerializer xml = new XmlSerializer(typeof(AIStruct[]));
            System.IO.StreamWriter writeStream = new System.IO.StreamWriter("XMLs//" + Sxml);
            xml.Serialize(writeStream, AIs);
            writeStream.Dispose();
            System.Windows.Forms.MessageBox.Show("AIsettings.xml saved.");
        }
        #endregion
        // ---------------------------------------------------------------------
<<<<<<< HEAD
        public static DIStruct[] DIs = new DIStruct[Sett.nDI * 32];
=======
        public DIStruct[] DIs;// = new DIStruct[Sett.nDI * 32];
>>>>>>> 6be81fe050c7780aa1f670b432bd4bc9ff1dafdc
        #region DIsettings.xml
        public void LoadSettDI(string Sxml = "DIsettings.xml")
        {
            XmlSerializer xml = new XmlSerializer(typeof(DIStruct[]));
            System.IO.StreamReader reader = null;
            try
            {
                reader = new System.IO.StreamReader("XMLs//" + Sxml);
                DIs = (DIStruct[])xml.Deserialize(reader);
                reader.Dispose();
                System.Windows.Forms.MessageBox.Show("DIsettings.xml loaded.");
            }
            catch
            {
                System.IO.StreamWriter writer = new System.IO.StreamWriter("XMLs//" + Sxml);
                xml.Serialize(writer, DIs);
                writer.Dispose();
            }
        }
        // ---------------------------------------------------------------------
        void SaveSettDI(string Sxml = "DIsettings.xml")
        {
            XmlSerializer xml = new XmlSerializer(typeof(DIStruct[]));
            System.IO.StreamWriter writeStream = new System.IO.StreamWriter("XMLs//" + Sxml);
            xml.Serialize(writeStream, DIs);
            writeStream.Dispose();
            System.Windows.Forms.MessageBox.Show("DIsettings.xml saved.");
        }
        #endregion
        // ---------------------------------------------------------------------
<<<<<<< HEAD
        public static DOStruct[] DOs = new DOStruct[Sett.nDO * 32];
=======
        public DOStruct[] DOs;// new DOStruct[Sett.nDO * 32];
>>>>>>> 6be81fe050c7780aa1f670b432bd4bc9ff1dafdc
        #region DOsettings.xml
        public void LoadSettDO(string Sxml = "DOsettings.xml")
        {
            XmlSerializer xml = new XmlSerializer(typeof(DOStruct[]));
            System.IO.StreamReader reader = null;
            try
            {
                reader = new System.IO.StreamReader("XMLs//" + Sxml);
                DOs = (DOStruct[])xml.Deserialize(reader);
                reader.Dispose();
                System.Windows.Forms.MessageBox.Show("DOsettings.xml loaded.");
            }
            catch
            {
                System.IO.StreamWriter writer = new System.IO.StreamWriter("XMLs//" + Sxml);
                xml.Serialize(writer, DOs);
                writer.Dispose();
            }
        }
        // ---------------------------------------------------------------------
        void SaveSettDO(string Sxml = "DOsettings.xml")
        {
            XmlSerializer xml = new XmlSerializer(typeof(DOStruct[]));
            System.IO.StreamWriter writeStream = new System.IO.StreamWriter("XMLs//" + Sxml);
            xml.Serialize(writeStream, DOs);
            writeStream.Dispose();
            System.Windows.Forms.MessageBox.Show("DOsettings.xml saved.");
        }
        #endregion
        // ---------------------------------------------------------------------
        // ---------------------------------------------------------------------
        public ZDStruct[] ZDs;// = new ZDStruct[Sett.nZD];
        #region ZDsettings.xml
        public void LoadSettZD(string Sxml = "ZDsettings.xml")
        {
            XmlSerializer xml = new XmlSerializer(typeof(ZDStruct[]));
            System.IO.StreamReader reader = null;
            try
            {
                reader = new System.IO.StreamReader("XMLs//" + Sxml);
                ZDs = (ZDStruct[])xml.Deserialize(reader);
                reader.Dispose();
                System.Windows.Forms.MessageBox.Show("ZDsettings.xml loaded.");
            }
            catch
            {
                System.IO.StreamWriter writer = new System.IO.StreamWriter("XMLs//" + Sxml);
                xml.Serialize(writer, ZDs);
                writer.Dispose();
            }
        }
        // ---------------------------------------------------------------------
        void SaveSettZD(string Sxml = "ZDsettings.xml")
        {
            XmlSerializer xml = new XmlSerializer(typeof(ZDStruct[]));
            System.IO.StreamWriter writeStream = new System.IO.StreamWriter("XMLs//" + Sxml);
            xml.Serialize(writeStream, ZDs);
            writeStream.Dispose();
            System.Windows.Forms.MessageBox.Show("ZDsettings.xml saved.");
        }
        #endregion
        // ---------------------------------------------------------------------
        // ---------------------------------------------------------------------
        public KLStruct[] KLs;// = new KLStruct[Sett.nKL];
        #region KLsettings.xml
        public void LoadSettKL(string Sxml = "KLsettings.xml")
        {
            XmlSerializer xml = new XmlSerializer(typeof(KLStruct[]));
            System.IO.StreamReader reader = null;
            try
            {
                reader = new System.IO.StreamReader("XMLs//" + Sxml);
                KLs = (KLStruct[])xml.Deserialize(reader);
                reader.Dispose();
                System.Windows.Forms.MessageBox.Show("KLsettings.xml loaded.");
            }
            catch
            {
                System.IO.StreamWriter writer = new System.IO.StreamWriter("XMLs//" + Sxml);
                xml.Serialize(writer, KLs);
                writer.Dispose();
            }
        }
        // ---------------------------------------------------------------------
        void SaveSettKL(string Sxml = "KLsettings.xml")
        {
            XmlSerializer xml = new XmlSerializer(typeof(KLStruct[]));
            System.IO.StreamWriter writeStream = new System.IO.StreamWriter("XMLs//" + Sxml);
            xml.Serialize(writeStream, KLs);
            writeStream.Dispose();
            System.Windows.Forms.MessageBox.Show("KLsettings.xml saved.");
        }
        #endregion
        // ---------------------------------------------------------------------
        // ---------------------------------------------------------------------
        public VSStruct[] VSs;// = new VSStruct[Sett.nVS];
        #region VSsettings.xml
        public void LoadSettVS(string Sxml = "VSsettings.xml")
        {
            XmlSerializer xml = new XmlSerializer(typeof(VSStruct[]));
            System.IO.StreamReader reader = null;
            try
            {
                reader = new System.IO.StreamReader("XMLs//" + Sxml);
                VSs = (VSStruct[])xml.Deserialize(reader);
                reader.Dispose();
                System.Windows.Forms.MessageBox.Show("VSsettings.xml loaded.");
            }
            catch
            {
                System.IO.StreamWriter writer = new System.IO.StreamWriter("XMLs//" + Sxml);
                xml.Serialize(writer, VSs);
                writer.Dispose();
            }
        }
        // ---------------------------------------------------------------------
        void SaveSettVS(string Sxml = "VSsettings.xml")
        {
            XmlSerializer xml = new XmlSerializer(typeof(VSStruct[]));
            System.IO.StreamWriter writeStream = new System.IO.StreamWriter("XMLs//" + Sxml);
            xml.Serialize(writeStream, VSs);
            writeStream.Dispose();
            System.Windows.Forms.MessageBox.Show("VSsettings.xml saved.");
        }
        #endregion
        // ---------------------------------------------------------------------
        // ---------------------------------------------------------------------
        public MPNAStruct[] MPNAs;// = new MPNAStruct[Sett.nMPNA];
        #region MPNAsettings.xml
        public void LoadSettMPNA(string Sxml = "MPNAsettings.xml")
        {
            XmlSerializer xml = new XmlSerializer(typeof(MPNAStruct[]));
            System.IO.StreamReader reader = null;
            try
            {
                reader = new System.IO.StreamReader("XMLs//" + Sxml);
                MPNAs = (MPNAStruct[])xml.Deserialize(reader);
                reader.Dispose();
                System.Windows.Forms.MessageBox.Show("MPNAsettings.xml loaded.");
            }
            catch
            {
                System.IO.StreamWriter writer = new System.IO.StreamWriter("XMLs//" + Sxml);
                xml.Serialize(writer, MPNAs);
                writer.Dispose();
            }
        }
        // ---------------------------------------------------------------------
        void SaveSettMPNA(string Sxml = "MPNAsettings.xml")
        {
            XmlSerializer xml = new XmlSerializer(typeof(MPNAStruct[]));
            System.IO.StreamWriter writeStream = new System.IO.StreamWriter("XMLs//" + Sxml);
            xml.Serialize(writeStream, MPNAs);
            writeStream.Dispose();
            System.Windows.Forms.MessageBox.Show("MPNAsettings.xml saved.");
        }
        #endregion
        // ---------------------------------------------------------------------
        void load_dataGridDO()
        {
            dataGridDO.DataContext = new DOTableViewModel(DOs);
        }
        // ---------------------------------------------------------------------
        void load_dataGridDI()
        {
            /*   DataTable dt = new DataTable();
               dt.Columns.Add("En", typeof(bool));
               dt.Columns.Add("ValDI", typeof(bool));
               dt.Columns.Add("indxArrDI", typeof(int));
               dt.Columns.Add("indxBitDI", typeof(int));
               dt.Columns.Add("indxW", typeof(int));
               dt.Columns.Add("TegDI", typeof(string));
               dt.Columns.Add("NameDI", typeof(string));
               dt.Columns.Add("Nsign", typeof(int));
               dt.Columns.Add("InvertDI", typeof(bool));
               dt.Columns.Add("DelayDI", typeof(int));
               foreach (DIStruct DI_item in DIs)
               {
                   DataRow row;
                   row = dt.NewRow();
                   row["En"] = DI_item.En;
                   row["ValDI"] = DI_item.ValDI;
                   row["indxArrDI"] = DI_item.indxArrDI;
                   row["indxBitDI"] = DI_item.indxBitDI;
                   row["indxW"] = DI_item.indxW;
                   row["TegDI"] = DI_item.TegDI;
                   row["NameDI"] = DI_item.NameDI;
                   row["Nsign"] = DI_item.Nsign;
                   row["InvertDI"] = DI_item.InvertDI;
                   row["DelayDI"] = DI_item.DelayDI;
                   dt.Rows.Add(row);
               }
               dataGridDI.ItemsSource = dt.DefaultView;*/
            dataGridDI.DataContext = new DITableViewModel(DIs);
        }
        // ---------------------------------------------------------------------
        void load_dataGridAI()
        {
            /* DataTable dt = new DataTable();
             dt.Columns.Add("En", typeof(bool));
             dt.Columns.Add("indxAI", typeof(int));
             dt.Columns.Add("indxW", typeof(int));
             dt.Columns.Add("TegAI", typeof(string));
             dt.Columns.Add("NameAI", typeof(string));
             dt.Columns.Add("ValACD", typeof(ushort));
             dt.Columns.Add("minACD", typeof(ushort));
             dt.Columns.Add("maxACD", typeof(ushort));
             dt.Columns.Add("minPhis", typeof(float));
             dt.Columns.Add("maxPhis", typeof(float));
             dt.Columns.Add("fValAI", typeof(float));
             dt.Columns.Add("DelayAI", typeof(int));
             foreach (AIStruct AI_item in AIs)
             {
                 DataRow row;
                 row = dt.NewRow();
                 row["En"] = AI_item.En;
                 row["indxAI"] = AI_item.indxAI;
                 row["indxW"] = AI_item.indxW;
                 row["TegAI"] = AI_item.TegAI;
                 row["NameAI"] = AI_item.NameAI;
                 row["ValACD"] = AI_item.ValACD;
                 row["minACD"] = AI_item.minACD;
                 row["maxACD"] = AI_item.maxACD;
                 row["minPhis"] = AI_item.minPhis;
                 row["maxPhis"] = AI_item.maxPhis;
                 row["fValAI"] = AI_item.fValAI;
                 row["DelayAI"] = AI_item.DelayAI;
                 dt.Rows.Add(row);
             }

             dataGridAI.ItemsSource = dt.DefaultView;*/
            dataGridAI.DataContext = new AITableViewModel(AIs);
        }
        // ---------------------------------------------------------------------
        void OpenXMLs()
        {
            LoadSettings();
            LoadSettAI();
            load_dataGridAI();
            LoadSettDI();
            load_dataGridDI();
            LoadSettDO();
            load_dataGridDO();
        }
        // ---------------------------------------------------------------------
        void SaveXMLs()
        {
            SaveSettings();
            SaveSettAI();
            SaveSettDI();
            SaveSettDO();
        }

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
        private void MenuItem_Click_2(object sender, RoutedEventArgs e) // Save Settings xml
        {
            SaveXMLs();
        }
        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
             OpenXMLs();   
        }
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            //временный код! сохранения DO таблицы
            XmlSerializer xml = new XmlSerializer(typeof(DOStruct[]));
            System.IO.StreamReader reader = null;

            System.IO.StreamWriter writer = new System.IO.StreamWriter("NEW_DOsettings.xml");
            xml.Serialize(writer, DOs);
            writer.Dispose();

           /* SaveXMLs();
            SaveSettZD();
            SaveSettKL();
            SaveSettVS();
            SaveSettMPNA();*/

        }
        private void btnSaveAll_Click(object sender, RoutedEventArgs e)
        {
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

    }
}
