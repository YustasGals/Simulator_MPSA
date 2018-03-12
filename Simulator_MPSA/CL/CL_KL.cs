using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
namespace Simulator_MPSA
{
    class CL_KL
    {
    }
    // -------------------------------------------------------------------------------------------------
    [Serializable]
    public class KLStruct : INotifyPropertyChanged
    {
        public bool En
        { get; set; }
        private string name;
        public string Name
        {
            get { return name; }
            set { name = value; OnPropertyChanged("Name"); }
        }

        private string group;
        public string Group
        {
            get { return group; }
            set { group = value; OnPropertyChanged("Group"); }
        }

        private DOStruct DOB=null;
        private int _DOBindxArrDO;
        public int DOBindxArrDO
        {
            get { return _DOBindxArrDO; }
            set {
                _DOBindxArrDO = value;
                OnPropertyChanged("DOBindxArrDI");
                DOB = DOStruct.FindByIndex(_DOBindxArrDO);
            }
        }
        private int _DKBindxArrDO; 
        public int DKBindxArrDO
        {
            get { return _DKBindxArrDO; }
            set { _DKBindxArrDO = value; OnPropertyChanged("DKBindxArrDI"); }
        }

        public bool changedDO=false;

        private int _OKCindxArrDI=0;
        public int OKCindxArrDI
        {
            get { return _OKCindxArrDI; }
            set { _OKCindxArrDI = value; OnPropertyChanged("OKCindxArrDI"); }
        }
        private int _CKCindxArrDI = 0;
        public int CKCindxArrDI
        {
            get { return _CKCindxArrDI; }
            set { _CKCindxArrDI = value; OnPropertyChanged("CKCindxArrDI"); }
        }
        public bool changedDI=false;

        private float _KLProc=0.0f;
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

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

    }


}
