﻿using System.Collections.Specialized; //xml
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Diagnostics; // for DEBUG 
using System.IO;
using System.Data;
using Simulator_MPSA.CL;
using System.ComponentModel;
using Simulator_MPSA.Scripting;
using Microsoft.Win32;
using Simulator_MPSA.ViewModel;
using Simulator_MPSA.CL.Signal;
using Simulator_MPSA.CL.Commands;

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

    /// <summary>
    /// Буфер чтения из памяти устройства по Modbus
    /// </summary>
    public static class RB
    {
        public static ushort[] R;// = new ushort[(Sett.iNRackEnd) * 50];//[(29 - 3 + 1) * 50]    =1450   From IOScaner CPU
        public static void InitBuffer()
        {
            RB.R = new ushort[Sett.Instance.rdBufSize];//[(29 - 3 + 1) * 50]    =1450   From IOScaner CPU
            //LogViewModel.WriteLine("Размер буфера чтения обновлен: "+RB.R.Count().ToString() +" рег.");
            LogWriter.AppendLog("Размер буфера чтения обновлен: " + RB.R.Count().ToString() + " рег.");
        }
    }
    /// <summary>
    /// Буфер записи в ПЛК по протоколу Modbus
    /// </summary>
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

            WB.W_a4 = new ushort[Sett.Instance.A4BufSize];
            WB.W_a4_prev = new ushort[WB.W_a4.Length];

            // LogViewModel.WriteLine("Размеры буферов записи обновлены: " + WB.W.Count().ToString() +"/"+WB.W_a3.Count().ToString()+"/"+WB.W_a4.Count().ToString()+ " рег.");
            LogWriter.AppendLog("Размеры буферов записи обновлены: " + WB.W.Count().ToString() + "/" + WB.W_a3.Count().ToString() + "/" + WB.W_a4.Count().ToString() + " рег.");
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
    /// Класс позволяет потокобезопасно выводить информацию в журнал сообщений
    /// </summary>
    static class LogWriter
    {
        delegate void InvokeDelegate(string text);

        static void DelegateMethod(string line)
        {
            refMainWindow.log.AppendText(now.Hour.ToString() + ":" + now.Minute.ToString() + ":" + now.Second.ToString() + ":  " + line + Environment.NewLine);
        }
        public static MainWindow refMainWindow;
        static DateTime now;
        public static void AppendLog(string line)
        {
            now = DateTime.Now;
            if (refMainWindow != null)
            {

                refMainWindow.Dispatcher.BeginInvoke(new InvokeDelegate(DelegateMethod), line);
            }
        }
    }


    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
      //  Opc.Da.Server srv;

        public delegate void DEndWrite(int nJob);
        public DEndWrite EndCycle;
        /*public DEndWrite EndCycle2;
        public DEndWrite EndCycle3;
        public DEndWrite EndCycle4;
        public DEndWrite EndCycle5;
        public DEndWrite EndCycle6;*/
        public DWriteLog WriteLog;

        public delegate void DDisconnected();
        public DDisconnected delegateDisconnected;

        public delegate void OFSFail();
        public OFSFail ofsFail;

        public delegate void DEndRead();
        public DEndRead delegateEndRead;

        public delegate void DWriteLog(string text);
        public DWriteLog delgateWriteLog;

        MainViewModel mainVM = new MainViewModel();

        /// <summary>
        /// DataContext для таблицы DI
        /// </summary>
        ViewModelCollection<DIViewModel, DIStruct> divm;
        ViewModelCollection<DOViewModel, DOStruct> dovm;
        ViewModelCollection<AIViewModel, AIStruct> aivm;
        ViewModelCollection<AOViewModel, AOStruct> aovm;

        //  WatchTableViewModel watchViewModel;
        ViewModelCollection<WatchItemViewModel, WatchItem> watchvm;

        //
        //List<Opc.Da.ItemValue> opcitems = new List<Opc.Da.ItemValue>();
        //Opc.Da.ItemValue itm;

        public MainWindow()
        {

            InitializeComponent();

            DataContext = mainVM;

            ScriptInfo.refMainWindow = this;

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

            //  watchViewModel = new WatchTableViewModel();
            watchvm = new ViewModelCollection<WatchItemViewModel, WatchItem>(WatchItem.Items);
            dataGridWatch.DataContext = watchvm;

            ScriptTableViewModel.Init();
            dataGridScript.ItemsSource = ScriptTableViewModel.Items;

            statusText.Content = "Остановлен";
            statusText.Background = System.Windows.Media.Brushes.Yellow;

            // --------------------- ПРИВЯЗКА КОМАНД  ----------------------------------
            MenuItem_about.Command = new ShowAboutCommand();
            menu_import_xlsx.Command = new ExcelImportCommand();

            EndCycle += new DEndWrite((int njob) => {
                TimeSpan ts = DateTime.Now - writingTime[njob];

                switch (njob)
                {
                    case 0: StatusW1.Content = ts.TotalSeconds.ToString("F2") + " | "; break;
                    case 1: StatusW2.Content = ts.TotalSeconds.ToString("F2") + " | "; break;
                    case 2: StatusW3.Content = ts.TotalSeconds.ToString("F2") + " | "; break;
                    case 3: StatusW4.Content = ts.TotalSeconds.ToString("F2") + " | "; break;
                    case 4: StatusW5.Content = ts.TotalSeconds.ToString("F2") + " | "; break;
                    case 5: StatusW6.Content = ts.TotalSeconds.ToString("F2") + " | "; break;
                    case 6: StatusW7.Content = ts.TotalSeconds.ToString("F2") + " | "; break;
                    case 7: StatusW8.Content = ts.TotalSeconds.ToString("F2") + " | "; break;
                    case 8: StatusW9.Content = ts.TotalSeconds.ToString("F2") + " | "; break;

                }


                writingTime[njob] = DateTime.Now;
            });


            ofsFail += new OFSFail(() =>
            {
                BtnStop_Click(null, null);
                System.Windows.MessageBox.Show("Ошибка соединения по протоколу OPC!");
            }

            );
            WriteLog += new DWriteLog((text) => { LogWriter.AppendLog(text); });



            delegateDisconnected += new DDisconnected(() => {
                BtnStop_Click(null, null);
                System.Windows.MessageBox.Show("Ошибка соединения по протоколу Modbus TCP!");
            });
            delegateEndRead += new DEndRead(() =>
            {
                TimeSpan ts = DateTime.Now - readingTime;
                StatusR.Content = "Время чтения: " + ts.TotalSeconds.ToString("F2");
                readingTime = DateTime.Now;
            });

            LogWriter.refMainWindow = this;


            if (Properties.Settings.Default.LastFileName != "" && System.IO.File.Exists(Properties.Settings.Default.LastFileName))
            {
                if (System.Windows.MessageBox.Show("Открыть последний файл конфигурации?" + Environment.NewLine + Properties.Settings.Default.LastFileName, "Вопрос", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    OpenConfig(Properties.Settings.Default.LastFileName);
                    currentFileName = Properties.Settings.Default.LastFileName;
                    label_filename.Content = Properties.Settings.Default.LastFileName;
                }
            }

            //по умолчанию отключить колонки с адресами
            MenuItem_toggleAddr(this, null);

            menuImportDI.Command = new ImportDICommand();
            menuImportAI.Command = new ImportAICommand();

            menuImportDO.Command = new ImportDOCommand();
            menuImportAO.Command = new ImportAOCommand();

            menuExportDI.Command = new ExportDICommand();
            menuExportDO.Command = new ExportDOCommand();
            menuExportAI.Command = new ExportAICommand();
            menuExportAO.Command = new ExportAOCommand();
        }


        /// <summary>
        /// время последней записи в ПЛК
        /// </summary>
        DateTime[] writingTime = new DateTime[9];

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
                if (ZDTableViewModel.ZDs != null && ZDTableViewModel.ZDs.Count > 0)
                    foreach (ZDStruct zd in ZDTableViewModel.ZDs)
                        zd.UpdateZD(dt_sec);

                if (KLTableViewModel.KL != null && KLTableViewModel.KL.Count > 0)
                    foreach (KLStruct kl in KLTableViewModel.KL)
                        kl.UpdateKL(dt_sec);

                if (VSTableViewModel.VS != null && VSTableViewModel.VS.Count > 0)
                    foreach (VSStruct vs in VSTableViewModel.VS)
                        vs.UpdateVS(dt_sec);

                if (MPNATableViewModel.MPNAs != null && MPNATableViewModel.MPNAs.Count > 0)
                    foreach (MPNAStruct mpna in MPNATableViewModel.MPNAs)
                        mpna.UpdateMPNA(dt_sec);



                if (ScriptTableViewModel.Items != null && ScriptTableViewModel.Items.Count > 0)
                    foreach (ScriptInfo script in ScriptTableViewModel.Items)
                        script.Run(dt_sec);
                //--------------- формирование массивов для передачи в ПЛК ---------------------
                //for (int i = 0; i < AIStruct.items.Length; i++)
                lock (WB.W)
                {
                    if (AIStruct.items != null && AIStruct.items.Count > 0)
                        foreach (AIStruct ai in AIStruct.items)
                        {
                            //записываем в буфер только если тег opc пуст, в противном случае сигнал будет записан по протоколу OPC
                            if (ai.En && !(ai.OPCtag!="" && Sett.Instance.UseOPC==true))
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

                    if (DIStruct.items != null && DIStruct.items.Count > 0)
                        foreach (DIStruct di in DIStruct.items)
                        {
                            //записываем в буфер только если тег opc пуст, в противном случае сигнал будет записан по протоколу OPC
                            if (di.En && !(di.OPCtag != "" && Sett.Instance.UseOPC == true))
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
                                    continue;
                                }

                                if (di.PLCAddr >= Sett.Instance.iBegAddrA4 && di.PLCAddr < (Sett.Instance.iBegAddrA4 + Sett.Instance.A4BufSize))
                                {
                                    SetBit(ref (WB.W_a4[di.PLCAddr - Sett.Instance.iBegAddrA4]), (di.indxBitDI), di.ValDI ^ di.InvertDI);
                                    continue;
                                }

                            }
                        }

                    //записываем счетчики УСО
                    if (CountersTableViewModel.Counters != null && CountersTableViewModel.Counters.Count > 0)
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
                    if (DiagTableModel.Instance.DiagRegs != null && DiagTableModel.Instance.DiagRegs.Count > 0)
                        foreach (DIStruct di in DiagTableModel.Instance.DiagRegs)
                        {
                            if (di.En)
                            {

                                if (di.PLCAddr >= Sett.Instance.BegAddrW && di.PLCAddr < (Sett.Instance.BegAddrW + Sett.Instance.wrBufSize))
                                {
                                    SetBit(ref (WB.W[di.PLCAddr - Sett.Instance.BegAddrW]), (di.indxBitDI), di.ValDI ^ di.InvertDI);
                                    continue;
                                }

                                if (di.PLCAddr >= Sett.Instance.iBegAddrA3 && di.PLCAddr < (Sett.Instance.iBegAddrA3 + Sett.Instance.A3BufSize))
                                {
                                    SetBit(ref (WB.W_a3[di.PLCAddr - Sett.Instance.iBegAddrA3]), (di.indxBitDI), di.ValDI ^ di.InvertDI);
                                    continue;
                                }

                                if (di.PLCAddr >= Sett.Instance.iBegAddrA4 && di.PLCAddr < (Sett.Instance.iBegAddrA4 + Sett.Instance.A4BufSize))
                                {
                                    SetBit(ref (WB.W_a4[di.PLCAddr - Sett.Instance.iBegAddrA4]), (di.indxBitDI), di.ValDI ^ di.InvertDI);
                                    continue;
                                }
                            }
                        }

                }//lock

                //-------------  проверка связи только при использовании модбаса -----------------------
                if ((DateTime.Now - readingTime).TotalSeconds > 5f && Sett.Instance.UseModbus)
                {
                    // wrThread.Stop();
                    // btnPause_Click(null, new RoutedEventArgs());
                    this.Dispatcher.Invoke(delegateDisconnected);
                    BtnStop_Click(null, new RoutedEventArgs());
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

       // bool isConfigLoaded = false;


        /// <summary>
        /// Загрузка конфигурации станции из единого xml по запросу пользователя
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Menu_OpenAll(object sender, RoutedEventArgs e)
        {


            System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog
            {
                Filter = "XML Files (*.xml)|*.xml",
                FilterIndex = 0,
                DefaultExt = "xml"
            };
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                OpenConfig(dialog.FileName);
            }


        }

        /// <summary>
        /// Открытие файла xml конфига
        /// </summary>
        /// <param name="filename"></param>
        private void OpenConfig(string filename)
        {
            Station station = new Station();

            DITab.DataContext = null;
            AITab.DataContext = null;
            DOTab.DataContext = null;
            AOTab.DataContext = null;

            AIStruct.EnableAutoIndex = false;
            AOStruct.EnableAutoIndex = false;
            DIStruct.EnableAutoIndex = false;
            DOStruct.EnableAutoIndex = false;

            if (station.Load(filename) == StationLoadResult.OK)
            {
                currentFileName = filename;
                label_filename.Content = currentFileName;
                //   label_confVersion.Content = "Версия файла: " + station.Version;
                btnSave.IsEnabled = true;

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

                dataGridVS.DataContext = VSTableViewModel.Instance;
                dataGridKL.DataContext = KLTableViewModel.Instance;
                dataGridMPNA.DataContext = MPNATableViewModel.Instance;
                dataGridZD.DataContext = ZDTableViewModel.Instance;

                dataGridScript.ItemsSource = ScriptTableViewModel.Items;

                MenuItem_showMPNA.IsChecked = Sett.Instance.ShowTab_MPNA;
                if (!MenuItem_showMPNA.IsChecked)
                    tabMPNA.Visibility = Visibility.Collapsed;
                else
                    tabMPNA.Visibility = Visibility.Visible;

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

                LogWriter.AppendLog("Конфигурация загружена");
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
                Station s = new Station
                {
                    settings = Sett.Instance
                };
                s.Save(dialog.FileName);

                currentFileName = dialog.FileName;
                label_filename.Content = currentFileName;
                //    label_confVersion.Content = "Версия файла: " + s.Version.ToString();
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
                //  label_confVersion.Content = "Версия файла: " + s.Version.ToString();
                //LogViewModel.WriteLine("Файл конфигурации сохранен : "+currentFileName);
                // log.AppendText("Файл конфигурации сохранен : " + currentFileName + Environment.NewLine);
                LogWriter.AppendLog("Файл конфигурации сохранен : " + currentFileName + Environment.NewLine);
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

        private void BtnStart_Click(object sender, RoutedEventArgs e)
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
                    rdThread = new ReadThread(Properties.Settings.Default.ModbusIPAddr, 502)
                    {
                        //  rdThread = new ReadThread(Sett.Instance.HostName, Sett.Instance.MBPort);
                        refMainWindow = this
                    };
                    rdThread.Start();

                    //потоки на запись модбас
                    wrThread = new WritingThread(Properties.Settings.Default.ModbusIPAddr, 502)
                    {

                        // wrThread = new WritingThread(Sett.Instance.HostName, Sett.Instance.MBPort);
                        refMainWindow = this
                    };
                    wrThread.Start();
                }
                readingTime = DateTime.Now;
                for (int i = 0; i < 6; i++)
                    writingTime[i] = DateTime.Now;

                if (Sett.Instance.UseOPC)
                {
                    opcThread = new OPCThread(Properties.Settings.Default.OPCAddr + Properties.Settings.Default.OPCServerName, 50)
                    {
                        //  opcThread = new OPCThread(Sett.Instance.OFSServerPrefix + Sett.Instance.OFSServerName, 50);
                        refMainWindow = this
                    };
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
                    statusText.Background = System.Windows.Media.Brushes.LightGreen;
                    btnStart.IsEnabled = false;

                    btnStop.IsEnabled = true;

                    // LogViewModel.WriteLine("Симулятор запущен");
                    //  log.AppendText("Симулятор запущен; " + "Modbus: " + (Sett.Instance.UseModbus ? "ВКЛ" : "ВЫКЛ") + "; OPC: " + (Sett.Instance.UseOPC ? "ВКЛ" : "ВЫКЛ") + Environment.NewLine);
                    LogWriter.AppendLog("Симулятор запущен; " + "Modbus: " + (Sett.Instance.UseModbus ? "ВКЛ" : "ВЫКЛ") + "; OPC: " + (Sett.Instance.UseOPC ? "ВКЛ" : "ВЫКЛ") + Environment.NewLine);
                    IsSimulationActive = true;
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
        /*   private void On_WritingCycleEnd()
           {
               TimeSpan ts = DateTime.Now - writingTime[0];
               StatusW1.Content = ts.TotalSeconds.ToString("F2") + " | ";
               writingTime[0] = DateTime.Now;
           }

           private void On_WritingCycle2End()
           {
               TimeSpan ts = DateTime.Now - writingTime[1];

               StatusW2.Content = ts.TotalSeconds.ToString("F2") + " | ";
               writingTime[1] = DateTime.Now;
           }

           private void On_WritingCycle3End()
           {
               TimeSpan ts = DateTime.Now - writingTime[2];

               StatusW3.Content = ts.TotalSeconds.ToString("F2") + " | ";
               writingTime[2] = DateTime.Now;
           }

           private void On_WritingCycle4End()
           {
               TimeSpan ts = DateTime.Now - writingTime[3];

               StatusW4.Content = ts.TotalSeconds.ToString("F2") + " | ";
               writingTime[3] = DateTime.Now;
           }

           private void On_WritingCycle5End()
           {
               TimeSpan ts = DateTime.Now - writingTime[4];
               StatusW5.Content = ts.TotalSeconds.ToString("F2");

               writingTime[4] = DateTime.Now;
           }

           private void On_WritingCycle6End()
           {
               TimeSpan ts = DateTime.Now - writingTime[5];
               StatusW6.Content = ts.TotalSeconds.ToString("F2");

               writingTime[6] = DateTime.Now;
           }

           private void On_ReadingCycleEnd()
           {
               TimeSpan ts = DateTime.Now - readingTime;
               StatusR.Content = "Время чтения: " + ts.TotalSeconds.ToString("F2");
               readingTime = DateTime.Now;
           }*/
        #endregion

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            // LogViewModel.WriteLine("Симулятор остановлен");
            //log.AppendText("Симулятор остановлен" + Environment.NewLine);
            LogWriter.AppendLog("Симулятор остановлен" + Environment.NewLine);
            statusText.Content = "Остановлен";
            statusText.Background = System.Windows.Media.Brushes.Yellow;

            btnStart.IsEnabled = true;
            btnStop.IsEnabled = false;


            if (wrThread != null)
                wrThread.Stop();
            wrThread = null;

            if (rdThread != null)
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


            IsSimulationActive = false;
        }


        SetupDialog dialog;


        private void DataGridMPNA_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if ((dialog != null) && (dialog.IsLoaded))
            {
                dialog.Activate();
            }
            else
            {
                if (dataGridMPNA.SelectedItem is MPNAStruct temp)
                {
                    dialog = new SetupDialog(temp);
                    dialog.Show();
                }
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

                if (opcThread != null)
                    opcThread.Stop();


                if (editor != null)
                    editor.Close();

                // string subkey = @"software\NA\Simulator";

                //  RegistryKey configKey = Registry.CurrentUser.CreateSubKey(subkey);
                //  string lastFilename = (string)configKey.GetValue("last file", "");
                // configKey.SetValue("last file", currentFileName);
                Properties.Settings.Default.LastFileName = currentFileName;
                Properties.Settings.Default.Save();
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
        private void VSMenu_toggle_BS_Click(object sender, RoutedEventArgs e)
        {
            (dataGridVS.SelectedItem as VSStruct).ToggleBusState();
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
                if (dataGridVS.SelectedItem is VSStruct temp)
                {
                    dialog = new SetupDialog(temp);
                    dialog.Show();
                }
            }
        }
        private void DataGridVS_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            VSMenu_settings_Click(sender, e);
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


        private void ZDMenu_mufta_Click(object sender, RoutedEventArgs e)
        {
            (dataGridZD.SelectedItem as ZDStruct).ToggleMufta();
        }
        private void ZDMenu_BS_Click(object sender, RoutedEventArgs e)
        {
            (dataGridZD.SelectedItem as ZDStruct).ToggleBS();
        }


        private void ZDMenu_settings_Click(object sender, RoutedEventArgs e)
        {
            if ((dialog != null) && (dialog.IsLoaded))
            {
                dialog.Activate();
            }
            else
            {
                if (dataGridZD.SelectedItem is ZDStruct temp)
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
                if (dataGridKL.SelectedItem is KLStruct temp)
                {
                    dialog = new SetupDialog(temp);
                    dialog.Show();
                }
            }
        }

        private void MPNAMenu_settings_Click(object sender, RoutedEventArgs e)
        {
            if ((dialog != null) && (dialog.IsLoaded))
            {
                dialog.Activate();
                //         dialog.Close();
            }
            else
            {
                if (dataGridMPNA.SelectedItem is MPNAStruct temp)
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


        private void TextBoxDiagFilter_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
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
        /// <summary>
        /// отображать колонки с адресами
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem_toggleAddr(object sender, RoutedEventArgs e)
        {
            /*    MenuItem_showAddr.IsChecked = !MenuItem_showAddr.IsChecked;
                Visibility visibility = MenuItem_showAddr.IsChecked ? Visibility.Visible : Visibility.Collapsed;
                DataGridAI_Addr.Visibility = visibility;
                DataGridAI_OPC.Visibility = visibility;
                DataGridAI_Type.Visibility = visibility;

                dataGridDI_Addr.Visibility = visibility;
                dataGridDI_Bit.Visibility = visibility;
                dataGridDI_OPC.Visibility = visibility;

                dataGridDO_Addr.Visibility = visibility;
                dataGridDO_Bit.Visibility = visibility;
                dataGridDO_OPC.Visibility = visibility;

                DataGridAO_Addr.Visibility = visibility;
                DataGridAO_OPC.Visibility = visibility;*/

        }

        private void MenuItem_toggleZD(object sender, RoutedEventArgs e)
        {
            MenuItem_showZD.IsChecked = !MenuItem_showZD.IsChecked;

            Sett.Instance.ShowTab_ZD = MenuItem_showZD.IsChecked;
            if (!MenuItem_showZD.IsChecked)
                tabZD.Visibility = Visibility.Collapsed;
            else
                tabZD.Visibility = Visibility.Visible;
        }

        ScriptEditor editor;
        private void ScriptMenu_EditClick(object sender, RoutedEventArgs e)
        {


            if (dataGridScript.SelectedItem is Scripting.ScriptInfo script)
            {
                if (editor != null)
                    editor.Close();

                editor = new ScriptEditor(script);

                editor.Show();
            }

        }

        private void ScriptMenu_RunClick(object sender, RoutedEventArgs e)
        {

            if (dataGridScript.SelectedItem is Scripting.ScriptInfo script)
            {

                script.Prepare();
                script.En = true;
                //    script.Run(0, true);
            }
        }
        private void ScriptMenu_RunOnceClick(object sender, RoutedEventArgs e)
        {

            if (dataGridScript.SelectedItem is Scripting.ScriptInfo script)
            {
                script.Prepare();
                script.Run(0, true);
            }
        }

        //новая конфигурация
        private void Menu_New(object sender, RoutedEventArgs e)
        {
            ZDTableViewModel.ZDs.Clear();
            VSTableViewModel.VS.Clear();
            KLTableViewModel.KL.Clear();
            MPNATableViewModel.MPNAs.Clear();

            ScriptTableViewModel.Items.Clear();

            DIStruct.items.Clear();
            DOStruct.items.Clear();
            AIStruct.items.Clear();
            AOStruct.items.Clear();

            CountersTableViewModel.Counters.Clear();
            DiagTableModel.Instance.DiagRegs.Clear();

            Station s = new Station
            {
                Version = 0
            };
            s = null;

            currentFileName = "";
            btnSave.IsEnabled = false;
            //  btnSaveAs.IsEnabled = false;

            label_filename.Content = "";
            //LogViewModel.WriteLine("Новая конфигурация создана");
            //log.AppendText("Новая конфигурация создана" + Environment.NewLine);
            LogWriter.AppendLog("Новая конфигурация создана" + Environment.NewLine);
        }

        private void Menu_Export(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog
            {
                Filter = "текстовый файл с разделителями (.csv)|*.csv"
            };
            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                // Simulator_MPSA.CL.IO.StationExporter exporter = new CL.IO.StationExporter();
                CSVWorker.ExportCSV(Station.Instance, sfd.FileName);
            }
        }

        StationSetup setupWindow;
        private string currentFileName = "";

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

        private void ButtonHelp_Click(object sender, RoutedEventArgs e)
        {
            if (Process.GetProcessesByName("hh").Length == 0)
                Process.Start("SimHelp.chm");
        }


        #region ADD_BUTTONS
        private void Add_AI_Click(object sender, RoutedEventArgs e)
        {
            AIStruct.items.Add(new AIStruct());
        }

        private void Add_AO_Click(object sender, RoutedEventArgs e)
        {
            AOStruct.items.Add(new AOStruct());
        }

        private void Add_DI_Click(object sender, RoutedEventArgs e)
        {
            DIStruct.items.Add(new DIStruct());
        }

        private void Add_DO_Click(object sender, RoutedEventArgs e)
        {
            DOStruct.items.Add(new DOStruct());
        }

        private void Add_ZD_Click(object sender, RoutedEventArgs e)
        {
            ZDTableViewModel.ZDs.Add(new ZDStruct());
        }

        private void Add_KL_Click(object sender, RoutedEventArgs e)
        {
            KLTableViewModel.KL.Add(new KLStruct());
        }

        private void Add_Script_Click(object sender, RoutedEventArgs e)
        {
            ScriptTableViewModel.Items.Add(new ScriptInfo());
        }

        private void Add_VS_Click(object sender, RoutedEventArgs e)
        {
            VSTableViewModel.VS.Add(new VSStruct());
        }

        private void Add_MPNA_Click(object sender, RoutedEventArgs e)
        {
            MPNATableViewModel.MPNAs.Add(new MPNAStruct());
        }
        #endregion
        #region ПРОВЕРКИ
        /// <summary>
        /// проверка
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnCheck_Click(object sender, RoutedEventArgs e)
        {
            string report = "";

            if (DIStruct.items != null && DIStruct.items.Count > 0)
                for (int i = 0; i < DIStruct.items.Count(); i++)
                {
                    if (DIStruct.items[i].indxArrDI != i)
                    {
                        report += "Индексы дискретных сигналов таблицы DI->PLC не последовательны, начиная с элемента: " + i.ToString() + Environment.NewLine;
                        break;
                    }
                }

            if (DOStruct.items != null && DOStruct.items.Count > 0)
                for (int i = 0; i < DOStruct.items.Count(); i++)
                {
                    if (DOStruct.items[i].indxArrDO != i)
                    {
                        report += "Индексы дискретных сигналов таблицы PLC->DO не последовательны, начиная с элемента: " + i.ToString() + Environment.NewLine;
                        break;
                    }
                }

            if (AOStruct.items != null && AOStruct.items.Count > 0)
                for (int i = 0; i < AOStruct.items.Count(); i++)
                {
                    if (AOStruct.items[i].indx != i)
                    {
                        report += "Индексы дискретных сигналов таблицы AO->PLC не последовательны, начиная с элемента: " + i.ToString() + Environment.NewLine;
                        break;
                    }
                }

            if (AIStruct.items != null && AIStruct.items.Count > 0)
                for (int i = 0; i < AIStruct.items.Count(); i++)
                {
                    if (AIStruct.items[i].indxAI != i)
                    {
                        report += "Индексы дискретных сигналов таблицы AI->PLC не последовательны, начиная с элемента: " + i.ToString() + Environment.NewLine;
                        break;
                    }
                }

            if (DIStruct.items != null && DIStruct.items.Count > 0)
                foreach (DIStruct item in DIStruct.items)
                {
                    if ((item.PLCAddr < Sett.Instance.BegAddrW || item.PLCAddr >= (Sett.Instance.BegAddrW + Sett.Instance.wrBufSize)) &&
                        (item.PLCAddr < Sett.Instance.iBegAddrA3 || item.PLCAddr >= (Sett.Instance.iBegAddrA3 + Sett.Instance.A3BufSize)) &&
                        (item.PLCAddr < Sett.Instance.iBegAddrA4 || item.PLCAddr >= (Sett.Instance.iBegAddrA4 + Sett.Instance.A4BufSize)) &&
                        item.OPCtag == "")
                        report += "Адрес сигнала " + "\"" + item.NameDI + "\"" + " не попадает ни в один из буферов записи" + Environment.NewLine;
                }

            if (DOStruct.items != null && DOStruct.items.Count > 0)
                foreach (DOStruct item in DOStruct.items)
                {
                    if ((item.PLCAddr < Sett.Instance.BegAddrR || item.PLCAddr >= (Sett.Instance.BegAddrR + Sett.Instance.rdBufSize)) && item.OPCtag == "")
                        report += "Адрес сигнала " + "\"" + item.NameDO + "\"" + " не попадает в буфер чтения" + Environment.NewLine;
                }

            if (AIStruct.items != null && AIStruct.items.Count > 0)
                foreach (AIStruct item in AIStruct.items)
                {
                    if ((item.PLCAddr < Sett.Instance.BegAddrW || item.PLCAddr >= (Sett.Instance.BegAddrW + Sett.Instance.wrBufSize)) &&
                        (item.PLCAddr < Sett.Instance.iBegAddrA3 || item.PLCAddr >= (Sett.Instance.iBegAddrA3 + Sett.Instance.A3BufSize)) &&
                        (item.PLCAddr < Sett.Instance.iBegAddrA4 || item.PLCAddr >= (Sett.Instance.iBegAddrA4 + Sett.Instance.A4BufSize)) &&
                        item.OPCtag == "")
                        report += "Адрес сигнала " + "\"" + item.NameAI + "\"" + " не попадает ни в один из буферов записи" + Environment.NewLine;
                }

            if (AOStruct.items != null && AOStruct.items.Count > 0)
                foreach (AOStruct item in AOStruct.items)
                {
                    if ((item.PLCAddr < Sett.Instance.BegAddrR || item.PLCAddr >= (Sett.Instance.BegAddrR + Sett.Instance.rdBufSize)) && item.OPCtag == "")
                        report += "Адрес сигнала " + "\"" + item.Name + "\"" + " не попадает в буфер чтения" + Environment.NewLine;
                }

            if (report == "")
                System.Windows.MessageBox.Show("Ошибок не обнаружено");
            else
            {
                System.IO.StreamWriter writer = new System.IO.StreamWriter("checkReport.txt", false);
                writer.WriteLine(report);
                writer.Close();

                if (System.Windows.MessageBox.Show("Обнаружены ошибки, открыть отчет?", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    Process.Start("checkReport.txt");
                }
            }
        }
        #endregion
        private void DIMenu_set_Click(object sender, RoutedEventArgs e)
        {
            // <DIViewModel> items = (dataGridDI.SelectedItems as IList<DIViewModel>);
            List<DIViewModel> items = dataGridDI.SelectedItems.Cast<DIViewModel>().ToList();
            if (items != null)
                foreach (DIViewModel item in items)
                    item.ForcedValue = true;
        }

        private void DIMenu_reset_click(object sender, RoutedEventArgs e)
        {
            //IList<DIViewModel> items = (dataGridDI.SelectedItems as IList<DIViewModel>);
            List<DIViewModel> items = dataGridDI.SelectedItems.Cast<DIViewModel>().ToList();
            if (items != null)
                foreach (DIViewModel item in items)
                    item.ForcedValue = false;
        }

        private void Log_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            try
            {
                log.ScrollToEnd();
            }
            catch
            { }
        }
        #region СОБЫТИЯ ФИЛЬТРЫ
        private void 
            TextBoxDIFilter_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                divm.NameFilter = textBoxDIFilter.Text;
            }
        }

        private void TextBoxDITagFilter_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                divm.TagFilter = textBoxDITagFilter.Text;
            }
        }

        private void TextBoxAIFilter_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                aivm.NameFilter = textBoxAIFilter.Text;
            }
        }

        private void TextBoxAITagFilter_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                aivm.TagFilter = textBoxAITagFilter.Text;
            }
        }

        private void TextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                aovm.NameFilter = textBoxAOFilter.Text;
            }
        }

        private void TextBoxAOTagFilter_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                aovm.TagFilter = textBoxAOTagFilter.Text;
            }
        }

        private void TextBoxDOFilter_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                dovm.NameFilter = textBoxDOFilter.Text;
            }
        }

        private void TextBoxDOTagFilter_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                dovm.TagFilter = textBoxDOTagFilter.Text;
            }
        }
        #endregion
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {

        }
        #region ADD ITEM
        private void DIMenu_ADDItem_Click(object sender, RoutedEventArgs e)
        {
            List<DIViewModel> items = dataGridDI.SelectedItems.Cast<DIViewModel>().ToList();
            if (items != null)
                foreach (DIViewModel item in items)
                    WatchItem.Items.Add(new WatchItem(item.GetModel()));
            //  watchViewModel.Items.Add(new WatchItemViewModel(item.GetModel()));
        }

        private void AIMenu_ADDItem_Click(object sender, RoutedEventArgs e)
        {
            List<AIViewModel> items = dataGridAI.SelectedItems.Cast<AIViewModel>().ToList();
            if (items != null)
                foreach (AIViewModel item in items)
                    WatchItem.Items.Add(new WatchItem(item.GetModel()));
        }

        private void AOMenu_ADDItem_Click(object sender, RoutedEventArgs e)
        {
            List<AOViewModel> items = dataGridAO.SelectedItems.Cast<AOViewModel>().ToList();
            if (items != null)
                foreach (AOViewModel item in items)
                    WatchItem.Items.Add(new WatchItem(item.GetModel()));
        }

        private void DOMenu_ADDItem_Click(object sender, RoutedEventArgs e)
        {
            List<DOViewModel> items = dataGridDO.SelectedItems.Cast<DOViewModel>().ToList();
            if (items != null)
                foreach (DOViewModel item in items)
                    WatchItem.Items.Add(new WatchItem(item.GetModel()));
        }
        #endregion
        private void MenuItem_ChangeDev_Click(object sender, RoutedEventArgs e)
        {
            ChangeDevDialog dialog = new ChangeDevDialog();
            dialog.ShowDialog();
        }

        private void SetOnTop(object sender, RoutedEventArgs e)
        {
            this.Topmost = true;
        }

        private void UnsetOnTop(object sender, RoutedEventArgs e)
        {
            this.Topmost = false;
        }

        /// <summary>
        /// скрыть столбцы с адресами
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HideColumns(object sender, RoutedEventArgs e)
        {
            DataGridAI_Addr.Visibility = Visibility.Collapsed;
          //  DataGridAI_OPC.Visibility = Visibility.Collapsed;
            DataGridAI_Type.Visibility = Visibility.Collapsed;

            dataGridDI_Addr.Visibility = Visibility.Collapsed;
           // dataGridDI_Bit.Visibility = Visibility.Collapsed;
           // dataGridDI_OPC.Visibility = Visibility.Collapsed;

            dataGridDO_Addr.Visibility = Visibility.Collapsed;
           // dataGridDO_Bit.Visibility = Visibility.Collapsed;
           // dataGridDO_OPC.Visibility = Visibility.Collapsed;

            DataGridAO_Addr.Visibility = Visibility.Collapsed;
           // DataGridAO_OPC.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// показать столбцы с адресами
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowColumns(object sender, RoutedEventArgs e)
        {
            DataGridAI_Addr.Visibility = Visibility.Visible;
        //    DataGridAI_OPC.Visibility = Visibility.Visible;
            DataGridAI_Type.Visibility = Visibility.Visible;

            dataGridDI_Addr.Visibility = Visibility.Visible;
        //    dataGridDI_Bit.Visibility = Visibility.Visible;
        //    dataGridDI_OPC.Visibility = Visibility.Visible;

            dataGridDO_Addr.Visibility = Visibility.Visible;
        //    dataGridDO_Bit.Visibility = Visibility.Visible;
        //    dataGridDO_OPC.Visibility = Visibility.Visible;

            DataGridAO_Addr.Visibility = Visibility.Visible;
        //    DataGridAO_OPC.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// блокировка некоторых элементов интерфейса на время работы симулятора
        /// </summary>
        bool IsSimulationActive
        {
            set
            {
                dataGridAI.CanUserDeleteRows = !value;
                dataGridDI.CanUserDeleteRows = !value;
                dataGridDO.CanUserDeleteRows = !value;
                dataGridAO.CanUserDeleteRows = !value;

                menu_import_csv.IsEnabled = !value;
                menu_import_xlsx.IsEnabled = !value;

                menu_new.IsEnabled = !value;
                menu_open.IsEnabled = !value;

                btnNew.IsEnabled = !value;
                btnOpen.IsEnabled = !value;

                DataGridAI_OPC.IsReadOnly = value;
                DataGridAO_OPC.IsReadOnly = value;

                dataGridDI_OPC.IsReadOnly = value;
                dataGridDO_OPC.IsReadOnly = value;

                MenuItem_Sim.IsEnabled = !value;
                //MenuItem_ChangeDev.IsEnabled = !value;
            }
        }

        private void MenuItem_AppSettings_Click(object sender, RoutedEventArgs e)
        {
            AppSettings dialog = new AppSettings();
            dialog.ShowDialog();
        }
    }

    class MainViewModel
    {

    }
}
