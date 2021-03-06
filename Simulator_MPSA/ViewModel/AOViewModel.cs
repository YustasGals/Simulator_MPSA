﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simulator_MPSA.CL;
using Simulator_MPSA.CL.Signal;
namespace Simulator_MPSA.ViewModel
{
    class AOViewModel : IViewModel<AOStruct>, INotifyPropertyChanged
    {
        private AOStruct model;
        static CultureInfo culture = new CultureInfo("en-US");
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
        /// <summary>
        /// универсальное свойство для отображения/записи адреса одним значением
        /// </summary>
        public string AddressTag
        {
            set
            {
                if (value.Substring(0, 3).ToUpper() == "MB:")
                {
                    string mbaddr = value.Substring(3);
                    try
                    {
                        model.PLCAddr = int.Parse(mbaddr.Trim(' '));
                    }
                    catch
                    {
                        System.Windows.MessageBox.Show(Properties.Resources.AddressingHintAnalog);
                    }
                }
                else if (value.Substring(0, 4).ToUpper() == "OPC:")
                {
                    string opcaddr = value.Substring(4);
                    model.OPCtag = opcaddr.Trim(' ');
                }
                else
                    System.Windows.MessageBox.Show(Properties.Resources.AddressingHintAnalog);


            }
            get
            {
                if (model.OPCtag != "")
                    return "OPC:" + model.OPCtag;
                else
                    return "MB:" + model.PLCAddr;
            }
        }

        public int Indx
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

        public ushort MinACD
        {
            get { return model.minACD; }
            set { model.minACD = value; }
        }

        public ushort MaxACD
        {
            get { return model.maxACD; }
            set { model.maxACD = value; }
        }

        public string MinPhis
        {
            get { return model.minPhis.ToString(culture); }
            set
            {
                try
                {
                    model.minPhis = float.Parse(value.Replace(',', '.'), culture);
                }
                catch
                {
                    model.minPhis = 0;
                }
               
                }
        }

        public string MaxPhis
        {
            get { return model.maxPhis.ToString(culture); }
            set
            {
                try
                {
                    model.maxPhis = float.Parse(value.Replace(',', '.'), culture);
                }
                catch
                {
                    model.maxPhis = 0;
                }

            }
        }

        public string ForcedValue
        {
            get { return model.fVal.ToString(culture); }
            set {
                try
                {
                    model.ForceValue = float.Parse(value.Replace(',', '.'), culture);
                }
                catch 
                {
                    model.ForceValue = 0;
                }
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
