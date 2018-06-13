using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Xml.Serialization;

namespace Simulator_MPSA.CL
{
    class AOStruct:BaseViewModel
    {
        public static ObservableCollection<DOStruct> items = new ObservableCollection<DOStruct>();
        private bool _En;
        public bool En
        {
            get { return _En; }
            set { _En = value; OnPropertyChanged("En"); }
        }

        /// <summary>
        /// включить принудительную запись, игнорировать значения полученные из ПЛК
        /// </summary>
        public bool Forced
        { set; get; }

        float valAO;
        /// <summary>
        /// Принудительная запись значения
        /// </summary>
        public float ForcedValue
        {
            set
            {
                if (Forced)
                    valAO = value;
            }
            get { return valAO; }
        }
    }
}
