using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulator_MPSA.CL
{
    class DITableViewModel : BaseViewModel
    {
        private DIStruct[] _DIs;
        public DIStruct[] DIs
        {
            get { return _DIs;  }
            set { _DIs = value; OnPropertyChanged("DIs"); }
        }

        public DITableViewModel()
        {
            DIs = new DIStruct[1];
            _DIs[0] = new DIStruct();
            _DIs[0].NameDI = "reserved";
        }

        public DITableViewModel(DIStruct[] table)
        {
            DIStruct[] DIViewModels = new DIStruct[table.Length];
            for (int i = 0; i < table.Length; i++)
            {
                DIViewModels[i] =table[i];
            }
            DIs = DIViewModels;
        }
    }
}
