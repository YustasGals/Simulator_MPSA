using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simulator_MPSA.CL.Signal;
namespace Simulator_MPSA.ViewModel
{
    class AIViewModel : IViewModel<AIStruct>, INotifyPropertyChanged
    {
        private AIStruct ai;
        static CultureInfo culture = new CultureInfo("en-US");

        public AIViewModel()
        {
            ai = new AIStruct();
            ai.PropertyChanged += _do_PropertyChanged;
        }

    /*    public BufType Buffer
        {
            get { return ai.Buffer; }
            set { ai.Buffer = value; }
        }*/

        public EPLCDestType PLCDestType
        {
            get { return ai.PLCDestType; }
            set { ai.PLCDestType = value; }
        }

        public string OPCtag
        {
            get { return ai.OPCtag; }
            set { ai.OPCtag = value; }
        }

        public bool Forced
        {
            get { return ai.Forced; }
            set { ai.Forced = value; }
        }

        public string ForcedValue
        {
            get { return ai.ForcedValue.ToString(culture); }
            set
            {
                try
                {
                    ai.ForcedValue = float.Parse(value.Replace(',', '.'), culture);
                }
                catch 
                {
                    ai.ForcedValue = 0;
                }
            }
        }

        public int PLCAddr
        {
            get { return ai.PLCAddr; }
            set { ai.PLCAddr = value; }
        }

        public string NameAI
        {
            get { return ai.NameAI; }
            set { ai.NameAI = value; }
        }

        public bool En
        {
            get { return ai.En; }
            set { ai.En = value; }
        }
        
        public int indxAI
        {
            get { return ai.indxAI; }
            set { ai.indxAI = value; }
         
        }
        public ushort ValACD
        {
            get
            {
                return ai.ValACD;
            }
            set { ai.ValACD = value; }
        }
        public ushort minACD
        {
            get { return ai.minACD; }
            set { ai.minACD = value; }
        }
        public ushort maxACD
        {
            get { return ai.maxACD; }
            set { ai.maxACD = value; }
        }
        public string minPhis
        {
            get { return ai.minPhis.ToString(culture); }
            set {
                try
                {
                    ai.minPhis = float.Parse(value.Replace(',', '.'), culture);
                }
                catch 
                {
                    ai.minPhis = 0f;
                }
            }
        }
        public string maxPhis
        {
            get { return ai.maxPhis.ToString(culture); }
            set
            {
                try
                {
                    ai.maxPhis = float.Parse(value.Replace(',', '.'), culture);
                }
                catch
                {
                    ai.maxPhis = 0f;
                }
            }
        }

   /*     public string fValAI
        {
            get { return ai.fValAI.ToString(); }
            set
            {
               ai.fValAI = float.Parse(value.Replace(',', '.'));
                        
            }
        }
        */
        public string TegAI
        {
            get { return ai.TegAI; }
            set { ai.TegAI = value; }
        }
        public AIStruct GetModel()
        {
            return ai;
           // throw new NotImplementedException();
        }

        public string GetName()
        {
            return ai.NameAI;
         //   throw new NotImplementedException();
        }

        public string GetTag()
        {
            return ai.TegAI;

         //   throw new NotImplementedException();
        }

        public void SetModel(AIStruct model)
        {
            ai = model;
            ai.PropertyChanged += _do_PropertyChanged;
         //   throw new NotImplementedException();
        }
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        protected void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            PropertyChanged(this, args);
        }
        private void _do_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e);
            // throw new NotImplementedException();
        }
    }
}
