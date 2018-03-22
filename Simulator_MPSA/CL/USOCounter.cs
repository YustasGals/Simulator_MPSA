using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.ObjectModel;

namespace Simulator_MPSA.CL
{
    [Serializable]
    public class USOCounter : BaseViewModel
    {
        private bool _en=true;
        private ushort _value=0;
        private int _PLCAddr = 0;


        public USOCounter(int PLCAddr)
        {
            _PLCAddr = PLCAddr;
        }

        public bool En
        { set { _en = value; } get { return _en; } }

    

        public ushort Value
        {
            get { return _value; }
            set { _value = value; OnPropertyChanged("Value"); }
        }
        public int PLCAddr
        {
            get { return _PLCAddr; }
            set {
                _PLCAddr = value;
            }
        }
        public string Name
        { set; get;
        }
        public USOCounter()
        {
        }
        public void Update(float dt)
        {
            if (_en)
            {
                //  Value += dt;
                _value += 1;
                if (_value > 10000)
                    _value = 0;

                OnPropertyChanged("Value");
            }
        }
    }
}
