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
    public class DOStruct
    {
        public static DOStruct[] DOs = new DOStruct[0];
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
            for (int i = 0; i < DOs.Length; i++)
                if (DOs[i].indxArrDO == index)
                {
                    return DOs[i];
                }
            return null;

        }


    }

}
