using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulator_MPSA.CL
{
    class DOTableViewModel : BaseViewModel
    {
        private DOViewModel[] _DOs;
        public DOViewModel[] DOs
        {
            get { return _DOs; }
            set { _DOs = value; OnPropertyChanged("DOs"); }
        }

        public DOTableViewModel()
        {
            var dos = SetupArray();
            DOViewModel[] DOViewModels = new DOViewModel[dos.Length];
            for (int i = 0; i < dos.Length; i++)
            {
                DOViewModels[i] = new DOViewModel(dos[i]);
            }
            DOs = DOViewModels;
        }
        private DOStruct[] SetupArray()
        {
            DOStruct[] result = new DOStruct[4];
            result[0].NameDO = "Сигнал 1";
            result[1].NameDO = "Сигнал 2";
            result[2].NameDO = "Сигнал 3";
            result[3].NameDO = "Сигнал 4";
            return result;

        }
    }
}
