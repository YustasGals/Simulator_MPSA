using Simulator_MPSA.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulator_MPSA.CL
{
    class WatchTableViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<WatchItemViewModel> Items
        { get; set;}

        public WatchTableViewModel(Collection<WatchItem> modelCollection)
        {
            Items = new ObservableCollection<WatchItemViewModel>();
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };
    }
}
