using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Simulator_MPSA.CL;
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


        private DOStruct DOB = null;
        private int _DOBindxArrDO;
        /// <summary>
        /// для вывода состояиня в datagrid
        /// </summary>
        public bool DOBState
        {
            get
            {
                if (DOB != null)
                    return DOB.ValDO;
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
                DOB = DOStruct.FindByIndex(_DOBindxArrDO);
            }
        }
        /// <summary>
        /// команда "клапан открыть" - наименование сигнала
        /// </summary>
        public string DOBName
        { get
            {
                if (DOB != null)
                    return DOB.NameDO;
                else return "сигнал не назначен"; 
            }
        }

        private DOStruct DKB = null;
        private int _DKBindxArrDO;
        public bool DKBState
        {
            get
            {
                if (DKB != null)
                    return DKB.ValDO;
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
                DKB = DOStruct.FindByIndex(_DKBindxArrDO);
            }
        }
        /// <summary>
        /// команда "клапан закрыть" - наименование сигнала
        /// </summary>
        public string DKBName
        {
            get
            {
                if (DKB != null)
                    return DKB.NameDO;
                else return "сигнал не назначен";
            }
        }

        public bool changedDO=false;


        private DIStruct OKC = null;
        private int _OKCindxArrDI=0;
        public bool OKCState
        {
            get
            {
                if (OKC != null)
                    return OKC.ValDI;
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
                OKC = DIStruct.FindByIndex(_OKCindxArrDI);
            }
        }
        /// <summary>
        /// сигнал "клапан открыт" - наименование сигнала
        /// </summary>
        public string OKCName
        {
            get
            {
                if (OKC != null)
                    return OKC.NameDI;
                else return "сигнал не назначен";
            }
        }

        private DIStruct CKC = null;
        private int _CKCindxArrDI = 0;
        public bool CKCState
        {
            get
            {
                if (CKC != null)
                    return CKC.ValDI;
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
                CKC = DIStruct.FindByIndex(_CKCindxArrDI);
            }
        }
        /// <summary>
        /// сигнал "клапан закрыт" - наименование сигнала
        /// </summary>
        public string CKCName
        {
            get
            {
                if (CKC != null)
                    return CKC.NameDI;
                else return "сигнал не назначен";
            }
        }

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
        public int TmoveKL = 5;

        public KLStruct()
        {
            KLProc = 0f;
            State = KLState.Close;


        }//

        /// <summary>
        /// обновление состояния вспомсистемы
        /// </summary>
        /// <param name="dt">задержка между циклами обновления в секундах</param>
        /// <returns></returns>
        public void UpdateKL(float dt)
        {
            //открыть
            if ((DOB != null) && (DOB.ValDO))
            {
                if (State == KLState.Close || State == KLState.Middle)
                {
                    State = KLState.Opening;
                }
            }

            //закрыть
            if ((DKB != null) && (DKB.ValDO))
            {
                if (State == KLState.Open || State == KLState.Middle)
                {
                    State = KLState.Closing;
                }
            }

            //состояние - открывается
            if (State == KLState.Opening)
            {
                if (CKC !=null) CKC.ValDI = true;
                if (OKC !=null)  OKC.ValDI = true;

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
                if (CKC != null) CKC.ValDI = true;
                if (OKC != null) OKC.ValDI = true;

                KLProc -= dt / TmoveKL * 100;
                if (KLProc < 0f)
                {
                    KLProc = 0f;
                    State = KLState.Close;
                  
                }
            }

            if (State == KLState.Close)
            {
                if (CKC != null) CKC.ValDI = true;
                if (OKC != null) OKC.ValDI = false;
                KLProc = 0f;
            }

            if (State == KLState.Open)
            {
                if (CKC != null) CKC.ValDI = false;
                if (OKC != null) OKC.ValDI = true;
                KLProc = 100f;
            }

            if (State == KLState.Middle)
            {
                if (CKC != null) CKC.ValDI = true;
                if (OKC != null) OKC.ValDI = true;
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

    }


}
