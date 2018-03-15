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
        public bool Enabled
        {
            get { return _do.En; }
            set
            {
                _do.En = value; OnPropertyChanged("En");
            }
        }
        public bool Value
        {
            get { return _do.ValDO; }
            set
            {
                _do.ValDO = value; OnPropertyChanged("ValDO");
            }
        }
        public int indxArrDO
        {
            get { return _do.indxArrDO; }
            set
            {
                _do.indxArrDO = value; OnPropertyChanged("indxArrDO");
            }
        }
        public int indxBitDO
        {
            get { return _do.indxBitDO; }
            set
            {
                _do.indxBitDO = value; OnPropertyChanged("indxBitDO");
            }
        }
        public int indxR
        {
            get { return _do.indxR; }
            set
            {
                _do.indxR = value; OnPropertyChanged("indxR");
            }
        }
        public string TegDO
        {
            get { return _do.TegDO; }
            set
            {
                _do.TegDO = value; OnPropertyChanged("TegDO");
            }
        }
        public string ShortDesc
        {
            get { return _do.NameDO; }
            set
            {
                _do.NameDO = value; OnPropertyChanged("ShortDesc");
            }
        }
        public int Nsign
        {
            get { return _do.Nsign; }
            set
            {
                _do.Nsign = value; OnPropertyChanged("Nsign");
            }
        }
        public bool InvertDO
        {
            get { return _do.InvertDO; }
            set
            {
                _do.InvertDO = value; OnPropertyChanged("InvertDO");
            }
        }
        public bool changedDO
        {
            get { return _do.changedDO; }
            set
            {
                _do.changedDO = value; OnPropertyChanged("chandegDO");
            }
        }

    }
}
