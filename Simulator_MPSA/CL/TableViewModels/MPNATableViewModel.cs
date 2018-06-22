using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
namespace Simulator_MPSA.CL
{
    class MPNATableViewModel : BaseViewModel
    {
        
        private static MPNATableViewModel instance;
        public static MPNATableViewModel Instance
        {
            get
            {
                if (instance == null)
                    instance = new MPNATableViewModel();
                return instance;
            }
            set { instance = value; }
        }

        private static ObservableCollection<MPNAStruct> _mpnas;
        public static ObservableCollection<MPNAStruct> MPNAs
        {
            get { return _mpnas; }
            set { _mpnas = value; }
        }

        public MPNATableViewModel()
        {
            _mpnas = new ObservableCollection<MPNAStruct>();
            _mpnas.CollectionChanged += _mpnas_CollectionChanged;

            _mpnas.Add(new MPNAStruct());
        }

        private static void _mpnas_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            //  throw new NotImplementedException();
            for (int i = 0; i < _mpnas.Count(); i++)
                _mpnas[i].Index = i;
        }

        private MPNATableViewModel(MPNAStruct[] mnas)
        {
            ObservableCollection<MPNAStruct> temp = new ObservableCollection<MPNAStruct>();
            foreach (MPNAStruct m in mnas)
                temp.Add(m);
            _mpnas = temp;
           // _mpnas.CollectionChanged += _mpnas_CollectionChanged;
        }

        public static void Init(MPNAStruct[] mnas)
        {
            instance = new MPNATableViewModel(mnas);
            _mpnas.CollectionChanged += _mpnas_CollectionChanged;
        }
        public static MPNAStruct[] GetArray()
        {
            MPNAStruct[] temp = new MPNAStruct[_mpnas.Count];
            _mpnas.CopyTo(temp, 0);
            return temp;
        }
    }
}
