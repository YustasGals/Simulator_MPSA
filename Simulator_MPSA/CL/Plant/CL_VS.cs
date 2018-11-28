using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Simulator_MPSA.CL.Signal;
using System.Xml.Serialization;
using Simulator_MPSA.CL;
using System.Timers;
using System.Diagnostics;

namespace Simulator_MPSA
{
    public enum VSState {  Stop,   //остановлен
                 //   Starting,   //запускается
                 //   Stoping,    //останавливается
                    Work,    //в работе
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
        private int _index=0;
        public int Index
        {
            get { return _index; }
            set { _index = value; OnPropertyChanged("Index"); }
        }



        /// <summary>
        /// Наличие напряжения на секции шин
        /// </summary>
        private DIStruct bs;

        /// <summary>
        /// состояине секции шин с учетом задержек времени
        /// </summary>
        private bool corrBSState=false;

        /// <summary>
        /// предыдущее состояние СШ
        /// </summary>
        private bool prevBSState = false;
        [XmlIgnore]
        public DIStruct BS
        {
            get { return bs; }
        }

        private int _bussec_index=-1;
        public int BusSecIndex
        {
            get { return _bussec_index; }
            set
            {
              //  if (_bussec_index != value || bs == null)
              //  {
                    if (bs != null)
                    {
                     //   bs.IndexChanged -= BS_IndexChanged;
                        bs.PropertyChanged -= BS_PropertyChanged;
                    }
                    _bussec_index = value;
                    if (value > -1)
                        bs = DIStruct.FindByIndex(value);
                   
                    if (bs != null)
                    {
                  //      bs.IndexChanged += BS_IndexChanged;
                        bs.PropertyChanged += BS_PropertyChanged;
                    }
              //  }
            }
        }

        private List<DIItem> _CustomDIs = new List<DIItem>();

        /// <summary>
        /// ссылки на дискретные сигналы, относящиеся к вспомсистеме но не участвующие в алгоритмах
        /// </summary>
        internal List<DIItem> CustomDIs
        {
            get
            { return _CustomDIs; }
            set
            { _CustomDIs = value; }
        }
       
        #region Обработчики событий изменения значения сигналов
        private void BS_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // throw new NotImplementedException();
            OnPropertyChanged("BSVoltage");
        }
        private void EC_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged("IsVoltageOK");
        }
        private void MP_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged("IsMPOn");
        }
       /* private void ABO_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged("CMDStop");
        }*/
        private void ABB_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged("CMDStart");
        }
        #endregion

        #region Property для отображения состояния сигналов в DataGrid
        /// <summary>
        /// есть напряжение
        /// </summary>
        [XmlIgnore]
        public bool? IsVoltageOk
        {
            get
            {
                if (ec == null)
                    return null;
                else return ec.ValDI;
            }
        }

        [XmlIgnore]
        public bool? BSVoltage
        {
            set
            {
            }
            get
            {
                if (bs != null)
                    return bs.ValDI;
                else
                    return null;
            }
        }

        /// <summary>
        /// есть команда на включение
        /// </summary>
         [XmlIgnore]
        public bool? CmdStart
        {
            get
            {
                if (abb == null)
                    return null;
                else return abb.ValDO;
            }
        }
        /// <summary>
        /// есть команда на отключение
        /// </summary>
        [XmlIgnore]
        public bool? CmdStop
        {
            get
            {
                if (abo == null)
                    return null;
                else return abo.ValDO;
            }
        }

        /// <summary>
        /// состояние МП
        /// </summary>
        [XmlIgnore]
        public bool? IsMPOn
        {
            get
            {
                if (mpc == null)
                    return null;
                else
                    return mpc.ValDI;
            }
        }
        #endregion
     

        // состояния входов-выходов
        /// <summary>
        /// сигнал - оманда на включение
        /// </summary>
        private DOStruct abb;
        private int _ABBindxArrDO = -1;
        [XmlIgnore]
        public DOStruct ABB
        {
            get { return abb; }
        }
      
      
        /// <summary>
        /// сигнал - команда на включение, индекс в таблице DO
        /// </summary>
        public int ABBindxArrDO
        {
            get { return _ABBindxArrDO; }
            set {
            
                    _ABBindxArrDO = value;

                    //ищем ссылку на новый сигнал по индексу
                    abb = DOStruct.FindByIndex(_ABBindxArrDO);

                    //подписываемся на новый сигнал
                    if (abb != null)
                    {
                        //    abb.IndexChanged += ABB_IndexChanged;
                        // abb.PropertyChanged += ABB_PropertyChanged;
                        abb.PropertyChanged += delegate { OnPropertyChanged("CMDStart"); };
                        OnPropertyChanged("CMDStart");
                    }
 
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
   /*     private void ABB_IndexChanged(object sender, CL.Signal.IndexChangedEventArgs e)
        {
            _ABBindxArrDO = e.newIndex;
            LogViewModel.WriteLine("\"" + Description + "\": " + "изменен индекс сигнала DO - команда на включение");
        }*/

       /* public string ABBName
        { get
            {
              
                if (_ABBindxArrDO > -1 && _ABBindxArrDO < DOStruct.items.Count)
                    return DOStruct.items[_ABBindxArrDO].NameDO;
                else return "сигнал не назначен";                
            }
        }*/
        /// <summary>
        /// сигнал - команда на отключение
        /// </summary>
        private DOStruct abo;
        /// <summary>
        /// команда на отключение
        /// </summary>
        public DOStruct ABO
        {
            get
            {
                if (abo != null)
                    return abo;
                else return null;
                /*else
                    if (_ABOindxArrDO >= 0 && _ABOindxArrDO < DOStruct.items.Count)
                {
                    abo = DOStruct.items[_ABOindxArrDO];
                    return abo;
                }
                return null;
                      */
            }
        }
       
        /// <summary>
        /// индекс команды на отключение в массиве DOStruct.items
        /// </summary>
        private int _ABOindxArrDO = -1;
        /// <summary>
        /// команда на отключение, индекс в таблице DOStruct
        /// </summary>
        public int ABOindxArrDO
        {
            get { return _ABOindxArrDO; }
            set
            {

                    _ABOindxArrDO = value;

                    //ссылка изменена - отписывается от старого элемента
                  //  if (abo != null)
                   //     abo.IndexChanged -= ABO_IndexChanged;
                    //находим новый сигнал                   
                    abo = DOStruct.FindByIndex(_ABOindxArrDO);
                    //подписываемся на него
                    if (abo != null)
                    {
                        abo.PropertyChanged += delegate { OnPropertyChanged("CMDStop"); };
                    ///    abo.IndexChanged += ABO_IndexChanged;
                        OnPropertyChanged("CMDStop");
                    }

            }
        }

        public bool changedDO;

        /// <summary>
        /// индекс сигнала "наличие напряжения" в массиве DIstruct.items
        /// </summary>
        private int _ecindx = -1;
     
        /// <summary>
        /// индекс сигнала "МП" в массиве DIStruct.items
        /// </summary>
        private int _MPCindxArrDI = -1;

        /// <summary>
        /// наличие давления индекс дискретного сигнала в массиве DIStruct.items
        /// </summary>
        private int _PCindxArrDI = -1;
        /// <summary>
        /// наличие давления индекс аналогового сигнала в массиве AIStruct.items
        /// </summary>
     //   private int _PCindxArrAI = -1;

        /// <summary>
        /// Стоп по месту
        /// </summary>
        private bool flagManualStop = false;

        /// <summary>
        /// ссылка на сигнал - МП
        /// </summary>
        private DIStruct mpc = null;
        /// <summary>
        /// ссылка на сигнал - МП
        /// </summary>
        [XmlIgnore]
        public DIStruct MPC
        {
            get { return mpc; }
        }

        /// <summary>
        /// ссылка на сигнал - наличие напряжения
        /// </summary>
        
        private DIStruct ec = null;
        [XmlIgnore]
        public DIStruct EC
        {
            get { return ec; }
        }
        
        private bool? ECValue
        {
            get
            {
                if (ec != null)
                    return ec.ValDI;
                else return null;
            }
            set
            {
                if (ec != null)
                    ec.ValDI = (value == true);
            }
        }


        /// <summary>
        /// ссылка на сигнал DI наличие давления
        /// </summary>
        private DIStruct pc = null;
        [XmlIgnore]
        public DIStruct PC
        {
            get { return pc; }
        }
        /// <summary>
        /// ссылка на аналоговый сигнал давления на выходе
        /// </summary>
        public AnalogIOItem[] controledAIs;

        private int _anCmdIndex=-1;

        /// <summary>
        /// Индекс уставки в таблице AO
        /// </summary>
        public int AnCmdIndex
        {
            set
            {
             //   if (_anCmdIndex != value || analogCommand == null)
             //   {
                 //   if (analogCommand != null)
                  //      analogCommand.IndexChanged -= AnalogCommand_IndexChanged;
                    _anCmdIndex = value;
                    if (value >= 0 && value < AOStruct.items.Count)
                    {
                        analogCommand = AOStruct.items[value];
                    //    analogCommand.IndexChanged += AnalogCommand_IndexChanged;
                    }
                    else
                        analogCommand = null;
              //  }
            }
            get { return _anCmdIndex; }
        }

        /// <summary>
        /// Отслеживание изменения индекса в таблице AO
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
       /* private void AnalogCommand_IndexChanged(object sender, CL.Signal.IndexChangedEventArgs e)
        {
            _anCmdIndex = e.newIndex;
            LogViewModel.WriteLine("\""+Description + "\": "+"изменен индекс сигнала АО");
        }*/


        /// <summary>
        /// аналоговый вход - уставка
        /// </summary>
        public AOStruct analogCommand;

        public AOStruct AnalogCommand
        {
            get { return analogCommand; }
        }

        public int ECindxArrDI
        { get { return _ecindx; }
            set {
             //   if (_ecindx != value || ec ==null)
             //   {
                   // if (ec != null) ec.IndexChanged -= EC_DI_IndexChanged;
                    _ecindx = value;
                    
                    OnPropertyChanged("ECindxArrDI");

                    ec = DIStruct.FindByIndex(value);
                    if (ec != null)
                    {
                       // ec.IndexChanged += EC_DI_IndexChanged;
                        ec.PropertyChanged += EC_PropertyChanged;
                        OnPropertyChanged("IsVoltageOk");
                    }

              //  }
            }
        }

       /* private void EC_DI_IndexChanged(object sender, CL.Signal.IndexChangedEventArgs e)
        {
         
            _ecindx = e.newIndex;
            LogViewModel.WriteLine("\"" + Description + "\": " + "изменен индекс сигнала DI - наличие напряжения");
        }*/


        /// <summary>
        /// Индекс сигнала магнитного пускателя в таблице DI
        /// </summary>
        public int MPCindxArrDI    //
        {
            get { return _MPCindxArrDI; }
            set
            {
            //    if (_MPCindxArrDI != value || mpc == null)
            //    {
                   /* if (mpc != null)
                        mpc.IndexChanged -= MPC_DI_IndexChanged;*/

                    _MPCindxArrDI = value; OnPropertyChanged("MPCindxArrDI");
                    mpc = DIStruct.FindByIndex(_MPCindxArrDI);
                    if (mpc != null)
                    {
                      //  mpc.IndexChanged += MPC_DI_IndexChanged;
                        mpc.PropertyChanged += MP_PropertyChanged;
                        OnPropertyChanged("IsMPOn");
                    }
             //   }
            }
        }
        /*
        private void MPC_DI_IndexChanged(object sender, CL.Signal.IndexChangedEventArgs e)
        {
            _MPCindxArrDI = e.newIndex;
            LogViewModel.WriteLine("\"" + Description + "\": " + "изменен индекс сигнала DI - сигнал МП");
        }*/


        /// <summary>
        /// индекс сигнала давления в таблице DI
        /// </summary>
        public int PCindxArrDI
        {
            get { return _PCindxArrDI; }
            set {
          //      if (_PCindxArrDI != value || pc == null)
          //      {
                    _PCindxArrDI = value;

                   /* if (pc != null)
                        pc.IndexChanged -= PC_DI_IndexChanged;
                        */
                    OnPropertyChanged("PCindxArrDI");
                    pc = DIStruct.FindByIndex(value);

                  /*  if (pc != null)
                        pc.IndexChanged += PC_DI_IndexChanged;*/
                }
          //  }
        }

        /// <summary>
        /// изменен индекс сигнала PC
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param
        /*
        private void PC_DI_IndexChanged(object sender, CL.Signal.IndexChangedEventArgs e)
        {
           
            _PCindxArrDI = e.newIndex;
            LogViewModel.WriteLine("\"" + Description + "\": " + "изменен индекс сигнала DI - наличие давления");
        }
        */

        /// <summary>
        /// состояние вспомсистемы
        /// </summary>
        private VSState state;
        /// <summary>
        /// состояние вспомсистемы
        /// </summary>
        public VSState State
        { get {
                return state;
            }
            set
            {
                state = value;
                OnPropertyChanged("StateRUS");
            }
        }

        /// <summary>
        /// вывод состояния в datagrid на русском языке
        /// </summary>
        public string StateRUS
        {
            set { }
            get
            {
                switch (state)
                {
                    case VSState.Work: return "В работе";
                    case VSState.Stop: return "Остановлен";
                    default:
                        return "не определено";
                }
            }
        }

        /// <summary>
        /// конструктор по умолчанию
        /// </summary>
        public VSStruct()
        {
            Description = "Empty";
            Group = "NoGroup";
            ECindxArrDI = -1;
         
        

            MPCindxArrDI = -1;
           // isMPCAnalog = false;
      

            PCindxArrDI = -1;

            state = VSState.Stop;



            bsOnTimer = new Timer(BSOnTime);
            bsOffTimer = new Timer(BSOffTime);
            manualStopTimer = new Timer(noVoltTime);


            manualStopTimer.Elapsed += (sender, e) => { flagManualStop = false; };
            manualStopTimer.AutoReset = false;

            bsOffTimer.Elapsed += (sender, e) => { corrBSState = false; };
            bsOffTimer.AutoReset = false;

            bsOnTimer.Elapsed += (sender, e) => { corrBSState = true; };
            bsOnTimer.AutoReset = false;


        }

        /// <summary>
        /// Обновление ссылок
        /// </summary>
        public void UpdateRefs()
        {
            ECindxArrDI = _ecindx;
            MPCindxArrDI = _MPCindxArrDI;
            PCindxArrDI = _PCindxArrDI;
            ABOindxArrDO = _ABOindxArrDO;
            ABBindxArrDO = _ABBindxArrDO;
            BusSecIndex = _bussec_index;

            AnCmdIndex = _anCmdIndex;

            if (controledAIs != null && controledAIs.Count() > 0)
                foreach (AnalogIOItem item in controledAIs)
                    item.Index = item._index;
        }

        #region ТАЙМЕРЫ
        /// <summary>
        /// таймер для кратковременного пропадания напряжения, уставка таймера
        /// </summary>
        float noVoltTime = 1000;

        /// <summary>
        /// время отсутствия напряжения на СШ, уставка таймера
        /// </summary>
        double BSOffTime = 200;

        /// <summary>
        /// время появления напряжения на СШ, уставка таймера
        /// </summary>
        double BSOnTime = 200;

        /// <summary>
        /// обновление состояния вспомсистемы
        /// </summary>
        /// <param name="dt">задержка между циклами обновления в секундах</param>
        /// <returns></returns>
        /// 
        Timer bsOnTimer;
        Timer bsOffTimer;
        Timer manualStopTimer;

        #endregion

        public void UpdateVS(float dt)
        {
            //алгоритм отключен
            if (!_en)
                return;

            // нет напряжения на секции шин, гасим все дискреты
            if  (corrBSState == false)
            {

                if (mpc != null) mpc.ValDI = false;
                if (ec != null) ec.ValDI = false;
                if (pc != null) pc.ValDI = false;
                State = VSState.Stop;
            }
            else
            {
        //        if (ec != null) ec.ValDI = true;
            }

    
            //----------------------- пропадание напряжения на секции шин ------------------------

            //напруга на СШ пропала
            if (BSVoltage == false && prevBSState == true)
            {
                bsOffTimer.Start();
            //    Debug.WriteLine("Voltage OFF");
            }
            //напруга на СШ появилась
            if (BSVoltage != false && prevBSState == false)
            {
                bsOnTimer.Start();
           //     Debug.WriteLine("Voltage ON");

            }
            prevBSState = (BSVoltage != false);


            //-------------------------- снятие напряжения при команде "стоп по месту" -----------------------------
            if (flagManualStop)
                ECValue = false;
            else
                ECValue = corrBSState;


            //команда включить - включить пускатель
            if ((CmdStart==true || (analogCommand!=null && analogCommand.fVal>0))&&(IsVoltageOk!=false))
                    {
                        if (state == VSState.Stop)
                        {
                            if (mpc != null)
                                mpc.ValDI = true;

                            State = VSState.Work;
                        }
                    }

                    //команда выключить - отключить пускатель
                    if (CmdStop==true || (analogCommand!=null && analogCommand.fVal==0))
                    {
                        if (State == VSState.Work)
                        {
                            if (mpc != null)
                                mpc.ValDI = false;

                            State = VSState.Stop;

                            if (pc != null) pc.ValDI = false;

                        }
                    }



                    if (state == VSState.Work)
                    {
                        /*if (PC_AI != null)
                        {
                            PC_AI.fValAI += (valuePC - PC_AI.fValAI + valuePC/10f) * valuePCspd * dt;
                            if (PC_AI.fValAI > valuePC)
                                PC_AI.fValAI = valuePC;
                        }*/
                        if (pc != null) pc.ValDI = true;
                        if (mpc != null) mpc.ValDI = true;
                        if (controledAIs != null)
                            foreach (AnalogIOItem analog in controledAIs)
                            {
                                if (analog.AI != null)
                                {
                                    if (analogCommand!=null)
                                    {
                                        analog.AI.fValAI += (analogCommand.fVal - analog.AI.fValAI + analogCommand.fVal / 20f) * dt * analog.ValueSpd;
                                        if (analog.AI.fValAI > analogCommand.fVal) analog.AI.fValAI = analogCommand.fVal;
                                    }
                                    else
                                    {
                                        analog.AI.fValAI += (analog.ValueNom - analog.AI.fValAI + analog.ValueNom / 20f) * dt * analog.ValueSpd;
                                        if (analog.AI.fValAI > analog.ValueNom) analog.AI.fValAI = analog.ValueNom;
                                    }
                                }
                            }
                    }

                    /*if (isAVOA)
                    {
                        int indxR = SetRPM_Addr - Sett.Instance.BegAddrR -1;
                        if (indxR >=0 && indxR < RB.R.Length)
                        {
                            SetRPM_Value = RB.R[indxR]/ADCtoRPM;
                        }
                    }*/
                    if (state == VSState.Stop)
                    {
                        /* if (PC_AI != null)
                         {
                             PC_AI.fValAI -= (PC_AI.fValAI + valuePC / 10f) * valuePCspd * dt;
                             if (PC_AI.fValAI <=0)
                                 PC_AI.fValAI = 0;
                         }*/
                        //управление всеми аналогами
                        if (pc != null) pc.ValDI = false;
                        if (mpc != null) mpc.ValDI = false;
                        if (controledAIs != null)
                            foreach (AnalogIOItem analog in controledAIs)
                            {
                                if (analog.AI != null)
                                {
                                    if (analogCommand != null)
                                    {
                                        analog.AI.fValAI -= (analog.AI.fValAI +1) * dt * analog.ValueSpd;
                                        if (analog.AI.fValAI < 0) analog.AI.fValAI = 0;
                                    }
                                    else
                                    {
                                        analog.AI.fValAI -= (analog.AI.fValAI + analog.ValueNom / 20f) * dt * analog.ValueSpd;
                                        if (analog.AI.fValAI < 0) analog.AI.fValAI = 0;
                                    }
                                }
                            }
                    }

            if (state == VSState.Work && prevState != VSState.Work)
                LogWriter.LogWriteLine(Description + ": В работе");

            if (state == VSState.Stop && prevState != VSState.Stop)
                LogWriter.LogWriteLine(Description + ": Остановлен");

            prevState = state;

        }
        VSState prevState;

        /// <summary>
        /// сброс состояния
        /// </summary>
        public void Reset()
        {
            State = VSState.Stop;
            if (mpc != null) mpc.ValDI = false;
        }
        /// <summary>
        /// пуск по месту
        /// </summary>
        public void ManualStart()
        {
            LogWriter.LogWriteLine(Description + ": Пуск по месту");
            //--напряжение на секции шин, сигнал назначен --
            if (bs != null && bs.ValDI == false)
            {
                LogWriter.LogWriteLine(Description + ": Пуск невозможен, нет напряжения на секции шин (сигнал: "+bs.NameDI+")");
            }

            if (state == VSState.Stop)
            if (mpc != null) mpc.ValDI = true;
            State = VSState.Work;
        }

        /// <summary>
        /// стоп по месту
        /// </summary>
        public void ManualStop()
        {
            LogWriter.LogWriteLine(Description + ": Стоп по месту");
            if (mpc != null) mpc.ValDI = false;

            if (state == VSState.Work)
                State = VSState.Stop;

            flagManualStop = true;

            manualStopTimer.Start();

        }



        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        /// <summary>
        /// Установить/снять напряжение на скеции шин извне
        /// </summary>
        /// <param name="value"></param>
        public void SetBusState(bool value)
        {
            if (bs == null)
            {
                bs = new DIStruct();
            }
            if (BusSecIndex == -1)
            bs.ValDI = value;
        }

        /// <summary>
        /// переключить состояние на секции шин
        /// </summary>
        public void ToggleBusState()
        {
            if (bs != null)
            {
                bs.ValDI = !bs.ValDI;
            }
        }

        
    }


}
