using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulator_MPSA.CL.Signal
{
    class IndexChangedEventArgs:EventArgs
    {
        public IndexChangedEventArgs(int index)
        {
            newIndex = index;
        }
        int newIndex;
    }
}
