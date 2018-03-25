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

        private BufType _buffer;
        public BufType buffer
        {
            get { return _buffer; }
            set { _buffer = value; OnPropertyChanged("buffer"); }
        }

        public USOCounter(int PLCAddr)
        {
            _PLCAddr = PLCAddr;
            buffer = BufType.USO;
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


                if ((_PLCAddr > Sett.Instance.iBegAddrA3) && (_PLCAddr < (Sett.Instance.iBegAddrA3 + Sett.Instance.A3BufSize)))
                    buffer = BufType.A3;

                if ((_PLCAddr > Sett.Instance.iBegAddrA4) && (_PLCAddr < (Sett.Instance.iBegAddrA4 + Sett.Instance.A4BufSize)))
                    buffer = BufType.A4;

                if ((_PLCAddr > Sett.Instance.BegAddrW) && (_PLCAddr < (Sett.Instance.BegAddrW + Sett.Instance.USOBufferSize)))
                    buffer = BufType.USO;
            }
        }
        /// <summary>
        /// обновление после десериализации из xml
        /// </summary>
        public void Refresh()
        {

            if ((_PLCAddr > Sett.Instance.iBegAddrA3) && (_PLCAddr < (Sett.Instance.iBegAddrA3 + Sett.Instance.A3BufSize)))
                buffer = BufType.A3;

            if ((_PLCAddr > Sett.Instance.iBegAddrA4) && (_PLCAddr < (Sett.Instance.iBegAddrA4 + Sett.Instance.A4BufSize)))
                buffer = BufType.A4;

            if ((_PLCAddr > Sett.Instance.BegAddrW) && (_PLCAddr < (Sett.Instance.BegAddrW + Sett.Instance.USOBufferSize)))
                buffer = BufType.USO;
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
