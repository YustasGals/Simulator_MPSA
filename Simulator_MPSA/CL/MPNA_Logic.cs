using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulator_MPSA
{
    public enum MPNAState { Work, Starting, Stoping, Stop};

    public partial class MPNAStruct
    {
        private MPNAState _state = MPNAState.Stop;
        public MPNAState State
        {
            get { return _state;  }
            set { _state = value; OnPropertyChanged("State"); }
        }


        /// <summary>
        /// Обновить состояние агрегата
        /// </summary>
        /// <param name="dt">время между циклами сек </param>
        /// <returns></returns>
        public void UpdateMPNA(float dt)
        {
            //логика отключена - выходим
            if (!_En) return;
            //все цепи исправны
           
            
            if (ECB != null) ECB.ValDI = true;
            if (ECO != null) ECO.ValDI = true;

            if (_state == MPNAState.Stop)
            {
                //вв включен
                if (MBC11 != null) MBC11.ValDI = false;
                if (MBC12 != null) MBC12.ValDI = false;

                //вв отключен
                if (MBC21 != null) MBC21.ValDI = true;
                if (MBC22 != null) MBC22.ValDI = true;
            }

            if (_state == MPNAState.Work)
            {
                //вв включен
                if (MBC11 != null) MBC11.ValDI = true;
                if (MBC12 != null) MBC12.ValDI = true;

                //вв отключен
                if (MBC21 != null) MBC21.ValDI = false;
                if (MBC22 != null) MBC22.ValDI = false;
            }

            //команда на включение
            if ((ABB != null) && (ABB.ValDO == true))
            {
                if (_state == MPNAState.Stop)
                {
                    _state = MPNAState.Starting;
                    if (MBC11 != null) MBC11.ValDI = true;
                    if (MBC12 != null) MBC12.ValDI = true;
                }
            }
            //команда на отключение
            if ((ABB != null) && (ABB.ValDO == true))
            {
                if (_state == MPNAState.Stop)
                {
                    _state = MPNAState.Starting;
                }
            }

        }

    }
}
