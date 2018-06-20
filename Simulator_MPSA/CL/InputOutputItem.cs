using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace Simulator_MPSA.CL
{
    /// <summary>
    /// класс дискретного сигнала
    /// используется для отображения в таблице настроек
    /// </summary>
    public class InputOutputItem : INotifyPropertyChanged
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
        {
            get { return _assignedsignalName; }
            set
            {
                _assignedsignalName = value;
                OnPropertyChanged("AssignedSignal");
            }
        }
        public InputOutputItem() { }

        public InputOutputItem(string name, int index, string assignedSignalName, bool isOk = true)
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
    /// класс для управления аналоговым сигналом
    /// используется для отображения в таблице настроек
    /// </summary>
    public class AnalogIOItem:INotifyPropertyChanged
    {
        /// <summary>
        /// название сигнала в таблице настроек (короткое название)
        /// </summary>
        public string Name
        { get; set; }

        private int _index;
        /// <summary>
        /// индекс в общей таблице AI
        /// </summary>
        public int Index
        { get { return _index; }
            set
            {
                if (_index != value || ai == null)
                    if (ai != null)
                        ai.IndexChanged -= Ai_IndexChanged;
                _index = value;
                if (_index > -1)
                    ai = AIStruct.FindByIndex(_index);

                if (ai!=null)
                    ai.IndexChanged += Ai_IndexChanged;
                OnPropertyChanged("AIName");
            }
        }

        private void Ai_IndexChanged(object sender, Signal.IndexChangedEventArgs e)
        {
            _index = e.newIndex;
            // throw new NotImplementedException();
            LogViewModel.WriteLine("\"" + Name + "\": " + "изменен индекс сигнала AI");
        }

        /// <summary>
        /// исправность сигнала
        /// </summary>
        public bool IsOK
        { get; set; }

        float _nomValue;
        /// <summary>
        /// Номинальное значение которое постепенно достигается при пуске агрегата
        /// </summary>
        public float ValueNom
        {
            get { return _nomValue; }
            set { _nomValue = value; OnPropertyChanged("ValueNom"); }
        }

        /// <summary>
        /// ссылка на аналог
        /// </summary>
        AIStruct ai;
        public AIStruct AI
        {
            set {}
            get
            {
                if (ai == null)
                {
                    if (Index > -1)
                        ai = AIStruct.FindByIndex(Index);
                    else
                        ai = new AIStruct();
                }

                    return ai;
            }
        }
        /// <summary>
        /// наименование сигнала по таблице аналогов
        /// </summary>
        public string AIName
        {
            set { }
            get { if (AI != null) return AI.NameAI; else return "не определен"; }
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
        public AnalogIOItem()
        {
        }
        public AnalogIOItem(string name, int index, float value, float spdVal, string assignedSignalName)
        {
            Name = name;
            Index = index;
           
            ValueNom = value;
            ValueSpd = spdVal;
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
