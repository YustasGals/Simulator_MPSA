using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml;
using System.IO;
using System.Globalization;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Simulator_MPSA.CL
{
    public enum StationLoadResult { OK, Fail};
    public enum StationSaveResult { OK, Fail };

    enum PageType {DI, DO, AI, AO, DiagDI, Counters, Scripts, ZD, VS, KL, MPNA,Nothing };
    /// <summary>
    /// Класс для загрузки/сохранения таблиц сигналов
    /// </summary>
    [Serializable]
    public class Station
    {
       public ObservableCollection<DIStruct> DIs;
       public ObservableCollection<DOStruct> DOs;
       public ObservableCollection<AIStruct> AIs;
        public ObservableCollection<AOStruct> AOs;
        public ObservableCollection<KLStruct> KLs;
       public ObservableCollection<MPNAStruct>  MPNAs;
       public Sett settings;
       public ObservableCollection<VSStruct> VSs;
       public ObservableCollection<ZDStruct> ZDs;
        public ObservableCollection<USOCounter> Counters;
        public ObservableCollection<DIStruct> DiagSignals;
        public ObservableCollection<Scripting.ScriptInfo> scripts;
        PageType currentPage;

     
        public Station()
        { }

        private static Station _instance;
        [XmlIgnore]
        public static Station instance
        {
            get {
                return _instance;
            }
        }

        public StationSaveResult Save(string filename)
        {
            //DIStruct.items = DITableViewModel.Instance.DIs;
            DIs = DIStruct.items;
            DOs = DOStruct.items;

            //AIStruct.items = AIStruct.items;
            AIs = AIStruct.items;

            AOs = AOStruct.items;

            KLs = KLTableViewModel.KL;
            MPNAs = MPNATableViewModel.MPNAs;
            //settings = Sett.
            VSs = VSTableViewModel.VS;
            ZDs = ZDTableViewModel.ZDs;
            settings = Sett.Instance;
            Counters = CountersTableViewModel.Counters;
            DiagSignals = DiagTableModel.Instance.DiagRegs;

            scripts = Scripting.ScriptInfo.Items;



            XmlSerializer xml = new XmlSerializer(typeof(Station));
            System.IO.StreamWriter writeStream = new System.IO.StreamWriter(filename);
            xml.Serialize(writeStream, this);
            writeStream.Dispose();
            System.Windows.MessageBox.Show("Файл " + filename + " сохранен ");
            return StationSaveResult.OK;
        }
        public StationLoadResult Load(string filename)
        {
            XmlSerializer xml = new XmlSerializer(typeof(Station));
            System.IO.StreamReader reader=null;
            FileStream stream=null;
            try
            {
                stream = File.OpenRead(filename); 

                _instance = (Station)xml.Deserialize(stream);

                stream.Dispose();
                Sett.Instance = _instance.settings;

                DIStruct.items.Clear();
                foreach (DIStruct item in _instance.DIs)
                    DIStruct.items.Add(item);

                foreach (DIStruct di in DIStruct.items)
                    di.PLCAddr = di.PLCAddr;

                DOStruct.items.Clear();
                foreach (DOStruct d in _instance.DOs)
                    DOStruct.items.Add(d);

                foreach (DOStruct d in DOStruct.items)
                    d.PLCAddr = d.PLCAddr;

                AIStruct.items.Clear();
                foreach (AIStruct ai in _instance.AIs)
                {
                    AIStruct.items.Add(ai);                   
                }

                foreach (AIStruct ai in AIStruct.items)
                {
                    ai.PLCAddr = ai.PLCAddr;
                }


                AOStruct.items.Clear();
                foreach (AOStruct item in _instance.AOs)
                {
                    AOStruct.items.Add(item);
                }




                //при открытии старого файла где не указаны адреса в ПЛК пересчитываем их
                //foreach (AIStruct ai in AIStruct.items)
                //    if (ai.PLCAddr == 0)
                //        ai.PLCAddr = ai.indxW + Sett.Instance.BegAddrW + 1;

                KLTableViewModel.Init(_instance.KLs.ToArray());
                foreach (KLStruct kl in KLTableViewModel.KL)
                    kl.UpdateRefs();


                ZDTableViewModel.Init(_instance.ZDs.ToArray());
                foreach (ZDStruct zd in ZDTableViewModel.ZDs)
                    zd.UpdateRefs();

                VSTableViewModel.Init(_instance.VSs.ToArray());
                foreach (VSStruct vs in VSTableViewModel.VS)
                    vs.UpdateRefs();


                MPNATableViewModel.Init(_instance.MPNAs.ToArray());
                foreach (MPNAStruct mpna in MPNATableViewModel.MPNAs)
                    mpna.UpdateRefs();

                CountersTableViewModel.Counters = (_instance.Counters);
                foreach (USOCounter counter in CountersTableViewModel.Counters)
                    counter.Refresh();


                DiagTableModel.Instance.Init(_instance.DiagSignals.ToArray());
                foreach (DIStruct di in DiagTableModel.Instance.DiagRegs)
                    di.PLCAddr = di.PLCAddr;

                Scripting.ScriptInfo.Init();
                if (_instance.scripts != null && _instance.scripts.Count>0)
                foreach (Scripting.ScriptInfo scr in _instance.scripts)
                    Scripting.ScriptInfo.Items.Add(scr);

                
                System.Windows.MessageBox.Show("Файл " + filename + " считан ");
                return StationLoadResult.OK;
            }
            catch(Exception e)
            {
                if (stream != null)
                 stream.Dispose();
                if (reader != null)
                    reader.Dispose();
                System.Windows.MessageBox.Show("Ошибка чтения XML" + Environment.NewLine + e.Message,"Ошибка",System.Windows.MessageBoxButton.OK,System.Windows.MessageBoxImage.Error);
                return StationLoadResult.Fail;
                
            }
        }

        #region settings.xml
        public static void SaveSettings(string Sxml = "XMLs//" + "settings.xml")
        {
            XmlSerializer xml = new XmlSerializer(typeof(Sett));
            System.IO.StreamWriter writeStream = new System.IO.StreamWriter(Sxml);
            xml.Serialize(writeStream, Sett.Instance);
            writeStream.Dispose();
            System.Windows.MessageBox.Show("Файл " + Sxml + " сохранен ");
        }
        public static void LoadSettings(string Sxml = "XMLs//" + "settings.xml")
        {
            XmlSerializer xml = new XmlSerializer(typeof(Sett));
            System.IO.StreamReader reader = null;
            try
            {
                reader = new System.IO.StreamReader(Sxml);
                Sett.Instance = (Sett)xml.Deserialize(reader);
                reader.Dispose();
                System.Windows.MessageBox.Show("Файл " + Sxml + " считан ");
            }
            catch
            {
                if (reader != null)
                    reader.Dispose();
                System.IO.StreamWriter writer = new System.IO.StreamWriter(Sxml);
                xml.Serialize(writer, Sett.Instance);
                writer.Dispose();
                System.Windows.MessageBox.Show("Файл " + Sxml + " не считан !!! ");
            }
            /*RB.R = new ushort[(Sett.Instance.NRackEnd) * 50];//[(29 - 3 + 1) * 50]    =1450   From IOScaner CPU
            WB.W = new ushort[(Sett.Instance.NRackEnd - Sett.Instance.NRackBeg + 1) * 126]; // =3402 From IOScaner CPU
            WB.WB_old = new ushort[(Sett.Instance.NRackEnd - Sett.Instance.NRackBeg + 1) * 126];
            WB.W_a3 = new ushort[Sett.Instance.A3BufSize];
            WB.W_a3_prev = new ushort[WB.W_a3.Length];*/
            WB.InitBuffers();
            RB.InitBuffer();

            //AIStruct.items = new AIStruct[Sett.Instance.NAI];
            //ZDs = new ZDStruct[settings.NZD];
            //DOStruct.items = new DOStruct[Sett.Instance.NDO * 32];
            //ZDs = new ZDStruct[settings.NZD];
            /* KLStruct.KLs = new KLStruct[Sett.Instance.NKL];
             VSStruct.VSs = new VSStruct[Sett.Instance.NVS];
             MPNAStruct.MPNAs = new MPNAStruct[Sett.Instance.NMPNA];
             DIStruct.items = new DIStruct[Sett.Instance.NDI * 32];*/
            //TODO: вставить код активации кнопки
        }
        #endregion
        // -----------------------------------------------------------------

        // public AIStruct[] AIs;// = new AIStruct[settings.nAI];

        #region AIsettings.xml
        public  static void LoadSettAI(string Sxml = "XMLs//" + "AIsettings.xml")
        {
            XmlSerializer xml = new XmlSerializer(typeof(AIStruct[]));
            System.IO.StreamReader reader = null;
            try
            {
                reader = new System.IO.StreamReader(Sxml);
                AIStruct.items = (ObservableCollection<AIStruct>)xml.Deserialize(reader);
                reader.Dispose();

                System.Windows.Forms.MessageBox.Show("AIsettings.xml loaded.");
            }
            catch
            {
                System.IO.StreamWriter writer = new System.IO.StreamWriter(Sxml);
                xml.Serialize(writer, AIStruct.items);
                writer.Dispose();
            }
        }
        // ---------------------------------------------------------------------
        public static void SaveSettAI(string Sxml = "XMLs//" + "AIsettings.xml")
        {
            XmlSerializer xml = new XmlSerializer(typeof(AIStruct[]));
            System.IO.StreamWriter writeStream = new System.IO.StreamWriter(Sxml);
            xml.Serialize(writeStream, AIStruct.items);
            writeStream.Dispose();
            System.Windows.Forms.MessageBox.Show("AIsettings.xml saved.");
        }
        #endregion
        // ---------------------------------------------------------------------

        // public DIStruct[] DIs;// = new DIStruct[Sett.nDI * 32];

        #region DIsettings.xml
        public static void LoadSettDI(string Sxml = "XMLs//" + "DIsettings.xml")
        {
            XmlSerializer xml = new XmlSerializer(typeof(DIStruct[]));
            System.IO.StreamReader reader = null;
            try
            {
                reader = new System.IO.StreamReader(Sxml);
               // DIStruct.items = (ObservableCollection<DIStruct>)xml.Deserialize(reader);
                reader.Dispose();
                System.Windows.Forms.MessageBox.Show("DIsettings.xml loaded.");
            }
            catch
            {
                System.IO.StreamWriter writer = new System.IO.StreamWriter(Sxml);
                xml.Serialize(writer, DIStruct.items);
                writer.Dispose();
            }
        }
        // ---------------------------------------------------------------------
        public static void SaveSettDI(string Sxml = "XMLs//" + "DIsettings.xml")
        {
            XmlSerializer xml = new XmlSerializer(typeof(DIStruct[]));
            System.IO.StreamWriter writeStream = new System.IO.StreamWriter(Sxml);
            xml.Serialize(writeStream, DIStruct.items);
            writeStream.Dispose();
            System.Windows.Forms.MessageBox.Show("DIsettings.xml saved.");
        }
        #endregion
        // ---------------------------------------------------------------------

        //  public DOStruct[] DOs;// new DOStruct[Sett.nDO * 32];

        #region DOsettings.xml
        public static void LoadSettDO(string Sxml = "XMLs//" + "DOsettings.xml")
        {
            XmlSerializer xml = new XmlSerializer(typeof(DOStruct[]));
            System.IO.StreamReader reader = null;
            try
            {
                reader = new System.IO.StreamReader(Sxml);
                DOStruct.items = (ObservableCollection<DOStruct>)xml.Deserialize(reader);
                reader.Dispose();
                System.Windows.Forms.MessageBox.Show("DOsettings.xml loaded.");
            }
            catch
            {
                System.IO.StreamWriter writer = new System.IO.StreamWriter(Sxml);
                xml.Serialize(writer, DOStruct.items);
                writer.Dispose();
            }
        }
        // ---------------------------------------------------------------------
        public static void SaveSettDO(string Sxml = "XMLs//" + "DOsettings.xml")
        {
            XmlSerializer xml = new XmlSerializer(typeof(DOStruct[]));
            System.IO.StreamWriter writeStream = new System.IO.StreamWriter(Sxml);
            xml.Serialize(writeStream, DOStruct.items);
            writeStream.Dispose();
            System.Windows.Forms.MessageBox.Show("DOsettings.xml saved.");
        }
        #endregion
        // ---------------------------------------------------------------------
        // ---------------------------------------------------------------------
        // = new ZDStruct[Sett.nZD];
        #region ZDsettings.xml
        public static void LoadSettZD(string Sxml = "XMLs//" + "ZDsettings.xml")
        {
            XmlSerializer xml = new XmlSerializer(typeof(ZDStruct[]));
            System.IO.StreamReader reader = null;
            try
            {
                reader = new System.IO.StreamReader(Sxml);
                ZDTableViewModel.Init((ZDStruct[])xml.Deserialize(reader));
                reader.Dispose();

                //dataGridZD.DataContext = ZDTableViewModel.Instance;

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
        public static void SaveSettZD(string Sxml = "XMLs//" + "ZDsettings.xml")
        {
            XmlSerializer xml = new XmlSerializer(typeof(ZDStruct[]));
            System.IO.StreamWriter writeStream = new System.IO.StreamWriter(Sxml);

            xml.Serialize(writeStream, ZDTableViewModel.GetArray());
            writeStream.Dispose();
            System.Windows.Forms.MessageBox.Show("ZDsettings.xml saved.");
        }
        #endregion
        // ---------------------------------------------------------------------
        // ---------------------------------------------------------------------
        //public KLStruct[] KLs;// = new KLStruct[Sett.nKL];
        #region KLsettings.xml
        public static void LoadSettKL(string Sxml = "XMLs//" + "KLsettings.xml")
        {
            XmlSerializer xml = new XmlSerializer(typeof(KLStruct[]));
            System.IO.StreamReader reader = null;
            try
            {
                reader = new System.IO.StreamReader(Sxml);
                KLTableViewModel.Init((KLStruct[])xml.Deserialize(reader));
                reader.Dispose();
                System.Windows.Forms.MessageBox.Show("KLsettings.xml loaded.");
            }
            catch
            {

            }
        }
        // ---------------------------------------------------------------------
        public static void SaveSettKL(string Sxml = "XMLs//" + "KLsettings.xml")
        {
            XmlSerializer xml = new XmlSerializer(typeof(KLStruct[]));
            System.IO.StreamWriter writeStream = new System.IO.StreamWriter(Sxml);
            xml.Serialize(writeStream, KLTableViewModel.GetArray());
            writeStream.Dispose();
            System.Windows.Forms.MessageBox.Show("KLsettings.xml saved.");
        }
        #endregion
        // ---------------------------------------------------------------------
        // ---------------------------------------------------------------------
        //public VSStruct[] VSs;// = new VSStruct[Sett.nVS];
        #region VSsettings.xml
        public static void LoadSettVS(string Sxml = "XMLs//" + "VSsettings.xml")
        {
            XmlSerializer xml = new XmlSerializer(typeof(VSStruct[]));
            System.IO.StreamReader reader = null;
            try
            {
                reader = new System.IO.StreamReader(Sxml);
                VSTableViewModel.Init((VSStruct[])xml.Deserialize(reader));
                reader.Dispose();
                System.Windows.Forms.MessageBox.Show("VSsettings.xml loaded.");
            }
            catch
            {
                System.IO.StreamWriter writer = new System.IO.StreamWriter(Sxml);
                xml.Serialize(writer, VSTableViewModel.GetArray());
                writer.Dispose();
            }
        }
        // ---------------------------------------------------------------------
        public static void SaveSettVS(string Sxml = "XMLs//" + "VSsettings.xml")
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
        //public MPNAStruct[] MPNAs;// = new MPNAStruct[Sett.nMPNA];
        #region MPNAsettings.xml
        public static void LoadSettMPNA(string Sxml = "XMLs//" + "MPNAsettings.xml")
        {
            XmlSerializer xml = new XmlSerializer(typeof(MPNAStruct[]));
            System.IO.StreamReader reader = null;
            try
            {
                reader = new System.IO.StreamReader(Sxml);
                MPNATableViewModel.Init((MPNAStruct[])xml.Deserialize(reader));
                reader.Dispose();
                System.Windows.Forms.MessageBox.Show("MPNAsettings.xml loaded.");
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show("Ошибка чтения " + Sxml + Environment.NewLine + e.Message);
            }
        }
        // ---------------------------------------------------------------------
        public static void  SaveSettMPNA(string Sxml = "XMLs//" + "MPNAsettings.xml")
        {
            XmlSerializer xml = new XmlSerializer(typeof(MPNAStruct[]));
            System.IO.StreamWriter writeStream = new System.IO.StreamWriter(Sxml);
            xml.Serialize(writeStream, MPNATableViewModel.GetArray());
            writeStream.Dispose();
            System.Windows.Forms.MessageBox.Show(Sxml + " saved.");
        }
        #endregion
        // ---------------------------------------------------------------------
        // ---------------------------------------------------------------------

    }
    public class CSVWorker
    {
        /// <summary>
        /// разделитель таблиц
        /// </summary>
        static string pageSeparator = "--EndPage--";
        /// <summary>
        /// разделитель списка аналогов
        /// </summary>
        static string aiSeparator = "--EndOfAI--";
        /// <summary>
        /// разделитель скриптов
        /// </summary>
        static string scriptSeparator = "---EndOfScript---";
        static char separator = '\t';

        static string pageHeaderDI = "----------- Дискретные входы (DI)-------------------------------------";
        static string pageHeaderDO = "----------- Дискретные выходы (DO) ----------------------------------";
        static string pageHeaderAI = "------------Аналоговые входы (AI) -----------------";
        static string pageHeaderAO = "------------Аналоговые входы (AO) -----------------";
        static string pageHeaderZD = "----------- Настройки задвижек ----------------";
        static string pageHeaderVS = "----------- Настройки вспомсистем ---------------";
        static string pageHeaderKL = "----------- Настройки клапанов -----------";
        static string pageHeaderMPNA = "----------- Настройки МПНА ------------------";
        static string pageHeaderDiagDI = "----------- Сигналы диагностики (DI) -------------";
        static string pageHeaderCounters = "----------- Счетчики -------------";
        static string pageHeaderScripts = "----------- Скрипты ------------";
        /// <summary>
        /// экспорт в CSV - файл
        /// </summary>
        /// <param name="station"></param>
        /// <param name="filename"></param>
        public static void exportCSV(Station station, string filename)
        {
            if (station == null || filename == null) return;

            System.IO.StreamWriter writer = new System.IO.StreamWriter(filename, false, Encoding.Unicode);

            //CultureInfo culture = new CultureInfo("ru-RU");
            CultureInfo culture = new CultureInfo("en-US");
            //CultureInfo ruProvider = new CultureInfo("ru-RU");
            //  System.Windows.Forms.MessageBox.Show("export");
            writer.WriteLine(pageHeaderDI);
            writer.WriteLine("Вкл." + separator + "Индекс" + separator + "OPC тэг" + separator +
                                "Адрес ModBus" + separator +
                                "Бит" + separator + "Принуд." + separator + "Принуд. знач" + separator +
                                "Значение" + separator + "Инвертировать" + separator + "Тэг" + separator + "Описание");
            if ((station.DIs != null) && (station.DIs.Count() > 0))
            {

                foreach (DIStruct di in DIStruct.items)
                    writer.WriteLine(di.En.ToString(culture) + separator +
                                        di.indxArrDI.ToString(culture) + separator +
                                        di.OPCtag.ToString(culture) + separator +
                                        di.PLCAddr.ToString(culture) + separator +
                                        di.indxBitDI.ToString(culture) + separator +
                                        di.Forced.ToString(culture) + separator +
                                        di.ForcedValue.ToString(culture) + separator +
                                        di.ValDI.ToString(culture) + separator +
                                        di.InvertDI.ToString(culture) + separator +
                                        di.TegDI.ToString(culture) + separator+
                                        di.NameDI.ToString(culture));

            }
            writer.WriteLine(pageSeparator);


            writer.WriteLine(pageHeaderDO);
            writer.WriteLine("Вкл" + separator +
                             "Индекс" + separator +
                             "OPC тэг" + separator +
                             "Адрес ModBus" + separator +
                             "Бит" + separator +
                             "Принуд." + separator +
                             "Принуд. знач." + separator +
                             "Значение" + separator +
                             "Инвертировать" + separator +
                             "Тэг" + separator +
                             "Описание" + separator
                             );
            if (station.DOs != null && station.DOs.Count() > 0)
            {
                foreach (DOStruct dos in station.DOs)
                {
                    writer.WriteLine(dos.En.ToString(culture) + separator +
                                        dos.indxArrDO.ToString(culture) + separator +
                                        dos.OPCtag.ToString(culture) + separator +
                                        dos.PLCAddr.ToString(culture) + separator +
                                        dos.indxBitDO.ToString(culture) + separator +
                                        dos.Forced.ToString(culture) + separator +
                                        dos.ForcedValue.ToString(culture) + separator +
                                        dos.ValDO.ToString(culture) + separator +
                                        dos.InvertDO.ToString(culture) + separator +
                                        dos.TegDO.ToString(culture) + separator +
                                        dos.NameDO.ToString(culture) + separator);
                }
            }
            writer.WriteLine(pageSeparator);

            writer.WriteLine(pageHeaderAI);

            writer.WriteLine("Вкл" + separator +
                                "Индекс" + separator +
                                "OPC тэг" + separator +
                                "Адрес ModBus" + separator +
                                "Тип" + separator +
                                "Принудительно" + separator +
                                "Принуд. значение" + separator +
                                "Значение физ." + separator +
                                "АЦП мин." + separator +
                                "АЦП макс." + separator +
                                "Физ мин." + separator +
                                "Физ макс." + separator +
                                "Тэг" + separator +
                                "Описание"
                                );
            if (station.AIs != null && station.AIs.Count() > 0)
            {
                foreach (AIStruct ai in station.AIs)
                {
                    writer.WriteLine(ai.En.ToString(culture) + separator +
                                        ai.indxAI.ToString(culture) + separator +
                                        ai.OPCtag.ToString(culture) + separator +
                                        ai.PLCAddr.ToString(culture) + separator +
                                        ai.PLCDestType.ToString() + separator +
                                        ai.Forced.ToString(culture) + separator +
                                        ai.ForcedValue.ToString(culture) + separator +
                                        ai.fValAI.ToString(culture) + separator +
                                        ai.minACD.ToString(culture) + separator +
                                        ai.maxACD.ToString(culture) + separator +
                                        ai.minPhis.ToString(culture) + separator +
                                        ai.maxPhis.ToString(culture) + separator +
                                        ai.TegAI.ToString(culture) + separator +
                                        ai.NameAI.ToString(culture)
                                        );
                }
               
            }
            writer.WriteLine(pageSeparator);

            writer.WriteLine(pageHeaderAO);
            writer.WriteLine("Вкл" + separator +
                                "Индекс" + separator +
                                "OPC тэг" + separator +
                                "Адрес ModBus" + separator +
                                "Тип" + separator +
                                "Принудительно" + separator +
                                "Принуд. значение" + separator +
                                "Значение физ." + separator +
                                "АЦП мин." + separator +
                                "АЦП макс." + separator +
                                "Физ мин." + separator +
                                "Физ макс." + separator +
                                "Тэг" + separator +
                                "Описание"
                                );

            if (station.AOs != null && station.AOs.Count() > 0)
            {
                foreach (AOStruct item in station.AOs)
                {
                    writer.WriteLine(item.En.ToString(culture) + separator +
                                        item.indx.ToString(culture) + separator +
                                        item.OPCtag.ToString(culture) + separator +
                                        item.PLCAddr.ToString(culture) + separator +
                                        item.PLCDestType.ToString() + separator +
                                        item.Forced.ToString(culture) + separator +
                                        //item.ForcedValue.ToString(culture) + separator +
                                        item.fVal.ToString(culture) + separator +
                                        item.minACD.ToString(culture) + separator +
                                        item.maxACD.ToString(culture) + separator +
                                        item.minPhis.ToString(culture) + separator +
                                        item.maxPhis.ToString(culture) + separator +
                                        item.TagName.ToString(culture) + separator +
                                        item.Name.ToString(culture)
                                        );
                }
                
            }
            writer.WriteLine(pageSeparator);

            writer.WriteLine();
                writer.WriteLine("Вкл" + separator +
                                    "КВО" + separator +
                                    "КВЗ" + separator +
                                    "МПО" + separator +
                                    "МПЗ" + separator +
                                    "%открытия индекс" + separator +
                                    "авария привода" + separator +
                                    "время хода, сек" + separator +
                                    "Муфта" + separator +
                                    "состояине" + separator +
                                    "наличие напряжения" + separator +
                                    "Наличие напряжения на СШ" + separator +
                                    "команда - открыть" + separator +
                                    "команда - закрыть" + separator +
                                    "команда - остановить" + separator +
                                    "команда - стоп закрытия" + separator +
                                    "Наименование");
                if (station.ZDs != null && station.ZDs.Count() > 0)
                {

                    foreach (ZDStruct zd in station.ZDs)
                    {
                        writer.WriteLine(zd.En.ToString(culture) + separator +
                                            zd.OKCindxArrDI.ToString(culture) + separator +
                                            zd.CKCindxArrDI.ToString(culture) + separator +
                                            zd.ODCindxArrDI.ToString(culture) + separator +
                                            zd.CDCindxArrDI.ToString(culture) + separator +
                                            zd.ZD_Pos_index.ToString(culture) + separator +
                                            zd.OPCindxArrDI.ToString(culture) + separator +
                                            zd.TmoveZD.ToString(culture) + separator +
                                            zd.MCindxArrDI.ToString(culture) + separator +
                                            zd.StateZD.ToString() + separator +
                                            zd.VoltindxArrDI.ToString() + separator +
                                            zd.BSIndex.ToString(culture) + separator +
                                            zd.DOBindxArrDO.ToString() + separator +
                                            zd.DKBindxArrDO.ToString() + separator +
                                            zd.DCBindxArrDO.ToString() + separator +
                                            zd.DCBZindxArrDO.ToString() + separator +
                                            zd.Description);
                    }
                }
                writer.WriteLine(pageSeparator);

                writer.WriteLine(pageHeaderKL);
                writer.WriteLine("Вкл." + separator +
                                    "Открыт" + separator +
                                    "Закрыт" + separator +
                                    "Открыть" + separator +
                                    "Закрыть" + separator +
                                    "%открытия" + separator +
                                    "состояние" + separator +
                                    "Наименование");
                if (station.KLs != null && station.KLs.Count() > 0)
                {
                    foreach (KLStruct kl in station.KLs)
                    {
                        writer.WriteLine(kl.En.ToString(culture) + separator +
                                            kl.OKCindxArrDI.ToString(culture) + separator +
                                            kl.CKCindxArrDI.ToString(culture) + separator +
                                            kl.DOBindxArrDO.ToString(culture) + separator +
                                            kl.DKBindxArrDO.ToString(culture) + separator +
                                            kl.KLProc.ToString(culture) + separator +
                                            kl.State.ToString() + separator +
                                            kl.Description);
                    }
                }
                writer.WriteLine(pageSeparator);

                writer.WriteLine(pageHeaderVS);
                writer.WriteLine("Вкл." + separator + "Команда-вкл" + separator +
                                    "Команда - откл." + separator + "Напряжение на СШ" + separator +
                                    "Напряжение" + separator + "МП" + separator +
                                    "Состояине" + separator +
                                    "уставка (АО)" + separator +
                                    "Описание"
                                    );
                if (station.VSs != null && station.VSs.Count() > 0)
                {
                    foreach (VSStruct vs in station.VSs)
                    {
                        writer.WriteLine(vs.En.ToString(culture) + separator +
                                            vs.ABBindxArrDO.ToString(culture) + separator +
                                            vs.ABOindxArrDO.ToString(culture) + separator +
                                            vs.BusSecIndex.ToString(culture) + separator +
                                            vs.ECindxArrDI.ToString(culture) + separator +
                                            vs.MPCindxArrDI.ToString(culture) + separator +
                                            vs.State.ToString() + separator +
                                            vs.AnCmdIndex.ToString() + separator +
                                        //    vs.isAVOA.ToString() + separator +
                                       //     vs.SetRPM_Value.ToString() + separator +
                                       //     vs.SetRPM_Addr.ToString() + separator +
                                            vs.Description);
                        writer.WriteLine("--- VS AI ----");
                        writer.WriteLine("Индекс в таблице AI" + separator + "Номинальное значение" + separator + "Интенсивность изменения");
                        if (vs.controledAIs != null && vs.controledAIs.Count() > 0)
                            foreach (AnalogIOItem io in vs.controledAIs)
                                writer.WriteLine(io.Index.ToString(/*culture*/) + separator +
                                                    io.ValueNom.ToString(culture) + separator +
                                                    io.ValueSpd.ToString(culture));

                        writer.WriteLine(aiSeparator);
                    }
                }
                writer.WriteLine(pageSeparator);

                writer.WriteLine(pageHeaderMPNA);
                writer.WriteLine("Вкл" + separator + "Исправность цепей вкл." + separator +
                                    "Исправность цепей выкл.1" + separator +
                                    "Исправность цепей выкл.2" + separator +
                                    "вв включен 1" + separator + "ВВ включен 2" + separator +
                                    "вв выключен 1" + separator + "ВВ выключен 2" + separator +
                                    "команда - включить" + separator +
                                    "команда - откл.1" + separator + "команда - откл.2" + separator +
                                    "Состояние" + separator +
                                    "ECx" + separator + "Наименование"
                                    );
                if (station.MPNAs != null && station.MPNAs.Count() > 0)
                    foreach (MPNAStruct mpna in station.MPNAs)
                    {
                        writer.WriteLine(mpna.En.ToString(culture) + separator +
                                            mpna.ECBindxArrDI.ToString() + separator +
                                            mpna.ECO11indxArrDI.ToString(culture) + separator +
                                            mpna.ECO12indxArrDI.ToString(culture) + separator +
                                            mpna.MBC11indxArrDI.ToString(culture) + separator +
                                            mpna.MBC12indxArrDI.ToString(culture) + separator +
                                            mpna.MBC21indxArrDI.ToString(culture) + separator +
                                            mpna.MBC22indxArrDI.ToString(culture) + separator +
                                            mpna.ABBindxArrDO.ToString(culture) + separator +
                                            mpna.ABOindxArrDO.ToString(culture) + separator +
                                            mpna.ABO2indxArrDO.ToString(culture) + separator +
                                            mpna.State.ToString() + separator +
                                            mpna.ECxindxArrDI.ToString() + separator +
                                            mpna.Description
                                            );
                        writer.WriteLine("--- MPNA AI ----");
                        writer.WriteLine("Индекс в таблице AI" + separator + "Номинальное значение" + separator + "Интенсивность изменения");
                        if (mpna.controledAIs != null && mpna.controledAIs.Count() > 0)
                            foreach (AnalogIOItem io in mpna.controledAIs)
                                writer.WriteLine(io.Index.ToString(/*culture*/) + separator +
                                                    io.ValueNom.ToString(culture) + separator +
                                                    io.ValueSpd.ToString(culture));

                        writer.WriteLine(aiSeparator);
                    }

                writer.WriteLine(pageSeparator);

                writer.WriteLine(pageHeaderDiagDI);

                writer.WriteLine("Вкл" + separator +
                                    "Адрес ModBus" + separator +
                                    "Бит" + separator +
                                    "Описание");
                if (station.DiagSignals != null && station.DiagSignals.Count() > 0)
                {
                    foreach (DIStruct di in station.DiagSignals)
                    {
                        writer.WriteLine(di.En.ToString() + separator +
                                            di.PLCAddr.ToString() + separator +
                                            di.indxBitDI.ToString() + separator +
                                            di.NameDI);
                    }
                }
                writer.WriteLine(pageSeparator);

                writer.WriteLine(pageHeaderCounters);

                writer.WriteLine("Вкл." + separator + "адрес ModBus");

                if (station.Counters != null && station.Counters.Count() > 0)
                    foreach (USOCounter cnt in station.Counters)
                        writer.WriteLine(cnt.En.ToString(culture) + separator +
                                            cnt.PLCAddr.ToString(culture));
                writer.WriteLine(pageSeparator);

                writer.WriteLine(pageHeaderScripts);
                if (station.scripts != null && station.scripts.Count() > 0)
                    foreach (Scripting.ScriptInfo scr in station.scripts)
                    {
                        writer.WriteLine(scr.En.ToString() + separator + scr.Name + separator);
                        writer.Write(scr.ScriptTxt + Environment.NewLine);
                        writer.WriteLine(scriptSeparator);
                    }
                writer.WriteLine(pageSeparator);



            writer.Close();
            System.Windows.Forms.MessageBox.Show("Экспорт завершен успешно");
        }

        /// <summary>
        /// Чтение из CSV таблицы DI
        /// </summary>
        /// <param name="reader"></param>
        static void ReadTableDI(StreamReader reader, out int count)
        {
            count = 0;
            string line;
            // CultureInfo culture = new CultureInfo("ru-RU");
            CultureInfo culture = new CultureInfo("en-US");
            //считываем страницу DI
            try
            {
                reader.ReadLine();//пропускаем строку с заголовками
                List<DIStruct> listDI = new List<DIStruct>();
                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine();

                    if (!line.Contains(pageSeparator))
                    {
                        string[] values = line.Split('\t');
                        if (values.Count() < 11)
                        {
                            System.Windows.Forms.MessageBox.Show("Ошибка чтения файла");
                        }
                        DIStruct di = new DIStruct();
                        di.En = bool.Parse(values[0]);
                        di.indxArrDI = int.Parse(values[1]);
                        di.OPCtag = values[2];
                        di.PLCAddr = int.Parse(values[3]);
                        di.indxBitDI = int.Parse(values[4]);

                        di.Forced = bool.Parse(values[5]);
                        di.ForcedValue = bool.Parse(values[6]);
                        di.ValDI = bool.Parse(values[7]);
                        di.InvertDI = bool.Parse(values[8]);

                        if (values.Length>9)
                        di.TegDI = values[9];

                        if (values.Length > 10)
                            di.NameDI = values[10];

                        listDI.Add(di);
                    }
                    else
                    {
                        Debug.WriteLine("Обнаружен конец таблицы");
                        break;
                    }
                }
                
                count = listDI.Count;
                Debug.WriteLine(count.ToString() + " successfuly parsed");
                DIStruct.items.Clear();
                foreach (DIStruct di in listDI)
                    DIStruct.items.Add(di);

                //DITableViewModel.Instance.Init(DIStruct.items);
            //    dataGridDI.ItemsSource = DITableViewModel.Instance.viewSource.View;
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("Ошибка импорта таблицы DI:\n\r" + ex.Message);
            }
        }

        /// <summary>
        /// чтение таблицы сигналов DO
        /// </summary>
        /// <param name="reader">поток</param>
        /// <param name="count">количество прочитанных элементов</param>
        static void ReadTableDO(StreamReader reader, out int count)
        {
            count = 0;
            string line;
            //CultureInfo culture = new CultureInfo("ru-RU");
            CultureInfo culture = new CultureInfo("en-US");
            //считываем страницу DI
            try
            {
                reader.ReadLine();//пропускаем строку с заголовками
                List<DOStruct> items = new List<DOStruct>();
                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine();

                    if (!line.Contains(pageSeparator))
                    {
                        string[] values = line.Split('\t');
                        if (values.Count() < 11)
                        {
                            System.Windows.Forms.MessageBox.Show("Ошибка чтения файла");
                        }
                        DOStruct item = new DOStruct();
                        item.En = bool.Parse(values[0]);
                        item.indxArrDO = int.Parse(values[1]);
                        item.OPCtag = values[2];
                        item.PLCAddr = int.Parse(values[3]);
                        item.indxBitDO = int.Parse(values[4]);

                        item.Forced = bool.Parse(values[5]);
                        item.ForcedValue = bool.Parse(values[6]);
                        item.ValDO = bool.Parse(values[7]);
                        item.InvertDO = bool.Parse(values[8]);

                        if (values.Length>9)
                            item.TegDO = values[9];

                        if (values.Length > 10)
                            item.NameDO = values[10];

                        items.Add(item);
                    }
                    else
                    {
                        Debug.WriteLine("Обнаружен конец таблицы");
                        break;
                    }
                }

                count = items.Count;
                Debug.WriteLine(count.ToString() + " successfuly parsed");
                DOStruct.items.Clear();
                foreach (DOStruct item in items)
                    DOStruct.items.Add(item);

                //DITableViewModel.Instance.Init(DIStruct.items);
                //    dataGridDI.ItemsSource = DITableViewModel.Instance.viewSource.View;
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("Ошибка импорта таблицы DO:\n\r" + ex.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        static void ReadTableAI(StreamReader reader, out int count)
        {
            count = 0;
            string line;
            //CultureInfo culture = new CultureInfo("ru-RU");
            CultureInfo culture = new CultureInfo("en-US");
            //считываем страницу DI
            try
            {
                reader.ReadLine();//пропускаем строку с заголовками
                List<AIStruct> items = new List<AIStruct>();
                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine();

                    if (!line.Contains(pageSeparator))
                    {
                        string[] values = line.Split('\t');
                        if (values.Count() < 11)
                        {
                            continue;
                          //  System.Windows.Forms.MessageBox.Show("Ошибка чтения файла");
                        }
                        AIStruct item = new AIStruct();
                        item.En = bool.Parse(values[0]);
                        item.indxAI = int.Parse(values[1]);
                        item.OPCtag = values[2];
                        item.PLCAddr = int.Parse(values[3]);
                        item.PLCDestType = (EPLCDestType)Enum.Parse(typeof(EPLCDestType),values[4]);

                        item.Forced = bool.Parse(values[5]);
                        item.ForcedValue = float.Parse(values[6],culture);
                        item.fValAI = float.Parse(values[7], culture);
                        item.minACD = ushort.Parse(values[8], culture);
                        item.maxACD = ushort.Parse(values[9], culture);
                        item.minPhis = float.Parse(values[10], culture);
                        item.maxPhis = float.Parse(values[11], culture);
                       

                        if (values.Length > 11)
                            item.TegAI = values[12];

                        if (values.Length > 12)
                            item.NameAI = values[13];

                        items.Add(item);
                    }
                    else
                    {
                        Debug.WriteLine("Обнаружен конец таблицы");
                        break;
                    }
                }
                count = items.Count;
                Debug.WriteLine(count.ToString() + " successfuly parsed");
                AIStruct.items.Clear();
                foreach (AIStruct item in items)
                    AIStruct.items.Add(item);

                //DITableViewModel.Instance.Init(DIStruct.items);
                //    dataGridDI.ItemsSource = DITableViewModel.Instance.viewSource.View;
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("Ошибка импорта таблицы AI:\n\r" + ex.Message);
            }
        }

        static void ReadTableAO(StreamReader reader, out int count)
        {
            count = 0;
            string line;
            //CultureInfo culture = new CultureInfo("ru-RU");
            CultureInfo culture = new CultureInfo("en-US");
            //считываем страницу DI
            AOStruct.items.Clear();
            try
            {
                reader.ReadLine();//пропускаем строку с заголовками
            //    List<AOStruct> items = new List<AOStruct>();
                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine();

                    if (!line.Contains(pageSeparator))
                    {
                        string[] values = line.Split('\t');
                        if (values.Count() < 11)
                        {
                            continue;
                            //  System.Windows.Forms.MessageBox.Show("Ошибка чтения файла");
                        }
                        AOStruct item = new AOStruct();
                        item.En = bool.Parse(values[0]);
                        item.indx = int.Parse(values[1]);
                        item.OPCtag = values[2];
                        item.PLCAddr = int.Parse(values[3]);
                        item.PLCDestType = (EPLCDestType)Enum.Parse(typeof(EPLCDestType), values[4]);

                        item.Forced = bool.Parse(values[5]);
                        //item.ForcedValue = float.Parse(values[6], culture);
                        item.fVal = float.Parse(values[6], culture);
                        item.minACD = ushort.Parse(values[7], culture);
                        item.maxACD = ushort.Parse(values[8], culture);
                        item.minPhis = float.Parse(values[9], culture);
                        item.maxPhis = float.Parse(values[10], culture);


                        if (values.Length > 10)
                            item.TagName = values[11];

                        if (values.Length > 11)
                            item.Name = values[12];

                    //    items.Add(item);
                        AOStruct.items.Add(item);
                    }
                    else
                    {
                        Debug.WriteLine("Обнаружен конец таблицы");
                        break;
                    }
                }
                count = AOStruct.items.Count;
                Debug.WriteLine(count.ToString() + " successfuly parsed");
  
                    

                //DITableViewModel.Instance.Init(DIStruct.items);
                //    dataGridDI.ItemsSource = DITableViewModel.Instance.viewSource.View;
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("Ошибка импорта таблицы AO:\n\r" + ex.Message);
            }
        }

        public static void importCSV(string filename)
        {
            if (filename == null) return;

            System.IO.StreamReader reader = new System.IO.StreamReader(filename);
            
            string line = "";
            Debug.WriteLine("Import started");

            string logText = "";
            int count=0;

            while (!reader.EndOfStream)
            {
                line = reader.ReadLine();
                if (line.Contains(pageHeaderDI))
                {
                    Debug.WriteLine("обнаружена таблица DI");
                    ReadTableDI(reader, out count);
                    logText += "сигналы DI: " + count.ToString() + Environment.NewLine;
                }

               

                if (line.Contains(pageHeaderDO))
                {
                    Debug.WriteLine("Обнаружена таблица DO");
                    ReadTableDO(reader, out count);
                    logText += "сигналы DO: " + count.ToString() + Environment.NewLine;
                }
                
                

                if (line.Contains(pageHeaderAI))
                {
                    Debug.WriteLine("Обнаружена таблица AI");
                    ReadTableAI(reader, out count);
                    logText += "сигналы AI: " + count.ToString() + Environment.NewLine;
                }

                

                if (line.Contains(pageHeaderAO))
                {
                    // Debug.WriteLine();
                    ReadTableAO(reader, out count);
                    logText += "сигналы AO: " + count.ToString() + Environment.NewLine;
                }

                
            }

            System.Windows.Forms.MessageBox.Show("Импорт завершен:\n\r"+logText);
        }
    }
}
