using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Simulator_MPSA.CL;
using System.Xml.Serialization;
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
        private int _index=0;
        public int Index
        {
            get { return _index; }
            set { _index = value; OnPropertyChanged("Index"); }
        }

        /// <summary>
        /// Наличие напряжения на секции шин
        /// </summary>
        private DIStruct BS;
        private int _bussec_index=-1;
        public int BusSecIndex
        {
            get { return _bussec_index; }
            set
            {
                if (_bussec_index != value || BS == null)
                {
                    if (BS != null)
                        BS.IndexChanged -= BS_IndexChanged;

                    _bussec_index = value;
                    if (value > -1)
                        BS = DIStruct.FindByIndex(value);

                    if (BS != null)
                        BS.IndexChanged += BS_IndexChanged;
                }
            }
        }

        private void BS_IndexChanged(object sender, CL.Signal.IndexChangedEventArgs e)
        {
            _bussec_index = e.newIndex;
            LogViewModel.WriteLine("\"" + Description + "\": " + "изменен индекс сигнала DI - наличие напряжения на СШ");
            //  throw new NotImplementedException();
        }

        public string BusSectionName
        {
            get
            {
                if (BS != null)
                    return BS.NameDI;
                else return "сигнал не назначен";
            }
            set { }
        }

        // состояния входов-выходов
        /// <summary>
        /// сигнал - оманда на включение
        /// </summary>
        private DOStruct ABB;
        private int _ABBindxArrDO = -1;
        /// <summary>
        /// сигнал - команда на включение, индекс в таблице DO
        /// </summary>
        public int ABBindxArrDO
        {
            get { return _ABBindxArrDO; }
            set {
                if (value != _ABBindxArrDO || ABB == null) //если индекс изменен
                {
                    //отписываемся от сигнала если он был
                    if (ABB != null)
                        ABB.IndexChanged -= ABB_IndexChanged;

                    
                    _ABBindxArrDO = value;

                    //ищем ссылку на новый сигнал по индексу
                    ABB = DOStruct.FindByIndex(_ABBindxArrDO);

                    //подписываемся на новый сигнал
                    if (ABB != null)
                        ABB.IndexChanged += ABB_IndexChanged;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ABB_IndexChanged(object sender, CL.Signal.IndexChangedEventArgs e)
        {
            // throw new NotImplementedException();
            _ABBindxArrDO = e.newIndex;
            LogViewModel.WriteLine("\"" + Description + "\": " + "изменен индекс сигнала DO - команда на включение");
        }

        public string ABBName
        { get
            {
                /*if (ABB != null)
                    return ABB.NameDO;
                else return "сигнал не назначен";*/
                if (_ABBindxArrDO > -1 && _ABBindxArrDO < DOStruct.items.Count)
                    return DOStruct.items[_ABBindxArrDO].NameDO;
                else return "сигнал не назначен";                
            }
        }
        /// <summary>
        /// сигнал - команда на отключение
        /// </summary>
        private DOStruct ABO;
        private int _ABOindxArrDO = -1;
        /// <summary>
        /// команда на отключение, индекс в таблице DO
        /// </summary>
        public int ABOindxArrDO
        {
            get { return _ABOindxArrDO; }
            set
            {
                if (value != _ABOindxArrDO || ABO == null)
                {
                    _ABOindxArrDO = value;

                    //ссылка изменена - отписывается от старого элемента
                    if (ABO != null)
                        ABO.IndexChanged -= ABO_IndexChanged;
                    //находим новый сигнал                   
                    ABO = DOStruct.FindByIndex(_ABOindxArrDO);
                    //подписываемся на него
                    if (ABO != null)
                        ABO.IndexChanged += ABO_IndexChanged;
                }
            }
        }

        private void ABO_IndexChanged(object sender, CL.Signal.IndexChangedEventArgs e)
        {
            _ABOindxArrDO = e.newIndex;
            LogViewModel.WriteLine("\"" + Description + "\": " + "изменен индекс сигнала DO - команда на отключение");
            //throw new NotImplementedException();
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
     
        private float _valueEC;

        private int _MPCindxArrDI = -1;

        /// <summary>
        /// наличие давления индекс дискретного сигнала
        /// </summary>
        private int _PCindxArrDI = -1;
        /// <summary>
        /// наличие давления индекс аналогового сигнала
        /// </summary>
        private int _PCindxArrAI = -1;

        private DIStruct MPC_DI = null;

        private DIStruct EC_DI = null;

        /// <summary>
        /// ссылка на сигнал DI наличие давления
        /// </summary>
        private DIStruct PC_DI = null;

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
                if (_anCmdIndex != value || analogCommand == null)
                {
                    if (analogCommand != null)
                        analogCommand.IndexChanged -= AnalogCommand_IndexChanged;
                    _anCmdIndex = value;
                    if (value >= 0 && value < AOStruct.items.Count)
                    {
                        analogCommand = AOStruct.items[value];
                        analogCommand.IndexChanged += AnalogCommand_IndexChanged;
                    }
                    else
                        analogCommand = null;
                }
            }
            get { return _anCmdIndex; }
        }

        /// <summary>
        /// Отслеживание изменения индекса в таблице AO
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AnalogCommand_IndexChanged(object sender, CL.Signal.IndexChangedEventArgs e)
        {
            _anCmdIndex = e.newIndex;
            LogViewModel.WriteLine("\""+Description + "\": "+"изменен индекс сигнала АО");
        }


        /// <summary>
        /// аналоговый вход - уставка
        /// </summary>
        public AOStruct analogCommand;

        [XmlIgnore]
        public string AnCmdName
        {
            set { }
            get
            {
                if (analogCommand != null)
                    return analogCommand.Name;
                else
                    return "Сигнал не определен";
            }
        }
        /// <summary>
        /// индекс сигнала напряжения в таблице DI, AI
        /// </summary>
        public int ECindxArrDI
        { get { return _ecindx; }
            set {
                if (_ecindx != value || EC_DI ==null)
                {
                    if (EC_DI != null) EC_DI.IndexChanged -= EC_DI_IndexChanged;
                    _ecindx = value;
                    
                    OnPropertyChanged("ECindxArrDI");

                    EC_DI = DIStruct.FindByIndex(value);
                    if (EC_DI != null)
                        EC_DI.IndexChanged += EC_DI_IndexChanged;

                }
            }
        }

        private void EC_DI_IndexChanged(object sender, CL.Signal.IndexChangedEventArgs e)
        {
            //    throw new NotImplementedException();
            _ecindx = e.newIndex;
            LogViewModel.WriteLine("\"" + Description + "\": " + "изменен индекс сигнала DI - наличие напряжения");
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
                if (_MPCindxArrDI != value || MPC_DI == null)
                {
                    if (MPC_DI != null)
                        MPC_DI.IndexChanged -= MPC_DI_IndexChanged;

                    _MPCindxArrDI = value; OnPropertyChanged("MPCindxArrDI");
                    MPC_DI = DIStruct.FindByIndex(_MPCindxArrDI);
                    if (MPC_DI != null)
                        MPC_DI.IndexChanged += MPC_DI_IndexChanged;
                }
            }
        }

        private void MPC_DI_IndexChanged(object sender, CL.Signal.IndexChangedEventArgs e)
        {
            _MPCindxArrDI = e.newIndex;
            LogViewModel.WriteLine("\"" + Description + "\": " + "изменен индекс сигнала DI - сигнал МП");
        }


        /// <summary>
        /// индекс сигнала давления в таблице DI
        /// </summary>
        public int PCindxArrDI
        {
            get { return _PCindxArrDI; }
            set {
                if (_PCindxArrDI != value || PC_DI == null)
                {
                    _PCindxArrDI = value;

                    if (PC_DI != null)
                        PC_DI.IndexChanged -= PC_DI_IndexChanged;

                    OnPropertyChanged("PCindxArrDI");
                    PC_DI = DIStruct.FindByIndex(value);

                    if (PC_DI != null)
                        PC_DI.IndexChanged += PC_DI_IndexChanged;
                }
            }
        }

        private void PC_DI_IndexChanged(object sender, CL.Signal.IndexChangedEventArgs e)
        {
            // throw new NotImplementedException();
            _PCindxArrDI = e.newIndex;
            LogViewModel.WriteLine("\"" + Description + "\": " + "изменен индекс сигнала DI - наличие давления");
        }


        /// <summary>
        /// Возвращает название присвоенного сигнала наличия напряжения
        /// </summary>
        public string ECName
        {
            get
            {
               
                    //if (EC_DI != null)
                    //    return EC_DI.NameDI;
                    if (_ecindx > -1)
                        return DIStruct.items[_ecindx].NameDI;
               

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
                    //if (MPC_DI != null)
                    //    return MPC_DI.NameDI;
                    if (MPC_DI != null)
                        return MPC_DI.NameDI;
                    else
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
                /* if (_PCindxArrDI > -1)
                     if (DIStruct.FindByIndex(_PCindxArrDI) != null)
                         return DIStruct.FindByIndex(_PCindxArrDI).NameDI;
                 */
                if (PC_DI != null)
                    return PC_DI.NameDI;
                else
                    return "сигнал не назначен";
            }
        }

        public bool changedDI;
        public float VSProc;
        public int TmoveVS;

        /// <summary>
        /// адрес задания частоты в контроллере
        /// </summary>
    //    public int SetRPM_Addr=-1;

        /// <summary>
        /// задание частоты %*640
        /// </summary>
    //    public int SetRPM_Value;
      

     //   public int ADCtoRPM = 640;
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
                OnPropertyChanged("StateRUS");
            }
        }
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

        public VSStruct()
        {
            Description = "Empty";
            Group = "NoGroup";
            ECindxArrDI = -1;
         
            valueEC = 0.0f;

            MPCindxArrDI = -1;
           // isMPCAnalog = false;
      

            PCindxArrDI = -1;
          
  


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
            ABOindxArrDO = _ABOindxArrDO;
            ABBindxArrDO = _ABBindxArrDO;
            BusSecIndex = _bussec_index;

            AnCmdIndex = _anCmdIndex;
        }
       
        /// <summary>
        /// обновление состояния вспомсистемы
        /// </summary>
        /// <param name="dt">задержка между циклами обновления в секундах</param>
        /// <returns></returns>
        public void UpdateVS(float dt)
        {


            if (_en)
            {
                // нет напряжения на секции шин
                if ((BS != null) && (BS.ValDI == false))
                {
                    if (MPC_DI != null) MPC_DI.ValDI = false;
                    if (EC_DI != null) EC_DI.ValDI = false;
                    if (PC_DI != null) PC_DI.ValDI = false;
                    State = VSState.Stop;
                }
                else
                {
                    if (EC_DI != null) EC_DI.ValDI = true;
                    //команда включить - включить пускатель
                    if ((ABB != null) && (ABB.ValDO) || (analogCommand!=null && analogCommand.fVal>0))
                    {
                        if ((state == VSState.Stop || state == VSState.Stoping))
                        {
                            if (MPC_DI != null)
                                MPC_DI.ValDI = true;

                            State = VSState.Work;
                        }
                    }

                    //команда выключить - отключить пускатель
                    if ((ABO != null) && (ABO.ValDO) || (analogCommand!=null && analogCommand.fVal==0))
                    {
                        if (State == VSState.Starting || State == VSState.Work)
                        {
                            if (MPC_DI != null)
                                MPC_DI.ValDI = false;

                            State = VSState.Stop;

                            if (PC_DI != null) PC_DI.ValDI = false;

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
                        if (PC_DI != null) PC_DI.ValDI = true;
                        if (MPC_DI != null) MPC_DI.ValDI = true;
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
                        if (PC_DI != null) PC_DI.ValDI = false;
                        if (MPC_DI != null) MPC_DI.ValDI = false;
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
                }//bs
            }
            else
            {
                if (EC_DI != null) EC_DI.ValDI = false;
            }
            

        }
        /// <summary>
        /// сброс состояния
        /// </summary>
        public void Reset()
        {
            State = VSState.Stop;
            if (MPC_DI != null) MPC_DI.ValDI = false;
        }
        /// <summary>
        /// пуск по месту
        /// </summary>
        public void ManualStart()
        {
            if (state == VSState.Stop || state == VSState.Stoping)
            if (MPC_DI != null) MPC_DI.ValDI = true;
            State = VSState.Work;
        }

        /// <summary>
        /// стоп по месту
        /// </summary>
        public void ManualStop()
        {
            if (MPC_DI != null) MPC_DI.ValDI = false;

            if (state == VSState.Starting || state == VSState.Work)
                State = VSState.Stop;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }


}
