using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;
namespace Simulator_MPSA.CL
{
    class KLTableViewModel
    {
        private static KLTableViewModel _instance;
        public static KLTableViewModel Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new KLTableViewModel();
                return _instance;
            }           
            
        }


        private static ObservableCollection<KLStruct> _kl;
        public static ObservableCollection<KLStruct> KL
        {
            get { return _kl; }
            set { _kl = value; }
        }

        public int Count
        { get { return _kl.Count; } }

        private KLTableViewModel()
        {
            _kl = new ObservableCollection<KLStruct>();
            _kl.CollectionChanged += CollectionChanged;
            _kl.Add(new KLStruct());
        }

        private KLTableViewModel(KLStruct[] kl_arr)
        {
            ObservableCollection<KLStruct> temp = new ObservableCollection<KLStruct>();
            
            foreach (KLStruct kl in kl_arr)
                temp.Add(kl);

            _kl = temp;
          //  _kl.CollectionChanged += CollectionChanged;
        }
        private static void CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            for (int i = 0; i < _kl.Count(); i++)
                _kl[i].Index = i;
            //throw new NotImplementedException();
        }

        public static void Init(KLStruct[] kl_arr)
        {
            _instance = new KLTableViewModel(kl_arr);
            _kl.CollectionChanged += CollectionChanged;
        }

        public static KLStruct[] GetArray()
        {
            KLStruct[] temp = new KLStruct[KL.Count];
            _kl.CopyTo(temp, 0);
            return temp;
        }
    }
}
