using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Simulator_MPSA.CL.Signal;
namespace Simulator_MPSA.CL.Signal
{
    class CL_DO
    {
    }
    // -------------------------------------------------------------------------------------------------
    [Serializable]
    public class DOStruct : BaseViewModel
    {
        /// <summary>
        /// Коллекция связанная с интерфейсом
        /// </summary>
        public static ObservableCollection<DOStruct> items = new ObservableCollection<DOStruct>();

        public static int FindByNsign(int nsign)
        {
            foreach (DOStruct item in items)
                if (item.Nsign == nsign) return item.indxArrDO;

            return -1;
        }

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
                        (e.NewItems[0] as DOStruct).indxArrDO = items.Count - 1;
                   // LogWriter.AppendLog("Добавлен сигнал DO: "+(e.NewItems[0] as DOStruct).NameDO+Environment.NewLine);
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    for (int i = 0; i < items.Count; i++)
                    {
                        items[i].indxArrDO = i;
                    }
                    LogWriter.AppendLog("Удален сигнал DO: " + (e.OldItems[0] as DOStruct).NameDO);
                    break;
            }
            //           Debug.WriteLine("DO count: " + items.Count.ToString());
           // LogViewModel.WriteLine("Изменение таблицы DO");

        }

        private bool _En=true;
        [XmlIgnore]
        public bool En
        {
            get { return _En; }
            set { _En = value; OnPropertyChanged("En"); }
        }

        /// <summary>
        /// включить принудительную запись, игнорировать значения полученные из ПЛК
        /// </summary>
        public bool Forced
        {
            set
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
        /// <summary>
        /// Запись и чтение через UI
        /// </summary>
        public bool ForcedValue
        {
            set {
               // if (Forced)
                _ValDO = value;

                if (ValueChanged != null)
                  ValueChanged(this, new EventArgs());
                OnPropertyChanged("ForcedValue");
            }
            get { return _ValDO; }
        }
        
        private bool _ValDO;

        /// <summary>
        /// Запись и чтение в логике
        /// </summary>
        public bool ValDO
        {
            get { return _ValDO; }
            set
            {
                if (!Forced)
                {
                    _ValDO = value;
                 //   OnPropertyChanged("Value");
                    OnPropertyChanged("ValDO");
                    OnPropertyChanged("ForcedValue");

                    if (ValueChanged != null)
                        ValueChanged(this, new EventArgs());
                }
            }
        }
        private int _indxArrDO;
        public int indxArrDO
        {
            get { return _indxArrDO; }
            set {
                _indxArrDO = value;
     //           OnIndexChanged(this, new IndexChangedEventArgs(value));
                OnPropertyChanged("indxArrDO");

                //сообщаем об изменении индекса подписчикам
                IndexChanged(this, new IndexChangedEventArgs(value));
            }
        }


        private int _indxBitDO;
        public int indxBitDO
        {
            get { return _indxBitDO; }
            set { _indxBitDO = value; OnPropertyChanged("indxBitDO"); }
        }

        private int _indxR;
        private int indxR
        {
            get { return _indxR; }
            set { _indxR = value; OnPropertyChanged("indxR"); }
        }

        private int _plcaddr;
        public int PLCAddr
        {
            get {
               /* if (_plcaddr == 0)
                    _plcaddr = Sett.Instance.BegAddrR + _indxR + 1;
*/
                return _plcaddr;
            }
            set {
                _plcaddr = value;
                OnPropertyChanged("PLCAddr"); }
        }
        private string _OPCtag = "";
        public string OPCtag
        {
            set
            {
                _OPCtag = value; OnPropertyChanged("OPCtag");
            }
            get { return _OPCtag; }
        }

        private string _TegDO = "";
        public string TegDO
        {
            get { return _TegDO; }
            set { _TegDO = value; OnPropertyChanged("TegDO"); }
        }

        private string _NameDO = "";
        public string NameDO
        {
            get { return _NameDO; }
            set
            {
                _NameDO = value; OnPropertyChanged("NameDO");
            }
        }

        private int _Nsign;
    
        public int Nsign
        {
            get { return _Nsign; }
            set { _Nsign = value; OnPropertyChanged("Nsign"); }
        }
      

        private bool _InvertDO;
        public bool InvertDO
        {
            get { return _InvertDO; }
            set { _InvertDO = value; OnPropertyChanged("InvertDO"); }
        }
        [XmlIgnore]
        public bool changedDO;
        public DOStruct()
        { }
        public DOStruct(bool En0 = true, bool ValDO0 = false, int indxArrDO0 = 0, int indxBitDO0 = 0, int indxR0 = 0, string TegDO0 = "Teg",
                 string NameDO0 = "Name", int Nsign0 = 0, bool InvertDO0 = false, bool changedDO0 = false)
        {
            En = En0;
            ValDO = ValDO0;
            indxArrDO = indxArrDO0;
            indxBitDO = indxBitDO0;
            indxR = indxR0;
            TegDO = TegDO0;
            NameDO = NameDO0;
            Nsign = Nsign0;
            InvertDO = InvertDO0;
            changedDO = changedDO0;
        }
        public static DOStruct FindByIndex(int index)
        {
            if (index >= 0 && index < items.Count)
                return items[index];
            return null;

        }

        public static string GetNameByIndex(int index)
        {
            if (index > 0 && index < items.Count)
                return items[index].NameDO;
            else return "сигнал не определен";
        }

        /// <summary>
        /// изменение значения
        /// </summary>
        public event EventHandler ValueChanged;
        //     public EventHandler OnIndexChanged = delegate { };

        public override string ToString()
        {
            return NameDO;
        }
    }

}
