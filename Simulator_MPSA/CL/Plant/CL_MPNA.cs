using Simulator_MPSA.CL;
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Xml.Serialization;
using Simulator_MPSA.CL.Signal;

namespace Simulator_MPSA
{
    // -------------------------------------------------------------------------------------------------
    [Serializable]
    public partial class MPNAStruct : INotifyPropertyChanged
    {
        //  public static MPNAStruct[] MPNAs;

        public bool _En = false; // наличие в обработке задвижки
        [XmlIgnore]
        public bool changedDI = false; // наличие изменений в выходных сигналах блока
        [XmlIgnore]
        public bool changedDO = false; // наличие изменений во входных сигналах блока

        private int _index;
        public int Index
        {
            get { return _index; }
            set { _index = value; OnPropertyChanged("Index"); }
        }

        /// <summary>
        /// наличие напряжения на секции шин
        /// </summary>
        private DIStruct BS;

        private int BusSecIndex
        { set; get; }
        /// <summary>
        /// команда на включение
        /// </summary>
        private DOStruct ABB;
        private int _ABBindxArrDO = -1; // ABB101  включение
        /// <summary>
        /// индекс сигнала "включить"
        /// </summary>
        public int ABBindxArrDO
        {
            get { return _ABBindxArrDO; }
            set { _ABBindxArrDO = value; OnPropertyChanged("ABBindxArrDO"); ABB = DOStruct.FindByIndex(_ABBindxArrDO); }
        }
        public string ABBName
        { get { if (ABB != null)
                    return ABB.NameDO;
                else return "сигнал не назначен";
            }
        }
        /// <summary>
        /// команда на отключение
        /// </summary>
        private DOStruct ABO;
        private int _ABOindxArrDO = -1; // ABO101-2  отключение
        /// <summary>
        /// индекс сигнала "выкл"
        /// </summary>
        public int ABOindxArrDO
        {
            get { return _ABOindxArrDO; }
            set { _ABOindxArrDO = value; OnPropertyChanged("ABOindxArrDO"); ABO = DOStruct.FindByIndex(_ABOindxArrDO); }
        }
        public string ABOName
        {
            get
            {
                if (ABO != null)
                    return ABO.NameDO;
                else return "сигнал не назначен";
            }
        }

        /// <summary>
        /// команда на отключение 2
        /// </summary>
        private DOStruct ABO2;
        private int _ABO2indxArrDO = -1;
        public int ABO2indxArrDO
        {
            get { return _ABO2indxArrDO; }
            set { _ABO2indxArrDO = value; OnPropertyChanged("ABO2indxArrDO"); ABO2 = DOStruct.FindByIndex(_ABO2indxArrDO); }
        }
        public string ABO2Name
        {
            get { if (ABO2 != null) return ABO2.NameDO; else return "сигнал не назначен"; }
        }

        /// <summary>
        /// ВВ включен 2
        /// </summary>
        private DIStruct MBC12;
        private int _MBC12indxArrDI = -1; // MBC101-2 ВВ включен 2
        /// <summary>
        /// ВВ включен 2
        /// </summary>
        public int MBC12indxArrDI
        {
            get { return _MBC12indxArrDI;  }
            set { _MBC12indxArrDI = value; OnPropertyChanged("MBC12indxArrDI"); MBC12 = DIStruct.FindByIndex(_MBC12indxArrDI); }
        }
        public string MBC12Name
        {
            get
            {
                if (MBC12 != null)
                    return MBC12.NameDI;
                else return "сигнал не назначен";
            }
        }
        /// <summary>
        /// вв отключен 2
        /// </summary>
        private DIStruct MBC22;
        private int _MBC22indxArrDI = -1; // MBC102-2 ВВ отключен 2
        /// <summary>
        /// ВВ включен 2
        /// </summary>
        public int MBC22indxArrDI
        {
            get { return _MBC22indxArrDI; }
            set { _MBC22indxArrDI = value; OnPropertyChanged("MBC22indxArrDI"); MBC22 = DIStruct.FindByIndex(_MBC22indxArrDI); }
        }
        public string MBC22Name
        {
            get
            {
                if (MBC22 != null)
                    return MBC22.NameDI;
                else return "сигнал не назначен";
            }
        }

        private DIStruct ECB;
        private int _ECBindxArrDI = -1; // ECB101 исправность цепей включения
        /// <summary>
        /// исправность цепей включения
        /// </summary>
        public int ECBindxArrDI
        {
            get { return _ECBindxArrDI; }
            set { _ECBindxArrDI = value; OnPropertyChanged("ECBindxArrDI"); ECB = DIStruct.FindByIndex(_ECBindxArrDI); }
        }
        public string ECBName
        {
            get
            {
                if (ECB != null)
                    return ECB.NameDI;
                else return "сигнал не назначен";
            }
        }

        private DIStruct ECO;
        private int _ECO12indxArrDI = -1; // ECO101-2 исправность цепей отключения 2
        /// <summary>
        /// исправность цепей отключения 2
        /// </summary>
        public int ECO12indxArrDI
        {
            get { return _ECO12indxArrDI; }
            set { _ECO12indxArrDI = value; OnPropertyChanged("ECO12indxArrDI"); ECO = DIStruct.FindByIndex(_ECO12indxArrDI); }
        }
        public string ECO12Name
        {
            get
            {
                if (ECO != null)
                    return ECO.NameDI;
                else return "сигнал не назначен";
            }
        }

        private AIStruct CT_AI;
        private int _CTindxArrDI = -1; // CT1011 сила тока ЭД (AI)
        /// <summary>
        /// сила тока ЭД (AI)
        /// </summary>
        public int CTindxArrDI
        {
            get { return _CTindxArrDI; }
            set { _CTindxArrDI = value; OnPropertyChanged("CTindxArrDI"); CT_AI = AIStruct.FindByIndex(_CTindxArrDI); }
        }
        public string CTName
        {
            get
            {
                if (CT_AI != null)
                    return CT_AI.NameAI;
                else return "сигнал не назначен";
            }
        }

        /// <summary>
        /// ВВ включен 1
        /// </summary>
        private DIStruct MBC11;
        private int _MBC11indxArrDI = -1; // MBC101-1 ВВ включен 1
        /// <summary>
        /// ВВ включен 1
        /// </summary>
        public int MBC11indxArrDI
        {
            get { return _MBC11indxArrDI; }
            set { _MBC11indxArrDI = value; OnPropertyChanged("MBC11indxArrDI"); MBC11 = DIStruct.FindByIndex(_MBC11indxArrDI); }
        }
        public string MBC11Name
        {
            get
            {
                if (MBC11 != null)
                    return MBC11.NameDI;
                else return "сигнал не назначен";
            }
        }

        /// <summary>
        /// вв отключен 1
        /// </summary>
        private DIStruct MBC21;
        private int _MBC21indxArrDI = -1; // MBC102-1 ВВ отключен 1
        /// <summary>
        /// ВВ отключен 1
        /// </summary>
        public int MBC21indxArrDI
        {
            get { return _MBC21indxArrDI; }
            set { _MBC21indxArrDI = value; OnPropertyChanged("MBC21indxArrDI"); MBC21 = DIStruct.FindByIndex(_MBC21indxArrDI); }
        }
        public string MBC21Name
        {
            get
            {
                if (MBC21 != null)
                    return MBC21.NameDI;
                else return "сигнал не назначен";
            }
        }

        private DIStruct ECx;
        private int _ECxindxArrDI = -1; // ECx02
        /// <summary>
        /// ECx02
        /// </summary>
        public int ECxindxArrDI
        {
            get { return _ECxindxArrDI; }
            set { _ECxindxArrDI = value; OnPropertyChanged("ECxindxArrDI"); ECx = DIStruct.FindByIndex(_ECxindxArrDI); }
        }
        public string ECxName
        {
            get
            {
                if (ECx != null)
                    return ECx.NameDI;
                else return "сигнал не назначен";
            }
        }

        private DIStruct ECO11;
        private int _ECO11indxArrDI = -1; // ECO101-1 исправность цепей отключения 1
        /// <summary>
        /// исправность цепей отключения 1
        /// </summary>
        public int ECO11indxArrDI
        {
            get { return _ECO11indxArrDI; }
            set { _ECO11indxArrDI = value; OnPropertyChanged("ECO11indxArrDI"); ECO11 = DIStruct.FindByIndex(_ECO11indxArrDI); }
        }
        public string ECO11Name
        {
            get
            {
                if (ECO11 != null)
                    return ECO11.NameDI;
                else return "сигнал не назначен";
            }
        }


        private DIStruct EC;
        private int _ECindxArrDI = -1; // EC108
        /// <summary>
        /// EC108
        /// </summary>
        public int ECindxArrDI
        {
            get { return _ECindxArrDI; }
            set { _ECindxArrDI = value; OnPropertyChanged("ECindxArrDI"); EC = DIStruct.FindByIndex(_ECindxArrDI); }
        }
        public string ECName
        {
            get
            {
                if (EC != null)
                    return EC.NameDI;
                else return "сигнал не назначен";
            }
        }

        /// <summary>
        /// список аналогов которые управляются агрегатом
        /// </summary>
        public AnalogIOItem[] controledAIs;

        /// <summary>
        /// список ссылок на дискретные сигналы относящиеся к агрегату, не участвуют в алгоритмах
        /// </summary>
        public List<DIItem> CustomDIs
        {
            get { return _customDIs; }
            set { _customDIs = value; }
        }
        private List<DIItem> _customDIs = new List<DIItem>();
        
        public bool En
        {
            get { return _En; }
            set { _En = value; OnPropertyChanged("En"); }
        }
        private string _description;
        public string Description
        {
            get { return _description; }
            set { _description = value; OnPropertyChanged("Description"); }
        }

        private string _group;
        public string Group
        {
            get { return _group; }
            set { _group = value; OnPropertyChanged("Group"); }
        }

        public MPNAStruct()
        {
        }
       
        /// <summary>
        /// Обновление ссылок
        /// </summary>
        public void UpdateRefs()
        {
            ABB = DOStruct.FindByIndex(_ABBindxArrDO);
            ABO = DOStruct.FindByIndex(_ABOindxArrDO);
            ABO2 = DOStruct.FindByIndex(_ABO2indxArrDO);

            MBC12 = DIStruct.FindByIndex(_MBC12indxArrDI);
            MBC22 = DIStruct.FindByIndex(_MBC22indxArrDI);
            ECB = DIStruct.FindByIndex(_ECBindxArrDI);
            ECO = DIStruct.FindByIndex(_ECO12indxArrDI);
            CT_AI = AIStruct.FindByIndex(_CTindxArrDI);
            MBC11 = DIStruct.FindByIndex(_MBC11indxArrDI);
            MBC21 = DIStruct.FindByIndex(_MBC21indxArrDI);
            ECx = DIStruct.FindByIndex(_ECxindxArrDI);
            ECO11 = DIStruct.FindByIndex(_ECO11indxArrDI);
            EC = DIStruct.FindByIndex(_ECindxArrDI);
            /*current = AIStruct.FindByIndex(_current_indexArrAi);
            RPM = AIStruct.FindByIndex(_RPMindxArrAI);*/
        }
        /// <summary>
        /// Вернуть список всех дискретных сигналов агрегата
        /// </summary>
        /// <returns></returns>
        public List<DIStruct> GetDIs()
        {
            List<DIStruct> result = new List<DIStruct>();

            if (MBC12!=null)
            result.Add(MBC12);

            if (MBC22 != null)
                result.Add(MBC22);

            if (ECB != null)
                result.Add(ECB);

            if (ECO != null)
            result.Add(ECO);

            if (MBC11 != null)
                result.Add(MBC11);

            if (MBC21 != null)
                result.Add(MBC21);

            if (ECx != null)
                result.Add(ECx);

            if (ECO11 != null)
                result.Add(ECO11);

            if (EC != null)
                result.Add(EC);

            return result;
        }
        /// <summary>
        /// вернуть список всех дискретных команда агрегата
        /// </summary>
        /// <returns></returns>
        public List<DOStruct> GetDOs()
        {
            List<DOStruct> result = new List<DOStruct>();
            if (ABB != null)
                result.Add(ABB);

            if (ABO != null)
                result.Add(ABO);

            if (ABO2 != null)
                result.Add(ABO2);

            return result;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
