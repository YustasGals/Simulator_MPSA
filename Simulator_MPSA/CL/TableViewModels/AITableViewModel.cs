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
    class AITableViewModel : BaseViewModel
    {
        private static AITableViewModel _instance;
        public static AITableViewModel Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new AITableViewModel();
                return _instance;
            }
        }
        private ObservableCollection<AIStruct> _AIs = new ObservableCollection<AIStruct>();
        //private AIStruct[] _AIs;
        //public AIStruct[] AIs
        
        public ObservableCollection<AIStruct> AIs
        {
            get { return _AIs; }
            set { _AIs = value; OnPropertyChanged("AIs"); }
        }
        private AITableViewModel()
        {
            //AIs = new AIStruct[1];
            AIs = new ObservableCollection<AIStruct>();
            _AIs.Add(new AIStruct());
            _AIs[0].NameAI = "empty";
        }

        public void Init(AIStruct[] table)
        {
        //    AIStruct[] AIViewModels = new AIStruct[table.Length];
            _AIs.Clear();
            //_AIs = new ObservableCollection<AIStruct>();
            foreach (AIStruct ai in table)
                _AIs.Add(ai);
            

            viewSource.Source = _AIs;
            viewSource.Filter += new FilterEventHandler(Name_Filter);
            viewSource.IsLiveFilteringRequested = true;

            _AIs.CollectionChanged += OnCollectionChanged;
        }
        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            AIStruct.items = _AIs.ToArray();
        }

        public CollectionViewSource viewSource = new CollectionViewSource();

        private string _nameFilter = "";
        public string NameFilter
        {
            get { return _nameFilter; }
            set
            {
                _nameFilter = value;
             //   ApplyFilter();
            }
        }
        private string _tagFilter = "";
        public string TagFilter
        {
            get { return _tagFilter; }
            set
            {
                _tagFilter = value;
            //   ApplyFilter();
            }
        }

        public void ApplyFilter()
        {
           
            viewSource.Filter += new FilterEventHandler(Filter_Func);
        }
        /// <summary>
        /// скрывать пустые поля
        /// </summary>
        public bool hideEmpty=false;
        private void Name_Filter(object sender, FilterEventArgs e)
        {
     
            AIStruct item = e.Item as AIStruct;

            if (item != null)
            {
                if (item.NameAI != null)
                {
                    if (item.NameAI.ToLower().Contains(NameFilter.ToLower()))
                        e.Accepted = true;
                    else e.Accepted = false;
                }
            }
        }

        private void Empty_Filter(object sender, FilterEventArgs e)
        {

            AIStruct item = e.Item as AIStruct;

            if (item != null)
            {
                if (item.NameAI == null)
                    e.Accepted = !hideEmpty;
                else e.Accepted = true;
            }
        }

        private void Tag_Filter(object sender, FilterEventArgs e)
        {
            AIStruct item = e.Item as AIStruct;

            if (item != null)
            {
                if (item.TegAI != null)
                {
                    if (item.TegAI.ToLower().Contains(_tagFilter.ToLower()))
                        e.Accepted = true;
                    else e.Accepted = false;
                }
            }
        }
        private void Filter_Func(object sender, FilterEventArgs e)
        {
            AIStruct item = e.Item as AIStruct;

            if (item != null)
            {
                if (item.NameAI == "")
                    e.Accepted = !hideEmpty;
                else
                {
                    if ((item.NameAI.ToLower().Contains(_nameFilter.ToLower())) && (item.TegAI.ToLower().Contains(_tagFilter.ToLower())))
                        e.Accepted = true;
                    else
                        e.Accepted = false;
                }
            }
        }
    }
}
