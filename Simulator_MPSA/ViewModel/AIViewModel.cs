﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simulator_MPSA.CL;
namespace Simulator_MPSA.ViewModel
{
    class AIViewModel : IViewModel<AIStruct>
    {
        private AIStruct ai;

        public AIViewModel()
        {
            ai = new AIStruct();
        }

        public BufType Buffer
        {
            get { return ai.Buffer; }
            set { ai.Buffer = value; }
        }

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

        public float ForcedValue
        {
            get { return ai.ForcedValue; }
            set { ai.ForcedValue = value; }
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
        public float minPhis
        {
            get { return ai.minPhis; }
            set { ai.minPhis = value; }
        }
        public float maxPhis
        {
            get { return ai.maxPhis; }
            set { ai.maxPhis = value; }
        }

        public float fValAI
        {
            get { return ai.fValAI; }
            set
            {
              ai.fValAI = value;                  
            }
        }

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
         //   throw new NotImplementedException();
        }
    }
}