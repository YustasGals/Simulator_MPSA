using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Collections.ObjectModel;
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
            AIStruct[] AIViewModels = new AIStruct[table.Length];
            _AIs.Clear();
            //_AIs = new ObservableCollection<AIStruct>();
            foreach (AIStruct ai in table)
                _AIs.Add(ai);
            

            viewSource.Source = _AIs;
            viewSource.Filter += new FilterEventHandler(Name_Filter);
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
            AIStruct item = e.Item as AIStruct;

            if ((item != null) && (item.NameAI != null))
            {
                //if (item.NameAI.Contains(_nameFilter)) 
                if (item.NameAI.ToLower().Contains(NameFilter.ToLower()))
                    e.Accepted = true;
                else e.Accepted = false;
            }
        }
    }
}
