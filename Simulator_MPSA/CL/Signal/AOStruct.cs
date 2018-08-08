using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Xml.Serialization;
using Simulator_MPSA.CL.Signal;
using System.Diagnostics;

namespace Simulator_MPSA.CL.Signal
{
    public class AOStruct:BaseViewModel
    {
        /// <summary>
        /// Коллекция связанная с интерфейсом
        /// </summary>
        public static ObservableCollection<AOStruct> items = new ObservableCollection<AOStruct>();

        /// <summary>
        /// Копия коллекции для более быстрого доступа по индексу
        /// </summary>
        public static AOStruct[] itemArray;

       private static bool _enableAutoIndex;
        
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

        private static void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    for (int i = 0; i < e.NewItems.Count; i++)
                        (e.NewItems[0] as AOStruct).indx = items.Count-1;
                //    LogWriter.AppendLog("Добавлен сигнал AO: "+ (e.NewItems[0] as AOStruct).Name+Environment.NewLine);
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
                    for (int i = 0; i < items.Count; i++)
                    {
                        items[i].indx = i;
                    }
                    LogWriter.AppendLog("Удален сигнал AO: " + (e.OldItems[0] as AOStruct).Name+Environment.NewLine);

                    break;
            }
            //  Debug.WriteLine("AO count: " + items.Count.ToString());
         //   LogViewModel.WriteLine("Изменение таблицы AO");
        }

        public AOStruct()
        {
            Forced = false;
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

        private string _OPCtag = "";
        /// <summary>
        /// тег для чтения по OPC
        /// </summary>
        public string OPCtag
        {
            set
            {
                _OPCtag = value; OnPropertyChanged("OPCtag");
            }
            get { return _OPCtag; }
        }


        private EPLCDestType _plcDestType=EPLCDestType.ADC;
        /// <summary>
        /// тип данных в ПЛК
        /// </summary>
        public EPLCDestType PLCDestType
        {
            get { return _plcDestType; }
            set { _plcDestType = value; OnPropertyChanged("PLCDestType"); }
        }

        /// <summary>
        /// Принудительная запись значения
        /// </summary>
        /*public float ForcedValue
        {
            set
            {
                if (Forced)
                    _fVal = value;
            }
            get { return _fVal; }
        }*/

        public event EventHandler<IndexChangedEventArgs> IndexChanged= delegate { };

        private int _indx; // index in AI
        /// <summary>
        /// индекс сигнала должен быть уникальным
        /// </summary>
        public int indx
        {
            get { return _indx; }
            set {
                _indx = value;
                OnPropertyChanged("indx");
                IndexChanged(this, new IndexChangedEventArgs(value));
            }
        }


        private int _plcAddr = 0;
        /// <summary>
        /// MODBUS адрес
        /// </summary>
        public int PLCAddr
        {
            get
            {
                return _plcAddr;
            }
            set
            {
                _plcAddr = value;
            }
        }


        private string _name = "";
        public string Name
        {
            get
            {
                return _name;
            }
            set { _name = value; OnPropertyChanged("Name"); }
        }

        private ushort _valADC;
        /// <summary>
        /// значение вычитываемое из ПЛК
        /// </summary>
        public ushort ValACD
        {
            get
            {
                return _valADC;
            }
            set
            {
                _valADC = value;
                OnPropertyChanged("ValACD");
                if (!Forced)
                {                    
                    RefreshValue();
                }
            }
        }
        private ushort _minACD;
        public ushort minACD
        {
            get { return _minACD; }
            set { _minACD = value; OnPropertyChanged("minACD");
                if (!Forced)
                    RefreshValue(); }
        }
        private ushort _maxACD;
        public ushort maxACD
        {
            get { return _maxACD; }
            set { _maxACD = value; OnPropertyChanged("maxACD");
                if (!Forced)
                    RefreshValue(); }
        }
        private float _minPhis;
        public float minPhis
        {
            get { return _minPhis; }
            set { _minPhis = value; OnPropertyChanged("minPhis");
                if (!Forced)
                    RefreshValue(); }
        }
        private float _maxPhis;
        public float maxPhis
        {
            get { return _maxPhis; }
            set { _maxPhis = value; OnPropertyChanged("maxPhis");
                if (!Forced)
                    RefreshValue(); }
        }
        private float _fVal;
        public float fVal
        {
            get { return _fVal; }
            set
            {
                if (!Forced)
                {
                    _fVal = value;
                    OnPropertyChanged("ForcedValue");
                }
            }
        }

        public float ForceValue
        {
            set
            {
                if (Forced)
                {
                    _fVal = value;
                    OnPropertyChanged("ForcedValue");
                }
            }
        }

        private void RefreshValue()
        {
            float df = 0f;
            try
            {
                df = ((float)ValACD - (float)minACD) / ((float)maxACD - (float)minACD);
            }
            catch
            {
            }
            //int res = (int)dadc + minACD;
            //ValACD = (ushort)res;
            fVal = (maxPhis - minPhis) * df + minPhis;
        }

        private string _TagName = "";
        public string TagName
        {
            get { return _TagName; }
            set { _TagName = value; OnPropertyChanged("TagName"); }
        }
    }
}
