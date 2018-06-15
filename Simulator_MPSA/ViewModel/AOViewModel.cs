﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simulator_MPSA.CL;
namespace Simulator_MPSA.ViewModel
{
    class AOViewModel : IViewModel<AOStruct>, INotifyPropertyChanged
    {
        private AOStruct model;

        public bool En
        {
            get { return model.En; }
            set { model.En = value; }
        }

        public AOViewModel()
        {
            model = new AOStruct();
            model.PropertyChanged += _do_PropertyChanged;
        }
        public AOStruct GetModel()
        {
            return model;
       //     throw new NotImplementedException();
        }

        public string GetName()
        {
            return model.Name;
         //   throw new NotImplementedException();
        }

        public string GetTag()
        {
            return model.TagName;
        //    throw new NotImplementedException();
        }

        public void SetModel(AOStruct model)
        {
            this.model = model;
            model.PropertyChanged += _do_PropertyChanged;
            //    throw new NotImplementedException();
        }

        public int indx
        {
            get { return model.indx; }
            set { model.indx = value; }
        }

        public int PLCAddr
        {
            get { return model.PLCAddr; }
            set { model.PLCAddr = value; }
        }

        public bool Forced
        {
            get { return model.Forced; }
            set { model.Forced = value; }
        }

        public string OPCtag
        {
            get { return model.OPCtag; }
            set { model.OPCtag = value; }
        }

        public string TagName
        {
            get { return model.TagName; }
            set { model.TagName = value; }
        }

        public string Name
        {
            get { return model.Name; }
            set { model.Name = value; }
        }

        public EPLCDestType PLCDestType
        {
            get { return model.PLCDestType; }
            set { model.PLCDestType = value; }
        }

        public ushort ValACD
        {
            get { return model.ValACD; }
            set { model.ValACD = value; }
        }

        public ushort minACD
        {
            get { return model.minACD; }
            set { model.minACD = value; }
        }

        public ushort maxACD
        {
            get { return model.maxACD; }
            set { model.maxACD = value; }
        }

        public float minPhis
        {
            get { return model.minPhis; }
            set { model.minPhis = value; }
        }

        public float maxPhis
        {
            get { return model.maxPhis; }
            set { model.maxPhis = value; }
        }

        public float ForcedValue
        {
            get { return model.fVal; }
            set {
                if (Forced)
                model.fVal = value;
            }
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
