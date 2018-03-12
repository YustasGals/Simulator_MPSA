using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Simulator_MPSA.CL;
namespace Simulator_MPSA
{
    public enum VSState {  Stop,   //остановлен
                    Starting,   //запускается
                    Stoping,    //останавливается
                    Work    //в работе
                  };
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

        // состояния входов-выходов

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

        private DIStruct MPC_DI=null;
        private AIStruct MPC_AI=null;

        private DIStruct EC_DI=null;
        private AIStruct EC_AI=null;

        /// <summary>
        /// ссылка на сигнал DI наличие давления
        /// </summary>
        private DIStruct PC_DI=null;
        /// <summary>
        /// ссылка на аналоговый сигнал давления на выходе
        /// </summary>
        private AIStruct PC_AI=null;

        /// <summary>
        /// индекс сигнала напряжения в таблице DI, AI
        /// </summary>
        public int ECindxArrDI 
        { get { return _ecindx; }
            set {
                _ecindx = value; OnPropertyChanged("ECindxArrDI");
                if (_isECAnalog)
                    EC_AI = AIStruct.FindByIndex(value);
                else
                    EC_DI = DIStruct.FindByIndex(value);
            }
        }
        /// <summary>
        /// тип сигнала наличия напряжения
        /// </summary>
        public bool isECAnalog 
        { get { return _isECAnalog; }
            set {
                _isECAnalog = value; OnPropertyChanged("isECAnalog");
                if (_isECAnalog)
                    EC_AI = AIStruct.FindByIndex(_ecindx);
                else
                    EC_DI = DIStruct.FindByIndex(_ecindx);
            }
        }
        /// <summary>
        /// Значение напряжения если это аналог
        /// </summary>
        public float valueEC 
        {
            get { return _valueEC; }
            set { _valueEC = value; OnPropertyChanged("valueEC"); } 
            
        }
        /// <summary>
        /// Индекс сигнала магнитного пускателя в таблице DI или AI
        /// </summary>
        public int MPCindxArrDI    //
        {
            get { return _MPCindxArrDI; }
            set
            {
                _MPCindxArrDI = value; OnPropertyChanged("MPCindxArrDI");
                if (_isMPCanalog)
                    MPC_AI = AIStruct.FindByIndex(_MPCindxArrDI);
                else
                    MPC_DI = DIStruct.FindByIndex(_MPCindxArrDI);
            }
        }
        /// <summary>
        /// тип сигнала магнитного пускателя, аналог/дискрет
        /// </summary>
        public bool isMPCAnalog
        {
            get { return _isMPCanalog; }
            set {
                _isMPCanalog = value; OnPropertyChanged("isMPCanalog");
                if (_isMPCanalog)
                    MPC_AI = AIStruct.FindByIndex(_MPCindxArrDI);
                else
                    MPC_DI = DIStruct.FindByIndex(_MPCindxArrDI);
            }
        }
        /// <summary>
        /// значение сигнала МП если это аналог
        /// </summary>
        public float valueMPC
        {
            get { return _valueMPC; }
            set { _valueMPC = value; OnPropertyChanged("valueMPC"); }
        }
        /// <summary>
        /// индекс сигнала давления в таблице DI, AI
        /// </summary>
        public int PCindxArrDI
        {
            get { return _PCindxArrDI; }
            set { _PCindxArrDI = value;
                  OnPropertyChanged("PCindxArrDI");
                if (_isPCAnalog)
                    PC_AI = AIStruct.FindByIndex(value);
                else
                    PC_DI = DIStruct.FindByIndex(value);
            }
        }
        /// <summary>
        /// тип сигнала давления аналог/дискрет
        /// </summary>
        public bool isPCAnalog
        {
            get { return _isPCAnalog; }
            set {
                _isPCAnalog = value;
                OnPropertyChanged("isPCAnalog");
                if (_isPCAnalog)
                    PC_AI = AIStruct.FindByIndex(_PCindxArrDI);
                else
                    PC_DI = DIStruct.FindByIndex(_PCindxArrDI);
            }
        }
        /// <summary>
        /// значение давления на выходе если это аналог
        /// </summary>
        public float valuePC
        {
            get { return _valuePC; }
            set { _valuePC = value; OnPropertyChanged("valuePC"); }
        }

        /// <summary>
        /// Возвращает название присвоенного сигнала наличия напряжения
        /// </summary>
        public string ECName
        {
            get
            {
                if (isECAnalog)
                {
                    if (EC_AI != null)
                        return EC_AI.NameAI;
                }
                else
                {
                    if (EC_DI != null)
                        return EC_DI.NameDI;
                }

                return "сигнал не назначен";
            }
        }
        /// <summary>
        /// Возвращает название присвоенного сигнала магнитного пускателя
        /// </summary>
        public string MPCName
        {
            get
            {
                if (isMPCAnalog)
                {
                    if (MPC_AI != null)
                        return MPC_AI.NameAI;
                }
                else
                {
                    if (MPC_DI != null)
                        return MPC_DI.NameDI;
                }

                return "сигнал не назначен";
            }
        }
        /// <summary>
        /// Возвращает название присвоенного сигнала давления на выходе
        /// </summary>
        public string PCName
        {
            get
            {
                if (isPCAnalog)
                {
                    if (PC_AI != null)
                        return PC_AI.NameAI;
                }
                else
                {
                    if (PC_DI != null)
                        return PC_DI.NameDI;
                }

                return "сигнал не назначен";
            }
        }

        public bool changedDI;
        public float VSProc;
        public int TmoveVS;

        /// <summary>
        /// состояние вспомсистемы
        /// </summary>
        private VSState state;
        public VSState State
        { get { return state; }
            set { state = value; }
        }
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

            state = VSState.Stop;
        }
      

        /// <summary>
        /// обновление состояния вспомсистемы
        /// </summary>
        /// <param name="dt">задержка между циклами обновления в секундах</param>
        /// <returns></returns>
        public float UpdateVS(float dt)
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
