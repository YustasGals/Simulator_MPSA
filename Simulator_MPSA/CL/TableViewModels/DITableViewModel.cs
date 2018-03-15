using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Simulator_MPSA.CL
{
    class DITableViewModel
    {
        private static DITableViewModel _instance;
        public static DITableViewModel Instance
        {
            get { if (_instance == null)
                    _instance = new DITableViewModel();
                return _instance;
            }
        }
        private DIStruct[] _DIs;
        public DIStruct[] DIs
        {
            get { return _DIs;  }
            set { _DIs = value; /*OnPropertyChanged("DIs"); */}
        }

        private DITableViewModel()
        {
            DIs = new DIStruct[1];
            _DIs[0] = new DIStruct();
            _DIs[0].NameDI = "reserved";
        }

        public void Init(DIStruct[] table)
        {
            DIStruct[] DIViewModels = new DIStruct[table.Length];
            for (int i = 0; i < table.Length; i++)
            {
                DIViewModels[i] =table[i];
            }
            DIs = DIViewModels;

            viewSource.Source = _DIs;
            viewSource.Filter += new FilterEventHandler(DIs_Filter);
        }

        public CollectionViewSource viewSource = new CollectionViewSource();

        private string _nameFilter = "";
        public string NameFilter
        { get { return _nameFilter; }
            set
            {
                _nameFilter = value;
                viewSource.LiveFilteringProperties.Clear();
                viewSource.LiveFilteringProperties.Add(_nameFilter);
            }
                }
        private void DIs_Filter(object sender, FilterEventArgs e)
        {
            DIStruct di = e.Item as DIStruct;

            if ((di != null) && (di.NameDI != null))
            {

                if (di.NameDI.ToLower().Contains(NameFilter.ToLower()))
                    e.Accepted = true;
                else e.Accepted = false;
            }
        }
    }
}
