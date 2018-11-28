using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simulator_MPSA.CL;
using System.ComponentModel;
using Simulator_MPSA.CL.Signal;
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
        /*
        public BufType Buffer
        {
            get { return di.Buffer; }
            set { di.Buffer = value; }
        }*/

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

        /// <summary>
        /// универсальное свойство для отображения/записи адреса одним значением
        /// </summary>
        public string AddressTag
        {
            set
            {
                if (value.Substring(0, 3).ToUpper() == "MB:")
                {
                    string mbaddr = value.Substring(3);
                    try
                    {
                        if (mbaddr.Contains('.'))
                        {
                            int dotindex = mbaddr.IndexOf('.');
                            string register = mbaddr.Substring(0, dotindex);
                            string bit = mbaddr.Substring(dotindex + 1);
                            di.PLCAddr = int.Parse(register.Trim(' '));
                            di.indxBitDI = int.Parse(bit.Trim(' '));
                        }
                        else throw new Exception("Неверный формат строки");                     
                        
                    }
                    catch
                    {
                        System.Windows.MessageBox.Show(Properties.Resources.AddressingHintDiskrete);
                    }
                }
                else if (value.Substring(0, 4).ToUpper() == "OPC:")
                {
                    string opcaddr = value.Substring(4);
                    di.OPCtag = opcaddr.Trim(' ');
                }
                else
                    System.Windows.MessageBox.Show(Properties.Resources.AddressingHintDiskrete);


            }
            get
            {
                if (di.OPCtag != "")
                    return "OPC:" + di.OPCtag;
                else
                    return "MB:" + di.PLCAddr.ToString()+'.'+di.indxBitDI.ToString();
            }
        }
    }
}
