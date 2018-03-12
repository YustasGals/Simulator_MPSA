using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace Simulator_MPSA.CL
{
    class InputItem : INotifyPropertyChanged
    {
        public string Name
        { get; set; }
        public int Index
        { get; set; }
        public bool IsAnalog
        { get; set; }
        public float ActivationValue
        { get; set; }
        private string _assignedsignal;
        public string AssignedSignal
        { get { return _assignedsignal; }
            set
            {
                _assignedsignal = value;
                OnPropertyChanged("AssignedSignal");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
    class SetupTableModel
    {
        private InputItem[] items;
        public string VSName
        { get; set; }
        public string VSGroup
        { get; set; }

        public InputItem[] Items
        {
            get { return items; }
            set { items = value; }
        }

        public SetupTableModel(VSStruct vs)
        {
            VSName = vs.Description;
            VSGroup = vs.Group;

            items = new InputItem[3];
            items[0] = new InputItem();
            items[0].Name = "Наличие напряжения";
            items[0].Index = vs.ECindxArrDI;
            items[0].IsAnalog = vs.isECAnalog;
            items[0].ActivationValue = vs.valueEC;
            items[0].AssignedSignal = vs.ECName;

            items[1] = new InputItem();
            items[1].Name = "Магнитный пускатель";
            items[1].Index = vs.MPCindxArrDI;
            items[1].IsAnalog = vs.isMPCAnalog;
            items[1].ActivationValue = vs.valueMPC;
            items[1].AssignedSignal = vs.MPCName;

            items[2] = new InputItem();
            items[2].Name = "Давление на выходе агрегата";
            items[2].Index = vs.PCindxArrDI;
            items[2].IsAnalog = vs.isPCAnalog;
            items[2].ActivationValue = vs.valuePC;
            items[2].AssignedSignal = vs.PCName;
        }
    }
}
