using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Simulator_MPSA.CL;
using System.Xml.Serialization;
using Simulator_MPSA.CL.Signal;

namespace Simulator_MPSA
{
    class CL_KL
    {
    }
    public enum KLState {Open, Opening, Middle, Close, Closing }; //состояние клапана
    // -------------------------------------------------------------------------------------------------
    [Serializable]
    public class KLStruct : INotifyPropertyChanged
    {
        private bool _en;
        public bool En
        { get { return _en; } set { _en = value; OnPropertyChanged("En"); } }

        private KLState _state=KLState.Close;
        public KLState State
        { get { return _state; }
            set { _state = value; OnPropertyChanged("State"); }
        }
        [XmlIgnore]
        public string StateRus
        {
            set { }
            get
            {
                switch (_state)
                {
                    case KLState.Close: return "Закрыто";
                    case KLState.Open: return "Открыто";
                    case KLState.Opening: return "Открывается";
                    case KLState.Closing: return "Закрывается";
                    case KLState.Middle: return "Промежуточное";
                    default: return "не определено";
                }
            }
        }
        private int _index;
        public int Index
        {
            get { return _index; }
            set { _index = value; OnPropertyChanged("Index"); }
        }
        private string name;
        public string Description
        {
            get { return name; }
            set { name = value; OnPropertyChanged("Description"); }
        }

        private string group;
        public string Group
        {
            get { return group; }
            set { group = value; OnPropertyChanged("Group"); }
        }


        private DOStruct dob = null;
        private int _DOBindxArrDO=-1;
        /// <summary>
        /// для вывода состояиня в datagrid
        /// </summary>
        public bool DOBState
        {
            get
            {
                if (dob != null)
                    return dob.ValDO;
                else return false;
            }
        }
        /// <summary>
        /// команда "клапан открыть" индекс сигнала
        /// </summary>
        public int DOBindxArrDO
        {
            get { return _DOBindxArrDO; }
            set {
                _DOBindxArrDO = value;
                OnPropertyChanged("DOBindxArrDI");
                dob = DOStruct.FindByIndex(_DOBindxArrDO);
            }
        }
        /// <summary>
        /// команда "клапан открыть" - наименование сигнала
        /// </summary>
        public string DOBName
        { get
            {
                if (dob != null)
                    return dob.NameDO;
                else return "сигнал не назначен"; 
            }
        }

        private DOStruct dkb = null;
        private int _DKBindxArrDO=-1;
        public bool DKBState
        {
            get
            {
                if (dkb != null)
                    return dkb.ValDO;
                else return false;
            }
        }

        
        /// <summary>
        /// команда "клапан закрыть" индекс сигнала
        /// </summary>
        public int DKBindxArrDO
        {
            get { return _DKBindxArrDO; }
            set
            {
                _DKBindxArrDO = value;
                OnPropertyChanged("DKBindxArrDI");
                dkb = DOStruct.FindByIndex(_DKBindxArrDO);
            }
        }
        /// <summary>
        /// команда "клапан закрыть" - наименование сигнала
        /// </summary>
        public string DKBName
        {
            get
            {
                if (dkb != null)
                    return dkb.NameDO;
                else return "сигнал не назначен";
            }
        }

        [XmlIgnore]
        public bool changedDO=false;


        private DIStruct okc = null;
        private int _OKCindxArrDI=-1;
        public bool OKCState
        {
            get
            {
                if (okc != null)
                    return okc.ValDI;
                else return false;
            }
        }
        /// <summary>
        /// сигнал "клапан открыт" индекс сигнала
        /// </summary>
        public int OKCindxArrDI
        {
            get { return _OKCindxArrDI; }
            set
            {
                _OKCindxArrDI = value;
                OnPropertyChanged("OKCindxArrDI");
                okc = DIStruct.FindByIndex(_OKCindxArrDI);
            }
        }
        /// <summary>
        /// сигнал "клапан открыт" - наименование сигнала
        /// </summary>
        public string OKCName
        {
            get
            {
                if (okc != null)
                    return okc.NameDI;
                else return "сигнал не назначен";
            }
        }

        private DIStruct ckc = null;
        private int _CKCindxArrDI = -1;
        public bool CKCState
        {
            get
            {
                if (ckc != null)
                    return ckc.ValDI;
                else return false;
            }
        }
        /// <summary>
        /// сигнал "клапан закрыт"
        /// </summary>
        public int CKCindxArrDI
        {
            get { return _CKCindxArrDI; }
            set
            {
                _CKCindxArrDI = value;
                OnPropertyChanged("CKCindxArrDI");
                ckc = DIStruct.FindByIndex(_CKCindxArrDI);
            }
        }
        /// <summary>
        /// сигнал "клапан закрыт" - наименование сигнала
        /// </summary>
        public string CKCName
        {
            get
            {
                if (ckc != null)
                    return ckc.NameDI;
                else return "сигнал не назначен";
            }
        }
        [XmlIgnore]
        public bool changedDI=false;

        private float _KLProc=0.0f;
        /// <summary>
        /// положение клапана %
        /// </summary>
        public float KLProc
        {
            get { return _KLProc; }
            set { _KLProc = value; OnPropertyChanged("KLProc"); }
        }
        private int _TmoveKL = 5;
        public int TmoveKL
        {
            get { return _TmoveKL;  }
            set { _TmoveKL = value; OnPropertyChanged("TmoveKL"); }
        }

        public KLStruct()
        {
            KLProc = 0f;
            State = KLState.Close;


        }//
        bool? bDKB=null;
        bool? bDOB=null;
        /// <summary>
        /// обновление состояния вспомсистемы
        /// </summary>
        /// <param name="dt">задержка между циклами обновления в секундах</param>
        /// <returns></returns>
        public void UpdateKL(float dt)
        {
            if (!_en)
                return;

            if (dob != null)
                bDOB = dob.ValDO;

            if (dkb != null)
                bDKB = dkb.ValDO;

            //открыть если есть команда 
            if (bDOB==true)
            {
                if (State == KLState.Close || State == KLState.Middle)
                {
                    State = KLState.Opening;
                }
            }

            //закрыть если есть команда, если команда "закрыть" не привязана то закрываем при отсутствии команды "открыть"
            if (bDKB == true || (bDKB==null && bDOB == false))
            {
                if (State == KLState.Open || State == KLState.Middle)
                {
                    State = KLState.Closing;
                }
            }

            //состояние - открывается
            if (State == KLState.Opening)
            {
                if (ckc !=null) ckc.ValDI = true;
                if (okc !=null)  okc.ValDI = true;

                KLProc += dt / TmoveKL * 100;
                if (KLProc > 100f)
                {
                    KLProc = 100;
                    State = KLState.Open;
                  
                }

            }

            //состояние - закрывается
            if (State == KLState.Closing)
            {
                if (ckc != null) ckc.ValDI = true;
                if (okc != null) okc.ValDI = true;

                KLProc -= dt / TmoveKL * 100;
                if (KLProc < 0f)
                {
                    KLProc = 0f;
                    State = KLState.Close;
                  
                }
            }

            if (State == KLState.Close)
            {
                if (ckc != null) ckc.ValDI = true;
                if (okc != null) okc.ValDI = false;
                KLProc = 0f;
            }

            if (State == KLState.Open)
            {
                if (ckc != null) ckc.ValDI = false;
                if (okc != null) okc.ValDI = true;
                KLProc = 100f;
            }

            if (State == KLState.Middle)
            {
                if (ckc != null) ckc.ValDI = true;
                if (okc != null) okc.ValDI = true;
            }


        }

        public void ManualOpen()
        {
            State = KLState.Opening;
        }
        public void ManualClose()
        {
            State = KLState.Closing;
        }
        public void ManualStop()
        {
            if (State == KLState.Opening || State == KLState.Closing)
            State = KLState.Middle;
        }

        public void UpdateRefs()
        {
            DOBindxArrDO = _DOBindxArrDO;
            DKBindxArrDO = _DKBindxArrDO;
            OKCindxArrDI = _OKCindxArrDI;
            CKCindxArrDI = _CKCindxArrDI;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        internal DIStruct OKC
        {
            get
            {
                if (okc != null)
                    return okc;
                else
                    if (OKCindxArrDI >= 0 && OKCindxArrDI < DIStruct.items.Count)
                    {
                        okc = DIStruct.items[OKCindxArrDI];
                        return okc;
                    }

                return null;
            }
        }

        internal DIStruct CKC
        {
            get
            {
                if (ckc != null)
                    return ckc;
                else
                    if (CKCindxArrDI >= 0 && CKCindxArrDI < DIStruct.items.Count)
                {
                    ckc = DIStruct.items[CKCindxArrDI];
                    return ckc;
                }

                return null;
            }
        }

        internal DOStruct DOB
        {
            get
            {
                if (dob != null)
                    return dob;
                else
                    if (DOBindxArrDO >=0 && DOBindxArrDO < DOStruct.items.Count)
                {
                    dob = DOStruct.items[DOBindxArrDO];
                    return dob;
                }

                return null;
            }
        }

        internal DOStruct DKB
        {
            get
            {
                if (dkb != null)
                    return dkb;
                else
                    if (DKBindxArrDO >= 0 && DKBindxArrDO < DOStruct.items.Count)
                {
                    dkb = DOStruct.items[DKBindxArrDO];
                    return dkb;
                }

                return null;
            }
        }


    }


}
