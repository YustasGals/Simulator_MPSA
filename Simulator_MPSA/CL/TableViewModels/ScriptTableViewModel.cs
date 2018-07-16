using Simulator_MPSA.Scripting;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulator_MPSA.CL
{
    class ScriptTableViewModel
    {
        private static ObservableCollection<ScriptInfo> _items = new ObservableCollection<ScriptInfo>();
        public static ObservableCollection<ScriptInfo> Items
        { set { _items = value; } get { return _items; } }

        public static void Init()
        {
            _items = new ObservableCollection<ScriptInfo>();
        }
    }
}
