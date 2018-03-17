using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
namespace Simulator_MPSA
{
    public enum MPNAState { Work, Starting, Stoping, Stop};

    public partial class MPNAStruct : INotifyPropertyChanged
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
            if (ECO11 != null) ECO11.ValDI = true;

            if (_state == MPNAState.Stop)
            {
                //вв включен
                if (MBC11 != null) MBC11.ValDI = false;
                if (MBC12 != null) MBC12.ValDI = false;

                //вв отключен
                if (MBC21 != null) MBC21.ValDI = true;
                if (MBC22 != null) MBC22.ValDI = true;

                if (current != null) current.fValAI = 0f;
                if (RPM != null) RPM.fValAI = 0f;
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
            if ((ABB != null) && (ABB.ValDO))
            {
                if (_state == MPNAState.Stop || _state == MPNAState.Stoping)
                {
                    State = MPNAState.Starting;
                    if (MBC11 != null) MBC11.ValDI = true;
                    if (MBC12 != null) MBC12.ValDI = true;

                    if (MBC21 != null) MBC21.ValDI = false;
                    if (MBC22 != null) MBC22.ValDI = false;
                }
            }
            //команда на отключение
            if (((ABO != null) && (ABO.ValDO))||((ABO2 !=null)&&(ABO2.ValDO)))
            {
                if (_state == MPNAState.Work || _state == MPNAState.Starting)
                {
                    State = MPNAState.Stoping;
                }

            }

            //запуск
            if (_state == MPNAState.Starting)
            {
                if (current != null)
                {
                    current.fValAI += Current_spd * dt;
                    if (current.fValAI > Current_nominal)
                    {
                        State = MPNAState.Work;
                    }
                }
                else State = MPNAState.Work;

                if (RPM != null)
                {
                    RPM.fValAI += RPM_spd * dt;
                    if (RPM.fValAI > RPM_nominal)
                    {
                        State = MPNAState.Work;
                    }
                }

            }

            //останов
            if (_state == MPNAState.Stoping)
            {
                if (current != null)
                {
                    current.fValAI -= Current_spd * dt;
                    if (current.fValAI < 0)
                    {
                        State = MPNAState.Stop;
                    }
                }
                else
                {
                    State = MPNAState.Stop;
                }
                if (RPM != null)
                {
                    RPM.fValAI -= RPM_spd * dt;
                    if (RPM.fValAI <0)
                    {
                        State = MPNAState.Stop;
                    }
                }

            }

            

            //
        }

    }
}
