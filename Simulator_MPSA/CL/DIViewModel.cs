using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Simulator_MPSA.CL
{
    class DIViewModel : BaseViewModel
    {
        private DIStruct _di;
        public DIViewModel(DIStruct DOelement)
        {
            _di = DOelement;
        }
        public bool Enabled
        {
            get { return _di.En; }
            set
            {
                _di.En = value; OnPropertyChanged("En");
            }
        }
        public bool Value
        {
            get { return _di.ValDI; }
            set
            {
                _di.ValDI = value; OnPropertyChanged("ValDI");
                Debug.WriteLine("value changed to " + value);
            }
        }
        public int indxArrDI
        {
            get { return _di.indxArrDI; }
            set
            {
                _di.indxArrDI = value; OnPropertyChanged("indxArrDI");
            }
        }
        public int indxBitDI
        {
            get { return _di.indxBitDI; }
            set
            {
                _di.indxBitDI = value; OnPropertyChanged("indxBitDI");
            }
        }
        public int indxW
        {
            get { return _di.indxW; }
            set
            {
                _di.indxW = value; OnPropertyChanged("indxW");
            }
        }
        public string TegDI
        {
            get { return _di.TegDI; }
            set
            {
                _di.TegDI = value; OnPropertyChanged("TegDI");
            }
        }
        public string ShortDesc
        {
            get { return _di.NameDI; }
            set
            {
                _di.NameDI = value; OnPropertyChanged("NameDI");
            }
        }
        public int Nsign
        {
            get { return _di.Nsign; }
            set
            {
                _di.Nsign = value; OnPropertyChanged("Nsign");
            }
        }
        public bool InvertDI
        {
            get { return _di.InvertDI; }
            set
            {
                _di.InvertDI = value; OnPropertyChanged("InvertDI");
            }
        }
        public int DelayDI
        {
            get { return _di.DelayDI; }
            set
            {
                _di.DelayDI = value; OnPropertyChanged("DelayDI");
            }
        }
    }
}
