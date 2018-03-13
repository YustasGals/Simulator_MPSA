using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulator_MPSA
{
    // -------------------------------------------------------------------------------------------------
    [Serializable]
    public class MPNAStruct
    {
        public bool En=false; // наличие в обработке задвижки
        public int ABBindxArrDO=-1; // ABB101
        public int ABOindxArrDO=-1; // ABO101-2
        public bool changedDO=false; // наличие изменений во входных сигналах блока
        public int MBC12indxArrDI=-1; // MBC101-2
        public int MBC22indxArrDI=-1; // MBC102-2
        public int ECBindxArrDI=-1; // ECB101
        public int ECO12indxArrDI=-1; // ECO101-2
        public int CTindxArrDI= -1; // CT1011
        public int MBC11indxArrDI = -1; // MBC101-1
        public int MBC21indxArrDI = -1; // MBC102-1
        public int ECxindxArrDI = -1; // ECx02
        public int ECO11indxArrDI = -1; // ECO101-1
        public int ECindxArrDI = -1; // EC108
        public float MPNAProc = 0.0f; // процент включенности МПНА
        public bool changedDI=false; // наличие изменений в выходных сигналах блока
        public int Tmove=1; // время включения , сек

        public MPNAStruct()
        {
        }
        /// <summary>
        /// Обновить состояние агрегата
        /// </summary>
        /// <param name="dt">время между циклами сек </param>
        /// <returns></returns>
        public float UpdateMPNA(float dt)
        {
            // тут будет логика задвижки !!!
            return MPNAProc;
        }

    }


}
