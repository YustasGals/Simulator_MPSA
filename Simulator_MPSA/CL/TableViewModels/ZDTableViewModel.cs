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
        private static ZDTableViewModel instance;
        public static ZDTableViewModel Instance
        {
            get {
                if (instance == null)                    
                    instance = new ZDTableViewModel();
                return instance;
            }
        }
        public static ObservableCollection<ZDStruct> ZDs
        {
            get { return _zds; }
            set { _zds = value; }
        }
        public int Count
        {
            get { return ZDs.Count; }
        }

        private ZDTableViewModel()
        {
            ZDs = new ObservableCollection<ZDStruct>();
            ZDs.Add(new ZDStruct());
        }

        private ZDTableViewModel(List<ZDStruct> valves)
        {
            ObservableCollection<ZDStruct> temp = new ObservableCollection<ZDStruct>();
            foreach (ZDStruct valve in valves)
                temp.Add(valve);
            _zds = temp;

        }

        private ZDTableViewModel(ZDStruct[] valves)
        {
            ObservableCollection<ZDStruct> temp = new ObservableCollection<ZDStruct>();
            foreach (ZDStruct valve in valves)
                temp.Add(valve);
            _zds = temp;
        }

        public static void Init(ZDStruct[] valves)
        {
            instance = new ZDTableViewModel(valves);
        }
        public static ZDStruct[] GetArray()
        {
            ZDStruct[] temp = new ZDStruct[_zds.Count];
            _zds.CopyTo(temp, 0);
            return temp;
        }
    }
}
