using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml;
using System.IO;
namespace Simulator_MPSA.CL
{
    public enum StationLoadResult { OK, Fail};
    public enum StationSaveResult { OK, Fail };

    /// <summary>
    /// Класс для загрузки/сохранения таблиц сигналов
    /// </summary>
    [Serializable]
    public class Station
    {
       public DIStruct[] DIs;
       public DOStruct[] DOs;
       public AIStruct[] AIs;

       public KLStruct[] KLs;
       public MPNAStruct[] MPNAs;
       public Sett settings;
       public VSStruct[] VSs;
       public ZDStruct[] ZDs;
        public USOCounter[] Counters;
        public DIStruct[] DiagSignals;

        public Station()
        { }

        public StationSaveResult Save(string filename)
        {
            DIs = DIStruct.items;
            DOs = DOStruct.items;

            AIStruct.items = AITableViewModel.Instance.AIs.ToArray();
            AIs = AIStruct.items;

            KLs = KLTableViewModel.GetArray();
            MPNAs = MPNATableViewModel.GetArray();
            //settings = Sett.
            VSs = VSTableViewModel.GetArray();
            ZDs = ZDTableViewModel.GetArray();
            settings = Sett.Instance;
            Counters = CountersTableViewModel.Counters.ToArray();
            DiagSignals = DiagTableModel.Instance.DiagRegs.ToArray();

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
                Station station = (Station)xml.Deserialize(stream);
                //  reader.Dispose();
                stream.Dispose();

                DIStruct.items = station.DIs;
                DOStruct.items = station.DOs;

                
                AIStruct.items = station.AIs;

                Sett.Instance = station.settings;

                //при открытии старого файла где не указаны адреса в ПЛК пересчитываем их
                //foreach (AIStruct ai in AIStruct.items)
                //    if (ai.PLCAddr == 0)
                //        ai.PLCAddr = ai.indxW + Sett.Instance.BegAddrW + 1;

                KLTableViewModel.Init(station.KLs);
                foreach (KLStruct kl in KLTableViewModel.KL)
                    kl.UpdateRefs();


                ZDTableViewModel.Init(station.ZDs);
                foreach (ZDStruct zd in ZDTableViewModel.ZDs)
                    zd.UpdateRefs();

                VSTableViewModel.Init(station.VSs);
                foreach (VSStruct vs in VSTableViewModel.VS)
                    vs.UpdateRefs();


                MPNATableViewModel.Init(station.MPNAs);
                foreach (MPNAStruct mpna in MPNATableViewModel.MPNAs)
                    mpna.UpdateRefs();

                CountersTableViewModel.Init(station.Counters);
                foreach (USOCounter counter in CountersTableViewModel.Counters)
                    counter.Refresh();


                DiagTableModel.Instance.Init(station.DiagSignals);

                System.Windows.MessageBox.Show("Файл " + filename + " считан ");
                return StationLoadResult.OK;
            }
            catch(Exception e)
            {
                if (stream != null)
                 stream.Dispose();
                if (reader != null)
                    reader.Dispose();
                System.Windows.MessageBox.Show("Ошибка чтения" + Environment.NewLine + e.Message,"Ошибка",System.Windows.MessageBoxButton.OK,System.Windows.MessageBoxImage.Error);
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
            WB.InitBuffers(Sett.Instance);

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
                AIStruct.items = (AIStruct[])xml.Deserialize(reader);
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
                DIStruct.items = (DIStruct[])xml.Deserialize(reader);
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
                DOStruct.items = (DOStruct[])xml.Deserialize(reader);
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
}
