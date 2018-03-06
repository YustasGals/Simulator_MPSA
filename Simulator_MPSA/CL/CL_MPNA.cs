using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulator_MPSA
{
    class CL_MPNA
    {
    }
    // -------------------------------------------------------------------------------------------------
    [Serializable]
    public struct MPNAStruct
    {
        public bool En; // наличие в обработке задвижки
        public int ABBindxArrDO; // ABB101
        public int ABOindxArrDO; // ABO101-2
        public bool changedDO; // наличие изменений во входных сигналах блока
        public int MBC12indxArrDI; // MBC101-2
        public int MBC22indxArrDI; // MBC102-2
        public int ECBindxArrDI; // ECB101
        public int ECO12indxArrDI; // ECO101-2
        public int CTindxArrDI; // CT1011
        public int MBC11indxArrDI; // MBC101-1
        public int MBC21indxArrDI; // MBC102-1
        public int ECxindxArrDI; // ECx02
        public int ECO11indxArrDI; // ECO101-1
        public int ECindxArrDI; // EC108
        public float MPNAProc; // процент включенности МПНА
        public bool changedDI; // наличие изменений в выходных сигналах блока
        public int Tmove; // время включения , сек

        public MPNAStruct(bool En0 = false,
                          int ABBindxArrDO0 = 0,
                          int ABOindxArrDO0 = 0,
                          bool changedDO0 = false,
                          int MBC12indxArrDI0 = 0,
                          int MBC22indxArrDI0=0,
                          int ECBindxArrDI0=0,
                          int ECO12indxArrDI0=0,
                          int CTindxArrDI0=0,
                          int MBC11indxArrDI0=0,
                          int MBC21indxArrDI0=0,
                          int ECxindxArrDI0=0,
                          int ECO11indxArrDI0=0,
                          int ECindxArrDI0=0,
                          float MPNAProc0=0.0f,
                          bool changedDI0=false,
                          int Tmove0=0)
        {
            En = En0;
            ABBindxArrDO = ABBindxArrDO0;
            ABOindxArrDO = ABOindxArrDO0;
            changedDO = changedDO0;
            MBC12indxArrDI = MBC12indxArrDI0;
            MBC22indxArrDI = MBC22indxArrDI0;
            ECBindxArrDI = ECBindxArrDI0;
            ECO12indxArrDI = ECO12indxArrDI0;
            CTindxArrDI = CTindxArrDI0;
            MBC11indxArrDI = MBC11indxArrDI0;
            MBC21indxArrDI = MBC21indxArrDI0;
            ECxindxArrDI = ECxindxArrDI0;
            ECO11indxArrDI = ECO11indxArrDI0;
            ECindxArrDI = ECindxArrDI0;
            MPNAProc = MPNAProc0;
            changedDI = changedDI0;
            Tmove = Tmove0;
        }

        public float UpdateMPNA()
        {
            // тут будет логика задвижки !!!
            return MPNAProc;
        }

    }


}
