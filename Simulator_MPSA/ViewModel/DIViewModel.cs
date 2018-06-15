using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simulator_MPSA.CL;
using System.ComponentModel;
namespace Simulator_MPSA.ViewModel
{
    /// <summary>
    /// View Model для дискретного входного сигнала
    /// </summary>
    class DIViewModel: IViewModel<DIStruct>,INotifyPropertyChanged
    {
        public DIViewModel()
        {
            di = new DIStruct();
            di.PropertyChanged += _do_PropertyChanged;
        }
        private DIStruct di;
        public bool En
        {
            get { return di.En; }
            set { di.En = value; }
        }
        public int indxArrDI
        {
            get { return di.indxArrDI;  }
            set { di.indxArrDI = value; }
        }
        public bool ForcedValue
        {
            get { return di.ForcedValue; }
            set { di.ForcedValue = value;  }
        }

        public bool Forced
        {
            get { return di.Forced; }
            set { di.Forced = value; }
        }
        public string OPCtag
        {
            set
            {
                di.OPCtag = value; 
            }
            get { return di.OPCtag; }
        }

        public DIStruct GetModel()
        {
            return di;
        }

        public void SetModel(DIStruct model)
        {
            di = model;
            di.PropertyChanged += _do_PropertyChanged;
        }

        public string GetName()
        {
            return di.NameDI;
        }

        public string GetTag()
        {
            return di.TegDI;
        }
    
        public int PLCAddr
        {
            get { return di.PLCAddr; }
            set { di.PLCAddr = value; }
        }
        public int indxBitDI
        {
            get { return di.indxBitDI;  }
            set { di.indxBitDI = value; }
        }
        public string TegDI
        {
            get { return di.TegDI; }
            set { di.TegDI = value; }
        }
        public string NameDI
        {
            get { return di.NameDI; }
            set { di.NameDI = value; }
        }
        public bool InvertDI
        {
            get { return di.InvertDI; }
            set { di.InvertDI = value; }
        }
        public BufType Buffer
        {
            get { return di.Buffer; }
            set { di.Buffer = value; }
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        protected void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            PropertyChanged(this, args);
        }
        private void _do_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e);
            // throw new NotImplementedException();
        }
    }
}
