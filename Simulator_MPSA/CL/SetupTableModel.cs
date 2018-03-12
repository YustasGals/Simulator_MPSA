using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulator_MPSA.CL
{
    class InputItem
    {
        public string Name
        { get; set; }
        public int Index
        { get; set; }
        public bool IsAnalog
        { get; set; }
        public float ActivationValue
        { get; set; }
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


            items[1] = new InputItem();
            items[1].Name = "Магнитный пускатель";
            items[1].Index = vs.MPCindxArrDI;
            items[1].IsAnalog = vs.isMPCAnalog;
            items[1].ActivationValue = vs.valueMPC;

            items[2] = new InputItem();
            items[2].Name = "Давление на выходе агрегата";
            items[2].Index = vs.PCindxArrDI;
            items[2].IsAnalog = vs.isPCAnalog;
            items[2].ActivationValue = vs.valuePC;
        }
    }
}
