using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulator_MPSA.CL.Signal
{
    public class IndexChangedEventArgs:EventArgs
    {
        public IndexChangedEventArgs(int index)
        {
            newIndex = index;
        }
        public int newIndex;
    }
}
