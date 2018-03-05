using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulator_MPSA
{
    class CL_DO
    {
    }
    // -------------------------------------------------------------------------------------------------
    [Serializable]
    public struct DOStruct
    {
        public bool En;
        public bool ValDO;
        public int indxArrDO; // index in AI
        public int indxBitDO;
        public int indxR;
        public string TegDO;
        public string NameDO;
        public int Nsign;
        public bool InvertDO;
        public bool changedDO;

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
        public object[] all
        {
            get { return new object[] { En, ValDO, indxArrDO, indxBitDO, indxR, TegDO, NameDO, Nsign, InvertDO, changedDO }; }
            set
            {
                En = (bool)value[0];
                ValDO = (bool)value[1];
                indxArrDO = (int)value[2];
                indxBitDO = (int)value[3];
                indxR = (int)value[4];
                TegDO = (string)value[5];
                NameDO = (string)value[6];
                Nsign = (int)value[7];
                InvertDO = (bool)value[8];
                changedDO = (bool)value[9];
            }
        }
    }




}
