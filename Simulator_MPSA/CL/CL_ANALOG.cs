using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulator_MPSA
{
    // -------------------------------------------------------------------------------------------------
    [Serializable]
    public class AIStruct
    {
        public static AIStruct[] items = new AIStruct[0];
        public bool En ;
        public int indxAI; // index in AI
        public int indxW;
        public string TegAI;
        public string NameAI;

        private ushort _valADC;
        public ushort ValACD
        {
            get {
                float df = (fValAI - minPhis) / (maxPhis - minPhis);
                float dadc = ((float)maxACD - (float)minACD) * df;
                int res = (int)dadc + minACD;
                return (ushort)res;
            }
            set { _valADC = value; }
        }

        public ushort minACD;
        public ushort maxACD;
        public float minPhis;
        public float maxPhis;
        private float _fValAI;
        public float fValAI
        {
            get { return _fValAI; }
            set { _fValAI = value; }
        }

        public int DelayAI;

        public AIStruct() {
        }
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
        public static AIStruct FindByIndex(int index)
        {
            for (int i = 0; i < items.Length; i++)
                if (items[i].indxAI == index)
                {
                    return items[i];
                }
            return null;
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
    // public AIStruct[] AIs = AIStruct[Sett.nAI] ;

}
