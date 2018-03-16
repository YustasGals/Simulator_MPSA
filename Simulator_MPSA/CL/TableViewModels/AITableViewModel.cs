using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

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

        private AIStruct[] _AIs;
        public AIStruct[] AIs
        {
            get { return _AIs; }
            set { _AIs = value; OnPropertyChanged("AIs"); }
        }
        private AITableViewModel()
        {
            AIs = new AIStruct[1];
            _AIs[0] = new AIStruct();
            _AIs[0].NameAI = "empty";
        }

        public void Init(AIStruct[] table)
        {
            AIStruct[] AIViewModels = new AIStruct[table.Length];
            for (int i = 0; i < table.Length; i++)
            {
                AIViewModels[i] = table[i];
            }

            AIs = AIViewModels;
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
