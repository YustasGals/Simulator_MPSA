using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simulator_MPSA.CL;
namespace Simulator_MPSA.ViewModel
{
    class DOViewModel : IViewModel<DOStruct>
    {
        private DOStruct _do;

        public DOViewModel()
        {
            _do = new DOStruct();
        }

        public bool Forced
        {
            set { _do.Forced = value; }
            get { return _do.Forced; }
        }

        public bool ForcedValue
        {
            set { _do.ForcedValue = value; }
            get { return _do.ForcedValue; }
        }
        public bool ValDO
        {
            set { _do.ValDO = value; }
            get { return _do.ValDO; }
        }

        public int indxArrDO
        {
            set { _do.indxArrDO = value; }
            get { return _do.indxArrDO; }
        }
        public int indxBitDO
        {
            set { _do.indxBitDO = value; }
            get { return _do.indxBitDO; }
        }

        public int PLCAddr
        {
            set { _do.PLCAddr = value; }
            get { return _do.PLCAddr; }
        }

        public string OPCtag
        {
            set { _do.OPCtag = value; }
            get { return _do.OPCtag; }
        }

        public string TegDO
        {
            set { _do.TegDO = value; }
            get { return _do.TegDO; }
        }

        public string NameDO
        {
            set { _do.NameDO = value; }
            get { return _do.NameDO; }
        }

        public int Nsign
        {
            set { _do.Nsign = value; }
            get { return _do.Nsign; }
        }
        public bool InvertDO
        {
            set { _do.InvertDO = value; }
            get { return _do.InvertDO; }
        }
        public DOStruct GetModel()
        {
            return _do;
            //throw new NotImplementedException();
        }

        public string GetName()
        {
            return _do.NameDO;
            //throw new NotImplementedException();
        }

        public string GetTag()
        {
            return _do.TegDO;
            //throw new NotImplementedException();
        }

        public void SetModel(DOStruct model)
        {
            _do = model;
            //throw new NotImplementedException();
        }
    }
}
