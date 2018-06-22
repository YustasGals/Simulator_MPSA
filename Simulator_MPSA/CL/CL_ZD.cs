using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simulator_MPSA.CL;
using System.Xml;
using System.Xml.Serialization;
using System.ComponentModel;

namespace Simulator_MPSA
{
    class CL_ZD
    {
    }
    public enum StateZD { Opening, Open, Middle, Closing, Close, Undef, NoVolt };
    // -------------------------------------------------------------------------------------------------
    [Serializable]
    public class ZDStruct : INotifyPropertyChanged
    {
        private bool _En = false; // наличие в обработке задвижки
        private int _DOBindxArrDO = -1;
        private int _DKBindxArrDO = -1;
        private int _DCBindxArrDO = -1;
        private int _DCBZindxArrDO = -1;
        private bool _changedDO = false; // наличие изменений во входных сигналах блока
        private int _OKCindxArrDI = -1;
        private int _CKCindxArrDI = -1;
        private int _ODCindxArrDI = -1;
        private int _CDCindxArrDI = -1;
        private int _DCindxArrDI = -1;
        private int _VoltindxArrDI = -1;
        private int _MCindxArrDI = -1;
        private int _OPCindxArrDI = -1;
        private float _ZDProc = 0.0f; // процент открытия задвижки
        private bool _changedDI = false; // наличие изменений в выходных сигналах блока
        private float _TmoveZD = 10; // время полного хода звдвижки, сек
        private int _TscZD = 1;  // время схода с концевиков, сек
        private string _description = "Задвижка";  //название задвижки
        private string _group = "";    //название подсистемы в которую входи задвижка

        
        private StateZD _stateZD = StateZD.Close;
        public StateZD StateZD
        {
            get { return _stateZD; }
            set { _stateZD = value; OnPropertyChanged("StateZDRus"); }
        }
        [XmlIgnore]
        public string StateZDRus
        {
            set { }
            get
            {
                switch (_stateZD)
                {
                    case StateZD.Close: return "Закрыта";
                    case StateZD.Open: return "Открыта";
                    case StateZD.Opening: return "Открывается";
                    case StateZD.Closing: return "Закрывается";
                    case StateZD.Middle: return "В промежуточном";
                    default:
                        return "не определено";
                }
            }
        }
        public ZDStruct()
        {

        }
        private int _index;
        public int Index
        {
            get { return _index; }
            set { _index = value; OnPropertyChanged("Index"); }
        }
        /// <summary>
        /// сброс состояния в "закрыто"
        /// </summary>
        public void Reset()
        {
            if (ODC != null) ODC.ValDI = false;
            if (CDC != null) CDC.ValDI = false;

            //состояние - закрыто
            if (OKC != null)
                OKC.ValDI = true;
            if (CKC != null)
                CKC.ValDI = false;

            ZDProc = 0f;
        }
        /// <summary>
        /// задержка отключения МПО МПЗ после появления сигнала с концевика (открыо закрыто)
        /// </summary>
        const float MPDelay_timeout = 1f;
        float MPDelay = 0f;
        /// <summary>
        /// обновить состояние задвижки
        /// </summary>
        /// <param name="dt">задержка между циклами, сек </param>
        /// <returns></returns>
        public void UpdateZD(float dt)
        {
            if (!En)
                return;

            if (ZD_position_ai != null) ZD_position_ai.fValAI = _ZDProc;

            //нет напряжения на секции шин
            if (BS != null && BS.ValDI)
            {
                if (StateZD == StateZD.Opening || StateZD == StateZD.Closing)
                    StateZD = StateZD.Middle;

                if (OKC != null) OKC.ValDI = false;
                if (CKC != null) CKC.ValDI = false;
                if (ODC != null) ODC.ValDI = false;
                if (CDC != null) CDC.ValDI = false;
                if (volt != null) volt.ValDI = false;
                if (MC != null) MC.ValDI = false;
            }
            else
            {
                if (_stateZD == StateZD.Close)
                {
                    //состояние - закрыто
                    if (OKC != null)
                        OKC.ValDI = true;
                    if (CKC != null)
                        CKC.ValDI = false;

                    //выключить МП после задержки
                    MPDelay -= dt;
                    if ((MPDelay < 0) && (CDC != null))
                        CDC.ValDI = false;
                }

                //состояние - открыто
                if (_stateZD == StateZD.Open)
                {
                    //состояние - закрыто
                    if (OKC != null)
                        OKC.ValDI = false;
                    if (CKC != null)
                        CKC.ValDI = true;

                    //выключить МП после задержки
                    MPDelay -= dt;
                    if ((MPDelay < 0) && (ODC != null))
                        ODC.ValDI = false;
                }

                //состояние -открывается
                if (_stateZD == StateZD.Opening)
                {


                    ZDProc += dt / TmoveZD * 100;

                    if (ZDProc >= 100f)
                    {
                        StateZD = StateZD.Open;
                        ZDProc = 100f;

                        MPDelay = MPDelay_timeout;
                        //отключить мпо
                        //    if (ODC != null) ODC.ValDI = false;
                    }
                    else
                    {
                        //включить МПО
                        if (ODC != null) ODC.ValDI = true;
                        //вкл концевики
                        if (OKC != null)
                            OKC.ValDI = true;
                        if (CKC != null)
                            CKC.ValDI = true;
                    }
                }

                //состояние -закрывается
                if (_stateZD == StateZD.Closing)
                {


                    ZDProc -= dt / TmoveZD * 100;

                    if (ZDProc <= 0f)
                    {
                        StateZD = StateZD.Close;
                        ZDProc = 0f;
                        MPDelay = MPDelay_timeout;
                        //отключить мпз
                        //  if (CDC != null) CDC.ValDI = false;
                    }
                    else
                    {
                        //включить МПЗ
                        if (CDC != null) CDC.ValDI = true;
                        //вкл концевики
                        if (OKC != null)
                            OKC.ValDI = true;
                        if (CKC != null)
                            CKC.ValDI = true;
                    }
                }

                if (_stateZD == StateZD.Middle)
                {
                    //отключить МП
                    if (CDC != null) CDC.ValDI = false;
                    if (ODC != null) ODC.ValDI = false;
                }

                //команда открыть
                if ((DOB != null) && (DOB.ValDO))
                {
                    //если закрыта или в промежутке
                    if (_stateZD == StateZD.Close || _stateZD == StateZD.Middle)
                    {
                        //вкл мпо
                        StateZD = StateZD.Opening;

                    }
                }

                //команда закрыть
                if ((DKB != null) && (DKB.ValDO))
                {
                    //если открыта или в промежутке
                    if (_stateZD == StateZD.Open || _stateZD == StateZD.Middle)
                    {
                        StateZD = StateZD.Closing;
                    }
                }

                //команда стоп
                if ((DCB != null) && (DCB.ValDO))
                {
                    if (StateZD == StateZD.Opening || StateZD == StateZD.Closing)
                        StateZD = StateZD.Middle;
                    //  {
                    //отключить МП
                    if (ODC != null) ODC.ValDI = false;
                    if (CDC != null) CDC.ValDI = false;

                }
            }
 
        }
        public void ManualOpen()
        {
            if (_stateZD == StateZD.Close || _stateZD == StateZD.Middle)
                StateZD = StateZD.Opening;
        }

        public void ManualClose()
        {
            if (_stateZD == StateZD.Open || _stateZD == StateZD.Middle)
                StateZD = StateZD.Closing;
        }

        public void ManualStop()
        {
            if (_stateZD == StateZD.Opening || _stateZD == StateZD.Closing)
                StateZD = StateZD.Middle;
        }

        /// <summary>
        /// переключение режима "дистанционный"
        /// </summary>
        public void ToggleDist()
        {
            if (DC != null)
                DC.ValDI = !DC.ValDI;
        }

        public bool En
        {
            get { return _En; }
            set { _En = value; OnPropertyChanged("En"); }
        }

        private int _ZD_Pos_index=-1;
        public int ZD_Pos_index
        {
            get { return _ZD_Pos_index; }
            set {
                _ZD_Pos_index = value;
                ZD_position_ai = AIStruct.FindByIndex(_ZD_Pos_index); }
        }
        /// <summary>
        /// положение затвора ссылка на аналоговый сигнал
        /// </summary>
        private AIStruct ZD_position_ai;

        /// <summary>
        /// положение затвора, свойство для таблицы
        /// </summary>
        public int Position
        {
            get {
                if (ZD_position_ai != null)
                    return (int)ZD_position_ai.fValAI;
                return 0;
            }
            set { }
        }

        public string PositionAIName
        {
            get
            {
                if (ZD_position_ai != null)
                    return ZD_position_ai.NameAI;
                return "сигнал не назначен";
            }
            set { }
        }

        /// <summary>
        /// команда открыть
        /// </summary>
        public int DOBindxArrDO
        {
            get { return _DOBindxArrDO; }
            set {
                _DOBindxArrDO = value;
                OnPropertyChanged("DOBindxArrDO");
                DOB = DOStruct.FindByIndex(_DOBindxArrDO);
            }
        }
        private DOStruct DOB;
        public string DOBName
            { 
                get {
                if (DOB != null)
                    return DOB.NameDO;
                else return "сигнал не назначен";
                    }
            }
        public bool DOBState { get { if (DOB != null) return DOB.ValDO; else return false; } set { } }
        /// <summary>
        /// команда закрыть
        /// </summary>
        public int DKBindxArrDO
        {
            get { return _DKBindxArrDO; }
            set {
                _DKBindxArrDO = value;
                OnPropertyChanged("DKBindxArrDO");
                DKB = DOStruct.FindByIndex(_DKBindxArrDO);
            }
        }
        private DOStruct DKB;
        public string DKBName
        {
            get
            {
                if (DKB != null)
                    return DKB.NameDO;
                else return "сигнал не назначен";
            }
        }
        public bool DKBState { get { if (DKB != null) return DKB.ValDO; else return false; } set { } }
        /// <summary>
        /// Команда - остановить
        /// </summary>
        public int DCBindxArrDO
        {
            get { return _DCBindxArrDO; }
            set {
                _DCBindxArrDO = value;
                OnPropertyChanged("DCBindxArrDO");
                DCB = DOStruct.FindByIndex(_DCBindxArrDO);           
            }
        }
        private DOStruct DCB;
        public string DCBName
        {
            get
            {
                if (DCB != null)
                    return DCB.NameDO;
                else return "сигнал не назначен";
            }
        }
        public bool DCBState { get { if (DCB != null) return DCB.ValDO; else return false; } set { } }
        /// <summary>
        /// команда стоп закрытия
        /// </summary>
        public int DCBZindxArrDO
        {
            get { return _DCBZindxArrDO; }
            set {
                _DCBZindxArrDO = value;
                OnPropertyChanged("DCBZindxArrDO");
                DCBZ = DOStruct.FindByIndex(_DCBZindxArrDO);
            }
        }
        private DOStruct DCBZ;
        public string DCBZName
        {
            get
            {
                if (DCBZ != null)
                    return DCBZ.NameDO;
                else return "сигнал не назначен";
            }
        }
        //public bool DCBZState { get { if (DCBZ != null) return DCBZ.ValDO; else return false; } set { } }

        public bool ChangedDO
        {
            get { return _changedDO; }
            set { _changedDO = value; }
        }
        /// <summary>
        /// Концевой выключатель открытия
        /// </summary>
        public int OKCindxArrDI
        {
            get { return _OKCindxArrDI; }
            set {
                _OKCindxArrDI = value;
               // OnPropertyChanged("DCBZindxArrDO");
                OKC = DIStruct.FindByIndex(_OKCindxArrDI);

                //подписываем наш метод на событие чтобы видеть изменения дискретов в таблице задвижек
                if (OKC != null)
                    OKC.PropertyChanged += OKC_PropertyChanged;
            }
        }

        private void OKC_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged("OKCState");
        }

        private DIStruct OKC;
        public string OKCName
        {
            get
            {
                /*if (OKC != null)
                    return OKC.NameDI;
                else return "сигнал не назначен";*/
                return DIStruct.GetNameByIndex(_OKCindxArrDI);
            }
        }
        public bool OKCState { get { if (OKC != null) return OKC.ValDI; else return false; } set { } }

        /// <summary>
        /// концевой выключатель закрытия
        /// </summary>
        public int CKCindxArrDI
        {
            get { return _CKCindxArrDI; }
            set {
                _CKCindxArrDI = value;
                //OnPropertyChanged("DCBZindxArrDO");
                CKC = DIStruct.FindByIndex(_CKCindxArrDI);
            }
        }
        private DIStruct CKC;
        public string CKCName
        {
            get
            {
                return DIStruct.GetNameByIndex(_CKCindxArrDI);
            }
        }
        public bool CKCState { get { if (CKC != null) return CKC.ValDI; else return false; } set { } }

        /// <summary>
        /// сигнал от МПО
        /// </summary>
        public int ODCindxArrDI
        {
            get { return _ODCindxArrDI; }
            set {
                _ODCindxArrDI = value;
                OnPropertyChanged("ODCindxArrDI");
                ODC = DIStruct.FindByIndex(_ODCindxArrDI);
            }
        }
        private DIStruct ODC;
        public string ODCName
        {
            get
            {
                if (ODC != null)
                    return ODC.NameDI;
                else return "сигнал не назначен";
            }
        }
        public bool ODCState { get { if (ODC != null) return ODC.ValDI; else return false; } set { } }


        /// <summary>
        /// наличие напряжения на секции шин
        /// </summary>
        private DIStruct BS;

        /// <summary>
        /// Наличие напряжения на СШ индекс сигнала
        /// </summary>
        public int _bsindex=-1;
        /// <summary>
        /// Наличие напряжения на СШ индекс сигнала
        /// </summary>
        public int BSIndex
        {
            get { return _bsindex; }
            set
            {
                _bsindex = value;
                if (_bsindex >= 0 && _bsindex < DIStruct.items.Count)
                    BS = DIStruct.items[_bsindex];
                else
                    BS = null;
            }
        }
        public string BSName
        {
            set { }
            get
            {
                if (BS != null)
                    return BS.NameDI;
                else return "сигнал не назначен";
            }
        }
        /// <summary>
        /// сигнал от МПЗ
        /// </summary>
        public int CDCindxArrDI
        {
            get { return _CDCindxArrDI; }
            set {
                _CDCindxArrDI = value;
                OnPropertyChanged("CDCindxArrDI");
                CDC = DIStruct.FindByIndex(_CDCindxArrDI);
            }
        }
        private DIStruct CDC;
        public string CDCName
        {
            get
            {
                if (CDC != null)
                    return CDC.NameDI;
                else return "сигнал не назначен";
            }
        }
        public bool CDCState { get { if (CDC != null) return CDC.ValDI; else return false; } set { } }

        /// <summary>
        /// дистанционное управление
        /// </summary>
        public int DCindxArrDI
        {
            get { return _DCindxArrDI; }
            set {
                _DCindxArrDI = value;
                OnPropertyChanged("DCindxArrDI");
                DC = DIStruct.FindByIndex(_DCindxArrDI);
            }
        }
        private DIStruct DC;
        public string DCName
        {
            get
            {
                if (DC != null)
                    return DC.NameDI;
                else return "сигнал не назначен";
            }
        }
        public bool DCState { get { if (DC != null) return DC.ValDI; else return false; } set { } }

        /// <summary>
        /// наличие напряжения
        /// </summary>
        public int VoltindxArrDI
        {
            get { return _VoltindxArrDI; }
            set {
                _VoltindxArrDI = value;
                OnPropertyChanged("VolindxArrDI");
                volt = DIStruct.FindByIndex(_VoltindxArrDI);
            }
        }
        private DIStruct volt;
        public string VoltName
        {
            get
            {
                if (volt != null)
                    return volt.NameDI;
                else return "сигнал не назначен";
            }
        }
        public bool VoltState { get { if (volt != null) return volt.ValDI; else return false; } set { } }

        /// <summary>
        /// сработала муфта 
        /// </summary>
        public int MCindxArrDI
        {
            get { return _MCindxArrDI; }
            set {
            
                _MCindxArrDI = value;
                OnPropertyChanged("MCindxArrDI");
                MC = DIStruct.FindByIndex(_MCindxArrDI);
            }
        }
        private DIStruct MC;
        public string MCName
        {
            get
            {
                if (MC != null)
                    return MC.NameDI;
                else return "сигнал не назначен";
            }
        }
        public bool MCState { get { if (MC != null) return MC.ValDI; else return false; } set { } }

        /// <summary>
        /// авария привода
        /// </summary>
        public int OPCindxArrDI
        {
            get { return _OPCindxArrDI; }
            set {
                _OPCindxArrDI = value;
                OnPropertyChanged("OPCindxArrDI");
                OPC = DIStruct.FindByIndex(_OPCindxArrDI);
            }
        }
        private DIStruct OPC;
        public bool OPCState { get { if (OPC != null) return OPC.ValDI; else return false; } set { } }

        public string OPCName
        {
            get
            {
                if (OPC != null)
                    return OPC.NameDI;
                else return "сигнал не назначен";
            }
        }

        public float ZDProc
        {
            get { return _ZDProc; }
            set { _ZDProc = value; OnPropertyChanged("Percent"); }
        }
        /// <summary>
        /// процент открытия для вывода на экран
        /// </summary>
        public int Percent { get { return (int)_ZDProc; } set { } }

        public bool ChangedDI
        {
            get { return _changedDI; }
            set { _changedDI = value; }
        }
        public float TmoveZD
        {
            get { return _TmoveZD; }
            set { _TmoveZD = value; }
        }
        public int TscZD
        {
            get { return _TscZD; }
            set { _TscZD = value; }
        }
        public string Description
        {
            get { return _description; }
            set { _description = value; OnPropertyChanged("Description"); }
        }
        public string Group
        {
            get { return _group; }
            set { _group = value; OnPropertyChanged("Group"); }
        }

        /// <summary>
        /// Обновление ссылок на сигналы DI,DO,AI, следует вызывать после загрузки файла настроек
        /// </summary>
        public void UpdateRefs()
        {
            
            DOBindxArrDO = _DOBindxArrDO;
            DCBindxArrDO = _DCBindxArrDO;
            DKBindxArrDO = _DKBindxArrDO;
            DCBZindxArrDO = _DCBZindxArrDO;
            ODCindxArrDI = _ODCindxArrDI;
            CDCindxArrDI = _CDCindxArrDI;
            OKCindxArrDI = _OKCindxArrDI;
            CKCindxArrDI = _CKCindxArrDI;
            DCindxArrDI = _DCindxArrDI;
            VoltindxArrDI = _VoltindxArrDI;
            MCindxArrDI = _MCindxArrDI;
            ZD_Pos_index = _ZD_Pos_index;
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }


   
}
