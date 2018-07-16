using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Xml.Serialization;
using System.Windows.Data;
using Simulator_MPSA.CL.Signal;
namespace Simulator_MPSA.CL
{
    class DiagTableModel
    {
        private static DiagTableModel _instance;
        public static DiagTableModel Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new DiagTableModel();
                return _instance;
            }
        }

        private ObservableCollection<DIStruct> _diag = new ObservableCollection<DIStruct>();
        public ObservableCollection<DIStruct> DiagRegs
        {
            set { _diag = value; }
            get { return _diag; }
        }

        public void Init(DIStruct[] items)
        {
            if (items != null)
            {
                DiagRegs = new ObservableCollection<DIStruct>();
                foreach (DIStruct di in items)
                    DiagRegs.Add(di);
            }
            viewSource.Source = DiagRegs;
        }
        public void ApplyFilter()
        {

            viewSource.Filter += new FilterEventHandler(Filter_Func);
        }
        public string nameFilter = "";
        public CollectionViewSource viewSource = new CollectionViewSource();

        private void Filter_Func(object sender, FilterEventArgs e)
        {
            DIStruct item = e.Item as DIStruct;

            if (item != null)
            {

                    if (item.NameDI.ToLower().Contains(nameFilter.ToLower()))
                        e.Accepted = true;
                    else
                        e.Accepted = false;

            }
        }
    }
}
