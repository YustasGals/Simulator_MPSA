using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simulator_MPSA.CL.Signal;
using Simulator_MPSA.CL;
namespace Simulator_MPSA.ViewModel
{

    class WatchItemViewModel : INotifyPropertyChanged, IViewModel<WatchItem>
    {
        WatchItem model;

        /*    public WatchItemViewModel(DIStruct item)
            {
                this.item = item;
                item.PropertyChanged += Item_PropertyChanged;
            }

            public WatchItemViewModel(DOStruct item)
            {
                this.item = item;
                item.PropertyChanged += Item_PropertyChanged;
            }

            public WatchItemViewModel(AIStruct item)
            {
                this.item = item;
                item.PropertyChanged += Item_PropertyChanged;
            }

            public WatchItemViewModel(AOStruct item)
            {
                this.item = item;
                item.PropertyChanged += Item_PropertyChanged;
            }*/
        public WatchItemViewModel(WatchItem item)
        {
            model = item;
            model.PropertyChanged += Item_PropertyChanged;
        }

        public WatchItemViewModel() { }

    //    public object item;

        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
            {
                if (e.PropertyName == "ForcedValue")
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Value"));
                //PropertyChanged(sender, new PropertyChangedEventArgs("Value"));


                if (e.PropertyName == "Name")
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Name"));
                //PropertyChanged(sender, new PropertyChangedEventArgs("Name"));

                if (e.PropertyName == "Forced")
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Forced"));

            }

        }

        public WatchItem GetModel()
        {
            return model;
        }

        public void SetModel(WatchItem model)
        {
            this.model = model;
            model.PropertyChanged += Item_PropertyChanged;
        }

        public string GetTag()
        {
            return "";
          //  throw new NotImplementedException();
        }

        public string GetName()
        {
            return "";
          //  throw new NotImplementedException();
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public string Name
        {
            get
            {
                if (model != null)
                {
                    try
                    {
                        switch (model.SignalType)
                        {
                            case ESignalType.AI:
                                return AIStruct.items[model.index].NameAI;

                            case ESignalType.DI:
                                return DIStruct.items[model.index].NameDI;
                            case ESignalType.DO:
                                return DOStruct.items[model.index].NameDO;
                            case ESignalType.AO:
                                return AOStruct.items[model.index].Name;

                        }
                    }
                    catch (Exception ex)
                    {
                        
                    }
                   
                }

                return "";
            }
            set { }
        }

        public float? Value
        {
            get
            {
               try
                {
                    switch (model.SignalType)
                    {
                        case ESignalType.AI:
                            return AIStruct.items[model.index].ForcedValue;

                        case ESignalType.DI:
                            return DIStruct.items[model.index].ForcedValue ? 1:0;
                        case ESignalType.DO:
                            return DOStruct.items[model.index].ForcedValue ? 1:0;
                        case ESignalType.AO:
                            return AOStruct.items[model.index].fVal;
                    }

                }
                catch(Exception e)
                {
                    //на случай если элемента не оказалось в массиве
                }

                return null;
            }
            set
            {
                if (model != null)
                {
                    switch (model.SignalType)
                    {
                        case ESignalType.AI:
                            if (value!=null)
                            AIStruct.items[model.index].ForcedValue = (float)value;
                            break;
                        case ESignalType.DI:
                            DIStruct.items[model.index].ForcedValue = (float)value>=1.0f;
                            break;
                        case ESignalType.DO:
                            DOStruct.items[model.index].ForcedValue = (float)value>=1.0f;
                            break;
                        case ESignalType.AO:
                            if (value!=null)
                            AOStruct.items[model.index].ForceValue = (float)value;
                            break;
                    }
                }
            }
            
        }

        public bool Forced
        {
            get
            {
                try
                {
                    switch (model.SignalType)
                    {
                        case ESignalType.AI:
                            return AIStruct.items[model.index].Forced;

                        case ESignalType.DI:
                            return DIStruct.items[model.index].Forced;
                        case ESignalType.DO:
                            return DOStruct.items[model.index].Forced;
                        case ESignalType.AO:
                            return AOStruct.items[model.index].Forced;
                    }

                }
                catch (Exception e)
                {
                    //на случай если элемента не оказалось в массиве
                }
                return false;
            }
            set
            {
                if (model != null)
                {
                    switch (model.SignalType)
                    {
                        case ESignalType.AI:
                                AIStruct.items[model.index].Forced = value;
                            break;
                        case ESignalType.DI:
                            DIStruct.items[model.index].Forced = value;
                            break;
                        case ESignalType.DO:
                            DOStruct.items[model.index].Forced = value;
                            break;
                        case ESignalType.AO:
                                AOStruct.items[model.index].Forced = value;
                            break;
                    }
                }
            }
        }

        public string ToolTip
        {
            get
            {
                try
                {
                    switch (model.SignalType)
                    {
                        case ESignalType.AI:
                            AIStruct ai = AIStruct.FindByIndex(model.index);
                            string tooltip = "мин АЦП: " + ai.minACD.ToString() + "; макс АЦП: " + ai.maxACD.ToString() + "; мин инж: " + ai.minPhis.ToString() + "; макс инж: " + ai.maxPhis.ToString();
                            return tooltip;
                        case ESignalType.DI: return "";
                        case ESignalType.AO: return "";
                        case ESignalType.DO: return "";

                    }
                }
                catch (Exception e)
                { }
                return "";
            }

            set { }
        }

        public string Type
        {
            get
            {
                try
                {
                    switch (model.SignalType)
                    {
                        case ESignalType.AI: return "AI -> PLC";
                        case ESignalType.DI: return "DI -> PLC";
                        case ESignalType.AO: return "AO <-- PLC";
                        case ESignalType.DO: return "DO <-- PLC";

                    }
                }
                catch (Exception e)
                { }
                return "";
            }
            set { }
        }

        public int Index
        {
            get
            {
                return model.index;
            }
            set { }
        }

        public string TagName
        {
            get
            {
                try
                {
                    switch (model.SignalType)
                    {
                        case ESignalType.AI: return AIStruct.items[model.index].TegAI;
                        case ESignalType.DI: return DIStruct.items[model.index].TegDI;
                        case ESignalType.AO: return AOStruct.items[model.index].TagName;
                        case ESignalType.DO: return DOStruct.items[model.index].TegDO;

                    }
                }
                catch (Exception e)
                { }
                return "";
            }
            set
            {
            }
        }
    }
}
