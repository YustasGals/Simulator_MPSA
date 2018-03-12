using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace Simulator_MPSA.CL
{
    class ZDTableViewModel : BaseViewModel
    {
        private static ObservableCollection<ZDStruct> _zds;

        public static ObservableCollection<ZDStruct> ZDs
        {
            get { return _zds; }
            set { _zds = value; }
        }
        public int Count
        {
            get { return ZDs.Count; }
        }

        public ZDTableViewModel()
        {
            ZDs = new ObservableCollection<ZDStruct>();
            ZDs.Add(new ZDStruct());
        }

        public ZDTableViewModel(List<ZDStruct> valves)
        {
            ObservableCollection<ZDStruct> temp = new ObservableCollection<ZDStruct>();
            foreach (ZDStruct valve in valves)
                temp.Add(valve);
            _zds = temp;

        }

        public ZDTableViewModel(ZDStruct[] valves)
        {
            ObservableCollection<ZDStruct> temp = new ObservableCollection<ZDStruct>();
            foreach (ZDStruct valve in valves)
                temp.Add(valve);
            _zds = temp;
        }

        public static ZDStruct[] GetArray()
        {
            ZDStruct[] temp = new ZDStruct[_zds.Count];
            _zds.CopyTo(temp, 0);
            return temp;
        }
    }
}
