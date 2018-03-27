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
//using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics; // for DEBUG 
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
    //тип буфера
    public enum BufType
    {
        /// <summary>
        /// не определен
        /// </summary>
        Undefined,
        /// <summary>
        /// буффер УСО
        /// </summary>
        USO,      //буферы УСО
        /// <summary>
        /// буффер КК 1
        /// </summary>
        A3,  
        /// <summary>
        /// буффер КК 2
        /// </summary>
        A4     
    };


    public static class RB
    {
        public static ushort[] R;// = new ushort[(Sett.iNRackEnd) * 50];//[(29 - 3 + 1) * 50]    =1450   From IOScaner CPU
    }
    public static class WB
    {
        /// <summary>
        /// буфер записи физических сигналов
        /// </summary>
        public static ushort[] W;// = new ushort[(Sett.iNRackEnd - Sett.iNRackBeg + 1) * 126]; // =3402 From IOScaner CPU
        public static ushort[] WB_old;   // предыдущее состояние буфера записи

        public static ushort[] W_a3;       //буфер записи корзины А3 осн
        public static ushort[] W_a3_prev;  //сохраненное состояние буфера

        public static ushort[] W_a4;       //буфер записи корзины А3 осн
        public static ushort[] W_a4_prev;  //сохраненное состояние буфера

        public static void InitBuffers(Sett settings)
        {
            RB.R = new ushort[(Sett.Instance.NRackEnd) * 50];//[(29 - 3 + 1) * 50]    =1450   From IOScaner CPU

            int regCount = (Sett.Instance.NRackEnd - Sett.Instance.NRackBeg + 1) * 126;
            int coilCount = (int)Math.Ceiling((float)regCount / 120f);

            int regCountRounded = coilCount * 120;
            WB.W = new ushort[regCountRounded]; // =3402 From IOScaner CPU
            WB.WB_old = new ushort[regCountRounded];

            WB.W_a3 = new ushort[Sett.Instance.A3BufSize];
            WB.W_a3_prev = new ushort[WB.W_a3.Length];

            WB.W_a4 = new ushort[Sett.Instance.A3BufSize];
            WB.W_a4_prev = new ushort[WB.W_a4.Length];
        }
    }
    public static class DebugInfo
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

        public delegate void DEndCycle(float dt);
        public DEndCycle EndCycle;

        public delegate void DDisconnected();
        public DDisconnected delegateDisconnected;

        public delegate void DEndRead();
        public DEndRead delegateEndRead;

        public MainWindow()
        {
          
            InitializeComponent();



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

            EndCycle += new DEndCycle(On_WritingCycleEnd);
            delegateDisconnected += new DDisconnected(On_Disconnected);
            delegateEndRead += new DEndRead(On_ReadingCycleEnd);
        }


        Task masterLoop;



        /// <summary>
        /// время последней записи в ПЛК
        /// </summary>
        DateTime writingTime;

        /// <summary>
        /// время последнего чтения из ПЛК
        /// </summary>
        DateTime readingTime;
        private void Watchdog()
        {
            while (true)   
            {
                
               /* nowTime = DateTime.Now.Second + ((float)DateTime.Now.Millisecond) / 1000f;

                
                
                if ((nowTime - prevTime) < 0)
                    dt_sec = 60f - prevTime + nowTime;
                else
                    dt_sec = nowTime - prevTime;
                prevTime = nowTime;
                if (dt_sec > 60)
                    dt_sec = 0;

                foreach (ZDStruct zd in ZDTableViewModel.ZDs)
                    zd.UpdateZD(dt_sec);

                foreach (KLStruct kl in KLTableViewModel.KL)
                    kl.UpdateKL(dt_sec);

                foreach (MPNAStruct mpna in MPNATableViewModel.MPNAs)
                    mpna.UpdateMPNA(dt_sec);

                foreach (VSStruct vs in VSTableViewModel.VS)
                    vs.UpdateVS(dt_sec);
                    */

               // ct1.ThrowIfCancellationRequested();                
              //  GetDOfromR(); // записываем значение DO из массива для чтения CPU
                              //  SendAItoW(); // записываем значение АЦП в массив для записи CPU
                              //   SendDItoW(); // записываем значение DI в массив для записи CPU

                //записываем счетчики УСО
                /* foreach (USOCounter c in CountersTableViewModel.Counters)
                 {

                     if (c.PLCAddr >= Sett.Instance.BegAddrW + 1)
                     {
                         c.Update(dt_sec);
                         WB.W[c.PLCAddr - Sett.Instance.BegAddrW - 1] = c.Value;
                     }
                 }
                         // Debug.WriteLine("Update()"); // + NReg + " " + tbStartAdress);*/

                //-------------  проверка связи  -----------------------
                if (wrThread != null && !wrThread.Paused)
                {
                    if ((DateTime.Now - writingTime).TotalSeconds > 6f)
                    {
                        // wrThread.Stop();
                        // btnPause_Click(null, new RoutedEventArgs());
                        this.Dispatcher.Invoke(delegateDisconnected);
                       // btnStop_Click(null, new RoutedEventArgs());
                    }
                }

                System.Threading.Thread.Sleep(Sett.Instance.TPause * 2);
            }
        }
       
   
       /* private void UpdateW0()
        {
            //измерение времени цикла записи
            float prevTime=0;
            float nowTime;
            int counter = 0;
            float dt;   //время цикла в сек

            while (!cancelTokenSrc.IsCancellationRequested)
            {
                

                int nTask = 0; // Номер потока
                int AreaW = WB.W.Length; //  (29 - 3 + 1) * 126]; // 3402 
                int NReg = 120; // количество регистров на запись не более 120

                ushort[] data = new ushort[NReg];   //буфер для записи одной бочки
                int TaskCoilCount = AreaW / NReg / Sett.Instance.NWrTask;   //количество бочек по 120 регистров которые записываются в данном потоке

                int destStartAddr = Sett.Instance.BegAddrW + nTask * NReg * TaskCoilCount; //адрес в ПЛК 
                
                for (int Coil_i = 0; Coil_i < TaskCoilCount; Coil_i++)
                {
                    bool isChanged = false;
                    for (int i_reg = NReg * (Coil_i + nTask * TaskCoilCount); i_reg < NReg * (Coil_i + 1 + nTask * TaskCoilCount); i_reg++)
                        if (WB.W[i_reg] != WB.WB_old[i_reg])
                        {
                            isChanged = true;
                            WB.WB_old[i_reg] = WB.W[i_reg];
                            break;
                        }

                    if (isChanged)
                    {

                        Array.Copy(WB.W, NReg * (Coil_i + nTask * TaskCoilCount), data, (0), NReg);
                        mbMasterW0.WriteMultipleRegisters(1, (ushort)(destStartAddr + NReg * Coil_i), data);
                    }
                    else
                        Debug.WriteLine("W1 skip");

                }

                nowTime = (float)DateTime.Now.Second + ((float)DateTime.Now.Millisecond) / 1000f;
                dt = nowTime - prevTime;
                prevTime = nowTime;
                
                Debug.WriteLine("W0 time = " + dt.ToString() );
                System.Threading.Thread.Sleep(Sett.Instance.TPause);
               // counter++;
               // StatusR0.Content = counter.ToString();
            }

        }
        private void UpdateW1()
        {
            //измерение времени цикла записи
            float prevTime = 0;
            float nowTime;
            int counter = 0;
            float dt;   //время цикла в сек
            while (!cancelTokenSrc.IsCancellationRequested)
            {
                int nTask = 1; // Номер потока
                int AreaW = WB.W.Length; //  (29 - 3 + 1) * 126]; // 3402 
                int NReg = 120; // количество регистров на запись не более 120

                ushort[] data = new ushort[NReg];   //буфер для записи одной бочки
                int TaskCoilCount = AreaW / NReg / Sett.Instance.NWrTask;   //количество бочек по 120 регистров которые записываются в данном потоке

                //адрес в ПЛК для данного потока
                int destStartAddr = Sett.Instance.BegAddrW + nTask * NReg * TaskCoilCount;
                //coil_i номер бочки передаваемой в потоке 0..7
                for (int Coil_i = 0; Coil_i < TaskCoilCount; Coil_i++)
                {
                    bool isChanged = false;
                    for (int i_reg = NReg * (Coil_i + nTask * TaskCoilCount); i_reg < NReg * (Coil_i + 1 + nTask * TaskCoilCount); i_reg++)
                        if (WB.W[i_reg] != WB.WB_old[i_reg])
                        {
                            WB.WB_old[i_reg] = WB.W[i_reg];
                            isChanged = true;
                            break;
                        }

                    if (isChanged)
                    {
                        Array.Copy(WB.W, NReg * (Coil_i + nTask * TaskCoilCount), data, (0), NReg);
                        mbMasterW0.WriteMultipleRegisters(1, (ushort)(destStartAddr + NReg * Coil_i), data);
                    }
                    else
                        Debug.WriteLine("W1 skip");
                }

                nowTime = (float)DateTime.Now.Second + ((float)DateTime.Now.Millisecond) / 1000f;
                dt = nowTime - prevTime;
                prevTime = nowTime;

                Debug.WriteLine("W1 time = " + dt.ToString());
                System.Threading.Thread.Sleep(Sett.Instance.TPause);
            }
        }
        private void UpdateW2()
        {
            //измерение времени цикла записи
            float prevTime = 0;
            float nowTime;
            int counter = 0;
            float dt;   //время цикла в сек
            while (!cancelTokenSrc.IsCancellationRequested)
            {
                int nTask = 2; // Номер потока
                int AreaW = WB.W.Length; //  (29 - 3 + 1) * 126]; // 3402 
                int NReg = 120; // количество регистров на запись не более 120

                ushort[] data = new ushort[NReg];   //буфер для записи одной бочки
                int TaskCoilCount = AreaW / NReg / Sett.Instance.NWrTask;   //количество бочек по 120 регистров которые записываются в данном потоке

                int destStartAddr = Sett.Instance.BegAddrW + nTask * NReg * TaskCoilCount; //адрес в ПЛК 
                //coil_i номер бочки передаваемой в потоке, coil_i - сквозная нумерация по всем потокам
                for (int Coil_i = 0; Coil_i < TaskCoilCount; Coil_i++)
                {
                    bool isChanged = false;
                    for (int i_reg = NReg * (Coil_i + nTask * TaskCoilCount); i_reg < NReg * (Coil_i + 1 + nTask * TaskCoilCount); i_reg++)
                        if (WB.W[i_reg] != WB.WB_old[i_reg])
                        {
                            WB.WB_old[i_reg] = WB.W[i_reg];
                            isChanged = true;
                            break;
                        }

                    if (isChanged)
                    {
                        Array.Copy(WB.W, NReg * (Coil_i + nTask * TaskCoilCount), data, (0), NReg);
                        mbMasterW0.WriteMultipleRegisters(1, (ushort)(destStartAddr + NReg * Coil_i), data);
                    }
                    else
                    {
                        Debug.WriteLine("W2 skip");
                    }
                }
                nowTime = (float)DateTime.Now.Second + ((float)DateTime.Now.Millisecond) / 1000f;
                dt = nowTime - prevTime;
                prevTime = nowTime;

                Debug.WriteLine("W2 time = " + dt.ToString());
                System.Threading.Thread.Sleep(Sett.Instance.TPause);
            }
        }
        private void UpdateW3()
        {
            //измерение времени цикла записи
            float prevTime = 0;
            float nowTime;
            int counter = 0;
            float dt;   //время цикла в сек
            while (!cancelTokenSrc.IsCancellationRequested)
            {
                int nTask = 3; // Номер потока
                int AreaW = WB.W.Length; //  (29 - 3 + 1) * 126]; // 3402 
                int NReg = 120; // количество регистров на запись не более 120

                ushort[] data = new ushort[NReg];   //буфер для записи одной бочки
                int TaskCoilCount = AreaW / NReg / Sett.Instance.NWrTask;   //количество бочек по 120 регистров которые записываются в данном потоке

                int destStartAddr = Sett.Instance.BegAddrW + nTask * NReg * TaskCoilCount; //адрес в ПЛК 
                                                                                           //coil_i номер бочки передаваемой в потоке, coil_i - сквозная нумерация по всем потокам
                for (int Coil_i = 0; Coil_i < TaskCoilCount; Coil_i++)
                {
                    bool isChanged = false;
                    for (int i_reg = NReg * (Coil_i + nTask * TaskCoilCount); i_reg < NReg * (Coil_i + 1 + nTask * TaskCoilCount); i_reg++)
                        if (WB.W[i_reg] != WB.WB_old[i_reg])
                        {
                            WB.WB_old[i_reg] = WB.W[i_reg];
                            isChanged = true;
                            break;
                        }

                    if (isChanged)
                    {
                        Array.Copy(WB.W, NReg * (Coil_i + nTask * TaskCoilCount), data, (0), NReg);
                        mbMasterW0.WriteMultipleRegisters(1, (ushort)(destStartAddr + NReg * Coil_i), data);
                    }else
                        Debug.WriteLine("W3 skip");

                }

                //запись последней бочки размером менее 120 рег
                int halfCoilLength = AreaW / (TaskCoilCount * NReg * Sett.Instance.NWrTask);
                if (halfCoilLength > 0)
                {
                    Array.Copy(WB.W, (NReg * TaskCoilCount * Sett.Instance.NWrTask), data, (0), halfCoilLength);
                    mbMasterW0.WriteMultipleRegisters(1, (ushort)(destStartAddr + TaskCoilCount * NReg * Sett.Instance.NWrTask), data);
                }

                nowTime = (float)DateTime.Now.Second + ((float)DateTime.Now.Millisecond) / 1000f;
                dt = nowTime - prevTime;
                prevTime = nowTime;

                Debug.WriteLine("W3 time = " + dt.ToString());
                System.Threading.Thread.Sleep(Sett.Instance.TPause);
            }
        }
  
        */
   

        void SendAItoW()  // записываем значение АЦП в массив для записи CPU
        {
            for (int i = 0; i < AIStruct.items.Length; i++)
            {
                if (AIStruct.items[i].En /* || true */)
                {
                //    AIStruct.items[i].updateAI();
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
           /* foreach (DIStruct item in DIStruct.items)
            {
                if (item.En)
                {
                    if (item.InvertDI)
                        SetBit(ref (WB.W[(item.indxW)]), (item.indxBitDI),  (!item.ValDI));
                    else
                        SetBit(ref (WB.W[(item.indxW)]), (item.indxBitDI), (item.ValDI));
                }
            }*/

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

        bool isConfigLoaded = false;
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
                    isConfigLoaded = true;
                    /* AIStruct.items = station.AIs;
                     DIStruct.items = station.DIs;
                     DOStruct.items = station.DOs;

                     VSStruct.VSs = station.VSs;
                     KLStruct.KLs = station.KLs;
                     MPNAStruct.MPNAs = station.MPNAs;*/
                   // Sett.Instance = station.settings;

                    /* RB.R = new ushort[(Sett.Instance.NRackEnd) * 50];//[(29 - 3 + 1) * 50]    =1450   From IOScaner CPU
                     WB.W = new ushort[(Sett.Instance.NRackEnd - Sett.Instance.NRackBeg + 1) * 126]; // =3402 From IOScaner CPU
                     WB.WB_old  = new ushort[(Sett.Instance.NRackEnd - Sett.Instance.NRackBeg + 1) * 126];*/

                    WB.InitBuffers(Sett.Instance);
                    AITableViewModel.Instance.Init(AIStruct.items);

                    if (AITableViewModel.Instance.AIs == null)
                        AITableViewModel.Instance.AIs.CollectionChanged +=new NotifyCollectionChangedEventHandler(OnAITableChanged);

                    dataGridAI.ItemsSource = AITableViewModel.Instance.viewSource.View;

                    DITableViewModel.Instance.Init(DIStruct.items);
                    dataGridDI.ItemsSource = DITableViewModel.Instance.viewSource.View;

                    dataGridCounters.ItemsSource = CountersTableViewModel.Counters;
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
        /// <summary>
        /// сохранение в один xml
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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


        }
        /// <summary>
        /// сохранение в раздельные xml
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// Токен отмены
        /// </summary>
        CancellationTokenSource cancelTokenSrc;
        CancellationToken cancellationToken;
        WritingThread wrThread;
        ReadThread rdThread;
        Thread watchThread;

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (!isConfigLoaded)
            {
                System.Windows.MessageBox.Show("Конфигурация не загружена!","Ошибка",MessageBoxButton.OK,MessageBoxImage.Error);
                return;
            }

            
            cancelTokenSrc = new CancellationTokenSource();
            cancellationToken = cancelTokenSrc.Token;
            try
            {
               

               /* mbMasterW0 = ModbusIpMaster.CreateIp(new TcpClient(Sett.Instance.HostName, Sett.Instance.MBPort));
                mbMasterW1 = ModbusIpMaster.CreateIp(new TcpClient(Sett.Instance.HostName, Sett.Instance.MBPort));
                mbMasterW2 = ModbusIpMaster.CreateIp(new TcpClient(Sett.Instance.HostName, Sett.Instance.MBPort));
                mbMasterW3 = ModbusIpMaster.CreateIp(new TcpClient(Sett.Instance.HostName, Sett.Instance.MBPort));*/
                //   mbMasterWLast = ModbusIpMaster.CreateIp(new TcpClient(Sett.Instance.HostName, Sett.Instance.MBPort));

                // masterLoop = Task.Factory.StartNew(new Action(Update), cancellationToken);
                /*   masterLoopR0 = Task.Factory.StartNew(new Action(UpdateR0), cancellationToken);
                   masterLoopR1 = Task.Factory.StartNew(new Action(UpdateR1), cancellationToken);
                   masterLoopR2 = Task.Factory.StartNew(new Action(UpdateR2), cancellationToken);
                   masterLoopR3 = Task.Factory.StartNew(new Action(UpdateR3), cancellationToken);*/

                /*         masterLoopW0 = Task.Factory.StartNew(new Action(UpdateW0), cancellationToken);
                         masterLoopW1 = Task.Factory.StartNew(new Action(UpdateW1), cancellationToken);
                         masterLoopW2 = Task.Factory.StartNew(new Action(UpdateW2), cancellationToken);
                         masterLoopW3 = Task.Factory.StartNew(new Action(UpdateW3), cancellationToken);*/
                //      masterLoopWLast = Task.Factory.StartNew(new Action(UpdateLast), cancellationToken);

                rdThread = new ReadThread(Sett.Instance.HostName, Sett.Instance.MBPort);
                rdThread.refMainWindow = this;

                    wrThread = new WritingThread(Sett.Instance.HostName, Sett.Instance.MBPort);
                    wrThread.refMainWindow = this;


                readingTime = DateTime.Now;
                    writingTime = DateTime.Now;

                watchThread = new Thread(new ThreadStart(Watchdog));
                watchThread.Start();

                statusText.Content = "Запущен";
                statusText.Background = System.Windows.Media.Brushes.Green;
                btnStart.IsEnabled = false;
                
                btnStop.IsEnabled = true;
                btnPause.IsEnabled = true;
            }
            catch(Exception ex)
            {
                System.Windows.MessageBox.Show("Ошибка: " +Environment.NewLine + ex.Message, "Ошибка");
            }
        }

        //------------ вызывается каждую итерацию цикла записи ----------------
        private void On_WritingCycleEnd(float dt)
        {
            StatusW.Content = "Время записи: " + dt.ToString("F2");
            writingTime = DateTime.Now;
        }
        //--------------------------------------------------------------------

        private void On_Disconnected()
        {
            btnStop_Click(null,null);
            System.Windows.MessageBox.Show("Соединение разорвано!");
        }

        //---------------------------- 
        private void On_ReadingCycleEnd()
        {
            TimeSpan ts = DateTime.Now - readingTime;
            StatusR.Content = "Время чтения: " + ts.TotalSeconds.ToString("F2");
            readingTime = DateTime.Now;
        }

        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            statusText.Content = "пауза";
            statusText.Background = System.Windows.Media.Brushes.Blue;

            btnStart.IsEnabled = true;
            btnStop.IsEnabled = false;
            btnPause.IsEnabled = false;

            wrThread.Paused = true;
            try
            {
                cancelTokenSrc.Cancel();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            statusText.Content = "Остановлен";
            statusText.Background = System.Windows.Media.Brushes.Yellow;

            btnStart.IsEnabled = true;
            btnStop.IsEnabled = false;
            btnPause.IsEnabled = false;

            wrThread.Stop();
            wrThread = null;
            rdThread.Stop();
            rdThread = null;

            if (watchThread!=null)
            watchThread.Abort();

        
            try
            {
                cancelTokenSrc.Cancel();
               // wrThread = null;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message,"Ошибка",MessageBoxButton.OK,MessageBoxImage.Error);
            }

           // foreach (AIStruct ai in AIStruct.items)
           //     ai.fValAI = 0f;

            //foreach (DIStruct di in DIStruct.items)
           //     di.ValDI = false;

            foreach (ZDStruct zd in ZDTableViewModel.ZDs)
                zd.Reset();

            //foreach (MPNAStruct mna in MPNATableViewModel.MPNAs)
                
            //TODO: добавить сброс остальных систем
        }

        private void MenuItem_Click_save(object sender, RoutedEventArgs e)
        {

        }

        SetupDialog dialog;

     
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

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (System.Windows.MessageBox.Show("Все несохраненные данные будут утеряны. Выйти из программы?", "Подтверждение выхода", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                if (wrThread != null)
                    wrThread.Stop();
                if (rdThread != null)
                    rdThread.Stop();
                if (watchThread != null)
                    watchThread.Abort();
            }
            else
            {
                e.Cancel = true;
            }            
        }

        #region context_menus
        //
        private void VSMenu_stop_Click(object sender, RoutedEventArgs e)
        {
            //          e.Source.ToString();
            //          (e.Source as VSStruct).ManualStart();
            (dataGridVS.SelectedItem as VSStruct).ManualStop();
        }

        private void VSMenu_start_Click(object sender, RoutedEventArgs e)
        {
            //         (e.Source as VSStruct).ManualStop();
            (dataGridVS.SelectedItem as VSStruct).ManualStart();
        }

        private void VSMenu_settings_Click(object sender, RoutedEventArgs e)
        {
            if ((dialog != null) && (dialog.IsLoaded))
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

        private void ZDMenu_open_Click(object sender, RoutedEventArgs e)
        {
            (dataGridZD.SelectedItem as ZDStruct).ManualOpen();
        }

        private void ZDMenu_close_Click(object sender, RoutedEventArgs e)
        {
            (dataGridZD.SelectedItem as ZDStruct).ManualClose();
        }

        private void ZDMenu_stop_Click(object sender, RoutedEventArgs e)
        {
            (dataGridZD.SelectedItem as ZDStruct).ManualStop();

        }
        private void ZDMenu_dist_Click(object sender, RoutedEventArgs e)
        {
            (dataGridZD.SelectedItem as ZDStruct).ToggleDist();
        }
        private void ZDMenu_settings_Click(object sender, RoutedEventArgs e)
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

        private void KLMenu_open_Click(object sender, RoutedEventArgs e)
        {
            (dataGridKL.SelectedItem as KLStruct).ManualOpen();
        }

        private void KLMenu_close_Click(object sender, RoutedEventArgs e)
        {
            (dataGridKL.SelectedItem as KLStruct).ManualClose();
        }

        private void KLMenu_stop_Click(object sender, RoutedEventArgs e)
        {
            (dataGridKL.SelectedItem as KLStruct).ManualStop();
        }

        private void KLMenu_settings_Click(object sender, RoutedEventArgs e)
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
        #endregion
        private void OnAITableChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            AIStruct.items = AITableViewModel.Instance.AIs.ToArray();
        }

    }
}
