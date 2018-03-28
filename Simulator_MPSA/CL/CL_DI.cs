﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Xml.Serialization;
namespace Simulator_MPSA.CL
{
    // -------------------------------------------------------------------------------------------------
    [Serializable]
    public class DIStruct : INotifyPropertyChanged 
    {
        public static DIStruct[] items = new DIStruct[0];
        public bool En
        { set; get; }

        private bool _valDI;
        public bool ValDI
        { set { _valDI = value; OnPropertyChanged("ValDI"); }  get { return _valDI; } }

        private int _plcaddr;
        public int PLCAddr
        {
            get
            {
                if (_plcaddr == 0)
                {
                    switch (_buffer)
                    {
                        case BufType.USO: _plcaddr = Sett.Instance.BegAddrW + indxW + 1; break;
                        case BufType.A3: _plcaddr = Sett.Instance.iBegAddrA3 + indxW + 1; break;
                        case BufType.A4: _plcaddr = Sett.Instance.iBegAddrA4 + indxW + 1; break;

                    }
                }
                    
                
                return _plcaddr;
            }

            set {
                _plcaddr = value;
                if ((_plcaddr > Sett.Instance.iBegAddrA3) && (_plcaddr < (Sett.Instance.iBegAddrA3 + Sett.Instance.A3BufSize)))
                    Buffer = BufType.A3;

                if ((_plcaddr > Sett.Instance.iBegAddrA4) && (_plcaddr < (Sett.Instance.iBegAddrA4 + Sett.Instance.A4BufSize)))
                    Buffer = BufType.A4;

                if ((_plcaddr > Sett.Instance.BegAddrW) && (_plcaddr < (Sett.Instance.BegAddrW + Sett.Instance.USOBufferSize)))
                    Buffer = BufType.USO;

                OnPropertyChanged("PLCAddr");
            }
        }

        private BufType _buffer = BufType.USO;
        [XmlIgnore]
        public BufType Buffer
        {
            set { _buffer = value; OnPropertyChanged("Buffer"); }
            get
            {
                return _buffer;
            }
        }

        public int indxArrDI // index in AI
        { set; get; }
        public int indxBitDI
        { set; get; }
        private int indxW
        { set; get; }
        public string TegDI
        { set; get; }
        public string NameDI
        { set; get; }
        [XmlIgnore]
        public int Nsign
        { set; get; }
        public bool InvertDI
        { set; get; }
        [XmlIgnore]
        public int DelayDI
        { set; get; }
        public DIStruct()
        { }
        public DIStruct(bool En0 = false, bool ValDI0 = false, int indxArrDI0 = 0, int indxBitDI0 = 0 , int indxW0 = 0, string TegDI0 = "Teg",
                 string NameDI0 = "Name", int Nsign0 = 0 , bool InvertDI0 = false, int DelayDI0 = 0)
        {
            En = En0;
            ValDI = ValDI0;
            indxArrDI = indxArrDI0;
            indxBitDI = indxBitDI0;
            indxW = indxW0;
            TegDI = TegDI0;
            NameDI = NameDI0;
            Nsign = Nsign0;
            InvertDI = InvertDI0;
            DelayDI = DelayDI0;
        }
        public static DIStruct FindByIndex(int index)
        {
            for (int i = 0; i < items.Length; i++)
                if (items[i].indxArrDI == index)
                {
                    return items[i];
                }
            return null;
        }
        public static string GetNameByIndex(int index)
        {
            if (index > 0 && index < items.Length)
                return items[index].NameDI;
            else return "сигнал не определен";
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

    }



}
