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
        private List<InputOutputItem> outputs;
        public List<InputOutputItem> Outputs
        {
            get { return outputs; }
            set { outputs = value; }
        }

        private List<InputOutputItem> inputs;
        public List<InputOutputItem> Inputs
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
        public bool En
        { get; set; }


        public SetupTableModel(VSStruct vs)
        {
            type = typeof(VSStruct);
            obj = vs;

            En = vs.En;
            Name = vs.Description;
            Group = vs.Group;

            outputs = new List<InputOutputItem>();
            outputs.Add( new InputOutputItem("Наличие напряжения", vs.ECindxArrDI, vs.isECAnalog, vs.valueEC, vs.ECName));
            outputs.Add( new InputOutputItem("Магнитный пускатель", vs.MPCindxArrDI, vs.isMPCAnalog, vs.valueMPC, vs.MPCName));
            outputs.Add( new InputOutputItem("Давление на выходе агрегата", vs.PCindxArrDI, vs.isPCAnalog, vs.valuePC, vs.PCName));

            inputs = new List<InputOutputItem>();
            inputs.Add( new InputOutputItem("Команда - пуск", vs.ABBindxArrDO, false, 0.0f, vs.ABBName));
            inputs.Add( new InputOutputItem("Команда - стоп", vs.ABOindxArrDO, false, 0.0f, vs.ABOName));
        }

        public SetupTableModel(KLStruct klapan)
        {
            type = typeof(KLStruct);
            obj = klapan;

            En = klapan.En;
            Name = klapan.Description;
            Group = klapan.Group;

            outputs = new List<InputOutputItem>();
            outputs.Add(new InputOutputItem("Открыт", klapan.OKCindxArrDI, klapan.OKCName));
            outputs.Add(new InputOutputItem("Закрыт", klapan.CKCindxArrDI, klapan.CKCName));

            inputs = new List<InputOutputItem>();
            inputs.Add(new InputOutputItem("Команда - открыть", klapan.DOBindxArrDO, klapan.DOBName));
            inputs.Add(new InputOutputItem("Команда - закрыть", klapan.DKBindxArrDO, klapan.DKBName));
        }

        public SetupTableModel(ZDStruct zd)
        {
            type = typeof(ZDStruct);
            obj = zd;

            En = zd.En;
            Name = zd.Description;
            Group = zd.Group;

            outputs = new List<InputOutputItem>();
            outputs.Add(new InputOutputItem("КВО", zd.OKCindxArrDI, zd.OKCName));
            outputs.Add(new InputOutputItem("КВЗ", zd.CKCindxArrDI, zd.CKCName));
            outputs.Add(new InputOutputItem("Наличие напряжения", zd.VoltindxArrDI, zd.VoltName));
            outputs.Add(new InputOutputItem("МПО", zd.ODCindxArrDI, zd.ODCName));
            outputs.Add(new InputOutputItem("МПЗ", zd.CDCindxArrDI, zd.CDCName));
            outputs.Add(new InputOutputItem("Муфта", zd.MCindxArrDI, zd.MCName));
            outputs.Add(new InputOutputItem("Дистанционное управление", zd.DCindxArrDI, zd.DCName));

            inputs = new List<InputOutputItem>();
            inputs.Add(new InputOutputItem("команда - открыть", zd.DOBindxArrDO, zd.DOBName));
            inputs.Add(new InputOutputItem("команда - остановить", zd.DCBindxArrDO, zd.DCBName));
            inputs.Add(new InputOutputItem("команда - закрыть", zd.DKBindxArrDO, zd.DKBName));
            inputs.Add(new InputOutputItem("команда - стоп закрытия", zd.DCBZindxArrDO, zd.DCBZName));

        }


        public SetupTableModel(MPNAStruct agr)
        {
            type = typeof(MPNAStruct);
            obj = agr;

            Name = agr.Description;
            Group = agr.Group;
            En = agr.En;

            outputs = new List<InputOutputItem>();
            outputs.Add(new InputOutputItem("ВВ включен сигнал 1",agr.MBC11indxArrDI, agr.MBC11Name));
            outputs.Add(new InputOutputItem("ВВ включен сигнал 2", agr.MBC12indxArrDI, agr.MBC12Name));
            outputs.Add(new InputOutputItem("ВВ отключен сигнал 1", agr.MBC21indxArrDI, agr.MBC21Name));
            outputs.Add(new InputOutputItem("ВВ отключен сигнал 2", agr.MBC22indxArrDI, agr.MBC22Name));
            outputs.Add(new InputOutputItem("Сила тока ЭД", agr.CTindxArrDI, true, 0, agr.CTName));
            outputs.Add(new InputOutputItem("Исправность цепей включения", agr.ECBindxArrDI, agr.ECBName));
            outputs.Add(new InputOutputItem("Исправность цепей отключения 1", agr.ECO11indxArrDI, agr.ECO11Name));
            outputs.Add(new InputOutputItem("Исправность цепей отключения 2", agr.ECO12indxArrDI, agr.ECO12Name));
            outputs.Add(new InputOutputItem("ECx02", agr.ECxindxArrDI, agr.ECxName));

            inputs = new List<InputOutputItem>();
            inputs.Add(new InputOutputItem("Команда на включение", agr.ABBindxArrDO, agr.ABBName));
            inputs.Add(new InputOutputItem("Команда на отключение", agr.ABOindxArrDO, agr.ABOName));
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
                temp.En = En;

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
                temp.En = En;
            }
            if (type == typeof(ZDStruct))
            {
                ZDStruct temp = obj as ZDStruct;

                temp.Description = Name;
                temp.Group = Group;
                temp.En = En;
                temp.OKCindxArrDI = outputs[0].Index;
                temp.CKCindxArrDI = outputs[1].Index;
                temp.VoltindxArrDI = outputs[2].Index;
                temp.ODCindxArrDI = outputs[3].Index;
                temp.CDCindxArrDI = outputs[4].Index;
                temp.MCindxArrDI = outputs[5].Index;
                temp.DCindxArrDI = outputs[6].Index;

                temp.DOBindxArrDO = inputs[0].Index;
                temp.DCBindxArrDO = inputs[1].Index;
                temp.DKBindxArrDO = inputs[2].Index;
                temp.DCBZindxArrDO = inputs[3].Index;
            }
            if (type == typeof(MPNAStruct))
            {
                MPNAStruct agr = obj as MPNAStruct;

                agr.Description = Name;
                agr.Group = Group;
                agr.En = En;
                agr.MBC11indxArrDI = outputs[0].Index;
                agr.MBC12indxArrDI = outputs[1].Index;
                agr.MBC21indxArrDI = outputs[2].Index;
                agr.MBC22indxArrDI = outputs[3].Index;
                agr.CTindxArrDI = outputs[4].Index;
                agr.ECBindxArrDI = outputs[5].Index;
                agr.ECO11indxArrDI = outputs[6].Index;
                agr.ECO12indxArrDI = outputs[7].Index;
                agr.ECxindxArrDI = outputs[8].Index;

                agr.ABBindxArrDO = inputs[0].Index;
                agr.ABOindxArrDO = inputs[1].Index;
            }
        }
    }
}
