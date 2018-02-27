using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulator_MPSA
{
    class CL_DI
    {
    }

    // -------------------------------------------------------------------------------------------------
    [Serializable]
    public struct DIStruct
    {
        public bool En;
        public bool ValDI;
        public int indxArrDI; // index in AI
        public int indxBitDI;
        public int indxW;
        public string TegDI;
        public string NameDI;
        public int Nsign;
        public bool InvertDI;
        public int DelayDI;

        public DIStruct(bool En0 = false, bool ValDI0 = false, int indxArrDI0 = 0, int indxBitDI0 = 0 , int indxW0 = 0, string TegDI0 = "Teg",
                 string NameDI0 = "Name", int Nsign0 = 0 , bool InvertDI0 = false, int DelayDI0 = 0)
        {
            En = En0;
            ValDI = ValDI0;
            indxArrDI = indxArrDI0;
            indxBitDI = indxBitDI0;
            indxW = indxW0;
            TegDI = TegDI0;
            NameDI = NameDI0;
            Nsign = Nsign0;
            InvertDI = InvertDI0;
            DelayDI = DelayDI0;
        }
    }



    }
