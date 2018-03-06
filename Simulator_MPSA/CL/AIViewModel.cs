using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulator_MPSA.CL
{
    class AIViewModel : BaseViewModel
    {
        private AIStruct _ai;
        public AIViewModel(AIStruct AIelement)
        {
            _ai = AIelement;
        }
        public bool En
        {
            get { return _ai.En; }
            set
            { _ai.En = value; OnPropertyChanged("En"); }
        }
        public int indxAI
        {
            get { return _ai.indxAI; }
            set { _ai.indxAI = value; OnPropertyChanged("indxAI"); }
        }
        public int indxW
        {
            get { return _ai.indxW; }
            set { _ai.indxW = value; OnPropertyChanged("indxW"); }
        }
        public string TegAI
        {
            get { return _ai.TegAI; }
            set { _ai.TegAI = value; OnPropertyChanged("TegAI"); }
        }
        public string NameAI
        {
            get { return _ai.NameAI; }
            set { _ai.NameAI = value; OnPropertyChanged("NameAI"); }
        }
        public ushort ValADC
        {
            get { return _ai.ValACD; }
            set { _ai.ValACD=value; OnPropertyChanged("ValACD"); }
        }
        public ushort minADC
        {
            get { return _ai.minACD; }
            set { _ai.minACD = value; OnPropertyChanged("minADC"); }
        }
        public ushort maxADC
        {
            get { return _ai.maxACD; }
            set { _ai.maxACD = value; OnPropertyChanged("maxADC"); }
        }
        public float minPhis
        {
            get { return _ai.minPhis; }
            set { _ai.minPhis = value; OnPropertyChanged("minPhis"); }
        }
        public float maxPhis
        {
            get { return _ai.maxPhis; }
            set { _ai.maxPhis = value; OnPropertyChanged("maxPhis"); }
        }
        public float fValAI
        {
            get { return _ai.fValAI; }
            set { _ai.fValAI = value; OnPropertyChanged("fValAI"); }
        }
        public int DelayAI
        {
            get { return _ai.DelayAI; }
            set { _ai.DelayAI = value; OnPropertyChanged("DelayAI"); }
        }
    }
}
