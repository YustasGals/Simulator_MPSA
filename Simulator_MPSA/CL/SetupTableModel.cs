using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace Simulator_MPSA.CL
{
    class InputOutputItem : INotifyPropertyChanged
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
        public InputOutputItem() { }
        public InputOutputItem(string name, int index, bool isAnalog, float value, string assignedSignalName) {
            Name = name;
            Index = index;
            IsAnalog = isAnalog;
            ActivationValue = value;
            _assignedsignal = assignedSignalName;
        }
        public InputOutputItem(string name, int index, string assignedSignalName)
        {
            Name = name;
            Index = index;
            _assignedsignal = assignedSignalName;
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
        /// <summary>
        /// выходы системы (DI)
        /// </summary>
        private InputOutputItem[] outputs;
        public InputOutputItem[] Outputs
        {
            get { return outputs; }
            set { outputs = value; }
        }

        private InputOutputItem[] inputs;
        public InputOutputItem[] Inputs
        {
            get { return inputs; }
            set { inputs = value; }
        }
        /// <summary>
        /// тип (KLStruct, VSStruct, задвижка, магистралка...)
        /// </summary>
        private Type type;
        /// <summary>
        /// ссылка на настраиваемую систему
        /// </summary>
        object obj; 

        public string Name
        { get; set; }
        public string Group
        { get; set; }



        public SetupTableModel(VSStruct vs)
        {
            type = typeof(VSStruct);
            obj = vs;

            Name = vs.Description;
            Group = vs.Group;

            outputs = new InputOutputItem[3];
            outputs[0] = new InputOutputItem();
            outputs[0].Name = "Наличие напряжения";
            outputs[0].Index = vs.ECindxArrDI;
            outputs[0].IsAnalog = vs.isECAnalog;
            outputs[0].ActivationValue = vs.valueEC;
            outputs[0].AssignedSignal = vs.ECName;

            outputs[1] = new InputOutputItem();
            outputs[1].Name = "Магнитный пускатель";
            outputs[1].Index = vs.MPCindxArrDI;
            outputs[1].IsAnalog = vs.isMPCAnalog;
            outputs[1].ActivationValue = vs.valueMPC;
            outputs[1].AssignedSignal = vs.MPCName;

            outputs[2] = new InputOutputItem();
            outputs[2].Name = "Давление на выходе агрегата";
            outputs[2].Index = vs.PCindxArrDI;
            outputs[2].IsAnalog = vs.isPCAnalog;
            outputs[2].ActivationValue = vs.valuePC;
            outputs[2].AssignedSignal = vs.PCName;

            inputs = new InputOutputItem[2];
            inputs[0] = new InputOutputItem("Команда - пуск", vs.ABBindxArrDO, false, 0.0f, vs.ABBName);
            inputs[1] = new InputOutputItem("Команда - стоп", vs.ABOindxArrDO, false, 0.0f, vs.ABOName);
        }

        public SetupTableModel(KLStruct klapan)
        {
            type = typeof(KLStruct);
            obj = klapan;

            Name = klapan.Description;
            Group = klapan.Group;

            outputs = new InputOutputItem[2];
            outputs[0] = new InputOutputItem("Открыт", klapan.OKCindxArrDI, klapan.OKCName);
            outputs[1] = new InputOutputItem("Закрыт", klapan.CKCindxArrDI, klapan.CKCName);

            inputs = new InputOutputItem[2];
            inputs[0] = new InputOutputItem("Команда - открыть", klapan.DOBindxArrDO, klapan.DOBName);
            inputs[1] = new InputOutputItem("Команда - закрыть", klapan.DKBindxArrDO, klapan.DKBName);
        }


        public void ApplyChanges()
        {
            if (type == typeof(VSStruct))
            {
                VSStruct temp = obj as VSStruct;
                temp.ECindxArrDI = Outputs[0].Index;
                temp.isECAnalog = Outputs[0].IsAnalog;
                temp.valueEC = Outputs[0].ActivationValue;

                temp.MPCindxArrDI = Outputs[1].Index;
                temp.isMPCAnalog = Outputs[1].IsAnalog;
                temp.valueMPC = Outputs[1].ActivationValue;

                temp.PCindxArrDI = Outputs[2].Index;
                temp.isPCAnalog =Outputs[2].IsAnalog;
                temp.valuePC = Outputs[2].ActivationValue;

                temp.Description = Name;
                temp.Group = Group;

                temp.ABBindxArrDO = inputs[0].Index;
                temp.ABOindxArrDO = inputs[1].Index;
            }
            if (type == typeof(KLStruct))
            {
                KLStruct temp = obj as KLStruct;
                temp.DOBindxArrDO = inputs[0].Index;
                temp.DKBindxArrDO = inputs[1].Index;

                temp.OKCindxArrDI = outputs[0].Index;
                temp.CKCindxArrDI = outputs[1].Index;

                temp.Description = Name;
                temp.Group = Group;
            }
        }
    }
}
