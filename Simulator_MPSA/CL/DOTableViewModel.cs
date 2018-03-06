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
        public DOTableViewModel(DOStruct[] table)
        {
            DOViewModel[] DOViewModels = new DOViewModel[table.Length];
            for (int i = 0; i < table.Length; i++)
            {
                DOViewModels[i] = new DOViewModel(table[i]);
            }
            DOs = DOViewModels;
        }
        private DOStruct[] SetupArray()
        {
            DOStruct[] result = new DOStruct[1];
            for (int i = 0; i < result.Length; i++)
                result[i] = new DOStruct();

            result[0].NameDO = "Сигнал 1";

            return result;

        }
    }
}
