using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulator_MPSA
{
    public class CL_ANALOG
    {
        //public bool En;
        //public int indxAI; // index in AI
        //public int indxW;
        //public string TegAI;
        //public string NameAI;
        //public short ValACD;
        //public short minACD;
        //public short maxACD;
        //public float minPhis;
        //public float maxPhis;
        //public float fValAI;
        //public int DelayAI;
        public bool En = false;
        public int indxAI=0; // index in AI
        public int indxW=0;
        public string TegAI="";
        public string NameAI="";
        public short ValACD=4000;
        public short minACD=4000;
        public short maxACD=20000;
        public float minPhis=0.0F;
        public float maxPhis=100.0F;
        public float fValAI=0.0F;
        public int DelayAI=0;

        //public CL_ANALOG() { }

        public void Cr_ANALOG(bool En0 = false, int indxAI0 = 0, int  indxW0 = 0, string TegAI0 = "",
                         string NameAI0 = "", short  ValACD0 = 4000, short minACD0 = 4000, short maxACD0 = 20000,
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

        public void setEn(bool En0) {En = En0;}
        public void setAI() { }
        public short getValACD() { return ValACD; }
        public void updateAI(float fValAI0)
        {
            fValAI = fValAI0;
            if (En) 
            {
                ValACD = Convert.ToInt16( minACD + ((maxACD-minACD) * ((fValAI-minPhis)/(maxPhis-minPhis))  )  );
            } else { }
        }
        public void updateAI(short ValACD0)
        {
            ValACD = ValACD0;
            if (En)
            {
                fValAI = (minPhis + ((maxPhis - minPhis) * ((ValACD - minACD) / (maxACD - minACD))));
            }
            else { }
        }
        public string PrintAI()
        {
            return ("En=" + En + "; indxAI=" + indxAI + "; indxW=" + indxW + "; TegAI=" + TegAI +
"; NameAI=" + NameAI + "; ValACD=" + ValACD + "; minACD=" + minACD + "; maxACD=" + maxACD +
"; minPhis=" + minPhis + "; maxPhis=" + maxPhis + "; fValAI=" + fValAI + "; DelauAI=" + DelayAI + "\n");
        }
    }

    
    public  class classAI
    {
        public CL_ANALOG[] AI = new CL_ANALOG[Sett.nAI];
        public void InitArrAI()
        {
            for (int i = 0; i < Sett.nAI; i++)
            {
                AI[i] = new CL_ANALOG();
            }
        }
    }

    // -------------------------------------------------------------------------------------------------
    public struct AIStruct
    {
        public bool En;
        public int indxAI; // index in AI
        public int indxW;
        public string TegAI;
        public string NameAI;
        public short ValACD;
        public short minACD;
        public short maxACD;
        public float minPhis;
        public float maxPhis;
        public float fValAI;
        public int DelayAI;

        public AIStruct(bool En0 = false, int indxAI0 = 0, int indxW0 = 0, string TegAI0 = "",
                 string NameAI0 = "", short ValACD0 = 4000, short minACD0 = 4000, short maxACD0 = 20000,
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
        public string PrintAI()
        {
            return ("En=" + En + "; indxAI=" + indxAI + "; indxW=" + indxW + "; TegAI=" + TegAI +
"; NameAI=" + NameAI + "; ValACD=" + ValACD + "; minACD=" + minACD + "; maxACD=" + maxACD +
"; minPhis=" + minPhis + "; maxPhis=" + maxPhis + "; fValAI=" + fValAI + "; DelauAI=" + DelayAI + "\n");
        }


    }
    // public AIStruct[] AIs = AIStruct[Sett.nAI] ;

}
