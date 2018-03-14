using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulator_MPSA.CL
{
    class AITableViewModel : BaseViewModel
    {
        private AIViewModel[] _AIs;
        public AIViewModel[] AIs
        {
            get { return _AIs; }
            set { _AIs = value; OnPropertyChanged("AIs"); }
        }
        public AITableViewModel()
        {
            AIs = new AIViewModel[1];
            _AIs[0] = new AIViewModel(new AIStruct());
            _AIs[0].NameAI = "empty";
        }

        public AITableViewModel(AIStruct[] table)
        {
            AIViewModel[] AIViewModels = new AIViewModel[table.Length];
            for (int i = 0; i < table.Length; i++)
            {
                AIViewModels[i] = new AIViewModel(table[i]);
            }
            AIs = AIViewModels;
        }
    }
}
