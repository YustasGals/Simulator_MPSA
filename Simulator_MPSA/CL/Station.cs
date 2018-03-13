using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml;
namespace Simulator_MPSA.CL
{
    public enum StationLoadResult { OK, Fail};
    public enum StationSaveResult { OK, Fail };

    [Serializable]
    public class Station
    {
       public DIStruct[] DIs=new DIStruct[0];
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
            Station station;
            try
            {
                reader = new System.IO.StreamReader(filename);
                station = (Station)xml.Deserialize(reader);
                reader.Dispose();
                System.Windows.MessageBox.Show("Файл " + filename + " считан ");

                this.DIs = station.DIs;
                this.DOs = station.DOs;
                this.AIs = station.AIs;

                this.KLs = station.KLs;
                this.VSs = station.VSs;
                this.ZDs = station.ZDs;
                this.MPNAs = station.MPNAs;
                this.settings = station.settings;

                return StationLoadResult.OK;
            }
            catch
            {
                if (reader != null)
                    reader.Dispose();
                System.Windows.MessageBox.Show("Файл не считан!");
                return StationLoadResult.Fail;
            }
        }
    }
}
