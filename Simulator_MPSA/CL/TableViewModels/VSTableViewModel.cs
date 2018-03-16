﻿using System;
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
            VS = new ObservableCollection<VSStruct>();
            VS.Add(new VSStruct());
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
        }
    }
}