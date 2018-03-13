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
        public bool En
        { get; set; }
        
        private KLState _state;
        public KLState State
        { get { return _state; }
          set { _state = value; }
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


        private DOStruct DOB=null;
        private int _DOBindxArrDO;
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
        public int TmoveKL=0;

        public KLStruct()
        { }

        /// <summary>
        /// обновление состояния вспомсистемы
        /// </summary>
        /// <param name="dt">задержка между циклами обновления в секундах</param>
        /// <returns></returns>
        public float UpdateKL(float dt)
        {
            // тут будет логика  !!!
            return KLProc;
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
