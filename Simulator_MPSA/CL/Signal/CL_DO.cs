using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Simulator_MPSA.CL.Signal;
namespace Simulator_MPSA.CL
{
    class CL_DO
    {
    }
    // -------------------------------------------------------------------------------------------------
    [Serializable]
    public class DOStruct : BaseViewModel
    {
        public static ObservableCollection<DOStruct> items = new ObservableCollection<DOStruct>();

 //       private void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
   //     {
       
  //      }

        private bool _En;
        public bool En
        {
            get { return _En; }
            set { _En = value; OnPropertyChanged("En"); }
        }

        /// <summary>
        /// включить принудительную запись, игнорировать значения полученные из ПЛК
        /// </summary>
        public bool Forced
        { set; get; }

        /// <summary>
        /// Запись и чтение через UI
        /// </summary>
        public bool ForcedValue
        {
            set {
                if (Forced)
                _ValDO = value;
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
        public DOStruct(bool En0 = false, bool ValDO0 = false, int indxArrDO0 = 0, int indxBitDO0 = 0, int indxR0 = 0, string TegDO0 = "Teg",
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
            for (int i = 0; i < items.Count; i++)
                if (items[i].indxArrDO == index)
                {
                    return items[i];
                }
            return null;

        }

        public static string GetNameByIndex(int index)
        {
            if (index > 0 && index < items.Count)
                return items[index].NameDO;
            else return "сигнал не определен";
        }

   //     public EventHandler OnIndexChanged = delegate { };

    }

}
