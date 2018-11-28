using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulator_MPSA.CL
{
    /// <summary>
    /// класс хранящий булеву переменную, отслеживает фронты/спады
    /// </summary>
    class EBool
    {
        public EBool() { }
        public EBool(bool initValue)
        {
            value = initValue;
            preVal = initValue;
        }
        private bool value;
        public bool Value
        {
            get { return value; }
            set
            {
                if (value && !preVal && OnFront!=null)
                    OnFront.Invoke(null,null);

                if (!value && preVal && OnFall != null)
                    OnFall.Invoke(null, null);

                preVal = this.value;
                this.value = value;


            }
        }
        private bool preVal = false;


        public bool IsFront
        {
            get { return value && !preVal; }
        }


        public bool IsFall
        {
            get { return !value && preVal; }
        }

        public EventHandler OnFront = delegate { };
        public EventHandler OnFall = delegate{};
    }
}
