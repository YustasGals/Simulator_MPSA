using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simulator_MPSA.CL.Signal;

namespace Simulator_MPSA.CL
{
    /// <summary>
    /// элемент отображения
    /// </summary>
    public class WatchItem:INotifyPropertyChanged
    {
        private static ObservableCollection<WatchItem> _items = new ObservableCollection<WatchItem>();
        public static ObservableCollection<WatchItem> Items
        {
            get { return _items; }
        }

        public int index = -1;
        ESignalType signalType=ESignalType.Nothing;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Index
        {
            get { return index; }
            set { index = value; }
        }

        public ESignalType SignalType
        {
            get { return signalType; }
            set { signalType = value; }
        }

        public WatchItem(DIStruct signal)
        {
            index = signal.indxArrDI;
            signalType = ESignalType.DI;
            signal.PropertyChanged += Signal_PropertyChanged;
        }

        private void Signal_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(sender, e);
        }

        public WatchItem(DOStruct signal)
        {
            index = signal.indxArrDO;
            signalType = ESignalType.DO;
            signal.PropertyChanged += Signal_PropertyChanged;
        }

        public WatchItem(AIStruct signal)
        {
            index = signal.indxAI;
            signalType = ESignalType.AI;
            signal.PropertyChanged += Signal_PropertyChanged;
        }

        public WatchItem(AOStruct signal)
        {
            index = signal.indx;
            signalType = ESignalType.AO;
            signal.PropertyChanged += Signal_PropertyChanged;
        }
        public WatchItem(int index, ESignalType type)
        {
            this.index = index;
            this.signalType = type;
            switch (type)
            {
                case ESignalType.DI:
                    DIStruct signaldi = DIStruct.FindByIndex(index);                   
                    signaldi.PropertyChanged += Signal_PropertyChanged;
                    break;
                case ESignalType.AI:
                    AIStruct signalai = AIStruct.FindByIndex(index);
                    signalai.PropertyChanged += Signal_PropertyChanged;
                    break;
                case ESignalType.DO:
                    DOStruct signaldo = DOStruct.FindByIndex(index);
                    signaldo.PropertyChanged += Signal_PropertyChanged;
                    break;
                case ESignalType.AO:
                    AOStruct signalao = AOStruct.FindByIndex(index);
                    signalao.PropertyChanged += Signal_PropertyChanged;
                    break;
            }
        }
        /// <summary>
        /// актуализировать привязку
        /// </summary>
        public void RefreshLink()
        {
            if (index>-1)
                switch (signalType)
                {
                    case ESignalType.DI:
                        DIStruct signaldi = DIStruct.FindByIndex(index);
                        signaldi.PropertyChanged += Signal_PropertyChanged;
                        break;
                    case ESignalType.AI:
                        AIStruct signalai = AIStruct.FindByIndex(index);
                        signalai.PropertyChanged += Signal_PropertyChanged;
                        break;
                    case ESignalType.DO:
                        DOStruct signaldo = DOStruct.FindByIndex(index);
                        signaldo.PropertyChanged += Signal_PropertyChanged;
                        break;
                    case ESignalType.AO:
                        AOStruct signalao = AOStruct.FindByIndex(index);
                        signalao.PropertyChanged += Signal_PropertyChanged;
                        break;
                }
        }
        public WatchItem()
        { }
    }
}
