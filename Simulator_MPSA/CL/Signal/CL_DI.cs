using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Xml.Serialization;
using System.Collections.ObjectModel;
using Simulator_MPSA.CL.Signal;
using System.Collections.Specialized;
using System.Diagnostics;

namespace Simulator_MPSA.CL.Signal
{
    // -------------------------------------------------------------------------------------------------
    [Serializable]
    public class DIStruct : INotifyPropertyChanged 
    {
        /// <summary>
        /// Коллекция связанная с интерфейсом
        /// </summary>
        public static ObservableCollection<DIStruct> items = new ObservableCollection<DIStruct>();

        public static int FindByNsign(int nsign)
        {
            foreach (DIStruct item in items)
                if (item.Nsign == nsign) return item.indxArrDI;

            return -1;
        }

        /// <summary>
        /// Для оповещения агрегатов об изменении индекса
        /// </summary>
        public event EventHandler<IndexChangedEventArgs> IndexChanged = delegate { };

        private static bool _enableAutoIndex=false;

        public static bool EnableAutoIndex
        {
            get { return _enableAutoIndex; }
            set
            {
                if (value && !_enableAutoIndex)
                {
                    items.CollectionChanged += Items_CollectionChanged;
                    _enableAutoIndex = true;
                }

                if (!value && _enableAutoIndex)
                {
                    items.CollectionChanged -= Items_CollectionChanged;
                    _enableAutoIndex = false;
                }
            }

        }

        private static void Items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    for (int i = 0; i < e.NewItems.Count; i++)
                        (e.NewItems[0] as DIStruct).indxArrDI = items.Count - 1;
                    LogWriter.AppendLog("Добавлен сигнал DI: " + (e.NewItems[0] as DIStruct).NameDI + Environment.NewLine);
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    //пересчитываем индексы всех сигналов кроме удаляемых
                    /*   for (int i = 0; i < items.Count;)
                       {
                           if (!e.OldItems.Contains(items[i]))
                           {
                               items[i].indx = i;
                               i++;
                           }
                       }*/
                    LogWriter.AppendLog("Удален сигнал DI: " + (e.OldItems[0] as DIStruct).NameDI + Environment.NewLine);
                    for (int i = 0; i < items.Count; i++)
                    {
                        items[i].indxArrDI = i;
                    }

                    break;
            }
        }

        [XmlIgnore]
        public bool En = true;

        private bool _valDI;
        public bool ValDI
        {
            set
            {
                if (!Forced)
                {
                    if (_valDI != value) isChanged = true;

                    _valDI = value;
                    OnPropertyChanged("ForcedValue");
                    if (ValueChanged != null)
                      ValueChanged(this, new EventArgs());
                }
            }
            get
            { return _valDI; }
        }

        /// <summary>
        /// принудительная запись значения
        /// </summary>
        public bool ForcedValue
        {
            get { return _valDI; }
            set
            {
                _valDI = value;
                IsChanged = true;
                OnPropertyChanged("ForcedValue");

                if (ValueChanged != null)
                  ValueChanged(this, new EventArgs());
            }
        }

        /// <summary>
        /// включить принудительную запись значения из таблицы, игнорировать алгоритмы и скрипты
        /// </summary>
        public bool Forced
        { set
            {
                _forced = value;
                OnPropertyChanged("Forced");
            }
            get
            {
                return _forced;
            }
        }
        private bool _forced;

        private int _plcaddr;
        public int PLCAddr
        {
            get
            {
                if (_plcaddr == 0)
                {
                    _plcaddr = Sett.Instance.BegAddrW + indxW;
                  /*  switch (_buffer)
                    {
                        case BufType.USO: _plcaddr = Sett.Instance.BegAddrW + indxW; break;
                        case BufType.A3: _plcaddr = Sett.Instance.iBegAddrA3 + indxW; break;
                        case BufType.A4: _plcaddr = Sett.Instance.iBegAddrA4 + indxW; break;

                    }*/
                }
                    
                
                return _plcaddr;
            }

            set {
                _plcaddr = value;
             /*   Buffer = BufType.Undefined;
                if ((_plcaddr >= Sett.Instance.iBegAddrA3) && (_plcaddr < (Sett.Instance.iBegAddrA3 + Sett.Instance.A3BufSize)))
                    Buffer = BufType.A3;

                if ((_plcaddr >= Sett.Instance.iBegAddrA4) && (_plcaddr < (Sett.Instance.iBegAddrA4 + Sett.Instance.A4BufSize)))
                    Buffer = BufType.A4;

                if ((_plcaddr >= Sett.Instance.BegAddrW) && (_plcaddr < (Sett.Instance.BegAddrW + Sett.Instance.wrBufSize)))
                    Buffer = BufType.USO;*/

                OnPropertyChanged("PLCAddr");
            }
        }

        private string _OPCtag = "";
        public string OPCtag
        {
            set { _OPCtag = value; OnPropertyChanged("OPCtag");
            }
            get { return _OPCtag; }
        }

       /* private BufType _buffer = BufType.USO;
        [XmlIgnore]
        public BufType Buffer
        {
            set { _buffer = value; OnPropertyChanged("Buffer"); }
            get
            {
                return _buffer;
            }
        }*/

        
        private int _indxArrDI;
        public int indxArrDI 
        {
            set
            {
                _indxArrDI = value;
                
                //сообщаем об изменении индекса наверх
                OnPropertyChanged("indxArrDI");

                //сообщаем об изменении индекса подписчикам
                IndexChanged(this, new IndexChangedEventArgs(value));
            }
            get { return _indxArrDI; }
        }


        public int indxBitDI
        { set; get; }
        private int indxW
        { set; get; }

        private string tag = "";
        public string TegDI
        { set
            { tag = value; }
            get
            { return tag; }
        }

        
        private bool isChanged=true;
        [XmlIgnore]
        public bool IsChanged
        {
            get { return isChanged; }
            set { isChanged = value; }
        }

        private string name = "";
        public string NameDI
        {
            set
            {
                name = value;
                OnPropertyChanged("Name");
            }
            get
            { return name; }
        }
      //  [XmlIgnore]
        public int Nsign
        { set; get; }
        private bool invert;

        public bool InvertDI
        {
            set
            {
                if (invert != value) isChanged = true;
                invert = value;
            }
            get { return invert; }
        }
        [XmlIgnore]
        public int DelayDI
        { set; get; }
        public DIStruct()
        {
            TegDI = "";
            NameDI = "";
            InvertDI = false;
            En = true;
            InvertDI = false;
            Forced = false;
        }
        public DIStruct(bool En0 = false, bool ValDI0 = false, int indxArrDI0 = 0, int indxBitDI0 = 0 , int indxW0 = 0, string TegDI0 = "Teg",
                 string NameDI0 = "Name", int Nsign0 = 0 , bool InvertDI0 = false, int DelayDI0 = 0)
        {
            En = En0;
            ValDI = ValDI0;
            indxArrDI = indxArrDI0;
            indxBitDI = indxBitDI0;
            indxW = indxW0;
            TegDI = TegDI0;
            NameDI = NameDI0;
            Nsign = Nsign0;
            InvertDI = InvertDI0;
            DelayDI = DelayDI0;
        }
        public static DIStruct FindByIndex(int index)
        {
            if (index >= 0 && index < items.Count)
                return items[index];
            return null;
        }
        public static string GetNameByIndex(int index)
        {
            if (index > 0 && index < items.Count)
                return items[index].NameDI;
            else return "";
        }
       // [XmlIgnore]
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
      //  [XmlIgnore]
        public event EventHandler ValueChanged;


    }



}
