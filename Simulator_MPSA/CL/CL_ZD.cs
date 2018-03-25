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
    public enum StateZD { Opening, Open, Middle, Closing, Close, Undef };
    // -------------------------------------------------------------------------------------------------
    [Serializable]
    public class ZDStruct : INotifyPropertyChanged
    {
        private bool _En = false; // наличие в обработке задвижки
        private int _DOBindxArrDO = 0;
        private int _DKBindxArrDO = 0;
        private int _DCBindxArrDO = 0;
        private int _DCBZindxArrDO = 0;
        private bool _changedDO = false; // наличие изменений во входных сигналах блока
        private int _OKCindxArrDI = 0;
        private int _CKCindxArrDI = 0;
        private int _ODCindxArrDI = 0;
        private int _CDCindxArrDI = 0;
        private int _DCindxArrDI = 0;
        private int _VoltindxArrDI = 0;
        private int _MCindxArrDI = 0;
        private int _OPCindxArrDI = 0;
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
            set { _stateZD = value; OnPropertyChanged("StateZD"); }
        }

        public ZDStruct()
        {

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

            if (volt !=null)
                volt.ValDI = _En;

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

                if (ZDProc > 100f)
                {
                    StateZD = StateZD.Open;
                    ZDProc = 100f;

                    MPDelay = MPDelay_timeout;
                    //отключить мпо
                //    if (ODC != null) ODC.ValDI = false;
                }
            }

            //состояние -закрывается
            if (_stateZD == StateZD.Closing)
            {
                ZDProc -= dt / TmoveZD * 100;

                if (ZDProc < 0f)
                {
                    StateZD = StateZD.Close;
                    ZDProc = 0f;
                    MPDelay = MPDelay_timeout;
                    //отключить мпз
                    //  if (CDC != null) CDC.ValDI = false;
                }
            }

            //команда открыть
            if ((DOB != null) && (DOB.ValDO))
            {
                //если закрыта или в промежутке
                if (_stateZD == StateZD.Close || _stateZD == StateZD.Middle)
                {
                    //вкл мпо
                    StateZD = StateZD.Opening;
                    if (ODC != null) ODC.ValDI = true;

                    //вкл концевики
                    if (OKC != null)
                        OKC.ValDI = true;
                    if (CKC != null)
                        CKC.ValDI = true;
                }
            }

            //команда закрыть
            if ((DKB != null) && (DKB.ValDO))
            {
                //если открыта или в промежутке
                if (_stateZD == StateZD.Open || _stateZD == StateZD.Middle)
                {
                    //вкл мпз
                    StateZD = StateZD.Closing;
                    if (CDC != null) CDC.ValDI = true;

                    //вкл концевики
                    if (OKC != null)
                        OKC.ValDI = true;
                    if (CKC != null)
                        CKC.ValDI = true;

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

        public bool En
        {
            get { return _En; }
            set { _En = value; OnPropertyChanged("En"); }
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
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }


   
}
