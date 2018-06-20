﻿using System.Configuration; // xml
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
using Microsoft.Win32;
using Simulator_MPSA.ViewModel;
using Opc;
using OpcCom;
using OpcXml;

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
        public static void InitBuffer()
        {
            RB.R = new ushort[Sett.Instance.rdBufSize];//[(29 - 3 + 1) * 50]    =1450   From IOScaner CPU
            LogViewModel.WriteLine("Размер буфера чтения обновлен: "+RB.R.Count().ToString() +" рег.");

        }
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

        public static void InitBuffers()
        {
            
            //int regCount = Sett.Instance.wrBufSize;
            //int coilCount = (int)Math.Ceiling((float)regCount / 120f);

            //int regCountRounded = coilCount * 120;
            WB.W = new ushort[Sett.Instance.wrBufSize]; // =3402 From IOScaner CPU
            WB.WB_old = new ushort[Sett.Instance.wrBufSize];

            WB.W_a3 = new ushort[Sett.Instance.A3BufSize];
            WB.W_a3_prev = new ushort[WB.W_a3.Length];

            WB.W_a4 = new ushort[Sett.Instance.A3BufSize];
            WB.W_a4_prev = new ushort[WB.W_a4.Length];

            LogViewModel.WriteLine("Размеры буферов записи обновлены: " + WB.W.Count().ToString() +"/"+WB.W_a3.Count().ToString()+"/"+WB.W_a4.Count().ToString()+ " рег.");
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
        Opc.Da.Server srv;

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

        ViewModelCollection<DIViewModel, DIStruct> divm;
        ViewModelCollection<DOViewModel, DOStruct> dovm;
        ViewModelCollection<AIViewModel, AIStruct> aivm;
        ViewModelCollection<AOViewModel, AOStruct> aovm;

        //
        List<Opc.Da.ItemValue> opcitems = new List<Opc.Da.ItemValue>();
        Opc.Da.ItemValue itm;   

        public MainWindow()
        {

            InitializeComponent();


            RB.InitBuffer();
            WB.InitBuffers();
 
            //-- Таблица DI --
            divm = new ViewModelCollection<DIViewModel, DIStruct>(DIStruct.items);

            DITab.DataContext = divm;
            hideEmptyDI.DataContext = divm;

            DIStruct.EnableAutoIndex = true;

            //-- Таблица AI --
            aivm = new ViewModelCollection<AIViewModel, AIStruct>(AIStruct.items);
            hideEmptyAI.DataContext = aivm;
            AITab.DataContext = aivm;

            AIStruct.EnableAutoIndex = true;

            //-- Таблица DO --
            dovm = new ViewModelCollection<DOViewModel, DOStruct>(DOStruct.items);
            hideEmptyDO.DataContext = dovm;
            DOTab.DataContext = dovm;

            DOStruct.EnableAutoIndex = true;

            //-- Таблица AO --
            aovm = new ViewModelCollection<AOViewModel, AOStruct>(AOStruct.items);
            AOTab.DataContext = aovm;
            AOHideEmpty.DataContext = aovm;
            AOStruct.EnableAutoIndex = true;

            //------

            dataGridZD.DataContext = ZDTableViewModel.Instance;
            dataGridVS.DataContext = VSTableViewModel.Instance;
            dataGridKL.DataContext = KLTableViewModel.Instance;
            dataGridMPNA.DataContext = MPNATableViewModel.Instance;

 
            ScriptInfo.Init();
            dataGridScript.ItemsSource = ScriptInfo.Items;

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


            string subkey = @"software\NA\Simulator";
            //    int ConfMode = (int)Microsoft.Win32.Registry.GetValue(Registry.CurrentUser.OpenSubKey(subkey), "ConfigMode", 0);

            RegistryKey configKey = Registry.CurrentUser.CreateSubKey(subkey);


            int ConfMode = (int)configKey.GetValue("ConfigMode", 0);

            configKey.SetValue("ConfigMode", ConfMode);
         //   SetConfigMode(/*ConfMode != 0*/true);
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
                if (ZDTableViewModel.ZDs != null && ZDTableViewModel.ZDs.Count>0)
                foreach (ZDStruct zd in ZDTableViewModel.ZDs)
                    zd.UpdateZD(dt_sec);

                if (KLTableViewModel.KL != null && KLTableViewModel.KL.Count>0)
                foreach (KLStruct kl in KLTableViewModel.KL)
                    kl.UpdateKL(dt_sec);

                if (VSTableViewModel.VS != null && VSTableViewModel.VS.Count>0)
                foreach (VSStruct vs in VSTableViewModel.VS)
                    vs.UpdateVS(dt_sec);

                if (MPNATableViewModel.MPNAs!=null && MPNATableViewModel.MPNAs.Count > 0)
                foreach (MPNAStruct mpna in MPNATableViewModel.MPNAs)
                    mpna.UpdateMPNA(dt_sec);



                if (ScriptInfo.Items != null && ScriptInfo.Items.Count>0)
                    foreach (Scripting.ScriptInfo script in Scripting.ScriptInfo.Items)
                        script.Run(dt_sec);
                //--------------- формирование массивов для передачи в ПЛК ---------------------
                //for (int i = 0; i < AIStruct.items.Length; i++)
                lock (WB.W)
                {
                    if (AIStruct.items != null && AIStruct.items.Count>0)
                    foreach (AIStruct ai in AIStruct.items)
                    {
                        if (ai.En /* || true */)
                        {

                            if (ai.PLCDestType == EPLCDestType.ADC)
                            {

                                    // if (ai.Buffer == BufType.USO)
                                  if (ai.PLCAddr >= Sett.Instance.BegAddrW && ai.PLCAddr < (Sett.Instance.BegAddrW + Sett.Instance.wrBufSize))
                                        WB.W[(ai.PLCAddr - Sett.Instance.BegAddrW)] = ai.ValACD; // записываем значение АЦП в массив для записи CPU

                                    //  if (ai.Buffer == BufType.A3)
                                  if (ai.PLCAddr >= Sett.Instance.iBegAddrA3 && ai.PLCAddr < (Sett.Instance.iBegAddrA3 + Sett.Instance.A3BufSize))
                                        WB.W_a3[(ai.PLCAddr - Sett.Instance.iBegAddrA3)] = ai.ValACD;

                                    //  if (ai.Buffer == BufType.A4)
                                    if (ai.PLCAddr >= Sett.Instance.iBegAddrA4 && ai.PLCAddr < (Sett.Instance.iBegAddrA4 + Sett.Instance.A4BufSize))
                                        WB.W_a4[(ai.PLCAddr - Sett.Instance.iBegAddrA4)] = ai.ValACD;
                            }
                            else
                            if (ai.PLCDestType == EPLCDestType.Float)
                            {
                                //разибиваем float на 2 word'a
                                byte[] bytes = BitConverter.GetBytes(ai.fValAI);
                                ushort w1 = BitConverter.ToUInt16(bytes, 0);
                                ushort w2 = BitConverter.ToUInt16(bytes, 2);

                                    //  if (ai.Buffer == BufType.USO)
                                    if (ai.PLCAddr >= Sett.Instance.BegAddrW && ai.PLCAddr < (Sett.Instance.BegAddrW + Sett.Instance.wrBufSize))
                                    {
                                    WB.W[ai.PLCAddr - Sett.Instance.BegAddrW] = w1;
                                    WB.W[ai.PLCAddr - Sett.Instance.BegAddrW + 1] = w2;
                                }

                                    //  if (ai.Buffer == BufType.A3)
                                    if (ai.PLCAddr >= Sett.Instance.iBegAddrA3 && ai.PLCAddr < (Sett.Instance.iBegAddrA3 + Sett.Instance.A3BufSize))
                                    {
                                    WB.W_a3[ai.PLCAddr - Sett.Instance.iBegAddrA3] = w1;
                                    WB.W_a3[ai.PLCAddr - Sett.Instance.iBegAddrA3 + 1] = w2;
                                }

                                    //   if (ai.Buffer == BufType.A4)
                                    if (ai.PLCAddr >= Sett.Instance.iBegAddrA4 && ai.PLCAddr < (Sett.Instance.iBegAddrA4 + Sett.Instance.A4BufSize))
                                    {
                                    WB.W_a4[ai.PLCAddr - Sett.Instance.iBegAddrA4] = w1;
                                    WB.W_a4[ai.PLCAddr - Sett.Instance.iBegAddrA4 + 1] = w2;
                                }
                            }
                        }//ai.en
                    }//foreach

                    if (DIStruct.items != null && DIStruct.items.Count>0)
                    foreach (DIStruct di in DIStruct.items)
                    {
                        if (di.En)
                        {
                                /*int indx = di.PLCAddr - Sett.Instance.BegAddrW;
                                if (indx > 0 && indx < WB.W.Length)*/

                                /*
                               if (di.Buffer == BufType.USO)
                                   SetBit(ref (WB.W[di.PLCAddr - Sett.Instance.BegAddrW]), (di.indxBitDI), di.ValDI ^ di.InvertDI);
                               else
                               if (di.Buffer == BufType.A3)
                                   SetBit(ref (WB.W_a3[di.PLCAddr - Sett.Instance.iBegAddrA3]), (di.indxBitDI), di.ValDI ^ di.InvertDI);
                               else
                               if (di.Buffer == BufType.A4)
                                   SetBit(ref (WB.W_a4[di.PLCAddr - Sett.Instance.iBegAddrA4]), (di.indxBitDI), di.ValDI ^ di.InvertDI);
                                   */
                                if (di.PLCAddr >= Sett.Instance.BegAddrW && di.PLCAddr < (Sett.Instance.BegAddrW + Sett.Instance.wrBufSize))
                                {
                                    SetBit(ref (WB.W[di.PLCAddr - Sett.Instance.BegAddrW]), (di.indxBitDI), di.ValDI ^ di.InvertDI);
                                    continue;
                                }

                                if (di.PLCAddr >= Sett.Instance.iBegAddrA3 && di.PLCAddr < (Sett.Instance.iBegAddrA3 + Sett.Instance.A3BufSize))
                                {
                                    SetBit(ref (WB.W_a3[di.PLCAddr - Sett.Instance.iBegAddrA3]), (di.indxBitDI), di.ValDI ^ di.InvertDI);
                                }

                                if (di.PLCAddr >= Sett.Instance.iBegAddrA4 && di.PLCAddr < (Sett.Instance.iBegAddrA4 + Sett.Instance.A4BufSize))
                                {
                                    SetBit(ref (WB.W_a4[di.PLCAddr - Sett.Instance.iBegAddrA4]), (di.indxBitDI), di.ValDI ^ di.InvertDI);
                                }

                            }
                    }

                    //записываем счетчики УСО
                    if (CountersTableViewModel.Counters != null && CountersTableViewModel.Counters.Count>0)
                    foreach (USOCounter c in CountersTableViewModel.Counters)
                    {
                        c.Update(dt_sec);
                        if (c.buffer == BufType.USO)
                            if (c.PLCAddr >= Sett.Instance.BegAddrW)
                            {
                                WB.W[c.PLCAddr - Sett.Instance.BegAddrW] = c.Value;
                            }

                        if (c.buffer == BufType.A3)
                            if ((c.PLCAddr >= Sett.Instance.iBegAddrA3) && ((c.PLCAddr - Sett.Instance.iBegAddrA3) < WB.W_a3.Length))
                            {
                                WB.W_a3[c.PLCAddr - Sett.Instance.iBegAddrA3] = c.Value;
                            }
                        if (c.buffer == BufType.A4)
                            if ((c.PLCAddr >= Sett.Instance.iBegAddrA4) && ((c.PLCAddr - Sett.Instance.iBegAddrA4) < WB.W_a4.Length))
                            {
                                WB.W_a4[c.PLCAddr - Sett.Instance.iBegAddrA4] = c.Value;
                            }
                    }

                    /* Таблицу диагностики временно убираем */
                    /*if (DiagTableModel.Instance.DiagRegs != null && DiagTableModel.Instance.DiagRegs.Count>0)
                    foreach (DIStruct di in DiagTableModel.Instance.DiagRegs)
                    {
                        if (di.En)
                        {
                            
                            if (di.Buffer == BufType.USO)
                                SetBit(ref (WB.W[di.PLCAddr - Sett.Instance.BegAddrW]), (di.indxBitDI), di.ValDI ^ di.InvertDI);
                            else
                            if (di.Buffer == BufType.A3)
                                SetBit(ref (WB.W_a3[di.PLCAddr - Sett.Instance.iBegAddrA3]), (di.indxBitDI), di.ValDI ^ di.InvertDI);
                            else
                            if (di.Buffer == BufType.A4)
                                SetBit(ref (WB.W_a4[di.PLCAddr - Sett.Instance.iBegAddrA4]), (di.indxBitDI), di.ValDI ^ di.InvertDI);
                        }
                    }*/

                }//lock

                //-------------  проверка связи только при использовании модбаса -----------------------
                if ((DateTime.Now - readingTime).TotalSeconds > 5f && Sett.Instance.UseModbus)
                {
                    // wrThread.Stop();
                    // btnPause_Click(null, new RoutedEventArgs());
                    this.Dispatcher.Invoke(delegateDisconnected);
                    btnStop_Click(null, new RoutedEventArgs());
                }
               
       

                //---------- вычисление время с момента предыдущей итерации ----------------
                dt_sec = (float)(DateTime.Now - prevCycleTime).TotalSeconds;
                prevCycleTime = DateTime.Now;
                Debug.WriteLine("Main time: " + dt_sec.ToString("F2"));
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
        /// <summary>
        /// Загрузка конфигурации станции из единого xml
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Menu_OpenAll(object sender, RoutedEventArgs e)
        {
            Station station = new Station();

            System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog();
            dialog.Filter = "XML Files (*.xml)|*.xml";
            dialog.FilterIndex = 0;
            dialog.DefaultExt = "xml";

            DITab.DataContext = null;
            AITab.DataContext = null;
            DOTab.DataContext = null;
            AOTab.DataContext = null;

            AIStruct.EnableAutoIndex = false;
            AOStruct.EnableAutoIndex = false;
            DIStruct.EnableAutoIndex = false;
            DOStruct.EnableAutoIndex = false;

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                if (station.Load(dialog.FileName) == StationLoadResult.OK)
                {
                    currentFileName = dialog.FileName;
                    btnSave.IsEnabled = true;
                  //  btnSaveAs.IsEnabled = true;
                //    isConfigLoaded = true;
                    DITab.DataContext = divm;
                    AITab.DataContext = aivm;
                    DOTab.DataContext = dovm;
                    AOTab.DataContext = aovm;

                    AIStruct.EnableAutoIndex = true;
                    AOStruct.EnableAutoIndex = true;
                    DIStruct.EnableAutoIndex = true;
                    DOStruct.EnableAutoIndex = true;

                    WB.InitBuffers();
                    RB.InitBuffer();


               //     dataGridCounters.ItemsSource = CountersTableViewModel.Counters;


                    dataGridVS.DataContext = VSTableViewModel.Instance;
                    dataGridKL.DataContext = KLTableViewModel.Instance;
                    dataGridMPNA.DataContext = MPNATableViewModel.Instance;
                    dataGridZD.DataContext = ZDTableViewModel.Instance;

            //        dataGridDiag.ItemsSource = DiagTableModel.Instance.viewSource.View;

                    dataGridScript.ItemsSource = Scripting.ScriptInfo.Items;

                    MenuItem_showMPNA.IsChecked = Sett.Instance.ShowTab_MPNA;
                    if (!MenuItem_showMPNA.IsChecked)
                        tabMPNA.Visibility = Visibility.Collapsed;
                    else
                        tabMPNA.Visibility = Visibility.Visible;

             /*       MenuItem_showCounters.IsChecked = Sett.Instance.ShowTab_Counter;
                    if (!MenuItem_showCounters.IsChecked)
                        tabDiagUSO.Visibility = Visibility.Collapsed;
                    else
                        tabDiagUSO.Visibility = Visibility.Visible;


                    MenuItem_showDiag.IsChecked = Sett.Instance.ShowTab_Diag;
                    if (!MenuItem_showCounters.IsChecked)
                        tabDiagMod.Visibility = Visibility.Collapsed;
                    else
                        tabDiagMod.Visibility = Visibility.Visible;
*/
                    MenuItem_showKL.IsChecked = Sett.Instance.ShowTab_KL;
                    if (!MenuItem_showKL.IsChecked)
                        tabKL.Visibility = Visibility.Collapsed;
                    else
                        tabKL.Visibility = Visibility.Visible;

                    MenuItem_showVS.IsChecked = Sett.Instance.ShowTab_VS;
                    if (!MenuItem_showVS.IsChecked)
                        tabVS.Visibility = Visibility.Collapsed;
                    else
                        tabVS.Visibility = Visibility.Visible;

                    MenuItem_showZD.IsChecked = Sett.Instance.ShowTab_ZD;
                    if (!MenuItem_showZD.IsChecked)
                        tabZD.Visibility = Visibility.Collapsed;
                    else
                        tabZD.Visibility = Visibility.Visible;
                }
        }
        /// <summary>
        /// сохранение в один xml
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Menu_SaveSingle(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.SaveFileDialog dialog = new System.Windows.Forms.SaveFileDialog
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

                currentFileName = dialog.FileName;
                btnSave.IsEnabled = true;
            }
        }

        /// <summary>
        /// сохранить текущий
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Menu_SaveCurrent(object sender, RoutedEventArgs e)
        {
            if (currentFileName != "")
            {
                Station s = new Station();
                //   s.settings = Sett.Instance;
                s.Save(currentFileName);
                LogViewModel.WriteLine("Файл конфигурации сохранен : "+currentFileName);
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
        OPCThread opcThread;

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
          /*  if (!isConfigLoaded)
            {
                System.Windows.MessageBox.Show("Конфигурация не загружена!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            */

            cancelTokenSrc = new CancellationTokenSource();
            cancellationToken = cancelTokenSrc.Token;
            try
            {
                if (Sett.Instance.UseModbus)
                {
                    //потоки на чтение модбас
                    rdThread = new ReadThread(Sett.Instance.HostName, Sett.Instance.MBPort);
                    rdThread.refMainWindow = this;
                    rdThread.Start();

                    //потоки на запись модбас
                    wrThread = new WritingThread(Sett.Instance.HostName, Sett.Instance.MBPort);
                    wrThread.refMainWindow = this;
                    wrThread.Start();
                }
                readingTime = DateTime.Now;
                for (int i = 0; i < 6; i++)
                    writingTime[i] = DateTime.Now;

                if (Sett.Instance.UseOPC)
                {
                    opcThread = new OPCThread(Sett.Instance.OFSServerPrefix + Sett.Instance.OFSServerName, 50);
                    opcThread.Start();
                  /*  srv = new Opc.Da.Server(new OpcCom.Factory(), new Opc.URL(Sett.Instance.OFSServerPrefix + Sett.Instance.OFSServerName));
                    srv.Connect();
                    if (!srv.IsConnected)
                        throw new Exception("не удалось подключиться к серверу OPC");*/
                }
                //запускаем основной поток
               // if (Sett.Instance.UseModbus || Sett.Instance.UseOPC)
                {
                    watchThread = new Thread(new ThreadStart(Watchdog));
                    watchThread.Start();

                    statusText.Content = "Запущен";
                    statusText.Background = System.Windows.Media.Brushes.Green;
                    btnStart.IsEnabled = false;

                    btnStop.IsEnabled = true;

                    LogViewModel.WriteLine("Симулятор запущен");
            
                }
            //    else
           //         throw new Exception("Нужно выбрать хотябы один драйвер MBE или OPC");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Ошибка соединения: " + Environment.NewLine + ex.Message, "Ошибка");
            }
        }

        #region WriteReadCycleEnd
        //------------ вызывается каждую итерацию цикла записи ----------------
        private void On_WritingCycleEnd()
        {
            TimeSpan ts = DateTime.Now - writingTime[0];
            StatusW1.Content = ts.TotalSeconds.ToString("F2") + " | ";
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

        private void On_ReadingCycleEnd()
        {
            TimeSpan ts = DateTime.Now - readingTime;
            StatusR.Content = "Время чтения: " + ts.TotalSeconds.ToString("F2");
            readingTime = DateTime.Now;
        }
        #endregion
        //--------------------------------------------------------------------

        private void On_Disconnected()
        {
            btnStop_Click(null, null);
            System.Windows.MessageBox.Show("Соединение разорвано!");
        }

        //---------------------------- 


        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            LogViewModel.WriteLine("Симулятор остановлен");
            statusText.Content = "Остановлен";
            statusText.Background = System.Windows.Media.Brushes.Yellow;

            btnStart.IsEnabled = true;
            btnStop.IsEnabled = false;
         

            if (wrThread!=null) 
                wrThread.Stop();
            wrThread = null;

            if (rdThread!=null)
                rdThread.Stop();

            rdThread = null;

            if (opcThread != null)
            {
                opcThread.Stop();
            }
            opcThread = null;
            
            if (watchThread != null)
                watchThread.Abort();


            try
            {
                cancelTokenSrc.Cancel();
                // wrThread = null;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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

                if (opcThread != null)
                    opcThread.Stop();

                if (logWindow != null)
                    logWindow.Close();
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
        private void OnCounterTableChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            
        }

/*
        void SetConfigMode(bool e)
        {
            if (e)
            {
                DataGridAI_Addr.Visibility = Visibility.Visible;
        //        DataGridAI_Buf.Visibility = Visibility.Visible;
                DataGridAI_On.Visibility = Visibility.Visible;
                DataGridAI_Type.Visibility = Visibility.Visible;

              
                tabDiagMod.Visibility = Visibility.Visible;
                tabDiagUSO.Visibility = Visibility.Visible;

                dataGridDI_Addr.Visibility = Visibility.Visible;
                dataGridDI_Bit.Visibility = Visibility.Visible;
             //   dataGridDI_Buf.Visibility = Visibility.Visible;
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

                dataGridDiag_Addr.Visibility = Visibility.Visible;
                dataGridDiag_Bit.Visibility = Visibility.Visible;
            }
            else
            {
                DataGridAI_Addr.Visibility = Visibility.Hidden;
         //       DataGridAI_Buf.Visibility = Visibility.Hidden;
                DataGridAI_On.Visibility = Visibility.Hidden;
                DataGridAI_Type.Visibility = Visibility.Hidden;


                dataGridZD.CanUserAddRows = false;
                dataGridZD.CanUserDeleteRows = false;

                dataGridKL.CanUserAddRows = false;
                dataGridKL.CanUserDeleteRows = false;

               

             //   tabSettings.Visibility = Visibility.Collapsed;
                tabDiagMod.Visibility = Visibility.Collapsed;
                tabDiagUSO.Visibility = Visibility.Collapsed;

                dataGridDI_Addr.Visibility = Visibility.Hidden;
                dataGridDI_Bit.Visibility = Visibility.Hidden;
          //      dataGridDI_Buf.Visibility = Visibility.Hidden;
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

      */

        private void textBoxDiagFilter_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
           //     DiagTableModel.Instance.nameFilter = textBoxDiagFilter.Text;
           //     DiagTableModel.Instance.ApplyFilter();
            }
        }

        private void MenuItem_toggleMPNA(object sender, RoutedEventArgs e)
        {
            MenuItem_showMPNA.IsChecked = !MenuItem_showMPNA.IsChecked;
            Sett.Instance.ShowTab_MPNA = MenuItem_showMPNA.IsChecked;
            if (!MenuItem_showMPNA.IsChecked)
                tabMPNA.Visibility = Visibility.Collapsed;
            else
                tabMPNA.Visibility = Visibility.Visible;
        }

        private void MenuItem_toggleKL(object sender, RoutedEventArgs e)
        {
            MenuItem_showKL.IsChecked = !MenuItem_showKL.IsChecked;

            Sett.Instance.ShowTab_KL = MenuItem_showKL.IsChecked;
            if (!MenuItem_showKL.IsChecked)
                tabKL.Visibility = Visibility.Collapsed;
            else
                tabKL.Visibility = Visibility.Visible;
        }

        private void MenuItem_toggleVS(object sender, RoutedEventArgs e)
        {
            MenuItem_showVS.IsChecked = !MenuItem_showVS.IsChecked;

            Sett.Instance.ShowTab_VS = MenuItem_showKL.IsChecked;
            if (!MenuItem_showVS.IsChecked)
                tabVS.Visibility = Visibility.Collapsed;
            else
                tabVS.Visibility = Visibility.Visible;
        }
/*
        private void MenuItem_toggleCounters(object sender, RoutedEventArgs e)
        {
            MenuItem_showCounters.IsChecked = !MenuItem_showCounters.IsChecked;

            Sett.Instance.ShowTab_Counter = MenuItem_showCounters.IsChecked;

            if (!MenuItem_showCounters.IsChecked)
                tabDiagUSO.Visibility = Visibility.Collapsed;
            else
                tabDiagUSO.Visibility = Visibility.Visible;
        }

        private void MenuItem_toggleDiags(object sender, RoutedEventArgs e)
        {
            MenuItem_showDiag.IsChecked = !MenuItem_showDiag.IsChecked;

            Sett.Instance.ShowTab_Diag = MenuItem_showDiag.IsChecked;
            if (!MenuItem_showDiag.IsChecked)
                tabDiagMod.Visibility = Visibility.Collapsed;
            else
                tabDiagUSO.Visibility = Visibility.Visible;
        }
        */
        private void MenuItem_toggleZD(object sender, RoutedEventArgs e)
        {
            MenuItem_showZD.IsChecked = !MenuItem_showZD.IsChecked;

            Sett.Instance.ShowTab_ZD = MenuItem_showZD.IsChecked;
            if (!MenuItem_showZD.IsChecked)
                tabZD.Visibility = Visibility.Collapsed;
            else
                tabZD.Visibility = Visibility.Visible;
        }


        private void ScriptMenu_EditClick(object sender, RoutedEventArgs e)
        {
            ScriptInfo script = dataGridScript.SelectedItem as Scripting.ScriptInfo;

            if (script != null)
            {
                ScriptEditor editor = new ScriptEditor(script);
                editor.ShowDialog();
            }
           
        }

        private void ScriptMenu_RunClick(object sender, RoutedEventArgs e)
        {
            ScriptInfo script = dataGridScript.SelectedItem as Scripting.ScriptInfo;

            if (script != null)
            {
                script.Prepare();
                script.Run(0,true);
            }
        }

        //новая конфигурация
        private void Menu_New(object sender, RoutedEventArgs e)
        {
            ZDTableViewModel.ZDs.Clear();
            VSTableViewModel.VS.Clear();
            KLTableViewModel.KL.Clear();
            MPNATableViewModel.MPNAs.Clear();

            ScriptInfo.Items.Clear();

            DIStruct.items.Clear();
            DOStruct.items.Clear();
            AIStruct.items.Clear();
            AOStruct.items.Clear();

            CountersTableViewModel.Counters.Clear();
            DiagTableModel.Instance.DiagRegs.Clear();

            currentFileName = "";
            btnSave.IsEnabled = false;
          //  btnSaveAs.IsEnabled = false;

            LogViewModel.WriteLine("Новая конфигурация создана");
        }

        private void Menu_Export(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog();
            sfd.Filter = "текстовый файл с разделителями (.csv)|*.csv";
            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
               // Simulator_MPSA.CL.IO.StationExporter exporter = new CL.IO.StationExporter();
                CSVWorker.exportCSV(Station.instance, sfd.FileName);
            }
        }

        private void Menu_Import(object sender, RoutedEventArgs e)
        {
            DIStruct.EnableAutoIndex = false;
            DOStruct.EnableAutoIndex = false;
            AOStruct.EnableAutoIndex = false;
            AIStruct.EnableAutoIndex = false;

            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
            ofd.Filter = "текстовый файл с разделителями (.csv)|*.csv";
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                CSVWorker.importCSV(ofd.FileName);
            }

            DIStruct.EnableAutoIndex = true;
            DOStruct.EnableAutoIndex = true;
            AOStruct.EnableAutoIndex = true;
            AIStruct.EnableAutoIndex = true;

        }

        StationSetup setupWindow;
        private string currentFileName="";

        private void MenuItem_Sim_Click(object sender, RoutedEventArgs e)
        {
            setupWindow = new StationSetup();
            setupWindow.ShowDialog();
            setupWindow.Closed += SetupWindow_Closed;
 
        }

        private void SetupWindow_Closed(object sender, EventArgs e)
        {
            if (setupWindow.accepted)
            {
                RB.InitBuffer();
                WB.InitBuffers();
            }

        }

        /// <summary>
        /// Поверх всех окон вкл
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem_Checked(object sender, RoutedEventArgs e)
        {
            this.Topmost = true;
        }

        /// <summary>
        /// поверх всех окон выкл
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem_Unchecked(object sender, RoutedEventArgs e)
        {
            this.Topmost = false;
        }
        Log logWindow;
        private void btnLog_Click(object sender, RoutedEventArgs e)
        {

            if (logWindow != null)
                logWindow.Close();

            logWindow = new Log();

            logWindow.Show();
        }

        private void ButtonHelp_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("SimHelp.chm");
        }
    }
}
