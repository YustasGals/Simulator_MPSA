using Simulator_MPSA.CL.Signal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Simulator_MPSA.CL
{
    /// <summary>
    /// класс ссылок на дискреты
    /// </summary>
    public class DIItem
    {
        public DIItem()
        {
        }

        public DIItem(string name)
        {
            this.name = name;
        }
        public DIItem(string name, int index)
        {
            this.name = name;
            this.Index = index;
        }

        string name;
        public string Name
        { get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }

        int index=-1;
        public int Index
        {
            set
            {
                index = value;
              /*  if ((value > -1) && (value < DIStruct.items.Count))
                {
                    DI = DIStruct.items[value];
                }*/
            }
            get
            {
                return index;
            }
        }

        [XmlIgnore]
        public DIStruct DI
        {
            get
            {
                if (index>=0 && index < DIStruct.items.Count)
                    return DIStruct.items[index];
                else
                    return null;
                
            }
        }
        public void SetValue(bool value)
        {
            if (DI != null)
                DI.ValDI = value;
        }
        public bool? GetValue()
        {
            if (DI != null)
                return DI.ValDI;
            else return null; 
        }

        /// <summary>
        /// имя привязанного сигнала
        /// </summary>
        [XmlIgnore]       
        public string GetSignalName
        {
            get { if (DI != null) return DI.NameDI; else return "не определен"; }
        }



    }
}
