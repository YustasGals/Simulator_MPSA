using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulator_MPSA
{
    class CL_VS
    {
    }
    // -------------------------------------------------------------------------------------------------
    [Serializable]
    public struct VSStruct
    {
        public bool En;
        public int ABBindxArrDO;
        public int ABOindxArrDO;
        public bool changedDO;
        public int ECindxArrDI;
        public int MPCindxArrDI;
        public int PCindxArrDI;
        public bool changedDI;
        public float VSProc;
        public int TmoveVS;

        public VSStruct(bool En0 = false,
                        int ABBindxArrDO0 = 0,
                        int ABOindxArrDO0 = 0,
                        bool changedDO0 = false,
                        int ECindxArrDI0 = 0,
                        int MPCindxArrDI0 = 0,
                        int PCindxArrDI0 = 0,
                        bool changedDI0 = false,
                        float VSProc0 = 0.0f,
                        int TmoveVS0 = 2)
        {
            En = En0;
            ABBindxArrDO = ABBindxArrDO0;
            ABOindxArrDO = ABOindxArrDO0;
            changedDO = changedDO0;
            ECindxArrDI = ECindxArrDI0;
            MPCindxArrDI = MPCindxArrDI0;
            PCindxArrDI = PCindxArrDI0;
            changedDI = changedDI0;
            VSProc = VSProc0;
            TmoveVS = TmoveVS0;
        }

        public float UpdateVS()
        {
            // тут будет логика  !!!
            return VSProc;
        }
    }


}
