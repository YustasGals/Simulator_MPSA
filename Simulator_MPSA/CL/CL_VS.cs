using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
namespace Simulator_MPSA
{
    class CL_VS
    {
    }
    // -------------------------------------------------------------------------------------------------
    [Serializable]
    public class VSStruct : INotifyPropertyChanged
    {
        public bool En
        { set; get; }
        private string _descr;
        public string Description
        {
            get { return _descr; }
            set { _descr = value; OnPropertyChanged("Description"); }
        }

        private string _group;
        public string Group
        {
            get { return _group; }
            set { _group = value; OnPropertyChanged("Group"); }
        }

        public int ABBindxArrDO;    //
        public int ABOindxArrDO;
        public bool changedDO;

        private int _ecindx;
        private bool _isECAnalog;
        private float _valueEC;

        private int _MPCindxArrDI;
        private bool _isMPCanalog;
        private float _valueMPC;

        private int _PCindxArrDI;
        private bool _isPCAnalog;
        private float _valuePC;

        public int ECindxArrDI //индекс сигнала напряжения в таблице DI, AI
        { get { return _ecindx; }
            set {
                _ecindx = value; OnPropertyChanged("ECindxArrDI");
            }
        }
        public bool isECAnalog  //тип сигнала
        { get { return _isECAnalog; }
            set { _isECAnalog = value; OnPropertyChanged("isECAnalog"); }
        }
        public float valueEC //уровень аналогового сигнала сработки
        {
            get { return _valueEC; }
            set { _valueEC = value; OnPropertyChanged("valueEC"); } 
            
        }
        public int MPCindxArrDI    //
        {
            get { return _MPCindxArrDI; }
            set { _MPCindxArrDI = value; OnPropertyChanged("MPCindxArrDI"); }
        }
        public bool isMPCAnalog
        {
            get { return _isMPCanalog; }
            set { _isMPCanalog = value; OnPropertyChanged("isMPCanalog"); }
        }
        public float valueMPC
        {
            get { return _valueMPC; }
            set { _valueMPC = value; OnPropertyChanged("valueMPC"); }
        }

        public int PCindxArrDI
        {
            get { return _PCindxArrDI; }
            set { _PCindxArrDI = value; OnPropertyChanged("PCindxArrDI"); }
        }
        public bool isPCAnalog
        {
            get { return _isPCAnalog; }
            set { _isPCAnalog = value; OnPropertyChanged("isPCAnalog"); }
        }
        public float valuePC
        {
            get { return _valuePC; }
            set { _valuePC = value; OnPropertyChanged("valuePC"); }
        }

        public bool changedDI;
        public float VSProc;
        public int TmoveVS;
        public VSStruct()
        {
            Description = "Empty";
            Group = "NoGroup";
            ECindxArrDI = 0;
            isECAnalog = false;
            valueEC = 0.0f;

            MPCindxArrDI = 0;
            isMPCAnalog = false;
            valueMPC = 0.0f;

            PCindxArrDI = 0;
            isPCAnalog = false;
            valuePC = 0;

        }
        public VSStruct(bool En0 = false,
                        int ABBindxArrDO0 = 0,
                        int ABOindxArrDO0 = 0,
                        bool changedDO0 = false,
                        int ECindxArrDI0 = 0,
                        int MPCindxArrDI0 = 0,
                        int PCindxArrDI0 = 0,
                        bool changedDI0 = false,
                        float VSProc0 = 0.0f,
                        int TmoveVS0 = 2)
        {
            En = En0;
            ABBindxArrDO = ABBindxArrDO0;
            ABOindxArrDO = ABOindxArrDO0;
            changedDO = changedDO0;
            ECindxArrDI = ECindxArrDI0;
            MPCindxArrDI = MPCindxArrDI0;
            PCindxArrDI = PCindxArrDI0;
            changedDI = changedDI0;
            VSProc = VSProc0;
            TmoveVS = TmoveVS0;
        }

        public float UpdateVS()
        {
            // тут будет логика  !!!
            return VSProc;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }


}
