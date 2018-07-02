using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Simulator_MPSA.CL
{
    public class VSTableViewModel
    {
        private static ObservableCollection<VSStruct> _vs;
        public static ObservableCollection<VSStruct> VS
        {
            get { return _vs; }
            set { _vs = value; }
        }
        private static VSTableViewModel instance;
        public static VSTableViewModel Instance
        {
            get { if (instance == null)
                    instance = new VSTableViewModel();
                return instance;
            }
        }



        public int Count
        {
            get { return VS.Count; }
        }

        private VSTableViewModel()
        {
            _vs = new ObservableCollection<VSStruct>();
            _vs.CollectionChanged += VS_CollectionChanged;
                      
        }

        private static void VS_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            for (int i = 0; i < VS.Count(); i++)
                _vs[i].Index = i;
            //throw new NotImplementedException();
        }

        private VSTableViewModel(VSStruct[] vs_arr)
        {
            ObservableCollection<VSStruct> temp = new ObservableCollection<VSStruct>();
            foreach (VSStruct agr in vs_arr)
                temp.Add(agr);
            _vs = temp;
        }
        public static VSStruct[] GetArray()
        {
            VSStruct[] temp = new VSStruct[_vs.Count];
            _vs.CopyTo(temp, 0);
            return temp;
        }
        public static void Init(VSStruct[] arr)
        {
            instance = new VSTableViewModel(arr);
            _vs.CollectionChanged += VS_CollectionChanged;
        }

       
    }
}
