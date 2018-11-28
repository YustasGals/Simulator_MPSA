using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simulator_MPSA.CL;
using Simulator_MPSA.CL.Signal;
namespace Simulator_MPSA.ViewModel
{
    class DOViewModel : IViewModel<DOStruct>,INotifyPropertyChanged
    {
        private DOStruct _do;

        public DOViewModel()
        {
            _do = new DOStruct();
            _do.PropertyChanged += _do_PropertyChanged;
        }

        public bool Forced
        {
            set { _do.Forced = value; }
            get { return _do.Forced; }
        }
        public bool En
        {
            set { _do.En = value; }
            get { return _do.En; }
        }
        public bool ForcedValue
        {
            set { _do.ForcedValue = value; }
            get { return _do.ForcedValue; }
        }

        public int indxArrDO
        {
            set { _do.indxArrDO = value; }
            get { return _do.indxArrDO; }
        }
        public int indxBitDO
        {
            set { _do.indxBitDO = value; }
            get { return _do.indxBitDO; }
        }

        public int PLCAddr
        {
            set { _do.PLCAddr = value; }
            get { return _do.PLCAddr; }
        }

        public string OPCtag
        {
            set { _do.OPCtag = value; }
            get { return _do.OPCtag; }
        }

        public string TegDO
        {
            set { _do.TegDO = value; }
            get { return _do.TegDO; }
        }

        public string NameDO
        {
            set { _do.NameDO = value; }
            get { return _do.NameDO; }
        }

        public int Nsign
        {
            set { _do.Nsign = value; }
            get { return _do.Nsign; }
        }
        public bool InvertDO
        {
            set { _do.InvertDO = value; }
            get { return _do.InvertDO; }
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        protected void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            PropertyChanged(this, args);
        }
        public DOStruct GetModel()
        {
            return _do;
            //throw new NotImplementedException();
        }

        public string GetName()
        {
            return _do.NameDO;
            //throw new NotImplementedException();
        }

        public string GetTag()
        {
            return _do.TegDO;
            //throw new NotImplementedException();
        }

        public void SetModel(DOStruct model)
        {
            _do = model;
            if (_do != null)
            _do.PropertyChanged += _do_PropertyChanged;
            //throw new NotImplementedException();
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
                            _do.PLCAddr = int.Parse(register.Trim(' '));
                            _do.indxBitDO = int.Parse(bit.Trim(' '));
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
                    _do.OPCtag = opcaddr.Trim(' ');
                }
                else
                    System.Windows.MessageBox.Show(Properties.Resources.AddressingHintDiskrete);


            }
            get
            {
                if (_do.OPCtag != "")
                    return "OPC:" + _do.OPCtag;
                else
                    return "MB:" + _do.PLCAddr.ToString() + '.' + _do.indxBitDO.ToString();
            }
        }
    }
}
