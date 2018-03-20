using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace Simulator_MPSA.CL
{
   
    /// <summary>
    /// класс дискретного сигнала
    /// используется для отображения в таблице настроек
    /// </summary>
    class InputOutputItem : INotifyPropertyChanged
    {
        /// <summary>
        /// название сигнала в таблице настроек (короткое название)
        /// </summary>
        public string Name
        { get; set; }
        /// <summary>
        /// индекс в общей таблице DO или DI
        /// </summary>
        public int Index
        { get; set; }

        /// <summary>
        /// исправность сигнала
        /// </summary>
        public bool IsOK
        { get; set; }

        /// <summary>
        /// название сигнала в таблице DI, DO, необходимо для отображения в таблицах настроек
        /// </summary>
        private string _assignedsignalName;
        public string AssignedSignalName
        { get { return _assignedsignalName; }
            set
            {
                _assignedsignalName = value;
                OnPropertyChanged("AssignedSignal");
            }
        }
        public InputOutputItem() { }

        public InputOutputItem(string name, int index, string assignedSignalName, bool isOk=true)
        {
            Name = name;
            Index = index;
            _assignedsignalName = assignedSignalName;
            IsOK = isOk;
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }

    /// <summary>
    /// класс аналогового сигнала
    /// используется для отображения в таблице настроек
    /// </summary>
    class AnalogIOItem : InputOutputItem
    {
        
        float _nomValue;
        /// <summary>
        /// Номинальное значение которое постепенно достигается при пуске агрегата
        /// </summary>
        public float ValueNom
        {
            get { return _nomValue; }
            set { _nomValue = value; OnPropertyChanged("ValueNom"); }
        }


        float _spdValue;
        /// <summary>
        /// скорость изменения аналогового параметра
        /// </summary>
        public float ValueSpd
        {
            get { return _spdValue; }
            set { _spdValue = value; OnPropertyChanged("ValueSpd"); }
        }

        public AnalogIOItem(string name, int index, float value, float spdVal, string assignedSignalName)
        {
            Name = name;
            Index = index;
            AssignedSignalName = assignedSignalName;
            ValueNom = value;
            ValueSpd = spdVal;
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
        private AnalogIOItem[] _analogs;
        public AnalogIOItem[] Analogs
        {
            get { return _analogs; }
            set { _analogs = value; }
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

            outputs = new InputOutputItem[3];
            outputs[0] = new InputOutputItem("Наличие напряжения", vs.ECindxArrDI, vs.ECName);
            outputs[1] =  new InputOutputItem("Магнитный пускатель", vs.MPCindxArrDI, vs.MPCName);
            outputs[2] = new InputOutputItem("Наличие давления на выходе", vs.PCindxArrDI, vs.PCNameDI);

            inputs = new InputOutputItem[2];
            inputs[0]= new InputOutputItem("Команда - пуск", vs.ABBindxArrDO, vs.ABBName);
            inputs[1]= new InputOutputItem("Команда - стоп", vs.ABOindxArrDO, vs.ABOName);

            _analogs = new AnalogIOItem[1];
            _analogs[0] = new AnalogIOItem("Давление на выходе", vs.PCindxArrAI,vs.valuePC,vs.valuePCspd, vs.PCNameAI);

        }

        public SetupTableModel(KLStruct klapan)
        {
            type = typeof(KLStruct);
            obj = klapan;

            En = klapan.En;
            Name = klapan.Description;
            Group = klapan.Group;

            outputs = new InputOutputItem[2];
            outputs[0] = new InputOutputItem("Открыт", klapan.OKCindxArrDI, klapan.OKCName);
            outputs[1] = new InputOutputItem("Закрыт", klapan.CKCindxArrDI, klapan.CKCName);

            inputs = new InputOutputItem[2];
            inputs[0] = new InputOutputItem("Команда - открыть", klapan.DOBindxArrDO, klapan.DOBName);
            inputs[1] = new InputOutputItem("Команда - закрыть", klapan.DKBindxArrDO, klapan.DKBName);
        }

        public SetupTableModel(ZDStruct zd)
        {
            type = typeof(ZDStruct);
            obj = zd;

            En = zd.En;
            Name = zd.Description;
            Group = zd.Group;

            outputs = new InputOutputItem[7];
            outputs[0] = new InputOutputItem("КВО", zd.OKCindxArrDI, zd.OKCName);
            outputs[1] = new InputOutputItem("КВЗ", zd.CKCindxArrDI, zd.CKCName);
            outputs[2] = new InputOutputItem("Наличие напряжения", zd.VoltindxArrDI, zd.VoltName);
            outputs[3] = new InputOutputItem("МПО", zd.ODCindxArrDI, zd.ODCName);
            outputs[4] = new InputOutputItem("МПЗ", zd.CDCindxArrDI, zd.CDCName);
            outputs[5] = new InputOutputItem("Муфта", zd.MCindxArrDI, zd.MCName);
            outputs[6] = new InputOutputItem("Дистанционное управление", zd.DCindxArrDI, zd.DCName);

            inputs = new InputOutputItem[4];
            inputs[0] = new InputOutputItem("команда - открыть", zd.DOBindxArrDO, zd.DOBName);
            inputs[1] = new InputOutputItem("команда - остановить", zd.DCBindxArrDO, zd.DCBName);
            inputs[2] = new InputOutputItem("команда - закрыть", zd.DKBindxArrDO, zd.DKBName);
            inputs[3] = new InputOutputItem("команда - стоп закрытия", zd.DCBZindxArrDO, zd.DCBZName);

            _analogs = new AnalogIOItem[1];
            _analogs[0] = new AnalogIOItem("Положение затвора", 0,0,0, "");
        }


        public SetupTableModel(MPNAStruct agr)
        {
            type = typeof(MPNAStruct);
            obj = agr;

            Name = agr.Description;
            Group = agr.Group;
            En = agr.En;

            outputs = new InputOutputItem[4];
            outputs[0] = new InputOutputItem("ВВ включен сигнал 1",agr.MBC11indxArrDI, agr.MBC11Name);
            outputs[1] = new InputOutputItem("ВВ включен сигнал 2", agr.MBC12indxArrDI, agr.MBC12Name);
            outputs[2] = new InputOutputItem("ВВ отключен сигнал 1", agr.MBC21indxArrDI, agr.MBC21Name);
            outputs[3] = new InputOutputItem("ВВ отключен сигнал 2", agr.MBC22indxArrDI, agr.MBC22Name);
         
            outputs[0] = new InputOutputItem("Исправность цепей включения", agr.ECBindxArrDI, agr.ECBName);
            outputs[1] = new InputOutputItem("Исправность цепей отключения 1", agr.ECO11indxArrDI, agr.ECO11Name);
            outputs[2] = new InputOutputItem("Исправность цепей отключения 2", agr.ECO12indxArrDI, agr.ECO12Name);
            outputs[3] = new InputOutputItem("ECx02", agr.ECxindxArrDI, agr.ECxName);

            inputs = new InputOutputItem[3];
            inputs[0] = new InputOutputItem("Команда на включение", agr.ABBindxArrDO, agr.ABBName);
            inputs[1] = new InputOutputItem("Команда на отключение", agr.ABOindxArrDO, agr.ABOName);
            inputs[2] = new InputOutputItem("Команда на отключение 2", agr.ABO2indxArrDO, agr.ABO2Name);


            _analogs = new AnalogIOItem[2];
            _analogs[0] = new AnalogIOItem("Сила тока", agr.CurrentIndx, agr.Current_nominal, agr.Current_spd, agr.TokName);
            _analogs[1] = new AnalogIOItem("Частота вращения", agr.RPMindxArrAI, agr.RPM_nominal, agr.RPM_spd, agr.RPMSignalName);
        }

        public void ApplyChanges()
        {
            if (type == typeof(VSStruct))
            {
                VSStruct temp = obj as VSStruct;
                temp.ECindxArrDI = Outputs[0].Index;
                temp.MPCindxArrDI = Outputs[1].Index;
                temp.PCindxArrDI = Outputs[2].Index;

                temp.PCindxArrAI = Analogs[0].Index;
                temp.valuePC = Analogs[0].ValueNom;
                temp.valuePCspd = Analogs[0].ValueSpd;

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

                //TODO: добавить запись настроек аналогов
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

                agr.ECBindxArrDI = outputs[4].Index;
                agr.ECO11indxArrDI = outputs[5].Index;
                agr.ECO12indxArrDI = outputs[6].Index;
                agr.ECxindxArrDI = outputs[7].Index;

                agr.ABBindxArrDO = inputs[0].Index;
                agr.ABOindxArrDO = inputs[1].Index;
                agr.ABO2indxArrDO = inputs[2].Index;

                agr.CurrentIndx = Analogs[0].Index;
                agr.Current_nominal = Analogs[0].ValueNom;
                agr.Current_spd = Analogs[0].ValueSpd;

                agr.RPMindxArrAI = Analogs[1].Index;
                agr.RPM_nominal = Analogs[1].ValueNom;
                agr.RPM_spd = Analogs[1].ValueSpd;
            }
        }
    }
}
