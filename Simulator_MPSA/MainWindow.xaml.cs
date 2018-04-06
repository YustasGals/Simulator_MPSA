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
using Simulator_MPSA.Scripting;
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

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
 

        public delegate void DEndWrite();
        public DEndWrite EndCycle;
        public DEndWrite EndCycle2;
        public DEndWrite EndCycle3;
        public DEndWrite EndCycle4;
        public DEndWrite EndCycle5;
        public DEndWrite EndCycle6;

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

            EndCycle += new DEndWrite(On_WritingCycleEnd);
            EndCycle2 += new DEndWrite(On_WritingCycle2End);
            EndCycle3 += new DEndWrite(On_WritingCycle3End);
            EndCycle4 += new DEndWrite(On_WritingCycle4End);
            EndCycle5 += new DEndWrite(On_WritingCycle5End);
            EndCycle6 += new DEndWrite(On_WritingCycle6End);


            delegateDisconnected += new DDisconnected(On_Disconnected);
            delegateEndRead += new DEndRead(On_ReadingCycleEnd);

            // myDelegate += new ddd(On_WritingCycleEnd);
            SetConfigMode(false);
        }


        /// <summary>
        /// время последней записи в ПЛК
        /// </summary>
        DateTime[] writingTime = new DateTime[6];

        /// <summary>
        /// время последнего чтения из ПЛК
        /// </summary>
        DateTime readingTime;

        DateTime prevCycleTime;

        float dt_sec;
        bool mainCycleEnabled = true;
        private void Watchdog()
        {
            prevCycleTime = DateTime.Now;
            dt_sec = 0f;
            while (mainCycleEnabled)   
            {
                //-------- обновление структур --------------------------
                foreach (ZDStruct zd in ZDTableViewModel.ZDs)
                    zd.UpdateZD(dt_sec);

                foreach (KLStruct kl in KLTableViewModel.KL)
                    kl.UpdateKL(dt_sec);

                foreach (VSStruct vs in VSTableViewModel.VS)
                    vs.UpdateVS(dt_sec);

                foreach (MPNAStruct mpna in MPNATableViewModel.MPNAs)
                    mpna.UpdateMPNA(dt_sec);



                if (ScriptInfo.Items != null)                    
                    foreach (Scripting.ScriptInfo script in Scripting.ScriptInfo.Items)
                        script.Run(dt_sec);
                //--------------- формирование массивов для передачи в ПЛК ---------------------
                //for (int i = 0; i < AIStruct.items.Length; i++)
                lock (WB.W)
                {
                    foreach (AIStruct ai in AIStruct.items)
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
                                    WB.W_a4[ai.PLCAddr - Sett.Instance.iBegAddrA4 - 1] = w1;
                                    WB.W_a4[ai.PLCAddr - Sett.Instance.iBegAddrA4] = w2;
                                }
                            }
                        }//ai.en
                    }//foreach

                    foreach (DIStruct di in DIStruct.items)
                    {
                        if (di.En)
                        {
                            /* int indx = di.PLCAddr - Sett.Instance.BegAddrW - 1;
                             if (indx > 0 && indx < WB.W.Length)
                             */
                            if (di.Buffer == BufType.USO)
                                SetBit(ref (WB.W[di.PLCAddr - Sett.Instance.BegAddrW - 1]), (di.indxBitDI), di.ValDI ^ di.InvertDI);
                            else
                            if (di.Buffer == BufType.A3)
                                SetBit(ref (WB.W_a3[di.PLCAddr - Sett.Instance.iBegAddrA3 - 1]), (di.indxBitDI), di.ValDI ^ di.InvertDI);
                            else
                            if (di.Buffer == BufType.A4)
                                SetBit(ref (WB.W_a4[di.PLCAddr - Sett.Instance.iBegAddrA4 - 1]), (di.indxBitDI), di.ValDI ^ di.InvertDI);
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
                            if ((c.PLCAddr >= Sett.Instance.iBegAddrA3 + 1) && ((c.PLCAddr - Sett.Instance.iBegAddrA3 - 1) < WB.W_a3.Length))
                            {
                                WB.W_a3[c.PLCAddr - Sett.Instance.iBegAddrA3 - 1] = c.Value;
                            }
                        if (c.buffer == BufType.A4)
                            if ((c.PLCAddr >= Sett.Instance.iBegAddrA4 + 1) && ((c.PLCAddr - Sett.Instance.iBegAddrA4 - 1) < WB.W_a4.Length))
                            {
                                WB.W_a4[c.PLCAddr - Sett.Instance.iBegAddrA4 - 1] = c.Value;
                            }
                    }

                    foreach (DIStruct di in DiagTableModel.Instance.DiagRegs)
                    {
                        if (di.En)
                        {
                            /* int indx = di.PLCAddr - Sett.Instance.BegAddrW - 1;
                             if (indx > 0 && indx < WB.W.Length)
                             */
                            if (di.Buffer == BufType.USO)
                                SetBit(ref (WB.W[di.PLCAddr - Sett.Instance.BegAddrW - 1]), (di.indxBitDI), di.ValDI ^ di.InvertDI);
                            else
                            if (di.Buffer == BufType.A3)
                                SetBit(ref (WB.W_a3[di.PLCAddr - Sett.Instance.iBegAddrA3 - 1]), (di.indxBitDI), di.ValDI ^ di.InvertDI);
                            else
                            if (di.Buffer == BufType.A4)
                                SetBit(ref (WB.W_a4[di.PLCAddr - Sett.Instance.iBegAddrA4 - 1]), (di.indxBitDI), di.ValDI ^ di.InvertDI);
                        }
                    }

                }//lock
                    
                //-------------  проверка связи  -----------------------
                if ((DateTime.Now - readingTime).TotalSeconds > 5f)
                    {
                        // wrThread.Stop();
                        // btnPause_Click(null, new RoutedEventArgs());
                        this.Dispatcher.Invoke(delegateDisconnected);
                       btnStop_Click(null, new RoutedEventArgs());
                    }

                //---------- вычисление время с момента предыдущей итерации ----------------
                dt_sec = (float)(DateTime.Now - prevCycleTime).TotalSeconds;
                prevCycleTime = DateTime.Now;
                Debug.WriteLine("Main time: "+dt_sec.ToString("F2"));
                System.Threading.Thread.Sleep(Sett.Instance.TPause);
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

                    dataGridDiag.ItemsSource = DiagTableModel.Instance.viewSource.View;

                    dataGridScript.ItemsSource = Scripting.ScriptInfo.Items;
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
               

                rdThread = new ReadThread(Sett.Instance.HostName, Sett.Instance.MBPort);
                rdThread.refMainWindow = this;
                
                wrThread = new WritingThread(Sett.Instance.HostName, Sett.Instance.MBPort);
                wrThread.refMainWindow = this;
                wrThread.Start();

                readingTime = DateTime.Now;
                for (int i= 0; i<6; i++)
                writingTime[i] = DateTime.Now;
                
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
        private void On_WritingCycleEnd()
        {
            TimeSpan ts = DateTime.Now - writingTime[0];
            StatusW1.Content =ts.TotalSeconds.ToString("F2")+" | ";
            writingTime[0] = DateTime.Now;
        }
    
        private void On_WritingCycle2End()
        {
            TimeSpan ts = DateTime.Now - writingTime[1];
            //   StatusW.Content = "Время записи: " + ts.TotalSeconds.ToString("F2");
            StatusW2.Content = ts.TotalSeconds.ToString("F2") + " | ";
            writingTime[1] = DateTime.Now;
        }
      
        private void On_WritingCycle3End()
        {
            TimeSpan ts = DateTime.Now - writingTime[2];
            // StatusW.Content = "Время записи: " + ts.TotalSeconds.ToString("F2");
            StatusW3.Content = ts.TotalSeconds.ToString("F2") + " | ";
            writingTime[2] = DateTime.Now;
        }
     
        private void On_WritingCycle4End()
        {
            TimeSpan ts = DateTime.Now - writingTime[3];
            //   StatusW.Content = "Время записи: " + ts.TotalSeconds.ToString("F2");
            StatusW4.Content = ts.TotalSeconds.ToString("F2") + " | ";
            writingTime[3] = DateTime.Now;
        }
      
        private void On_WritingCycle5End()
        {
            TimeSpan ts = DateTime.Now - writingTime[4];
            StatusW5.Content = ts.TotalSeconds.ToString("F2");
            //   StatusW.Content = "Время записи: " + ts.TotalSeconds.ToString("F2");
            writingTime[4] = DateTime.Now;
        }

        private void On_WritingCycle6End()
        {
            TimeSpan ts = DateTime.Now - writingTime[5];
            StatusW6.Content = ts.TotalSeconds.ToString("F2");
            //   StatusW.Content = "Время записи: " + ts.TotalSeconds.ToString("F2");
            writingTime[6] = DateTime.Now;
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

            wrThread.Stop();
            wrThread = null;
            rdThread.Stop();
            rdThread = null;

            if (watchThread != null)
                watchThread.Abort();
            //wrThread.Paused = true;
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
            {
                DITableViewModel.Instance.NameFilter = textBoxDIFilter.Text;
                DITableViewModel.Instance.TagFilter = textBoxDITagFilter.Text;
                DITableViewModel.Instance.ApplyFilter();
            }
        }

        private void textBoxAIFilter_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                AITableViewModel.Instance.NameFilter = textBoxAIFilter.Text;
                AITableViewModel.Instance.TagFilter = textBoxAITagFilter.Text;
                AITableViewModel.Instance.ApplyFilter();
            }
        }

        private void textBoxDOFilter_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                DOTableViewModel.Instance.NameFilter = textBoxDOFilter.Text;
                DOTableViewModel.Instance.tagFilter = textBoxDOTagFilter.Text;
                DOTableViewModel.Instance.ApplyFilter();
            }
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

        private void OnCounterTableChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            
        }

        private void hideEmptyAI_Checked(object sender, RoutedEventArgs e)
        {
            AITableViewModel.Instance.hideEmpty = true;
            AITableViewModel.Instance.ApplyFilter();
        }

        private void hideEmptyAI_Unchecked(object sender, RoutedEventArgs e)
        {
            AITableViewModel.Instance.hideEmpty = false;
            AITableViewModel.Instance.ApplyFilter();
        }
        private bool IsConfigMode
        {
            get {return MenuItem_config.IsChecked; }
        }

        void SetConfigMode(bool e)
        {
            if (e)
            {
                DataGridAI_Addr.Visibility = Visibility.Visible;
                DataGridAI_Buf.Visibility = Visibility.Visible;
                DataGridAI_On.Visibility = Visibility.Visible;
                DataGridAI_Type.Visibility = Visibility.Visible;
                dataGridAI.IsManipulationEnabled = true;
                dataGridAI.CanUserAddRows = true;
                dataGridAI.CanUserDeleteRows = true;

                tabSettings.Visibility = Visibility.Visible;
                tabDiagMod.Visibility = Visibility.Visible;
                tabDiagUSO.Visibility = Visibility.Visible;

                dataGridDI_Addr.Visibility = Visibility.Visible;
                dataGridDI_Bit.Visibility = Visibility.Visible;
                dataGridDI_Buf.Visibility = Visibility.Visible;
                dataGridDI_Invert.Visibility = Visibility.Visible;
                dataGridDI_On.Visibility = Visibility.Visible;

                dataGridDO_Addr.Visibility = Visibility.Visible;
                dataGridDO_Bit.Visibility = Visibility.Visible;
                dataGridDO_invert.Visibility = Visibility.Visible;
                dataGridDO_On.Visibility = Visibility.Visible;

                dataGridCounters_addr.Visibility = Visibility.Visible;
                dataGridCounters_buf.Visibility = Visibility.Visible;

                dataGridDiag.CanUserAddRows = true;
                dataGridDiag.CanUserDeleteRows = true;

                dataGridZD.CanUserAddRows = true;
                dataGridZD.CanUserDeleteRows = true;

                dataGridKL.CanUserAddRows = true;
                dataGridKL.CanUserDeleteRows = true;

                dataGridDI.CanUserAddRows = true;
                dataGridDI.CanUserDeleteRows = true;


                dataGridDiag_Addr.Visibility = Visibility.Visible;
                dataGridDiag_Bit.Visibility = Visibility.Visible;
            }
            else
            {
                DataGridAI_Addr.Visibility = Visibility.Hidden;
                DataGridAI_Buf.Visibility = Visibility.Hidden;
                DataGridAI_On.Visibility = Visibility.Hidden;
                DataGridAI_Type.Visibility = Visibility.Hidden;
                dataGridAI.IsManipulationEnabled = false;
                dataGridAI.CanUserAddRows = false;
                dataGridAI.CanUserDeleteRows = false;

                dataGridZD.CanUserAddRows = false;
                dataGridZD.CanUserDeleteRows = false;

                dataGridKL.CanUserAddRows = false;
                dataGridKL.CanUserDeleteRows = false;

                dataGridDI.CanUserAddRows = false;
                dataGridDI.CanUserDeleteRows = false;

                tabSettings.Visibility = Visibility.Collapsed;
                tabDiagMod.Visibility = Visibility.Collapsed;
                tabDiagUSO.Visibility = Visibility.Collapsed;

                dataGridDI_Addr.Visibility = Visibility.Hidden;
                dataGridDI_Bit.Visibility = Visibility.Hidden;
                dataGridDI_Buf.Visibility = Visibility.Hidden;
                dataGridDI_Invert.Visibility = Visibility.Hidden;
                dataGridDI_On.Visibility = Visibility.Hidden;

                dataGridDO_Addr.Visibility = Visibility.Hidden;
                dataGridDO_Bit.Visibility = Visibility.Hidden;
                dataGridDO_invert.Visibility = Visibility.Hidden;
                dataGridDO_On.Visibility = Visibility.Hidden;

                dataGridCounters_addr.Visibility = Visibility.Hidden;
                dataGridCounters_buf.Visibility = Visibility.Hidden;

                dataGridDiag.CanUserAddRows = false;
                dataGridDiag.CanUserDeleteRows = false;

                dataGridDiag_Addr.Visibility = Visibility.Hidden;
                dataGridDiag_Bit.Visibility = Visibility.Hidden;
            }
        }
        private void MenuItem_toggleConfigMode(object sender, RoutedEventArgs e)
        {
            MenuItem_config.IsChecked = !MenuItem_config.IsChecked;
            SetConfigMode(MenuItem_config.IsChecked);
        }

        private void hideEmptyDI_Checked(object sender, RoutedEventArgs e)
        {
            DITableViewModel.Instance.HideEmpty = true;
            DITableViewModel.Instance.ApplyFilter();
        }

        private void hideEmptyDI_Unchecked(object sender, RoutedEventArgs e)
        {
            DITableViewModel.Instance.HideEmpty = false;
            DITableViewModel.Instance.ApplyFilter();
        }

        private void hideEmptyDO_Checked(object sender, RoutedEventArgs e)
        {
            DOTableViewModel.Instance.hideEmpty = true;
            DOTableViewModel.Instance.ApplyFilter();

        }

        private void hideEmptyDO_Unchecked(object sender, RoutedEventArgs e)
        {
            DOTableViewModel.Instance.hideEmpty = false;
            DOTableViewModel.Instance.ApplyFilter();
        }

        private void textBoxDiagFilter_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                DiagTableModel.Instance.nameFilter = textBoxDiagFilter.Text;
                DiagTableModel.Instance.ApplyFilter();
            }
        }

        private void MenuItem_toggleMPNA(object sender, RoutedEventArgs e)
        {
            MenuItem_showMPNA.IsChecked = !MenuItem_showMPNA.IsChecked;
            if (!MenuItem_showMPNA.IsChecked)
                tabMPNA.Visibility = Visibility.Collapsed;
            else
                tabMPNA.Visibility = Visibility.Visible;
        }

        private void MenuItem_toggleKL(object sender, RoutedEventArgs e)
        {
            MenuItem_showKL.IsChecked = !MenuItem_showKL.IsChecked;
            if (!MenuItem_showKL.IsChecked)
                tabKL.Visibility = Visibility.Collapsed;
            else
                tabKL.Visibility = Visibility.Visible;
        }

        private void MenuItem_toggleVS(object sender, RoutedEventArgs e)
        {
            MenuItem_showVS.IsChecked = !MenuItem_showVS.IsChecked;

            if (!MenuItem_showVS.IsChecked)
                tabVS.Visibility = Visibility.Collapsed;
            else
                tabVS.Visibility = Visibility.Visible;
        }

        private void ScriptMenu_EditClick(object sender, RoutedEventArgs e)
        {
            ScriptEditor editor = new ScriptEditor(dataGridScript.SelectedItem as Scripting.ScriptInfo);
            editor.Show();
        }

        private void ScriptMenu_RunClick(object sender, RoutedEventArgs e)
        {
            ScriptInfo script = dataGridScript.SelectedItem as Scripting.ScriptInfo;

            if (script != null)
            {
                script.Prepare();
                script.Run(0);
            }
        }
    }
}
