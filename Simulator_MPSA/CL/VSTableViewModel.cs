using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Simulator_MPSA.CL
{
    public class VSTableViewModel
    {
        private ObservableCollection<VSStruct> _vs;
        public ObservableCollection<VSStruct> VS
        {
            get { return _vs; }
            set { _vs = value; }
        }

        public int Count
        {
            get { return VS.Count; }
        }

        public VSTableViewModel()
        {
            VS = new ObservableCollection<VSStruct>();
            VS.Add(new VSStruct());
        }

        public VSTableViewModel(VSStruct[] vs_arr)
        {
            ObservableCollection<VSStruct> temp = new ObservableCollection<VSStruct>();
            foreach (VSStruct agr in vs_arr)
                temp.Add(agr);
            _vs = temp;
        }
    }
}
