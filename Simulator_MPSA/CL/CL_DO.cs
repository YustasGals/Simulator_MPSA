﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Simulator_MPSA.CL
{
    class CL_DO
    {
    }
    // -------------------------------------------------------------------------------------------------
    [Serializable]
    public class DOStruct : BaseViewModel
    {
        public static DOStruct[] items = new DOStruct[0];
        private bool _En;
        public bool En
        {
            get { return _En; }
            set { _En = value; OnPropertyChanged("En"); }
        }
        private bool _ValDO;
        public bool ValDO
        {
            get { return _ValDO; }
            set { _ValDO = value; OnPropertyChanged("ValDO"); }
        }
        private int _indxArrDO;
        public int indxArrDO
        {
            get { return _indxArrDO; }
            set { _indxArrDO = value; OnPropertyChanged("indxArrDO"); }
        }


        private int _indxBitDO;
        public int indxBitDO
        {
            get { return _indxBitDO; }
            set { _indxBitDO = value; OnPropertyChanged("indxBitDO"); }
        }

        private int _indxR;
        public int indxR
        {
            get { return _indxR; }
            set { _indxR = value; OnPropertyChanged("indxR"); }
        }


        private string _TegDO = "";
        public string TegDO
        {
            get { return _TegDO; }
            set { _TegDO = value; OnPropertyChanged("TegDO"); }
        }

        public string _NameDO = "";
        public string NameDO
        {
            get { return _NameDO; }
            set
            {
                _NameDO = value; OnPropertyChanged("NameDO");
            }
        }

        private int _Nsign;
        public int Nsign
        {
            get { return _Nsign; }
            set { _Nsign = value; OnPropertyChanged("Nsign"); }
        }
      

        private bool _InvertDO;
        public bool InvertDO
        {
            get { return _InvertDO; }
            set { _InvertDO = value; OnPropertyChanged("InvertDO"); }
        }

        public bool changedDO;
        public DOStruct()
        { }
        public DOStruct(bool En0 = false, bool ValDO0 = false, int indxArrDO0 = 0, int indxBitDO0 = 0, int indxR0 = 0, string TegDO0 = "Teg",
                 string NameDO0 = "Name", int Nsign0 = 0, bool InvertDO0 = false, bool changedDO0 = false)
        {
            En = En0;
            ValDO = ValDO0;
            indxArrDO = indxArrDO0;
            indxBitDO = indxBitDO0;
            indxR = indxR0;
            TegDO = TegDO0;
            NameDO = NameDO0;
            Nsign = Nsign0;
            InvertDO = InvertDO0;
            changedDO = changedDO0;
        }
        public static DOStruct FindByIndex(int index)
        {
            for (int i = 0; i < items.Length; i++)
                if (items[i].indxArrDO == index)
                {
                    return items[i];
                }
            return null;

        }


    }

}
