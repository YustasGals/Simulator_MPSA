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
    public class Sett
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

        public Sett()
        {
            items.Add("HostName", new SettingsItem("IP", "192.168.201.1",typeof(String)));
            items.Add("MBPort", new SettingsItem("Порт", 502,typeof(int)));
            items.Add("TPause", new SettingsItem("TPause", 50, typeof(int)));
            items.Add("nWrTask", new SettingsItem("nWrTask", 4, typeof(int)));
            items.Add("iBegAddrR", new SettingsItem("iBegAddrR", 23170 - 1, typeof(int)));
            items.Add("iBegAddrW", new SettingsItem("iBegAddrW", 15100 - 1, typeof(int)));
            items.Add("iBegAddrA4", new SettingsItem("iBegAddrA4", 29475 - 1, typeof(int)));

            items.Add("iBegAddrA3", new SettingsItem("iBegAddrA3", 28850-1,typeof(int)));
            items.Add("A3BufSize", new SettingsItem("A3BufSize", 600, typeof(int)));
            items.Add("A4BufSize", new SettingsItem("A4BufSize", 600, typeof(int)));

            items.Add("iNRackBeg", new SettingsItem("iNRackBeg", 3, typeof(int)));
            items.Add("iNRackEnd", new SettingsItem("iNRackEnd", 29, typeof(int)));
            items.Add("nAI", new SettingsItem("nAI", 1024, typeof(int)));
            items.Add("nDI", new SettingsItem("nDI", 128, typeof(int)));
            items.Add("nDO", new SettingsItem("nDO", 64, typeof(int)));
            items.Add("nZD", new SettingsItem("nZD", 64, typeof(int)));
            items.Add("nKL", new SettingsItem("nKL", 64, typeof(int)));
            items.Add("nVS", new SettingsItem("nVS", 256, typeof(int)));
            items.Add("nMPNA", new SettingsItem("nMPNA", 16, typeof(int)));
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
        public int BegAddrR
        {
            get { return (int)items["iBegAddrR"].value; }
            set { items["iBegAddrR"].value = value; }
        }
        /// <summary>
        /// количество потоков на запись
        /// </summary>
        public int NWrTask
        {
            get { return (int)items["nWrTask"].value; }
            set { items["nWrTask"].value = value; }
        }
        /// <summary>
        /// задержка мсек
        /// </summary>
        public int TPause
        {
            get { return (int)items["TPause"].value;  }
            set { items["TPause"].value = value; }
        }
        public int NRackBeg
        {
            get { return (int)items["iNRackBeg"].value;  }
            set { items["iNRackBeg"].value = value; }
        }
        public int NRackEnd
        {
            get { return (int)items["iNRackEnd"].value;  }
            set { items["iNRackEnd"].value = value; }
        }
        public int NAI
        {
            get { return (int)items["nAI"].value; }
            set { items["nAI"].value = value; }
        }
        public int NZD
        {
            get { return (int)items["nZD"].value; }
            set { items["nZD"].value = value; }
        }
        public int NDO
        {
            get { return (int)items["nDO"].value; }
            set { items["nDO"].value = value; }
        }
        public int NKL
        {
            get { return (int)items["nKL"].value; }
            set { items["nKL"].value = value; }
        }
        public int NVS
        {
            get { return (int)items["nVS"].value; }
            set { items["nVS"].value = value; }
        }
        public int NMPNA
        {
            get { return (int)items["nMPNA"].value; }
            set { items["nMPNA"].value = value; }
        }
        public int NDI
        {
            get { return (int)items["nDI"].value; }
            set { items["nDI"].value = value; }
        }
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
            set { items["A3BufSize"].value = value; }
        }
        public int A4BufSize
        {
            get { return (int)items["A4BufSize"].value; }
            set { items["A4BufSize"].value = value; }
        }
        //размер буффера УСО
        public int USOBufferSize { get { return (NRackEnd - NRackBeg + 1) * 126; } }
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
