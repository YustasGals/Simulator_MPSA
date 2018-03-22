using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulator_MPSA.CL
{
    class CountersTableViewModel
    {
        private static USOCounter[] _counters;
        public static USOCounter[] Counters
        {
            get { return _counters; }
            set { _counters = value; }
        }

        public static void Init(USOCounter[] items)
        {
            _counters = items;
        }
    }
}
