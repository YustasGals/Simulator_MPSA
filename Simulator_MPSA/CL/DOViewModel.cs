using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulator_MPSA.CL
{
    class DOViewModel : BaseViewModel
    {
        private DOStruct _do;
        public DOViewModel(DOStruct DOelement)
        {
            _do = DOelement;
        }
        public bool En
        {
            get { return _do.En; }
            set
            {
                _do.En = value; OnPropertyChanged("En");
            }
        }
        public bool ValDO
        {
            get { return _do.ValDO;  }
            set
            {
                _do.ValDO = value; OnPropertyChanged("ValDO");
            }
        }

        public string NameDO
        {
            get { return _do.NameDO;  }
            set
            {
                _do.NameDO = value; OnPropertyChanged("NameDO");
            }
        }
    }
}
