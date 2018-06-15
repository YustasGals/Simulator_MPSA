using System.Collections.Generic;
using System;
using Simulator_MPSA.CL;
using System.Xml;
using System.Xml.Serialization;
namespace Simulator_MPSA {

    public class SettingsItem
    {
        public string name;
        public object value;
        public Type tp;
        public SettingsItem(string name, object value, Type t)
        {
            this.name = name;
            this.value = value;
            tp = t;
        }
    }
    [Serializable()]
    public class Sett: BaseViewModel
    {
        /* string HostName = "192.168.201.1"; // IP adress CPU
         int MBPort = 502; // Modbus TCP port adress
         int TPause = 50; // задержка между циклами чтения
         int nWrTask = 4; // потоков на запись в CPU
         int iBegAddrR = 23170 - 1; // начальный адрес для чтения выходов CPU
         int iBegAddrW = 15100 - 1; // начальный адрес для записи входов CPU
         int iNRackBeg = 3; // номер начальной корзины
         int iNRackEnd = 29; // номер конечной корзины
         int nAI = 1024; // count of AI 1000
         int nDI = 128; // count of DI 
         int nDO = 64; // count of DO 
         int nZD = 64; // count of ZD  200
         int nKL = 64; // count of KL 100
         int nVS = 256; // count of VS 200
         int nMPNA = 16; // count of MPNA 
                         // public const string AI_file = "AIsettings.xml";
                         // public static string DI_file = "";
                         // public static string DO_file = "";
                         // public static string ZD_file = "";
                         // public static string MPNA_file = "";
                         // public static string VS_file = "";*/
        private static Sett instance = null;
        public static Sett Instance
        {
            get
            {
                if (instance == null)
                    instance = new Sett();
                return instance;
            }
            set { instance = value;}
        }
        [XmlIgnore]
        [NonSerialized()]
        public Dictionary<string, SettingsItem> items = new Dictionary<string, SettingsItem>();

        public bool ShowTab_MPNA = true;
        public bool ShowTab_VS = true;
        public bool ShowTab_KL = true;
        public Sett()
        {
            items.Add("HostName", new SettingsItem("IP", "192.168.201.1",typeof(String)));
            items.Add("MBPort", new SettingsItem("Порт", 502,typeof(int)));
            items.Add("TPause", new SettingsItem("TPause", 50, typeof(int)));
           // items.Add("nWrTask", new SettingsItem("nWrTask", 4, typeof(int)));
            items.Add("iBegAddrR", new SettingsItem("iBegAddrR", 23170 - 1, typeof(int)));
            items.Add("iBegAddrW", new SettingsItem("iBegAddrW", 15100 - 1, typeof(int)));

            //параметры буфера КС 1
            items.Add("iBegAddrA3", new SettingsItem("iBegAddrA3", 28850 - 1, typeof(int)));
            items.Add("A3BufSize", new SettingsItem("A3BufSize", 600, typeof(int)));
            //параметры буфера КС 2
            items.Add("iBegAddrA4", new SettingsItem("iBegAddrA4", 29475 - 1, typeof(int)));
            items.Add("A4BufSize", new SettingsItem("A4BufSize", 600, typeof(int)));

            //количество регистров на запись
            items.Add("wrBufSize", new SettingsItem("wrBufSize", 1, typeof(int)));

            //количество регистров на чтение
            items.Add("rdBufSize", new SettingsItem("wrBufSize", 1, typeof(int)));

            //размер бочки (макс 120)
            items.Add("CoilSize", new SettingsItem("CoiSize", 1, typeof(int)));

            //размер бочки на чтение (макс 125)
            items.Add("rdCoilSize", new SettingsItem("CoiSize", 1, typeof(int)));
            //   items.Add("iNRackBeg", new SettingsItem("iNRackBeg", 3, typeof(int)));
            //   items.Add("iNRackEnd", new SettingsItem("iNRackEnd", 29, typeof(int)));

            items.Add("IncAddr", new SettingsItem("IncAddr", 0, typeof(int)));  //прибавлять единицу к адресам modbus?
            items.Add("UseModbus", new SettingsItem("UseModbus", true, typeof(bool)));
            items.Add("UseOPC", new SettingsItem("UseOPC", false, typeof(bool)));

            //имитировать контроллеры связи
            items.Add("UseKS1", new SettingsItem("UseKS1", false, typeof(bool)));
            items.Add("UseKS2", new SettingsItem("UseKS2", false, typeof(bool)));

            items.Add("OFSServerPrefix", new SettingsItem("OFSServerPrefix", "opcda://localhost/", typeof(string)));
            items.Add("OFSServerName", new SettingsItem("OFS Сервер", "Schneider.Aut", typeof(string)));
            items.Add("StationName", new SettingsItem("StationName", "", typeof(string)));

            //количество потоков на запись для буфера УСО
            items.Add("ConnectionCount", new SettingsItem("Количество подключений",4, typeof(int)));

            //количество потоков на чтение для буфера УСО
            items.Add("ReadConnectionCount", new SettingsItem("Количество подключений", 2, typeof(int)));

            /*   items.Add("nAI", new SettingsItem("nAI", 1024, typeof(int)));
               items.Add("nDI", new SettingsItem("nDI", 128, typeof(int)));
               items.Add("nDO", new SettingsItem("nDO", 64, typeof(int)));
               items.Add("nZD", new SettingsItem("nZD", 64, typeof(int)));
               items.Add("nKL", new SettingsItem("nKL", 64, typeof(int)));
               items.Add("nVS", new SettingsItem("nVS", 256, typeof(int)));
               items.Add("nMPNA", new SettingsItem("nMPNA", 16, typeof(int)));*/
               
        }
        public string HostName
        {
            get { return (string)items["HostName"].value; }
            set { items["HostName"].value = value; }
        }
        public int MBPort
        {
            get { return (int)items["MBPort"].value; }
            set { items["MBPort"].value = (int)value; }
        }
        public int BegAddrW
        {
            get { return (int)items["iBegAddrW"].value; }
            set { items["iBegAddrW"].value = value; }
        }
        /// <summary>
        /// количество регистров на запись
        /// </summary>
        public int wrBufSize
        {
            get { return (int)items["wrBufSize"].value; }
            set
            {
                int coilCount = (int)Math.Ceiling((double)value / (double)CoilSize);
                items["wrBufSize"].value = coilCount*CoilSize;
                OnPropertyChanged("wrBufSize");
                WB.InitBuffers();
            }
        }

        /// <summary>
        /// количество регистров на чтение
        /// </summary>
        public int rdBufSize
        {
            get { return (int)items["rdBufSize"].value; }
            set {
                int coilCount = (int)Math.Ceiling((double)value / (double)rdCoilSize);
                items["rdBufSize"].value = coilCount*rdCoilSize;
                OnPropertyChanged("rdBufSize");
                RB.InitBuffer();
            }
        }

        /// <summary>
        /// начальный адрес чтения из плк
        /// </summary>
        public int BegAddrR
        {
            get { return (int)items["iBegAddrR"].value; }
            set {
                items["iBegAddrR"].value = value;
            }
        }

        /// <summary>
        /// размер бочки макс 120 регистров
        /// </summary>
        public int CoilSize
        {
            get { return (int)items["CoilSize"].value; }
            set {
                if (value<1)
                    items["CoilSize"].value = 1;
                     else
                         if (value>120)
                             items["CoilSize"].value = 120;
                                 else
                                     items["CoilSize"].value = value;
                //обновить значения
                wrBufSize = wrBufSize;
            }
        }

        /// <summary>
        /// размер бочки макс 120 регистров
        /// </summary>
        public int rdCoilSize
        {
            get { return (int)items["rdCoilSize"].value; }
            set
            {
                if (value < 1)
                    items["rdCoilSize"].value = 1;
                else
                         if (value > 125)
                    items["rdCoilSize"].value = 125;
                else
                    items["rdCoilSize"].value = value;
                //обновить значения
                rdBufSize = rdBufSize; 
            }
        }

        /// <summary>
        /// инкремент адреса модбас, для плк quantum надо прибавлять единицу чтобы попасть в нужный, для m580 не надо
        /// </summary>
        public int IncAddr
        {
            get { return (int)items["IncAddr"].value; }
            set { items["IncAddr"].value = value; }
        }
        /// <summary>
        /// задержка мсек
        /// </summary>
        public int TPause
        {
            get { return (int)items["TPause"].value;  }
            set { items["TPause"].value = value; }
        }
      /*  public int NRackBeg
        {
            get { return (int)items["iNRackBeg"].value;  }
            set { items["iNRackBeg"].value = value; }
        }
        public int NRackEnd
        {
            get { return (int)items["iNRackEnd"].value;  }
            set { items["iNRackEnd"].value = value; }
        }*/
    
        public int iBegAddrA3
        {
            get { return (int)items["iBegAddrA3"].value;  }
            set { items["iBegAddrA3"].value = value; }
        }
        public int iBegAddrA4
        {
            get { return (int)items["iBegAddrA4"].value; }
            set { items["iBegAddrA4"].value = value; }
        }
        public int A3BufSize
        {
            get { return (int)items["A3BufSize"].value;  }
            set {
                int coilCount = (int)Math.Ceiling((double)value / (double)CoilSize);
                items["A3BufSize"].value = coilCount*CoilSize;
                OnPropertyChanged("A3BufSize");
                WB.InitBuffers();

            }
        }
        public int A4BufSize
        {
            get { return (int)items["A4BufSize"].value; }
            set
            {
                int coilCount = (int)Math.Ceiling((double)value / (double)CoilSize);
                items["A4BufSize"].value = coilCount * CoilSize;
                OnPropertyChanged("A4BufSize");
                WB.InitBuffers();
            }
        }
        
        public bool UseOPC
        {
            get { return (bool)items["UseOPC"].value; }
            set { items["UseOPC"].value = (object)value; }
          //  get { return _useOPC; }
          //  set { _useOPC = value; }
        }
        
        public bool UseModbus
        {
            get { return (bool)items["UseModbus"].value; }
            set { items["UseModbus"].value = (object)value; }
        }

        public string OFSServerName
        {
            get { return (string)items["OFSServerName"].value; }
            set { items["OFSServerName"].value = (string)value; }
        }

        
        public string OFSServerPrefix
        {
            get { return (string)items["OFSServerPrefix"].value; }
            set { items["OFSServerPrefix"].value = (string)value; }
        }

        public string StationName
        {
            get { return (string)items["StationName"].value; }
            set { items["StationName"].value = (string)value; }
        }

        public int ConnectionCount
        {
            get { return (int)items["ConnectionCount"].value; }
            set {
                if (value>4) items["ConnectionCount"].value = (int)4;
                    else
                    if (value<1)
                        items["ConnectionCount"].value = (int)1;
                else items["ConnectionCount"].value = (int)value;
            }
        }

        public int ReadConnectionCount
        {
            get { return (int)items["ReadConnectionCount"].value; }
            set
            {
                if (value > 4) items["ReadConnectionCount"].value = (int)4;
                else
                    if (value < 1)
                    items["ReadConnectionCount"].value = (int)1;
                else items["ReadConnectionCount"].value = (int)value;
            }
        }

        public bool UseKS1
        {
            get { return (bool)items["UseKS1"].value; }
            set { items["UseKS1"].value = (bool)value; }
        }

        public bool UseKS2
        {
            get { return (bool)items["UseKS2"].value; }
            set { items["UseKS2"].value = (bool)value; }
        }
    }
    class SettingsViewModel : BaseViewModel
    {
        private SettingsItem item;
        public SettingsViewModel(SettingsItem item)
        {
            this.item = item;
        }
        public string Name
        {
            get { return item.name; }
            set { item.name = value; }
        }
        public object Value
        {
            get { return item.value; }
            //если тип элемента int то самостоятельно преобразуем его из string
            set {
                if (item.tp == typeof(int))
                    item.value = Convert.ToInt32((string)value);
                else item.value = (string)value;
            }
        }
    }

    class SettingsTableViewModel : BaseViewModel
    {
        private SettingsViewModel[] _items;
        public SettingsViewModel[] Items
        {
            get { return _items; }
            set { _items = value; }
        }

        public SettingsTableViewModel()
        {
        }
        public SettingsTableViewModel(Sett settings)
        {
            SettingsViewModel[] temp = new SettingsViewModel[settings.items.Count];
            string[] keys = new string[settings.items.Count];
            settings.items.Keys.CopyTo(keys,0);

            for (int i = 0; i < settings.items.Count; i++)
            {
                temp[i] = new SettingsViewModel(settings.items[keys[i]]);
            }
            Items = temp;
        }
    }
}
