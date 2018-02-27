using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulator_MPSA
{
    // -------------------------------------------------------------------------------------------------
    [Serializable]
    public struct AIStruct
    {
        public bool En ;
        public int indxAI; // index in AI
        public int indxW;
        public string TegAI;
        public string NameAI;
        public ushort ValACD;
        public ushort minACD;
        public ushort maxACD;
        public float minPhis;
        public float maxPhis;
        public float fValAI;
        public int DelayAI;
        
        public AIStruct(bool En0 = false, int indxAI0 = 0, int indxW0 = 0, string TegAI0 = "Teg",
                 string NameAI0 = "Name", ushort ValACD0 = 4000, ushort minACD0 = 4000, ushort maxACD0 = 20000,
                 float minPhis0 = 0.0F, float maxPhis0 = 100.0F, float fValAI0 = 0.0F, int DelayAI0 = 0)
        {
            En = En0;
            indxAI = indxAI0;
            indxW = indxW0;
            TegAI = TegAI0;
            NameAI = NameAI0;
            ValACD = ValACD0;
            minACD = minACD0;
            maxACD = maxACD0;
            minPhis = minPhis0;
            maxPhis = maxPhis0;
            fValAI = fValAI0;
            DelayAI = DelayAI0;
        }
        public object[] all
        {
            get { return new object[] { En, indxAI, indxW, TegAI, NameAI, ValACD, minACD, maxACD, minPhis, maxPhis, fValAI, DelayAI }; }
            set
            {
                En = (bool)value[0];
                indxAI = (int)value[1];
                indxW = (int)value[2];
                TegAI = ((string)(value[3])); 
                NameAI = (string)value[4]; 
                ValACD = (ushort)value[5];
                minACD = (ushort)value[6];
                maxACD = (ushort)value[7];
                minPhis = (ushort)value[8];
                maxPhis = (ushort)value[9];
                fValAI = (float)value[10];
                DelayAI = (int)value[11];
            }
        }
        public string PrintAI()
        {
            return ("En=" + En + "; indxAI=" + indxAI + "; indxW=" + indxW + "; TegAI=" + TegAI +
"; NameAI=" + NameAI + "; ValACD=" + ValACD + "; minACD=" + minACD + "; maxACD=" + maxACD +
"; minPhis=" + minPhis + "; maxPhis=" + maxPhis + "; fValAI=" + fValAI + "; DelauAI=" + DelayAI + "\n");
        }
        public ushort getValACD() { return ValACD; }
        public void updateAI(float fValAI0)
        {
            fValAI = fValAI0;
            if (En)
            {
                ValACD = (ushort)(minACD + ((maxACD - minACD) * ((fValAI - minPhis) / (maxPhis - minPhis))));
            }
            else { }
        }
        public void updateAI(ushort ValACD0)
        {
            ValACD = ValACD0;
            if (En)
            {
                fValAI = (minPhis + ((maxPhis - minPhis) * ((ValACD - minACD) / (maxACD - minACD))));
            }
            else { }
        }
    }
}
