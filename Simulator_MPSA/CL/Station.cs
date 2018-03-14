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
       
        

        public Station()
        { }

        public StationSaveResult Save(string filename)
        {
            DIs = DIStruct.items;
            DOs = DOStruct.items;
            AIs = AIStruct.items;

            KLs = KLTableViewModel.GetArray();
            MPNAs = MPNATableViewModel.GetArray();
            //settings = Sett.
            VSs = VSTableViewModel.GetArray();
            ZDs = ZDTableViewModel.GetArray();
            settings = Sett.Instance;

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
            try
            {
                var stream = File.OpenRead(filename); 
                Station station = (Station)xml.Deserialize(stream);
              //  reader.Dispose();
               

                DIStruct.items = station.DIs;
                DOStruct.items = station.DOs;
                AIStruct.items = station.AIs;
                Sett.Instance = station.settings;

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
                
                System.Windows.MessageBox.Show("Файл " + filename + " считан ");
                return StationLoadResult.OK;
            }
            catch(Exception e)
            {
        //        if (reader != null)
        //            reader.Dispose();
                System.Windows.MessageBox.Show("Ошибка чтения" + Environment.NewLine + e.Message,"Ошибка",System.Windows.MessageBoxButton.OK,System.Windows.MessageBoxImage.Error);
                return StationLoadResult.Fail;
            }
        }
    }
}
