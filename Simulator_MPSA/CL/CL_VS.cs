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

        private bool _en;
        public bool En
        { get { return _en; } set { _en = value; OnPropertyChanged("En"); } }


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
        /// <summary>
        /// сигнал - оманда на включение
        /// </summary>
        private DOStruct ABB;
        private int _ABBindxArrDO = -1;
        public int ABBindxArrDO
        {
            get { return _ABBindxArrDO; }
            set {
                _ABBindxArrDO = value;
                ABB = DOStruct.FindByIndex(_ABBindxArrDO);
            }
        }
        public string ABBName
        { get
            {
                /*if (ABB != null)
                    return ABB.NameDO;
                else return "сигнал не назначен";*/
                if (_ABBindxArrDO > -1)
                    return DOStruct.items[_ABBindxArrDO].NameDO;
                else return "сигнал не назначен";
            }
        }
        /// <summary>
        /// сигнал - команда на отключение
        /// </summary>
        private DOStruct ABO;
        private int _ABOindxArrDO = -1;

        public int ABOindxArrDO
        {
            get { return _ABOindxArrDO; }
            set
            {
                _ABOindxArrDO = value;
                ABO = DOStruct.FindByIndex(_ABOindxArrDO);
            }
        }
        public string ABOName
        { get
            {
                /*if (ABO != null)
                    return ABO.NameDO;
                else return "сигнал не назначен";*/
                if (_ABOindxArrDO > -1)
                    return DOStruct.items[_ABOindxArrDO].NameDO;
                else return "сигнал не назначен";
            }
        }

        public bool changedDO;

        private int _ecindx = -1;
        private bool _isECAnalog;
        private float _valueEC;

        private int _MPCindxArrDI = -1;
        private bool _isMPCanalog;
        private float _valueMPC;

        /// <summary>
        /// наличие давления индекс дискретного сигнала
        /// </summary>
        private int _PCindxArrDI = -1;
        /// <summary>
        /// наличие давления индекс аналогового сигнала
        /// </summary>
        private int _PCindxArrAI = -1;
       // private bool _isPCAnalog;
        private float _valuePC;

        private DIStruct MPC_DI = null;
        private AIStruct MPC_AI = null;

        private DIStruct EC_DI = null;
        private AIStruct EC_AI = null;

        /// <summary>
        /// ссылка на сигнал DI наличие давления
        /// </summary>
        private DIStruct PC_DI = null;
        /// <summary>
        /// ссылка на аналоговый сигнал давления на выходе
        /// </summary>
        private AIStruct PC_AI = null;

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
        /// Индекс сигнала магнитного пускателя в таблице DI
        /// </summary>
        public int MPCindxArrDI    //
        {
            get { return _MPCindxArrDI; }
            set
            {
                _MPCindxArrDI = value; OnPropertyChanged("MPCindxArrDI");
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
        /// индекс сигнала давления в таблице DI
        /// </summary>
        public int PCindxArrDI
        {
            get { return _PCindxArrDI; }
            set { _PCindxArrDI = value;
                OnPropertyChanged("PCindxArrDI");
                    PC_DI = DIStruct.FindByIndex(value);
            }
        }

        /// <summary>
        /// индекс сигнала давления в таблице AI
        /// </summary>
        public int PCindxArrAI
        {
            get { return _PCindxArrAI; }
            set
            {
                _PCindxArrAI = value;
                OnPropertyChanged("PCindxArrAI");
                PC_AI = AIStruct.FindByIndex(value);
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

        //скорость изменения давления физ.ед/сек
        public float valuePCspd
        {
            get; set;
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
                    // if (EC_AI != null)
                    //     return EC_AI.NameAI;
                    if (_ecindx > -1)
                        return AIStruct.items[_ecindx].NameAI;
                }
                else
                {
                    //if (EC_DI != null)
                    //    return EC_DI.NameDI;
                    if (_ecindx > -1)
                        return DIStruct.items[_ecindx].NameDI;
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
                    // if (MPC_AI != null)
                    //     return MPC_AI.NameAI;
                    if (_MPCindxArrDI > -1)
                        return AIStruct.items[_MPCindxArrDI].NameAI;
                }
                else
                {
                    //if (MPC_DI != null)
                    //    return MPC_DI.NameDI;
                    if (_MPCindxArrDI > -1)
                        return DIStruct.items[_MPCindxArrDI].NameDI;
                }

                return "сигнал не назначен";
            }
        }
        /// <summary>
        /// Возвращает название присвоенного сигнала давления на выходе
        /// </summary>
        public string PCNameAI
        {
            get
            {
                if (_PCindxArrAI > -1)
                    if (AIStruct.FindByIndex(_PCindxArrAI) != null)
                        return AIStruct.FindByIndex(_PCindxArrAI).NameAI;
            
                return "сигнал не назначен";
            }
        }
        public string PCNameDI
        {
            get
            {
                if (_PCindxArrDI > -1)
                    if (DIStruct.FindByIndex(_PCindxArrDI) != null)
                        return DIStruct.FindByIndex(_PCindxArrDI).NameDI;
                
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
        { get {
                return state;
            }
            set
            {
                state = value;
                OnPropertyChanged("State");
            }
        }

        public VSStruct()
        {
            Description = "Empty";
            Group = "NoGroup";
            ECindxArrDI = -1;
            isECAnalog = false;
            valueEC = 0.0f;

            MPCindxArrDI = -1;
            isMPCAnalog = false;
            valueMPC = 0.0f;

            PCindxArrDI = -1;
          
            valuePC = 0;


            state = VSState.Stop;
        }

        /// <summary>
        /// Обновление ссылок
        /// </summary>
        public void UpdateRefs()
        {
            ECindxArrDI = _ecindx;
            MPCindxArrDI = _MPCindxArrDI;
            PCindxArrDI = _PCindxArrDI;
            PCindxArrAI = _PCindxArrAI;
            ABOindxArrDO = _ABOindxArrDO;
            ABBindxArrDO = _ABBindxArrDO;
        }
        float PCOnTimeout = 0.0f;
        /// <summary>
        /// обновление состояния вспомсистемы
        /// </summary>
        /// <param name="dt">задержка между циклами обновления в секундах</param>
        /// <returns></returns>
        public void UpdateVS(float dt)
        {
            if (EC_DI != null)
                EC_DI.ValDI = _en;

            if (_en)
            {
                //команда включить - включить пускатель
                if ((ABB!=null)&&(ABB.ValDO))
                {
                    if (state == VSState.Stop || state== VSState.Stoping)
                    {
                        if (MPC_DI != null)
                            MPC_DI.ValDI = true;

                        State = VSState.Starting;
                    }
                }

                //команда выключить - отключить пускатель
                if ((ABO != null) && (ABO.ValDO))
                {
                    if (State == VSState.Starting || State == VSState.Work)
                    {
                        if (MPC_DI != null)
                            MPC_DI.ValDI = false;

                        State = VSState.Stoping;

                        if (PC_DI != null) PC_DI.ValDI = false;

                    }
                }

                //запускается
                if (state== VSState.Starting)
                {
                    if (PC_AI != null)
                    {
                        PC_AI.fValAI += valuePCspd*dt ;
                        if (PC_AI.fValAI >= valuePC)
                        {
                            State = VSState.Work;
                            PC_AI.fValAI = valuePC;
                        }
                    }
                    
                }

                //останавливается
                if (state == VSState.Stoping)
                {
                    if (PC_AI != null)
                    {
                        PC_AI.fValAI -= valuePCspd * dt;
                        if (PC_AI.fValAI <= 0)
                        {
                            PC_AI.fValAI = 0;
                            State = VSState.Stop;

                        }
                    }

                }


            }
            

        }
        /// <summary>
        /// пуск по месту
        /// </summary>
        public void ManualStart()
        {
            if (state == VSState.Stop || state == VSState.Stoping)
            if (MPC_DI != null) MPC_DI.ValDI = true;
            State = VSState.Starting;
        }

        /// <summary>
        /// стоп по месту
        /// </summary>
        public void ManualStop()
        {
            if (MPC_DI != null) MPC_DI.ValDI = false;

            if (state == VSState.Starting || state == VSState.Work)
                State = VSState.Stoping;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }


}
