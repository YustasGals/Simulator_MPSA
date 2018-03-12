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
        private ObservableCollection<KLStruct> _kl;
        public ObservableCollection<KLStruct> KL
        {
            get { return _kl; }
            set { _kl = value; }
        }

        public int Count
        { get { return _kl.Count; } }

        public KLTableViewModel()
        {
            _kl = new ObservableCollection<KLStruct>();
            _kl.Add(new KLStruct());
        }

        public KLTableViewModel(KLStruct[] kl_arr)
        {
            ObservableCollection<KLStruct> temp = new ObservableCollection<KLStruct>();
            foreach (KLStruct kl in kl_arr)
                temp.Add(kl);

            _kl = temp;
        }
    }
}
