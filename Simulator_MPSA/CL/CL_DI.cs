using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Simulator_MPSA
{
    class CL_DI
    {
    }
    // -------------------------------------------------------------------------------------------------
    class MyTable
    {
        public MyTable(bool En, int Id, string Vocalist, string Album, int Year)
        {
            this.En = En;
            this.Id = Id;
            this.Vocalist = Vocalist;
            this.Album = Album;
            this.Year = Year;
        }

        public bool En { get; set; }
        public int Id { get; set; }
        public string Vocalist { get; set; }
        public string Album { get; set; }
        public int Year { get; set; }
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
        public object[] all
        {
            get { return new object[] { En, ValDI, indxArrDI, indxBitDI, indxW, TegDI, NameDI, Nsign, InvertDI, DelayDI }; }
            set
            {
                En = (bool)value[0];
                ValDI = (bool)value[1];
                indxArrDI = (int)value[2];
                indxBitDI = (int)value[3];
                indxW = (int)value[4];
                TegDI = (string)value[5];
                NameDI = (string)value[6];
                Nsign = (int)value[7];
                InvertDI = (bool)value[8];
                DelayDI = (int)value[9];
            }
        }

    }



}
