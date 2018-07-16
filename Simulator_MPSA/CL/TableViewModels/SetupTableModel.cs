using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace Simulator_MPSA.CL
{
   public class ANInput
    {
        public string Name
        { set; get; }
        public int PLCAddr
            { get; set;}
        public int ADCtoRPM
        { get; set; }
        public ANInput(string name, int plcaddr)
        {
            Name = name;
            PLCAddr= plcaddr;
            ADCtoRPM = 640;
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
        private ObservableCollection<AnalogIOItem> _analogs;
       // private AnalogIOItem[] _analogs;
        public ObservableCollection<AnalogIOItem> Analogs
        {
            get { return _analogs; }
            set { _analogs = value; }
        }

        private InputOutputItem[] _analogCommands;
        public InputOutputItem[] AnalogCommands
        {
            get { return _analogCommands; }
            set { _analogCommands = value; }
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

            outputs = new InputOutputItem[4];

            string[] names = new string[7];
            if (vs.EC != null)
                names[0] = vs.EC.NameDI;
            if (vs.MPC != null)
                names[1] = vs.MPC.NameDI;
            if (vs.PC != null)
                names[2] = vs.PC.NameDI;
            if (vs.BS != null)
                names[3] = vs.BS.NameDI;
            if (vs.ABB != null)
                names[4] = vs.ABB.NameDO;
            if (vs.ABO != null)
                names[5] = vs.ABO.NameDO;
            if (vs.AnalogCommand != null)
                names[6] = vs.AnalogCommand.Name;

            outputs[0] = new InputOutputItem("Наличие напряжения", vs.ECindxArrDI, names[0]);
            outputs[1] =  new InputOutputItem("Магнитный пускатель", vs.MPCindxArrDI, names[1]);
            outputs[2] = new InputOutputItem("Наличие давления на выходе", vs.PCindxArrDI, names[2]);
            outputs[3] = new InputOutputItem("Наличие напряжения на СШ", vs.BusSecIndex, names[3]);

            inputs = new InputOutputItem[2];
            inputs[0]= new InputOutputItem("Команда - пуск", vs.ABBindxArrDO, names[4]);
            inputs[1]= new InputOutputItem("Команда - стоп", vs.ABOindxArrDO, names[5]);

            if (vs.controledAIs != null)
                _analogs = new ObservableCollection<AnalogIOItem>(vs.controledAIs);
            else _analogs = new ObservableCollection<AnalogIOItem>();

            /* if (vs.isAVOA)
             {
                 ANInputs = new ObservableCollection<ANInput>();
                 ANInputs.Add(new ANInput("Задание частоты вращения", vs.SetRPM_Addr));
                 ANInputs[0].ADCtoRPM = vs.ADCtoRPM;
             }*/
            AnalogCommands = new InputOutputItem[1];
            AnalogCommands[0] = new InputOutputItem("Уставка", vs.AnCmdIndex, names[6]);
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

            outputs = new InputOutputItem[8];

          //  List<string> names = new List<string>();
            Dictionary<string, string> names = new Dictionary<string, string>();
            names.Add("OKC",zd.OKC !=null?      zd.OKC.NameDI: "не определен");
            names.Add("CKC",zd.CKC != null ?    zd.CKC.NameDI : "не определен");
            names.Add("Volt",zd.Volt != null ?  zd.Volt.NameDI : "не определен");
            names.Add("ODC",zd.ODC != null ?    zd.ODC.NameDI : "не определен");
            names.Add("CDC",zd.CDC != null ?    zd.CDC.NameDI : "не определен");
            names.Add("MC",zd.MC != null ?      zd.MC.NameDI : "не определен");
            names.Add("DC", zd.DC != null ?     zd.DC.NameDI : "не определен");
            names.Add("BS", zd.BS != null ?     zd.BS.NameDI : "не определен");

            names.Add("DOB", zd.DOB != null ? zd.DOB.NameDO : "не определен");
            names.Add("DCB", zd.DCB != null ? zd.DCB.NameDO : "не определен");
            names.Add("DKB", zd.DKB != null ? zd.DKB.NameDO : "не определен");
            names.Add("DCBZ", zd.DCBZ != null ? zd.DCBZ.NameDO : "не определен");

            outputs[0] = new InputOutputItem("КВО", zd.OKCindxArrDI, names["OKC"]);
            outputs[1] = new InputOutputItem("КВЗ", zd.CKCindxArrDI, names["CKC"]);
            outputs[2] = new InputOutputItem("Наличие напряжения", zd.VoltindxArrDI, names["Volt"]);
            outputs[3] = new InputOutputItem("МПО", zd.ODCindxArrDI, names["ODC"]);
            outputs[4] = new InputOutputItem("МПЗ", zd.CDCindxArrDI, names["CDC"]);
            outputs[5] = new InputOutputItem("Муфта", zd.MCindxArrDI, names["MC"]);
            outputs[6] = new InputOutputItem("Дистанционное управление", zd.DCindxArrDI, names["DC"]);
            outputs[7] = new InputOutputItem("наличие напряжения на СШ",zd.BSIndex,names["BS"]);

            inputs = new InputOutputItem[4];
            inputs[0] = new InputOutputItem("команда - открыть", zd.DOBindxArrDO, names["DOB"]);
            inputs[1] = new InputOutputItem("команда - остановить", zd.DCBindxArrDO, names["DCB"]);
            inputs[2] = new InputOutputItem("команда - закрыть", zd.DKBindxArrDO, names["DKB"]);
            inputs[3] = new InputOutputItem("команда - стоп закрытия", zd.DCBZindxArrDO, names["DCBZ"]);


            _analogs = new ObservableCollection<AnalogIOItem>();
            _analogs.Add( new AnalogIOItem("Положение затвора",zd.ZD_Pos_index, 100,0, zd.PositionAIName));
        }


        public SetupTableModel(MPNAStruct agr)
        {
            type = typeof(MPNAStruct);
            obj = agr;

            Name = agr.Description;
            Group = agr.Group;
            En = agr.En;

            outputs = new InputOutputItem[8];
            outputs[0] = new InputOutputItem("ВВ включен сигнал 1",agr.MBC11indxArrDI, agr.MBC11Name);
            outputs[1] = new InputOutputItem("ВВ включен сигнал 2", agr.MBC12indxArrDI, agr.MBC12Name);
            outputs[2] = new InputOutputItem("ВВ отключен сигнал 1", agr.MBC21indxArrDI, agr.MBC21Name);
            outputs[3] = new InputOutputItem("ВВ отключен сигнал 2", agr.MBC22indxArrDI, agr.MBC22Name);
         
            outputs[4] = new InputOutputItem("Исправность цепей включения", agr.ECBindxArrDI, agr.ECBName);
            outputs[5] = new InputOutputItem("Исправность цепей отключения 1", agr.ECO11indxArrDI, agr.ECO11Name);
            outputs[6] = new InputOutputItem("Исправность цепей отключения 2", agr.ECO12indxArrDI, agr.ECO12Name);
            outputs[7] = new InputOutputItem("ECx02", agr.ECxindxArrDI, agr.ECxName);

            inputs = new InputOutputItem[3];
            inputs[0] = new InputOutputItem("Команда на включение", agr.ABBindxArrDO, agr.ABBName);
            inputs[1] = new InputOutputItem("Команда на отключение", agr.ABOindxArrDO, agr.ABOName);
            inputs[2] = new InputOutputItem("Команда на отключение 2", agr.ABO2indxArrDO, agr.ABO2Name);

            if (agr.controledAIs != null)
                _analogs = new ObservableCollection<AnalogIOItem>(agr.controledAIs);
            else _analogs = new ObservableCollection<AnalogIOItem>();
           /* _analogs[0] = new AnalogIOItem("Сила тока", agr.CurrentIndx, agr.Current_nominal, agr.Current_spd, agr.TokName);
            _analogs[1] = new AnalogIOItem("Частота вращения", agr.RPMindxArrAI, agr.RPM_nominal, agr.RPM_spd, agr.RPMSignalName);*/
        }

        public void ApplyChanges()
        {
            if (type == typeof(VSStruct))
            {
                VSStruct temp = obj as VSStruct;
                temp.ECindxArrDI = outputs[0]._index;
                temp.MPCindxArrDI = outputs[1]._index;
                temp.PCindxArrDI = outputs[2]._index;
                temp.BusSecIndex = outputs[3]._index;
             //   temp.PCindxArrAI = Analogs[0].Index;
              /*  temp.valuePC = Analogs[0].ValueNom;
                temp.valuePCspd = Analogs[0].ValueSpd;
                */
                temp.Description = Name;
                temp.Group = Group;
                temp.En = En;

                temp.ABBindxArrDO = inputs[0]._index;
                temp.ABOindxArrDO = inputs[1]._index;

                if (_analogs.Count > 0)
                    temp.controledAIs = _analogs.ToArray();
                else
                    temp.controledAIs = null;

                /* if (ANInputs != null && ANInputs.Count > 0)
                 {
                     temp.SetRPM_Addr = ANInputs[0].PLCAddr;
                     temp.ADCtoRPM = ANInputs[0].ADCtoRPM;
                 }*/
                temp.AnCmdIndex = AnalogCommands[0]._index;
            }
            if (type == typeof(KLStruct))
            {
                KLStruct temp = obj as KLStruct;
                temp.DOBindxArrDO = inputs[0]._index;
                temp.DKBindxArrDO = inputs[1]._index;

                temp.OKCindxArrDI = outputs[0]._index;
                temp.CKCindxArrDI = outputs[1]._index;

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
                temp.OKCindxArrDI = outputs[0]._index;
                temp.CKCindxArrDI = outputs[1]._index;
                temp.VoltindxArrDI = outputs[2]._index;
                temp.ODCindxArrDI = outputs[3]._index;
                temp.CDCindxArrDI = outputs[4]._index;
                temp.MCindxArrDI = outputs[5]._index;
                temp.DCindxArrDI = outputs[6]._index;
                temp.BSIndex = outputs[7]._index;

                temp.DOBindxArrDO = inputs[0]._index;
                temp.DCBindxArrDO = inputs[1]._index;
                temp.DKBindxArrDO = inputs[2]._index;
                temp.DCBZindxArrDO = inputs[3]._index;

                temp.ZD_Pos_index = _analogs[0]._index;

                //TODO: добавить запись настроек аналогов
            }
            if (type == typeof(MPNAStruct))
            {
                MPNAStruct agr = obj as MPNAStruct;

                agr.Description = Name;
                agr.Group = Group;
                agr.En = En;
                agr.MBC11indxArrDI = outputs[0]._index;
                agr.MBC12indxArrDI = outputs[1]._index;
                agr.MBC21indxArrDI = outputs[2]._index;
                agr.MBC22indxArrDI = outputs[3]._index;

                agr.ECBindxArrDI = outputs[4]._index;
                agr.ECO11indxArrDI = outputs[5]._index;
                agr.ECO12indxArrDI = outputs[6]._index;
                agr.ECxindxArrDI = outputs[7]._index;

                agr.ABBindxArrDO = inputs[0]._index;
                agr.ABOindxArrDO = inputs[1]._index;
                agr.ABO2indxArrDO = inputs[2]._index;

                /*  agr.CurrentIndx = Analogs[0].Index;
                  agr.Current_nominal = Analogs[0].ValueNom;
                  agr.Current_spd = Analogs[0].ValueSpd;

                  agr.RPMindxArrAI = Analogs[1].Index;
                  agr.RPM_nominal = Analogs[1].ValueNom;
                  agr.RPM_spd = Analogs[1].ValueSpd;*/
                if (Analogs.Count > 0)
                    agr.controledAIs = Analogs.ToArray();
                else
                    agr.controledAIs = null;
            }
        }
    }
}
