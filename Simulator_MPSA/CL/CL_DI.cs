using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Simulator_MPSA.CL
{
    class CL_DI
    {
    }
    // -------------------------------------------------------------------------------------------------
    [Serializable]
    public class DIStruct
    {
        public bool En
        { set; get; }
        public bool ValDI
        { set; get; }
        public int IndxArrDI // index in AI
        { set; get; }
        public int IndxBitDI
        { set; get; }
        public int IndxW
        { set; get; }
        public string TegDI
        { set; get; }
        public string NameDI
        { set; get; }
        public int Nsign
        { set; get; }
        public bool InvertDI
        { set; get; }
        public int DelayDI
        { set; get; }
        public DIStruct()
        { }
        public DIStruct(bool En0 = false, bool ValDI0 = false, int indxArrDI0 = 0, int indxBitDI0 = 0 , int indxW0 = 0, string TegDI0 = "Teg",
                 string NameDI0 = "Name", int Nsign0 = 0 , bool InvertDI0 = false, int DelayDI0 = 0)
        {
            En = En0;
            ValDI = ValDI0;
            IndxArrDI = indxArrDI0;
            IndxBitDI = indxBitDI0;
            IndxW = indxW0;
            TegDI = TegDI0;
            NameDI = NameDI0;
            Nsign = Nsign0;
            InvertDI = InvertDI0;
            DelayDI = DelayDI0;
        }


    }



}
