using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

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
        private ObservableCollection<DIStruct> _DIs = new ObservableCollection<DIStruct>();
        public ObservableCollection<DIStruct> DIs
        {
            get { return _DIs;  }
            set { _DIs = value; /*OnPropertyChanged("DIs"); */}
        }

        private DITableViewModel()
        {
            _DIs = new ObservableCollection<DIStruct>();
            _DIs.Add(new DIStruct());
        }

        public void Init(DIStruct[] table)
        {
            /*DIStruct[] DIViewModels = new DIStruct[table.Length];
            for (int i = 0; i < table.Length; i++)
            {
                DIViewModels[i] =table[i];
            }
            */
            _DIs = new ObservableCollection<DIStruct>();
            foreach (DIStruct di in table)
                DIs.Add(di);
            // DIs = DIViewModels;

            DIs.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(On_CollectionChanged);
            viewSource.Source = _DIs;
          //viewSource.Filter += new FilterEventHandler(DIs_Filter);
        }
        private void On_CollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            DIStruct.items = _DIs.ToArray();
        }


        public CollectionViewSource viewSource = new CollectionViewSource();

        private string _nameFilter = "";
        public string NameFilter
        { get { return _nameFilter; }
            set
            {
                _nameFilter = value;
              //  viewSource.LiveFilteringProperties.Clear();
             //   viewSource.LiveFilteringProperties.Add(_nameFilter);
            }
                }
        private string _tagFilter="";
        public string TagFilter
        { set
            {
                _tagFilter = value;
            }
            get
            {
                return _tagFilter;
            }
        }

        public bool _hideEmpty=false;
        public bool HideEmpty
        { set
            {
                _hideEmpty = value;
            }
            get
            {
                return _hideEmpty;
            }
        }


        public void ApplyFilter()
        {
            viewSource.Filter += new FilterEventHandler(Filter_Func);
           
        }

        private void Filter_Func(object sender, FilterEventArgs e)
        {
            DIStruct item = e.Item as DIStruct;

            if (item != null)
            {
                if (item.NameDI == "")
                    e.Accepted = !_hideEmpty;
                else
                {
                    if ((item.NameDI.ToLower().Contains(_nameFilter.ToLower())) && (item.TegDI.ToLower().Contains(_tagFilter.ToLower())))
                        e.Accepted = true;
                    else
                        e.Accepted = false;
                }
            }
        }
    }
}
