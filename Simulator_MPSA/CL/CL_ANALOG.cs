﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
namespace Simulator_MPSA.CL
{
    /// <summary>
    /// тип записываемого значения в плк - единицы ацп, либо float
    /// </summary>
    public enum EPLCDestType { ADC, Float};
    // -------------------------------------------------------------------------------------------------
    [Serializable]
    public class AIStruct : BaseViewModel
    {
        public static ObservableCollection<AIStruct> items = new ObservableCollection<AIStruct>();
        private bool _En;
        public bool En
        {
            get { return _En; }
            set { _En = value; OnPropertyChanged("En"); }
        }

        private int _indxAI; // index in AI
        /// <summary>
        /// индекс сигнала должен быть уникальным
        /// </summary>
        public int indxAI
        {
            get { return _indxAI; }
            set { _indxAI = value; OnPropertyChanged("indxAI"); }
        }
        private BufType _buffer;
        [XmlIgnore]
        public BufType Buffer
        {
            get { return _buffer; }
            set { _buffer = value; OnPropertyChanged("Buffer"); }
        }
        private string _OPCtag = "";
        public string OPCtag
        {
            set
            {
                _OPCtag = value; OnPropertyChanged("OPCtag");
            }
            get { return _OPCtag; }
        }

        private EPLCDestType _plcDestType = EPLCDestType.ADC;
        /// <summary>
        /// тип данных в ПЛК
        /// </summary>
        public EPLCDestType PLCDestType
        {
            get { return _plcDestType; }
            set { _plcDestType = value; OnPropertyChanged("PLCDestType"); }
        }

        /// <summary>
        /// включить принудительную запись, игнорировать значение от алгоритмов и скриптов
        /// </summary>
        public bool Forced
        { set; get; }

        /// <summary>
        /// Property для ввода значения из DataGrid
        /// </summary>
        public float ForcedValue
        {
            set
            {
                _fValAI = value;
                RefreshADC();
            }
            get
            {
                return _fValAI;
            }
        }

        private int _plcAddr=0;
        /// <summary>
        /// MODBUS адрес
        /// </summary>
        public int PLCAddr
        {
            get {
                if (_plcAddr == 0)
                    _plcAddr = indxW + Sett.Instance.BegAddrW + 1;
                return _plcAddr;
            }
            set {
                _plcAddr = value;

                if ((_plcAddr > Sett.Instance.iBegAddrA3) && (_plcAddr < (Sett.Instance.iBegAddrA3 + Sett.Instance.A3BufSize)))
                    Buffer = BufType.A3;

                if ((_plcAddr > Sett.Instance.iBegAddrA4) && (_plcAddr < (Sett.Instance.iBegAddrA4 + Sett.Instance.A4BufSize)))
                    Buffer = BufType.A4;

                if ((_plcAddr > Sett.Instance.BegAddrW) && (_plcAddr < (Sett.Instance.BegAddrW + Sett.Instance.USOBufferSize)))
                    Buffer = BufType.USO;
            }
        }

        

        private int _indxW;
        /// <summary>
        /// индекс в массиве буфера записи (WB.W)
        /// </summary>
        private int indxW
        {
            get { return _indxW; }
            set { _indxW = value; OnPropertyChanged("indxW"); }
        }
        private string _TegAI="";
        public string TegAI
        {
            get { return _TegAI; }
            set { _TegAI = value; OnPropertyChanged("TegAI"); }
        }

        private string _nameai="";
        public string NameAI
        {
            get {
                return _nameai;
                  }
            set { _nameai = value; OnPropertyChanged("NameAI"); }
        }

        private ushort _valADC;
        public ushort ValACD
        {
            get {
                return _valADC;  
            }
            set { _valADC = value; OnPropertyChanged("ValACD"); }
        }
        private ushort _minACD;
        public ushort minACD
        {
            get { return _minACD; }
            set { _minACD = value; OnPropertyChanged("minACD"); RefreshADC(); }
        }
        private ushort _maxACD;
        public ushort maxACD
        {
            get { return _maxACD; }
            set { _maxACD = value; OnPropertyChanged("maxACD"); RefreshADC(); }
        }
        private float _minPhis;
        public float minPhis
        {
            get { return _minPhis; }
            set { _minPhis = value; OnPropertyChanged("minPhis"); RefreshADC(); }
        }
        private float _maxPhis;
        public float maxPhis
        {
            get { return _maxPhis; }
            set { _maxPhis = value; OnPropertyChanged("maxPhis"); RefreshADC(); }
        }
        private float _fValAI;
        public float fValAI
        {
            get { return _fValAI; }
            set
            {
                if (!Forced)
                {
                    _fValAI = value;
                    RefreshADC();
                    OnPropertyChanged("ForcedValue");
                }
            }
        }

        private void RefreshADC()
        {
            float df = 0f;
            try
            {
                df = (fValAI - minPhis) / (maxPhis - minPhis);
            }
            catch
            {
            }
            float dadc = ((float)maxACD - (float)minACD) * df;
            int res = (int)dadc + minACD;
            ValACD = (ushort)res;
        }

        public int DelayAI;

        public AIStruct() {
            Forced = false;
        }
        public AIStruct(bool En0 = false, int indxAI0 = 0, int indxW0 = 0, string TegAI0 = "Teg",
                 string NameAI0 = "Name", ushort ValACD0 = 4000, ushort minACD0 = 4000, ushort maxACD0 = 20000,
                 float minPhis0 = 0.0F, float maxPhis0 = 100.0F, float fValAI0 = 0.0F, int DelayAI0 = 0)
        {
            Forced = false;
            En = En0;
            indxAI = indxAI0;
            indxW = indxW0;
            TegAI = TegAI0;
            NameAI = NameAI0;
            ValACD = ValACD0;
            minACD = minACD0;
            maxACD = maxACD0;
            minPhis = minPhis0;
            maxPhis = maxPhis0;
            fValAI = fValAI0;
            DelayAI = DelayAI0;
        }
        public static AIStruct FindByIndex(int index)
        {
            for (int i = 0; i < items.Count; i++)
                if (items[i].indxAI == index)
                {
                    return items[i];
                }
            return null;
        }
        public static string GetNameByIndex(int index)
        {
            if (index > 0 && index < items.Count)
                return items[index].NameAI;
            else return "сигнал не определен";
        }

        public string PrintAI()
        {
            return ("En=" + En + "; indxAI=" + indxAI + "; indxW=" + indxW + "; TegAI=" + TegAI +
"; NameAI=" + NameAI + "; ValACD=" + ValACD + "; minACD=" + minACD + "; maxACD=" + maxACD +
"; minPhis=" + minPhis + "; maxPhis=" + maxPhis + "; fValAI=" + fValAI + "; DelauAI=" + DelayAI + "\n");
        }

        /*public void updateAI(float fValAI0)
        {
            fValAI = fValAI0;
            if (En)
            {
                ValACD = (ushort)(minACD + ((maxACD - minACD) * ((fValAI - minPhis) / (maxPhis - minPhis))));
            }
            else { }
        }
        public void updateAI(ushort ValACD0)
        {
            ValACD = ValACD0;
            if (En)
            {
                fValAI = (minPhis + ((maxPhis - minPhis) * ((ValACD - minACD) / (maxACD - minACD))));
            }
            else { }
        }*/

    }
    // public AIStruct[] AIs = AIStruct[Sett.nAI] ;

}
