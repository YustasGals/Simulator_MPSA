using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulator_MPSA.CL
{
    class DITableViewModel : BaseViewModel
    {
        private DIViewModel[] _DIs;
        public DIViewModel[] DIs
        {
            get { return _DIs;  }
            set { _DIs = value; OnPropertyChanged("DIs"); }
        }

        public DITableViewModel()
        {
            DIs = new DIViewModel[1];
            _DIs[0] = new DIViewModel(new DIStruct());
            _DIs[0].ShortDesc = "reserved";
        }

        public DITableViewModel(DIStruct[] table)
        {
            DIViewModel[] DIViewModels = new DIViewModel[table.Length];
            for (int i = 0; i < table.Length; i++)
            {
                DIViewModels[i] = new DIViewModel(table[i]);
            }
            DIs = DIViewModels;
        }
    }
}
