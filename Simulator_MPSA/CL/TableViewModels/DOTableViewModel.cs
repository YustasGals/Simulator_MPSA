﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Simulator_MPSA.CL
{
    class DOTableViewModel : BaseViewModel
    {
        private static DOTableViewModel _instance;
        public static DOTableViewModel Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new DOTableViewModel();
                return _instance;
            }
        }

        private DOViewModel[] _DOs;
        public DOViewModel[] DOs
        {
            get { return _DOs; }
            set { _DOs = value; OnPropertyChanged("DOs"); }
        }

        public DOTableViewModel()
        {
            var dos = SetupArray();
            DOViewModel[] DOViewModels = new DOViewModel[dos.Length];
            for (int i = 0; i < dos.Length; i++)
            {
                DOViewModels[i] = new DOViewModel(dos[i]);
            }
            DOs = DOViewModels;
        }
        public void Init(DOStruct[] table)
        {
            DOViewModel[] DOViewModels = new DOViewModel[table.Length];
            for (int i = 0; i < table.Length; i++)
            {
                DOViewModels[i] = new DOViewModel(table[i]);
            }
            DOs = DOViewModels;

            viewSource.Source = _DOs;
            viewSource.Filter += new FilterEventHandler(Name_Filter);
        }
        private DOStruct[] SetupArray()
        {
            DOStruct[] result = new DOStruct[1];
            for (int i = 0; i < result.Length; i++)
                result[i] = new DOStruct();

            result[0].NameDO = "Сигнал 1";

            return result;

        }

        public CollectionViewSource viewSource = new CollectionViewSource();

        private string _nameFilter = "";
        public string NameFilter
        {
            get { return _nameFilter; }
            set
            {
                _nameFilter = value;
                viewSource.LiveFilteringProperties.Clear();
                viewSource.LiveFilteringProperties.Add(_nameFilter);
            }
        }
        private void Name_Filter(object sender, FilterEventArgs e)
        {
            DOViewModel item = e.Item as DOViewModel;

            if (item != null) 
                if (item.ShortDesc != null)
                    {
                        {
                        //if (item.NameAI.Contains(_nameFilter)) 
                        if (item.ShortDesc.ToLower().Contains(NameFilter.ToLower()))
                            e.Accepted = true;
                        else e.Accepted = false;
                        }
                    }
                else
                    e.Accepted = false;
            
        }
    }
}
