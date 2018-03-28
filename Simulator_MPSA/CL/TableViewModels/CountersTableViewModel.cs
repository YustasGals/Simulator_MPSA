using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
namespace Simulator_MPSA.CL
{
    class CountersTableViewModel
    {
        
        private static ObservableCollection<USOCounter> _counters = new ObservableCollection<USOCounter>();
        public static ObservableCollection<USOCounter> Counters
        {
            get { return _counters; }
            set { _counters = value; }
        }

        public static void Init(USOCounter[] items)
        {
            _counters.Clear();
            if (items != null && items.Length > 0)
                foreach (USOCounter cnt in items)
                    _counters.Add(cnt);
        }
    }
}
