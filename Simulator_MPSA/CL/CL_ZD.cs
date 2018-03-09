using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simulator_MPSA.CL;
using System.Xml;
using System.Xml.Serialization;
namespace Simulator_MPSA
{
    class CL_ZD
    {
    }
    // -------------------------------------------------------------------------------------------------
    [Serializable]
    public class ZDStruct
    {
        private bool _En=false ; // наличие в обработке задвижки
        private int _DOBindxArrDO=0;
        private int _DKBindxArrDO=0;
        private int _DCBindxArrDO=0;
        private int _DCBZindxArrDO=0;
        private bool _changedDO=false; // наличие изменений во входных сигналах блока
        private int _OKCindxArrDI=0;
        private int _CKCindxArrDI=0;
        private int _ODCindxArrDI=0;
        private int _CDCindxArrDI=0;
        private int _DCindxArrDI=0;
        private int _VoltindxArrDI=0;
        private int _MCindxArrDI=0;
        private int _OPCindxArrDI=0;
        private float _ZDProc=0.0f; // процент открытия задвижки
        private bool _changedDI=false; // наличие изменений в выходных сигналах блока
        private int _TmoveZD=600; // время полного хода звдвижки, сек
        private int _TscZD=3;  // время схода с концевиков, сек
        private string _description="";  //название задвижки
        private string _group="";    //название подсистемы в которую входи задвижка
        public ZDStruct()
        {
        }

       /* public ZDStruct( bool En0 = false ,
                         int DOBindxArrDO0 = 0,
                         int DKBindxArrDO0 = 0,
                         int DCBindxArrDO0 = 0,
                         int DCBZindxArrDO0 = 0,
                         bool changedDO0 = false,
                         int OKCindxArrDI0 = 0,
                         int CKCindxArrDI0 = 0,
                         int ODCindxArrDI0 = 0,
                         int CDCindxArrDI0 = 0,
                         int DCindxArrDI0 = 0,
                         int VoltindxArrDI0 = 0,
                         int MCindxArrDI0 = 0,
                         int OPCindxArrDI0 = 0,
                         float ZDProc0 = 0.0f ,
                         bool changedDI0 = false,
                         int TmoveZD0 = 600,
                         int TscZD0 = 3)
        {
            En= En0;
            DOBindxArrDO= DOBindxArrDO0;
            DKBindxArrDO= DKBindxArrDO0;
            DCBindxArrDO= DCBindxArrDO0;
            DCBZindxArrDO= DCBZindxArrDO0;
            changedDO= changedDO0;
            OKCindxArrDI= OKCindxArrDI0;
            CKCindxArrDI= CKCindxArrDI0;
            ODCindxArrDI= ODCindxArrDI0;
            CDCindxArrDI= CDCindxArrDI0;
            DCindxArrDI= DCindxArrDI0;
            VoltindxArrDI= VoltindxArrDI0;
            MCindxArrDI= MCindxArrDI0;
            OPCindxArrDI= OPCindxArrDI0;
            ZDProc= ZDProc0;
            changedDI= changedDI0;
            TmoveZD = TmoveZD0;
            TscZD = TscZD0;
        }*/

        public float UpdateZD()
        {
            // тут будет логика задвижки !!!
            return _ZDProc;
        }

        public bool En
        {
            get { return _En; }
            set { _En = value; }
        }

        public int DOBindxArrDO
        {
            get { return _DOBindxArrDO; }
            set { _DOBindxArrDO = value; }
        }
        public int DKBindxArrDO
        {
            get { return _DKBindxArrDO; }
            set { _DKBindxArrDO = value; }
        }
        public int DCBindxArrDO
        {
            get { return _DCBindxArrDO; }
            set { _DCBindxArrDO = value; }
        }
        public int DCBZindxArrDO
        {
            get { return _DCBZindxArrDO; }
            set { _DCBZindxArrDO = value; }
        }
        public bool ChangedDO
        {
            get { return _changedDO; }
            set { _changedDO = value; }
        }
        public int OKCindxArrDI
        {
            get { return _OKCindxArrDI; }
            set { _OKCindxArrDI = value; }
        }
        public int CKCindxArrDI
        {
            get { return _CKCindxArrDI; }
            set { _CKCindxArrDI = value; }
        }
        public int ODCindxArrDI
        {
            get { return _ODCindxArrDI; }
            set { _ODCindxArrDI = value; }
        }
        public int CDCindxArrDI
        {
            get { return _CDCindxArrDI; }
            set { _CDCindxArrDI = value; }
        }
        public int DCindxArrDI
        {
            get { return _DCindxArrDI; }
            set { _DCindxArrDI = value; }
        }
        public int VoltindxArrDI
        {
            get { return _VoltindxArrDI; }
            set { _VoltindxArrDI = value; }
        }
        public int MCindxArrDI
        {
            get { return _MCindxArrDI; }
            set { _MCindxArrDI = value; }
        }
        public int OPCindxArrDI
        {
            get { return _OPCindxArrDI; }
            set { _OPCindxArrDI = value; }
        }
        public float ZDProc
        {
            get { return _ZDProc; }
            set { _ZDProc = value; }
        }
        public bool ChangedDI
        {
            get { return _changedDI; }
            set { _changedDI = value; }
        }
        public int TmoveZD
        {
            get { return _TmoveZD; }
            set { _TmoveZD = value; }
        }
        public int TscZD
        {
            get { return _TscZD; }
            set { _TscZD = value; }
        }
        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }
        public string Group
        {
            get { return _group; }
            set { _group = value; }
        }
    }


   
}
