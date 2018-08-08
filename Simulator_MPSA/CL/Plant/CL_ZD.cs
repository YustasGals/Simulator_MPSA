using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simulator_MPSA.CL;
using System.Xml;
using System.Xml.Serialization;
using System.ComponentModel;
using Simulator_MPSA.CL.Signal;

namespace Simulator_MPSA
{
    class CL_ZD
    {
    }
    public enum StateZD { Opening, Open, Middle, Closing, Close, Undef, NoVolt };
    // -------------------------------------------------------------------------------------------------
    [Serializable]
    public class ZDStruct : INotifyPropertyChanged
    {
        private bool _En = false; // наличие в обработке задвижки
        private int _DOBindxArrDO = -1;
        private int _DKBindxArrDO = -1;
        private int _DCBindxArrDO = -1;
        private int _DCBZindxArrDO = -1;
        private bool _changedDO = false; // наличие изменений во входных сигналах блока
        private int _OKCindxArrDI = -1;
        private int _CKCindxArrDI = -1;
        private int _ODCindxArrDI = -1;
        private int _CDCindxArrDI = -1;
        private int _DCindxArrDI = -1;
        private int _VoltindxArrDI = -1;
        private int _MCindxArrDI = -1;
        private int _OPCindxArrDI = -1;
        private float _ZDProc = 0.0f; // процент открытия задвижки
        private bool _changedDI = false; // наличие изменений в выходных сигналах блока
        private float _TmoveZD = 10; // время полного хода звдвижки, сек
        private int _TscZD = 1;  // время схода с концевиков, сек
        private string _description = "Задвижка";  //название задвижки
        private string _group = "";    //название подсистемы в которую входи задвижка


        private StateZD _stateZD = StateZD.Close;
        public StateZD StateZD
        {
            get { return _stateZD; }
            set { _stateZD = value; OnPropertyChanged("StateZDRus"); }
        }
        [XmlIgnore]
        public string StateZDRus
        {
            set { }
            get
            {
                switch (_stateZD)
                {
                    case StateZD.Close: return "Закрыта";
                    case StateZD.Open: return "Открыта";
                    case StateZD.Opening: return "Открывается";
                    case StateZD.Closing: return "Закрывается";
                    case StateZD.Middle: return "В промежуточном";
                    default:
                        return "не определено";
                }
            }
        }
        const int intrfOKC = 0;
        const int intrfCKC = 1;
        const int intrfODC = 2;
        const int intrfCDC = 3;
        const int intrfDC = 4;
        public ZDStruct()
        {
        //    DIs = new List<DIItem>();
        //    DIs.Add(new DIItem("RS485. Открыта"));//открыта
        //    DIs.Add(new DIItem("RS485. Закрыта"));//закрыта
        //    DIs.Add(new DIItem("RS485. Открывается"));//открывается
        //    DIs.Add(new DIItem("RS485. Закрывается"));//закрывается
        //    DIs.Add(new DIItem("RS485. В дистанции"));//закрыта
        //    DIs.Add(new DIItem("RS485. Наличие связи"));//наличие связи
            
        }
        private int _index;
        public int Index
        {
            get { return _index; }
            set { _index = value; OnPropertyChanged("Index"); }
        }

        /// <summary>
        /// задержка отключения МПО МПЗ после появления сигнала с концевика (открыо закрыто)
        /// </summary>
        const float MPDelay_timeout = 1f;
        float MPDelay = 0f;
        /// <summary>
        /// обновить состояние задвижки
        /// </summary>
        /// <param name="dt">задержка между циклами, сек </param>
        /// <returns></returns>
        public void UpdateZD(float dt)
        {
            if (!En)
                return;

            if (ZD_position_ai != null) ZD_position_ai.fValAI = _ZDProc;

            //нет напряжения на секции шин
            if (bs != null && bs.ValDI == false)
            {
                if (StateZD == StateZD.Opening || StateZD == StateZD.Closing)
                    StateZD = StateZD.Middle;

                if (okc != null) okc.ValDI = false;
                if (ckc != null) ckc.ValDI = false;
                if (odc != null) odc.ValDI = false;
                if (cdc != null) cdc.ValDI = false;
                if (volt != null) volt.ValDI = false;
                if (mc != null) mc.ValDI = false;

                foreach (DIItem c in DIs)
                    c.SetValue(false);
            }
            else
            {
                if (_ZDProc <= 5)
                {
                    //состояние - закрыто
                    if (okc != null)
                        okc.ValDI = true;
                    if (ckc != null)
                        ckc.ValDI = false;

                    if (DIs != null && DIs.Count >= 6)
                    {
                        DIs[intrfOKC].SetValue(true);
                        DIs[intrfCKC].SetValue(false);
                    }
                }
                else
                if (_ZDProc >= 95)
                {
                    //состояние - открыто
                    if (okc != null)
                        okc.ValDI = false;
                    if (ckc != null)
                        ckc.ValDI = true;

                    if (DIs != null && DIs.Count >= 6)
                    {
                        DIs[intrfOKC].SetValue(false);
                        DIs[intrfCKC].SetValue(true);
                    }
                }
                else
                {
                    //состояние - промежуточное
                    if (okc != null)
                        okc.ValDI = true;
                    if (ckc != null)
                        ckc.ValDI = true;

                    if (DIs != null && DIs.Count >= 6)
                    {
                        DIs[intrfOKC].SetValue(true);
                        DIs[intrfCKC].SetValue(true);
                    }
                }

                if (_stateZD == StateZD.Close)
                {
                    //состояние - закрыто

                    //выключить МПЗ
                    if ((cdc != null))
                        cdc.ValDI = false;

                    if (DIs != null && DIs.Count >= 6)
                    {
                        DIs[intrfCDC].SetValue(false);
                    }
                }

                //состояние - открыто
                if (_stateZD == StateZD.Open)
                {
                    //состояние - закрыто
                    if ((odc != null))
                        odc.ValDI = false;

                    if (DIs != null && DIs.Count >= 6)
                    {
                        DIs[intrfODC].SetValue(false);
                    }
                }

                //состояние -открывается
                if (_stateZD == StateZD.Opening)
                {


                    ZDProc += dt / TmoveZD * 100;

                    if (ZDProc >= 100f)
                    {
                        StateZD = StateZD.Open;
                        ZDProc = 100f;

                        MPDelay = MPDelay_timeout;
                        //отключить мпо
                        //    if (ODC != null) ODC.ValDI = false;
                    }
                    else
                    {
                        //включить МПО
                        if (odc != null) odc.ValDI = true;

                        if (DIs != null && DIs.Count >= 6)
                        {
                            DIs[intrfODC].SetValue(true);
                        }
                        //вкл концевики
                        /*   if (OKC != null)
                               OKC.ValDI = true;
                           if (CKC != null)
                               CKC.ValDI = true;*/
                    }
                }

                //состояние -закрывается
                if (_stateZD == StateZD.Closing)
                {


                    ZDProc -= dt / TmoveZD * 100;

                    if (ZDProc <= 0f)
                    {
                        StateZD = StateZD.Close;
                        ZDProc = 0f;
                        MPDelay = MPDelay_timeout;
                        //отключить мпз
                        //  if (CDC != null) CDC.ValDI = false;
                    }
                    else
                    {
                        //включить МПЗ
                        if (cdc != null) cdc.ValDI = true;

                        if (DIs != null && DIs.Count >= 6)
                        {
                            DIs[intrfCDC].SetValue(true);
                        }
                        //вкл концевики
                        /*   if (OKC != null)
                               OKC.ValDI = true;
                           if (CKC != null)
                               CKC.ValDI = true;*/
                    }
                }

                if (_stateZD == StateZD.Middle)
                {
                    //отключить МП
                    if (cdc != null) cdc.ValDI = false;
                    if (odc != null) odc.ValDI = false;

                    if (DIs != null && DIs.Count >= 6)
                    {
                        DIs[intrfODC].SetValue(false);
                        DIs[intrfCDC].SetValue(false);
                    }
                }

                //команда открыть
                if ((dob != null) && (dob.ValDO))
                {
                    //если закрыта или в промежутке
                    if (_stateZD == StateZD.Close || _stateZD == StateZD.Middle)
                    {
                        //вкл мпо
                        StateZD = StateZD.Opening;

                    }
                }

                //команда закрыть
                if ((dkb != null) && (dkb.ValDO))
                {
                    //если открыта или в промежутке
                    if (_stateZD == StateZD.Open || _stateZD == StateZD.Middle)
                    {
                        StateZD = StateZD.Closing;
                    }
                }

                //команда стоп
                if ((dcb != null) && (dcb.ValDO))
                {
                    if (StateZD == StateZD.Opening || StateZD == StateZD.Closing)
                        StateZD = StateZD.Middle;
                    //  {
                    //отключить МП
                    if (odc != null) odc.ValDI = false;
                    if (cdc != null) cdc.ValDI = false;

                }
            }

        }
        #region РУЧНОЕ УПРАВЛЕНИЕ
        public void ManualOpen()
        {
            if (_stateZD == StateZD.Close || _stateZD == StateZD.Middle)
                StateZD = StateZD.Opening;
        }

        public void ManualClose()
        {
            if (_stateZD == StateZD.Open || _stateZD == StateZD.Middle)
                StateZD = StateZD.Closing;
        }

        public void ManualStop()
        {
            if (_stateZD == StateZD.Opening || _stateZD == StateZD.Closing)
                StateZD = StateZD.Middle;
        }

        public void ToggleBS()
        {
            if (bs != null) bs.ValDI = !bs.ValDI;
        }


        /// <summary>
        /// переключение режима "дистанционный"
        /// </summary>
        public void ToggleDist()
        {
            if (dc != null)
                dc.ValDI = !dc.ValDI;

            if (DIs != null && DIs.Count > 6 && DIs[intrfDC]!=null)
            {
                DIs[intrfDC].DI.ValDI = !DIs[intrfDC].DI.ValDI;
            }
        }
        /// <summary>
        /// Установить/снять напряжение на скеции шин извне
        /// </summary>
        /// <param name="value"></param>
        public void SetBusState(bool value)
        {
            if (bs == null)
            {
                bs = new DIStruct();
            }
            if (BSIndex == -1)
                bs.ValDI = value;
        }
        #endregion
        /// <summary>
        /// включить задвижку
        /// </summary>
        public bool En
        {
            get { return _En; }
            set { _En = value; OnPropertyChanged("En"); }
        }
        /// <summary>
        /// положение затвора, индекс в таблице AIStruct
        /// </summary>
        private int _ZD_Pos_index = -1;
        /// <summary>
        /// положение затвора, индекс в таблице AIStruct
        /// </summary>
        public int ZD_Pos_index
        {
            get { return _ZD_Pos_index; }
            set {
                _ZD_Pos_index = value;
                ZD_position_ai = AIStruct.FindByIndex(_ZD_Pos_index); }
        }
        /// <summary>
        /// положение затвора ссылка на аналоговый сигнал
        /// </summary>
        private AIStruct ZD_position_ai;



        public string PositionAIName
        {
            get
            {
                if (ZD_position_ai != null)
                    return ZD_position_ai.NameAI;
                return "сигнал не назначен";
            }
            set { }
        }

        /// <summary>
        /// команда открыть
        /// </summary>
        public int DOBindxArrDO
        {
            get { return _DOBindxArrDO; }
            set {
                _DOBindxArrDO = value;
                OnPropertyChanged("DOBindxArrDO");
                dob = DOStruct.FindByIndex(_DOBindxArrDO);
                if (dob != null)
                    dob.ValueChanged += (sender, e) => { OnPropertyChanged("DOBState"); };
            }
        }

        private DOStruct dob;
        public DOStruct DOB
        {
            get { return dob; }
        }

        [XmlIgnore]
        public bool DOBState { get { if (dob != null) return dob.ValDO; else return false; } set { } }
        /// <summary>
        /// команда закрыть
        /// </summary>
        public int DKBindxArrDO
        {
            get { return _DKBindxArrDO; }
            set {
                _DKBindxArrDO = value;
                OnPropertyChanged("DKBindxArrDO");
                dkb = DOStruct.FindByIndex(_DKBindxArrDO);
                if (dkb != null)
                    dkb.ValueChanged += (sender, e) => { OnPropertyChanged("DKBState"); };
            }
        }

        private DOStruct dkb;
        public DOStruct DKB
        {
            get { return dkb; }
        }
        public string DKBName
        {
            get
            {
                if (dkb != null)
                    return dkb.NameDO;
                else return "сигнал не назначен";
            }
        }
        [XmlIgnore]
        public bool DKBState { get { if (dkb != null) return dkb.ValDO; else return false; } set { } }
        /// <summary>
        /// Команда - остановить
        /// </summary>
        public int DCBindxArrDO
        {
            get { return _DCBindxArrDO; }
            set {
                _DCBindxArrDO = value;
                OnPropertyChanged("DCBindxArrDO");
                dcb = DOStruct.FindByIndex(_DCBindxArrDO);
                if (dcb != null)
                    dcb.ValueChanged += (sender, e) => { OnPropertyChanged("DCBState"); };
            }
        }


        private DOStruct dcb;
        public DOStruct DCB
        {
            get { return dcb; }
        }

        public string DCBName
        {
            get
            {
                if (dcb != null)
                    return dcb.NameDO;
                else return "сигнал не назначен";
            }
        }
        [XmlIgnore]
        public bool DCBState { get { if (dcb != null) return dcb.ValDO; else return false; } set { } }
        /// <summary>
        /// команда стоп закрытия
        /// </summary>
        public int DCBZindxArrDO
        {
            get { return _DCBZindxArrDO; }
            set {
                _DCBZindxArrDO = value;
                OnPropertyChanged("DCBZindxArrDO");
                dcbz = DOStruct.FindByIndex(_DCBZindxArrDO);
                if (dcbz != null)
                    dcbz.ValueChanged += (sender, e) => { OnPropertyChanged("DCBZState"); };
            }
        }


        private DOStruct dcbz;
        public DOStruct DCBZ
        {
            get { return dcbz; }
        }
        public string DCBZName
        {
            get
            {
                if (dcbz != null)
                    return dcbz.NameDO;
                else return "сигнал не назначен";
            }
        }
        //public bool DCBZState { get { if (DCBZ != null) return DCBZ.ValDO; else return false; } set { } }

        public bool ChangedDO
        {
            get { return _changedDO; }
            set { _changedDO = value; }
        }
        /// <summary>
        /// Концевой выключатель открытия
        /// </summary>
        public int OKCindxArrDI
        {
            get { return _OKCindxArrDI; }
            set {
                _OKCindxArrDI = value;
                // OnPropertyChanged("DCBZindxArrDO");
                okc = DIStruct.FindByIndex(_OKCindxArrDI);

                //подписываем наш метод на событие чтобы видеть изменения дискретов в таблице задвижек
                if (okc != null)
                    okc.ValueChanged += (sender, e) => { OnPropertyChanged("KVO"); };
            }
        }

        /// <summary>
        /// КВО
        /// </summary>
        private DIStruct okc;
        public DIStruct OKC
        {
            get { return okc; }
        }
        /* public string OKCName
         {
             get
             {

                 return DIStruct.GetNameByIndex(_OKCindxArrDI);
             }
         }*/

        /// <summary>
        /// концевой выключатель закрытия
        /// </summary>
        public int CKCindxArrDI
        {
            get { return _CKCindxArrDI; }
            set {
                _CKCindxArrDI = value;
                //OnPropertyChanged("DCBZindxArrDO");
                ckc = DIStruct.FindByIndex(_CKCindxArrDI);
                if (ckc != null)
                    ckc.ValueChanged += (sender, e) => { OnPropertyChanged("KVZ"); };
            }
        }

        /// <summary>
        /// КВЗ
        /// </summary>
        private DIStruct ckc;

        /// <summary>
        /// КВЗ
        /// </summary>
        public DIStruct CKC
        {
            get { return ckc; }
        }
        /// <summary>
        /// сигнал от МПО
        /// </summary>
        public int ODCindxArrDI
        {
            get { return _ODCindxArrDI; }
            set {
                _ODCindxArrDI = value;
                OnPropertyChanged("ODCindxArrDI");
                odc = DIStruct.FindByIndex(_ODCindxArrDI);
                if (odc != null)
                    odc.ValueChanged += (sender, e) => { OnPropertyChanged("MPO"); };
            }
        }


        /// <summary>
        /// МПО
        /// </summary>
        private DIStruct odc;
        public DIStruct ODC
        {
            get { return odc; }
        }


        /// <summary>
        /// наличие напряжения на секции шин
        /// </summary>
        private DIStruct bs;
        public DIStruct BS
        {
            get { return bs; }
        }

        /// <summary>
        /// Наличие напряжения на СШ индекс сигнала
        /// </summary>
        public int _bsindex = -1;
        /// <summary>
        /// Наличие напряжения на СШ индекс сигнала
        /// </summary>
        public int BSIndex
        {
            get { return _bsindex; }
            set
            {
                _bsindex = value;
                if (_bsindex >= 0 && _bsindex < DIStruct.items.Count)
                {
                    bs = DIStruct.items[_bsindex];
                    bs.ValueChanged += (sender, e) => { OnPropertyChanged("BSState"); };
                }
                else
                    bs = null;
            }
        }
        [XmlIgnore]
        public string BSName
        {
            set { }
            get
            {
                if (bs != null)
                    return bs.NameDI;
                else return "сигнал не назначен";
            }
        }
        /// <summary>
        /// сигнал от МПЗ
        /// </summary>
        public int CDCindxArrDI
        {
            get { return _CDCindxArrDI; }
            set {
                _CDCindxArrDI = value;
                OnPropertyChanged("CDCindxArrDI");
                cdc = DIStruct.FindByIndex(_CDCindxArrDI);
                if (cdc != null)
                    cdc.ValueChanged += (sender, e) => { OnPropertyChanged("MPZ"); };
            }
        }
        /*
        private void CDC_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
           
            if (e.PropertyName=="ValDI")
            OnPropertyChanged("MPZ");
        }
        */
        /// <summary>
        /// МПЗ
        /// </summary>
        private DIStruct cdc;
        public DIStruct CDC
        {
            get { return cdc; }
        }
        [XmlIgnore]
        public string CDCName
        {
            get
            {
                if (cdc != null)
                    return cdc.NameDI;
                else return "сигнал не назначен";
            }
        }
        [XmlIgnore]
        public bool CDCState { get { if (cdc != null) return cdc.ValDI; else return false; } set { } }

        /// <summary>
        /// дистанционное управление
        /// </summary>
        public int DCindxArrDI
        {
            get { return _DCindxArrDI; }
            set {
                _DCindxArrDI = value;
                OnPropertyChanged("DCindxArrDI");
                dc = DIStruct.FindByIndex(_DCindxArrDI);
            }
        }
        private DIStruct dc;
        public DIStruct DC
        {
            get { return dc; }
        }
        [XmlIgnore]
        public string DCName
        {
            get
            {
                if (dc != null)
                    return dc.NameDI;
                else return "сигнал не назначен";
            }
        }
        [XmlIgnore]
        public bool DCState { get { if (dc != null) return dc.ValDI; else return false; } set { } }

        /// <summary>
        /// наличие напряжения
        /// </summary>
        public int VoltindxArrDI
        {
            get { return _VoltindxArrDI; }
            set {
                _VoltindxArrDI = value;
                OnPropertyChanged("VolindxArrDI");
                volt = DIStruct.FindByIndex(_VoltindxArrDI);
            }
        }
        private DIStruct volt;
        public DIStruct Volt
        {
            get { return volt; }
        }
        /*  [XmlIgnore]
          public string VoltName
          {
              get
              {
                  if (volt != null)
                      return volt.NameDI;
                  else return "сигнал не назначен";
              }
          }*/
        [XmlIgnore]
        public bool VoltState { get { if (volt != null) return volt.ValDI; else return false; } set { } }

        /// <summary>
        /// сработала муфта 
        /// </summary>
        public int MCindxArrDI
        {
            get { return _MCindxArrDI; }
            set {

                _MCindxArrDI = value;
                OnPropertyChanged("MCindxArrDI");
                mc = DIStruct.FindByIndex(_MCindxArrDI);
            }
        }
        /// <summary>
        /// авария привода
        /// </summary>
        private DIStruct mc;
        [XmlIgnore]
        public DIStruct MC
        {
            get { return mc; }
        }
        /*public string MCName
        {
            get
            {
                if (MC != null)
                    return MC.NameDI;
                else return "сигнал не назначен";
            }
        }*/
        // public bool MCState { get { if (MC != null) return MC.ValDI; else return false; } set { } }

        /// <summary>
        /// авария привода
        /// </summary>
        public int OPCindxArrDI
        {
            get { return _OPCindxArrDI; }
            set {
                _OPCindxArrDI = value;
                OnPropertyChanged("OPCindxArrDI");
                opc = DIStruct.FindByIndex(_OPCindxArrDI);
            }
        }
        /// <summary>
        /// авария привода
        /// </summary>
        private DIStruct opc;

        [XmlIgnore]
        public DIStruct OPC
        {
            get { return opc; }
        }

        #region Интерфейсные сигналы
        /*  private int _intrfOKC = -1; //индекс интерфейсного сигнала "открыта"
          private int _intrfCKC = -1; //индекс интерфейсного сигнала "закрыта"
          private int _intrfODC = -1; //индекс интерфейсного сигнала "открывается"
          private int _intrfCDC = -1; //индекс интерфейсного сигнала "закрывается"
          private int _intrfDC = -1; //индекс интерфейсного сигнала "ду"
          private int _intrfFault = -1; //индекс интерфейсного сигнала "привод в порядке"
          */

        /// <summary>
        /// интерфейсные сигналы
        /// </summary>
        public List<DIItem> DIs
        { set; get; }
        #endregion

        public float ZDProc
        {
            get { return _ZDProc; }
            set { _ZDProc = value; OnPropertyChanged("Percent"); }
        }
        /// <summary>
        /// процент открытия для вывода на экран
        /// </summary>
        public int Percent { get { return (int)_ZDProc; } set { } }

        public bool ChangedDI
        {
            get { return _changedDI; }
            set { _changedDI = value; }
        }
        public float TmoveZD
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
            set { _description = value; OnPropertyChanged("Description"); }
        }
        public string Group
        {
            get { return _group; }
            set { _group = value; OnPropertyChanged("Group"); }
        }

        /// <summary>
        /// Обновление ссылок на сигналы DI,DO,AI, следует вызывать после десериализации настроек
        /// для того чтобы экземпляр класса получил ссылки на объекты
        /// </summary>
        public void UpdateRefs()
        {

            DOBindxArrDO = _DOBindxArrDO;
            DCBindxArrDO = _DCBindxArrDO;
            DKBindxArrDO = _DKBindxArrDO;
            DCBZindxArrDO = _DCBZindxArrDO;
            ODCindxArrDI = _ODCindxArrDI;
            CDCindxArrDI = _CDCindxArrDI;
            OKCindxArrDI = _OKCindxArrDI;
            CKCindxArrDI = _CKCindxArrDI;
            DCindxArrDI = _DCindxArrDI;
            VoltindxArrDI = _VoltindxArrDI;
            MCindxArrDI = _MCindxArrDI;
            ZD_Pos_index = _ZD_Pos_index;
            BSIndex = _bsindex;

            if (DIs != null)
                foreach (DIItem c in DIs)
                    c.Index = c.Index;
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string prop = "") => PropertyChanged?.Invoke(sender: this, e: new PropertyChangedEventArgs(prop));



        #region свойства для отображения состояния дискретов в таблице
        /// <summary>
        /// положение затвора, свойство для таблицы
        /// </summary>
        public int Position
        {
            get
            {
                if (ZD_position_ai != null)
                    return (int)ZD_position_ai.fValAI;
                return 0;
            }
            set { }
        }
        /// <summary>
        /// КВО для отображения во view
        /// </summary>
        [XmlIgnore]
        public bool? KVO
        {
            get
            {
                if (okc != null)
                    return okc.ValDI;
                else return null;
            }
        }
        /// <summary>
        /// КВЗ для отображения во view
        /// </summary>
        [XmlIgnore]
        public bool? KVZ
        {
            get
            {
                if (ckc != null)
                    return ckc.ValDI;
                else return null;
            }
        }
        /// <summary>
        /// МПО для отображения во view
        /// </summary>
        [XmlIgnore]
        public bool? MPO
        {
            get
            {
                if (odc != null)
                    return odc.ValDI;
                else return null;
            }
        }
        /// <summary>
        /// МПЗ для отображения во view
        /// </summary>
        [XmlIgnore]
        public bool? MPZ
        {
            get
            {
                if (cdc != null)
                    return cdc.ValDI;
                else return null;
            }
        }
        /// <summary>
        /// команда открыть
        /// </summary>
        [XmlIgnore]
        public bool? DOBstate
        {
            get
            {
                if (dob != null)
                    return dob.ValDO;
                else return null;
            }
        }
        /// <summary>
        /// команда закрыть
        /// </summary>
        [XmlIgnore]
        public bool? DKBstate
        {
            get
            {
                if (dkb != null)
                    return dkb.ValDO;
                else return null;
            }
        }

        /// <summary>
        /// команда стоп
        /// </summary>
        [XmlIgnore]
        public bool? DCBstate
        {
            get
            {
                if (dcb != null)
                    return dcb.ValDO;
                else return null;
            }
        }
        /// <summary>
        /// команда стоп закрытия
        /// </summary>
        [XmlIgnore]
        public bool? DCBZstate
        {
            get
            {
                if (dcbz != null)
                    return dcbz.ValDO;
                else return null;
            }
        }
        /// <summary>
        /// наличие напряжения на СШ
        /// </summary>
        [XmlIgnore]
        public bool? BSState
        {
            get
            {
                if (bs != null)
                    return bs.ValDI;
                else return null;
            }
        }
        #endregion
    }



}
