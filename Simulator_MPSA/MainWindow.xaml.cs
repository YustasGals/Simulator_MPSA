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

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
 

        public delegate void DEndWrite();
        public DEndWrite EndCycle;

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
            delegateDisconnected += new DDisconnected(On_Disconnected);
            delegateEndRead += new DEndRead(On_ReadingCycleEnd);

           // myDelegate += new ddd(On_WritingCycleEnd);
        }


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

                //-------------  проверка связи  -----------------------
                    if ((DateTime.Now - readingTime).TotalSeconds > 3f)
                    {
                        // wrThread.Stop();
                        // btnPause_Click(null, new RoutedEventArgs());
                        this.Dispatcher.Invoke(delegateDisconnected);
                       btnStop_Click(null, new RoutedEventArgs());
                    }

                System.Threading.Thread.Sleep(Sett.Instance.TPause * 2);
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
        private void On_WritingCycleEnd()
        {
            TimeSpan ts = DateTime.Now - writingTime;
            StatusW.Content = "Время записи: " + ts.TotalSeconds.ToString("F2");
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
