using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulator_MPSA
{
    class CL_ZD
    {
    }
    // -------------------------------------------------------------------------------------------------
    [Serializable]
    public struct ZDStruct
    {
        public bool En ; // наличие в обработке задвижки
        public int DOBindxArrDO; 
        public int DKBindxArrDO;
        public int DCBindxArrDO;
        public int DCBZindxArrDO;
        public bool changedDO; // наличие изменений во входных сигналах блока
        public int OKCindxArrDI;
        public int CKCindxArrDI;
        public int ODCindxArrDI;
        public int CDCindxArrDI;
        public int DCindxArrDI;
        public int VoltindxArrDI;
        public int MCindxArrDI;
        public int OPCindxArrDI;
        public float ZDProc; // процент открытия задвижки
        public bool changedDI; // наличие изменений в выходных сигналах блока
        public int TmoveZD; // время полного хода звдвижки, сек
        public int TscZD;  // время схода с концевиков, сек

        public ZDStruct( bool En0 = false ,
                         int DOBindxArrDO0 = 0,
                         int DKBindxArrDO0 = 0,
                         int DCBindxArrDO0 = 0,
                         int DCBZindxArrDO0 = 0,
                         bool changedDO0 = false,
                         int OKCindxArrDI0 = 0,
                         int CKCindxArrDI0 = 0,
                         int ODCindxArrDI0 = 0,
                         int CDCindxArrDI0 = 0,
                         int DCindxArrDI0 = 0,
                         int VoltindxArrDI0 = 0,
                         int MCindxArrDI0 = 0,
                         int OPCindxArrDI0 = 0,
                         float ZDProc0 = 0.0f ,
                         bool changedDI0 = false,
                         int TmoveZD0 = 600,
                         int TscZD0 = 3)
        {
            En= En0;
            DOBindxArrDO= DOBindxArrDO0;
            DKBindxArrDO= DKBindxArrDO0;
            DCBindxArrDO= DCBindxArrDO0;
            DCBZindxArrDO= DCBZindxArrDO0;
            changedDO= changedDO0;
            OKCindxArrDI= OKCindxArrDI0;
            CKCindxArrDI= CKCindxArrDI0;
            ODCindxArrDI= ODCindxArrDI0;
            CDCindxArrDI= CDCindxArrDI0;
            DCindxArrDI= DCindxArrDI0;
            VoltindxArrDI= VoltindxArrDI0;
            MCindxArrDI= MCindxArrDI0;
            OPCindxArrDI= OPCindxArrDI0;
            ZDProc= ZDProc0;
            changedDI= changedDI0;
            TmoveZD = TmoveZD0;
            TscZD = TscZD0;
        }

        public float UpdateZD()
        {
            // тут будет логика задвижки !!!
            return ZDProc;
        }

    }

}
