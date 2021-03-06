﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Data;
using Simulator_MPSA.CL.Signal;

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
        //private InputOutputItem[] outputs;
        public ObservableCollection<InputOutputItem> Outputs
        {
            set; get;
        }

        private ObservableCollection<InputOutputItem> inputs;
        /// <summary>
        /// Массив ссылок на входа для алгоритма (команды на открытие/закрытие/пуск/стоп)
        /// </summary>
        public ObservableCollection<InputOutputItem> Inputs
        {
            get { return inputs; }
            set { inputs = value; }
        }
        private ObservableCollection<AnalogIOItem> _analogs;
        
        /// <summary>
        /// ссылки на алг
        /// </summary>
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

        public AddDICommand AddDICommand
        { get; set; }
        public RemoveDICommand RemDICommand
        { get; set; }

        private AddWatchCommand _cmdAddWatch=new AddWatchCommand();
        public AddWatchCommand AddToWatch
        {
            get
            {
                return _cmdAddWatch;
            }
            set
            {
                _cmdAddWatch = value;
            }
        }

        private bool showAO = true;
        private bool showAI = true;
        public Visibility AOVisible
        {
            get
            { return showAO ? Visibility.Visible : Visibility.Collapsed; }
        }
        public Visibility AIVisible
        {
            get
            { return showAI ? Visibility.Visible : Visibility.Collapsed; }
        }

        /// <summary>
        /// инициализация меню выбора сигналов
        /// </summary>
        void InitSelectMenu()
        {
            viewSourceDI.Source = DIStruct.items;
            viewSourceDO.Source = DOStruct.items;
            viewSourceAI.Source = AIStruct.items;
            viewSourceAO.Source = AOStruct.items;

            if (Name == null)
                Name = "";
                
                NameFilterDI = Name;              
                NameFilterDO = Name;              
                NameFilterAI = Name;       
                NameFilterAO = Name;
            
        }

        public SetupTableModel(VSStruct vs)
        {
            type = typeof(VSStruct);
            obj = vs;

            En = vs.En;
            Name = vs.Description;
            Group = vs.Group;

            Outputs = new ObservableCollection<InputOutputItem>();

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

            Outputs.Add( new InputOutputItem("Наличие напряжения", vs.ECindxArrDI, names[0], ESignalType.DI));
            Outputs.Add( new InputOutputItem("Магнитный пускатель", vs.MPCindxArrDI, names[1], ESignalType.DI));
            Outputs.Add( new InputOutputItem("Наличие давления на выходе", vs.PCindxArrDI, names[2], ESignalType.DI));
            Outputs.Add( new InputOutputItem("Наличие напряжения на СШ", vs.BusSecIndex, names[3], ESignalType.DI));

            // inputs = new InputOutputItem[2];
            Inputs = new ObservableCollection<InputOutputItem>();
            inputs.Add( new InputOutputItem("Команда - пуск", vs.ABBindxArrDO, names[4], ESignalType.DO));
            inputs.Add( new InputOutputItem("Команда - стоп", vs.ABOindxArrDO, names[5], ESignalType.DO));

            if (vs.controledAIs != null)
                _analogs = new ObservableCollection<AnalogIOItem>(vs.controledAIs);
            else _analogs = new ObservableCollection<AnalogIOItem>();

            foreach (DIItem c in vs.CustomDIs)
            {
                Outputs.Add(new InputOutputItem(c.Name, c.Index, c.GetSignalName, ESignalType.DI));
            }
            /* if (vs.isAVOA)
             {
                 ANInputs = new ObservableCollection<ANInput>();
                 ANInputs.Add(new ANInput("Задание частоты вращения", vs.SetRPM_Addr));
                 ANInputs[0].ADCtoRPM = vs.ADCtoRPM;
             }*/
            AnalogCommands = new InputOutputItem[1];
            AnalogCommands[0] = new InputOutputItem("Уставка", vs.AnCmdIndex, names[6], ESignalType.AO);

            AddDICommand = new AddDICommand(Outputs);
            RemDICommand = new RemoveDICommand(Outputs,4);


            InitSelectMenu();
        }

        public SetupTableModel(KLStruct klapan)
        {
            showAO = false;
            type = typeof(KLStruct);
            obj = klapan;

            En = klapan.En;
            Name = klapan.Description;
            Group = klapan.Group;

            Outputs = new ObservableCollection<InputOutputItem>
            {
                new InputOutputItem("Открыт", klapan.OKCindxArrDI, klapan.OKCName, ESignalType.DI),
                new InputOutputItem("Закрыт", klapan.CKCindxArrDI, klapan.CKCName, ESignalType.DI)
            };

            Inputs = new ObservableCollection<InputOutputItem>
            {
                new InputOutputItem("Команда - открыть", klapan.DOBindxArrDO, klapan.DOBName, ESignalType.DO),
                new InputOutputItem("Команда - закрыть", klapan.DKBindxArrDO, klapan.DKBName, ESignalType.DO)
            };
            AddDICommand = new AddDICommand(Outputs);
            RemDICommand = new RemoveDICommand(Outputs,2);

            InitSelectMenu();
        }

        public SetupTableModel(ZDStruct zd)
        {
            showAO = false;
            type = typeof(ZDStruct);
            obj = zd;

            En = zd.En;
            Name = zd.Description;
            Group = zd.Group;

            Outputs = new ObservableCollection<InputOutputItem>();

            //  List<string> names = new List<string>();
            Dictionary<string, string> names = new Dictionary<string, string>
            {
                { "OKC", zd.OKC != null ? zd.OKC.NameDI : "не определен" },
                { "CKC", zd.CKC != null ? zd.CKC.NameDI : "не определен" },
                { "Volt", zd.Volt != null ? zd.Volt.NameDI : "не определен" },
                { "ODC", zd.ODC != null ? zd.ODC.NameDI : "не определен" },
                { "CDC", zd.CDC != null ? zd.CDC.NameDI : "не определен" },
                { "MC", zd.MC != null ? zd.MC.NameDI : "не определен" },
                { "DC", zd.DC != null ? zd.DC.NameDI : "не определен" },
                { "BS", zd.BS != null ? zd.BS.NameDI : "не определен" },

                { "DOB", zd.DOB != null ? zd.DOB.NameDO : "не определен" },
                { "DCB", zd.DCB != null ? zd.DCB.NameDO : "не определен" },
                { "DKB", zd.DKB != null ? zd.DKB.NameDO : "не определен" },
                { "DCBZ", zd.DCBZ != null ? zd.DCBZ.NameDO : "не определен" }
            };

            Outputs.Add(new InputOutputItem("КВО", zd.OKCindxArrDI, names["OKC"], ESignalType.DI));
            Outputs.Add(new InputOutputItem("КВЗ", zd.CKCindxArrDI, names["CKC"], ESignalType.DI));
            Outputs.Add(new InputOutputItem("Наличие напряжения", zd.VoltindxArrDI, names["Volt"], ESignalType.DI));
            Outputs.Add(new InputOutputItem("МПО", zd.ODCindxArrDI, names["ODC"], ESignalType.DI));
            Outputs.Add(new InputOutputItem("МПЗ", zd.CDCindxArrDI, names["CDC"], ESignalType.DI));
            Outputs.Add(new InputOutputItem("Муфта", zd.MCindxArrDI, names["MC"], ESignalType.DI));
            Outputs.Add(new InputOutputItem("Дистанционное управление", zd.DCindxArrDI, names["DC"], ESignalType.DI));
            Outputs.Add(new InputOutputItem("Наличие напряжения на СШ",zd.BSIndex,names["BS"], ESignalType.DI));

            if (zd.CustomDIs == null || zd.CustomDIs.Count==0)
            {
                zd.CustomDIs = new List<DIItem>
                {
                    new DIItem("RS485. Открыта"),//открыта
                    new DIItem("RS485. Закрыта"),//закрыта
                    new DIItem("RS485. Открывается"),//открывается
                    new DIItem("RS485. Закрывается"),//закрывается
                    new DIItem("RS485. В дистанции"),//закрыта
                    new DIItem("RS485. Наличие связи")//наличие связи
                };
            }
            foreach (DIItem c in zd.CustomDIs)
            {
                Outputs.Add(new InputOutputItem(c.Name, c.Index, c.GetSignalName, ESignalType.DI));
            }


            inputs = new ObservableCollection<InputOutputItem>
            {
                new InputOutputItem("команда - открыть", zd.DOBindxArrDO, names["DOB"], ESignalType.DO),
                new InputOutputItem("команда - остановить", zd.DCBindxArrDO, names["DCB"], ESignalType.DO),
                new InputOutputItem("команда - закрыть", zd.DKBindxArrDO, names["DKB"], ESignalType.DO),
                new InputOutputItem("команда - стоп закрытия", zd.DCBZindxArrDO, names["DCBZ"], ESignalType.DO)
            };


            _analogs = new ObservableCollection<AnalogIOItem>
            {
                new AnalogIOItem("Положение затвора", zd.ZD_Pos_index, 100, 0, zd.PositionAIName)
            };

            AddDICommand = new AddDICommand(Outputs);
            RemDICommand = new RemoveDICommand(Outputs,14);

            InitSelectMenu();
        }


        public SetupTableModel(MPNAStruct agr)
        {
            showAO = false;
            type = typeof(MPNAStruct);
            obj = agr;

            Name = agr.Description;
            Group = agr.Group;
            En = agr.En;

            Outputs = new ObservableCollection<InputOutputItem>
            {
                new InputOutputItem("ВВ включен сигнал 1", agr.MBC11indxArrDI, agr.MBC11Name, ESignalType.DI),
                new InputOutputItem("ВВ включен сигнал 2", agr.MBC12indxArrDI, agr.MBC12Name, ESignalType.DI),
                new InputOutputItem("ВВ отключен сигнал 1", agr.MBC21indxArrDI, agr.MBC21Name, ESignalType.DI),
                new InputOutputItem("ВВ отключен сигнал 2", agr.MBC22indxArrDI, agr.MBC22Name, ESignalType.DI),

                new InputOutputItem("Исправность цепей включения", agr.ECBindxArrDI, agr.ECBName, ESignalType.DI),
                new InputOutputItem("Исправность цепей отключения 1", agr.ECO11indxArrDI, agr.ECO11Name, ESignalType.DI),
                new InputOutputItem("Исправность цепей отключения 2", agr.ECO12indxArrDI, agr.ECO12Name, ESignalType.DI)
            };

            if (agr.CustomDIs.Count > 0)
            {
                foreach (DIItem d in agr.CustomDIs)
                    Outputs.Add(new InputOutputItem(d.Name, d.DI));
            }
            //Outputs.Add(new InputOutputItem("ECx02", agr.ECxindxArrDI, agr.ECxName));

            inputs = new ObservableCollection<InputOutputItem>
            {
                new InputOutputItem("Команда на включение", agr.ABBindxArrDO, agr.ABBName, ESignalType.DO),
                new InputOutputItem("Команда на отключение", agr.ABOindxArrDO, agr.ABOName, ESignalType.DO),
                new InputOutputItem("Команда на отключение 2", agr.ABO2indxArrDO, agr.ABO2Name, ESignalType.DO)
            };

            if (agr.controledAIs != null)
                _analogs = new ObservableCollection<AnalogIOItem>(agr.controledAIs);
            else _analogs = new ObservableCollection<AnalogIOItem>();
            /* _analogs[0] = new AnalogIOItem("Сила тока", agr.CurrentIndx, agr.Current_nominal, agr.Current_spd, agr.TokName);
             _analogs[1] = new AnalogIOItem("Частота вращения", agr.RPMindxArrAI, agr.RPM_nominal, agr.RPM_spd, agr.RPMSignalName);*/

            AddDICommand = new AddDICommand(Outputs);
            RemDICommand = new RemoveDICommand(Outputs,7);

            InitSelectMenu();
        }

        public void ApplyChanges()
        {
            if (type == typeof(VSStruct))
            {
                VSStruct temp = obj as VSStruct;
                temp.ECindxArrDI = Outputs[0]._index;
                temp.MPCindxArrDI = Outputs[1]._index;
                temp.PCindxArrDI = Outputs[2]._index;
                temp.BusSecIndex = Outputs[3]._index;
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

                for (int i = 4; i < Outputs.Count; i++)
                    if (Outputs[i].Index !=null)
                    temp.CustomDIs.Add(new DIItem(Outputs[i].Name, Outputs[i]._index));
            }
            if (type == typeof(KLStruct))
            {
                KLStruct temp = obj as KLStruct;
                temp.DOBindxArrDO = inputs[0]._index;
                temp.DKBindxArrDO = inputs[1]._index;

                temp.OKCindxArrDI = Outputs[0]._index;
                temp.CKCindxArrDI = Outputs[1]._index;

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
                temp.OKCindxArrDI = Outputs[0]._index;
                temp.CKCindxArrDI = Outputs[1]._index;
                temp.VoltindxArrDI = Outputs[2]._index;
                temp.ODCindxArrDI = Outputs[3]._index;
                temp.CDCindxArrDI = Outputs[4]._index;
                temp.MCindxArrDI = Outputs[5]._index;
                temp.DCindxArrDI = Outputs[6]._index;
                temp.BSIndex = Outputs[7]._index;

                temp.DOBindxArrDO = inputs[0]._index;
                temp.DCBindxArrDO = inputs[1]._index;
                temp.DKBindxArrDO = inputs[2]._index;
                temp.DCBZindxArrDO = inputs[3]._index;

                temp.ZD_Pos_index = _analogs[0]._index;

                if (Outputs.Count > 8)
                {
                    temp.CustomDIs = new List<DIItem>();
                    for (int i = 8; i < Outputs.Count; i++)
                        temp.CustomDIs.Add(new DIItem(Outputs[i].Name,Outputs[i]._index));
                }
                //TODO: добавить запись настроек аналогов
            }
            if (type == typeof(MPNAStruct))
            {
                MPNAStruct agr = obj as MPNAStruct;

                agr.Description = Name;
                agr.Group = Group;
                agr.En = En;
                agr.MBC11indxArrDI = Outputs[0]._index;
                agr.MBC12indxArrDI = Outputs[1]._index;
                agr.MBC21indxArrDI = Outputs[2]._index;
                agr.MBC22indxArrDI = Outputs[3]._index;

                agr.ECBindxArrDI = Outputs[4]._index;
                agr.ECO11indxArrDI = Outputs[5]._index;
                agr.ECO12indxArrDI = Outputs[6]._index;
          //      agr.ECxindxArrDI = Outputs[7]._index;

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

                agr.CustomDIs = new List<DIItem>();
                for (int i = 7; i < Outputs.Count; i++)
                    if (Outputs[i].Index!=null)
                    agr.CustomDIs.Add(new DIItem(Outputs[i].Name, Outputs[i]._index));
            }
        }

        /// <summary>
        /// фильтр по имени в контекстном меню
        /// </summary>
        private string nameFilter = "";
        public string NameFilterDI
        {
            get
            {
                return nameFilter;
            }
            set
            {
                nameFilter = value;
                viewSourceDI.Filter += new FilterEventHandler(Filter_Func);           
            }
        }

        /// <summary>
        /// фильтр по имени в контекстном меню
        /// </summary>
        public string NameFilterAI
        {
            get
            {
                return nameFilter;
            }
            set
            {
                nameFilter = value;
                viewSourceAI.Filter += new FilterEventHandler(Filter_Func);
            }
        }
        /// <summary>
        /// фильтр по имени в контекстном меню
        /// </summary>
        public string NameFilterDO
        {
            get
            {
                return nameFilter;
            }
            set
            {
                nameFilter = value;
                viewSourceDO.Filter += new FilterEventHandler(Filter_Func);
            }
        }
        /// <summary>
        /// фильтр по имени в контекстном меню
        /// </summary>
        public string NameFilterAO
        {
            get
            {
                return nameFilter;
            }
            set
            {
                nameFilter = value;
                viewSourceAO.Filter += new FilterEventHandler(Filter_Func);
            }
        }

        private CollectionViewSource viewSourceDI = new CollectionViewSource();
        public CollectionViewSource ViewSourceDI
        { get { return viewSourceDI; } }

        private CollectionViewSource viewSourceDO = new CollectionViewSource();
        public CollectionViewSource ViewSourceDO
        { get { return viewSourceDO; } }

        private CollectionViewSource viewSourceAI = new CollectionViewSource();
        public CollectionViewSource ViewSourceAI
        { get { return viewSourceAI; } }

        private CollectionViewSource viewSourceAO = new CollectionViewSource();
        public CollectionViewSource ViewSourceAO
        { get { return viewSourceAO; } }


        private void Filter_Func(object sender, FilterEventArgs e)
        {
            string itemName = "";

            if (e.Item is DIStruct)
                itemName = (e.Item as DIStruct).NameDI;

            if (e.Item is DOStruct)
                itemName = (e.Item as DOStruct).NameDO;

            if (e.Item is AIStruct)
                itemName = (e.Item as AIStruct).NameAI;

            if (e.Item is AOStruct)
                itemName = (e.Item as AOStruct).Name;


     
                if (itemName == "")
                    e.Accepted = false;
                else
                {
                    if (itemName.ToLower().Contains(nameFilter.ToLower()))
                        e.Accepted = true;
                    else
                        e.Accepted = false;
                }
            
        }


        /// <summary>
        /// элемент выбранный в таблице дискретных сигналов
        /// </summary>
        public InputOutputItem SelectedIOitemDI
        {
            set; get;
        }
        public InputOutputItem SelectedIOitemDO
        {
            set; get;
        }
        //выбранный сигнал в таблице аналогов
        public AnalogIOItem SelectedAnalogAI
        {
            set; get;
        }

        public InputOutputItem SelectedAnalogAO
        {
            set; get;
        }
        //DI сигнал выбранный в контекстном меню
        private DIStruct _selectedDI;
        public DIStruct SelectedDI
        {
            get { return _selectedDI; }
            set
            {
                //осуществляем связывание
                _selectedDI = value;
                if (SelectedIOitemDI != null & _selectedDI != null)
                    SelectedIOitemDI.SetIndex(_selectedDI.indxArrDI);
            }
        }

        //DO сигнал выбранный в контекстном меню
        private DOStruct _selectedDO;
        public DOStruct SelectedDO
        {
            get { return _selectedDO; }
            set
            {
                //осуществляем связывание
                _selectedDO = value;
                if (SelectedIOitemDO != null & _selectedDO != null)
                    SelectedIOitemDO.SetIndex(_selectedDO.indxArrDO);
            }
        }
        //AI сигнал выбранный в контекстном меню
        private AIStruct _selectedAI;
        public AIStruct SelectedAI
        {
            get { return _selectedAI; }
            set
            {
                //осуществляем связывание
                _selectedAI = value;
                if (SelectedAnalogAI != null & _selectedAI != null)
                    SelectedAnalogAI.Index = _selectedAI.indxAI;
            }
        }
        //AO сигнал выбранный в контекстном меню
        private AOStruct _selectedAO;
        public AOStruct SelectedAO
        {
            get { return _selectedAO; }
            set
            {
                //осуществляем связывание
                _selectedAO = value;
                if (SelectedAnalogAO != null & _selectedAO != null)
                    SelectedAnalogAO.SetIndex(_selectedAO.indx);
            }
        }



    }

    public class AddWatchCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        public AddWatchCommand()
        {
        }
        public bool CanExecute(object parameter)
        {
            //      throw new NotImplementedException();
            return true;
        }

        public void Execute(object parameter)
        {
            System.Collections.IList list = (System.Collections.IList)parameter;
            
        
            try
            {                

                foreach (InputOutputItem itm in list.Cast<InputOutputItem>())
                {
                    if (itm.Index!=null)
                      WatchItem.Items.Add(new WatchItem(itm._index, itm.signalType));

                }
            }
            catch
            {
                foreach (AnalogIOItem itm in list.Cast<AnalogIOItem>())
                {
                    if (itm.Index != null)
                        WatchItem.Items.Add(new WatchItem((int)itm.Index,ESignalType.AI));

                }
            }

           
        }
    }

    public class AddDICommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        private Collection<InputOutputItem> outputs;
        public AddDICommand(Collection<InputOutputItem> outputs)
        {
            this.outputs = outputs;
        }

        public bool CanExecute(object parameter)
        {
            return true;
            //throw new NotImplementedException();
        }

        public void Execute(object parameter)
        {
            outputs.Add(new InputOutputItem());

               
        }
    }

    public class RemoveDICommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        private ObservableCollection<InputOutputItem> outputs;
        int minSize;
        public RemoveDICommand(ObservableCollection<InputOutputItem> outputs, int minSize=0)
        {
            this.minSize = minSize;
            this.outputs = outputs;
            outputs.CollectionChanged += delegate { CanExecuteChanged(this, null); };
        }
        public bool CanExecute(object parameter)
        {
            Debug.WriteLine("CanExecute remove command called");
            return  (outputs.Count > minSize);
            // throw new NotImplementedException();
        }

        public void Execute(object parameter)
        {
            //throw new NotImplementedException();
            outputs.RemoveAt(outputs.Count - 1);
        }
    }
}
