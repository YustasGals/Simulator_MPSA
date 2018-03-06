using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulator_MPSA
{
    class CL_KL
    {
    }
    // -------------------------------------------------------------------------------------------------
    [Serializable]
    public struct KLStruct
    {
        public bool En;
        public int DOBindxArrDO;
        public int DKBindxArrDO;
        public bool changedDO;
        public int OKCindxArrDI;
        public int CKCindxArrDI;
        public bool changedDI;
        public float KLProc;
        public int TmoveKL;

        public KLStruct(bool En0 = false,
                        int DOBindxArrDO0 = 0,
                        int DKBindxArrDO0 = 0,
                        bool changedDO0 = false,
                        int OKCindxArrDI0 = 0,
                        int CKCindxArrDI0 = 0,
                        bool changedDI0 = false,
                        float KLProc0 = 0.0f,
                        int TmoveKL0 = 3)
        {
            En = En0;
            DOBindxArrDO = DOBindxArrDO0;
            DKBindxArrDO = DKBindxArrDO0;
            changedDO = changedDO0;
            OKCindxArrDI = OKCindxArrDI0;
            CKCindxArrDI = CKCindxArrDI0;
            changedDI = changedDI0;
            KLProc = KLProc0;
            TmoveKL = TmoveKL0;
        }

        public float UpdateKL()
        {
            // тут будет логика  !!!
            return KLProc;
        }
    }


}
