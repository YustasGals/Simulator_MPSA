﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simulator_MPSA.CL;
using System.Collections.ObjectModel;
using LuaInterface;
using LuaInterface.Exceptions;
using System.Xml.Serialization;
using System.Diagnostics;
using Simulator_MPSA.CL.Signal;

namespace Simulator_MPSA.Scripting
{
    /// <summary>
    /// набор функции доступных из скрипта lua
    /// </summary>
    class Utils
    {
        public float dt;
        public float GetZDProc(int index)
        {
            return ZDTableViewModel.ZDs[index].ZDProc;
        }

        public bool GetVSState(int index)
        {
            if (index < VSTableViewModel.VS.Count)
                return VSTableViewModel.VS[index].State == VSState.Work;
            else
                return false;
        }
        /*
        /// <summary>
        /// установить состояние секции шин, если сигнал не подвязан
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        public void SetVSBS(int index, bool value)
        {
            if (index < VSTableViewModel.VS.Count)
                VSTableViewModel.VS[index].SetBusState(value);
        }

        /// <summary>
        /// установить состоянии секции шин
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        public void SetZDBS(int index, bool value)
        {
            if (index < ZDTableViewModel.ZDs.Count)
                ZDTableViewModel.ZDs[index].SetBusState(value);
        }

        /// <summary>
        /// установить состоянии секции шин, МПНА
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        public void SetMPNABS(int index, bool value)
        {
            if (index < MPNATableViewModel.MPNAs.Count)
                MPNATableViewModel.MPNAs[index].SetBusState(value);
        }
        */
        public bool GetMPNAState(int index)
        {
            if (index < MPNATableViewModel.MPNAs.Count)
                return MPNATableViewModel.MPNAs[index].State == MPNAState.Work;
            else
                return false;
        }
        public float GetKLState(int index)
        {
            if (index < KLTableViewModel.KL.Count)
                return KLTableViewModel.KL[index].KLProc;
            else return 0;
        }

        public void SetAI(int index, float value)
        {
            //AIStruct ai = AIStruct.FindByIndex(index);
            //if (ai != null)
            //    ai.fValAI = value;
            if (index>=0 && index < AIStruct.items.Count)
                AIStruct.items[index].fValAI = value;
        }
        public float GetAI(int index)
        {
            //  AIStruct ai = AIStruct.FindByIndex(index);
            //  if (ai != null)
            //      return ai.fValAI;
            //  else return 0;
            if (index >= 0 && index < AIStruct.items.Count)
                return AIStruct.items[index].fValAI;
            else return 0;
        }
        public void ChangeAI(int index,  float delta)
        {
            //AIStruct ai = AIStruct.FindByIndex(index);
            //if (ai != null)
            //    ai.fValAI += delta;
            if (index >= 0 && index < AIStruct.items.Count)
                AIStruct.items[index].fValAI += delta;
        }

        public void SetDI(int index, bool value)
        {
            //  DIStruct di = DIStruct.FindByIndex(index);
            //  if (di != null)
            //      di.ValDI = value;
            if (index >= 0 && index < DIStruct.items.Count)
                DIStruct.items[index].ValDI = value;
        }
        public bool GetDI(int index)
        {
            //  DIStruct di = DIStruct.FindByIndex(index);
            //  if (di != null)
            //      di.ValDI = value;
            if (index >= 0 && index < DIStruct.items.Count)
              return  DIStruct.items[index].ValDI;
            //else
            return false;
        }

        public bool GetDO(int index)
        {
            //DOStruct d = DOStruct.FindByIndex(index);
            // if (d != null)
            //     return d.ValDO;
            // else return false;
            if (index >= 0 && index < DOStruct.items.Count)
                return DOStruct.items[index].ValDO;
            else return false;
        }

        public float GetAO(int index)
        {
            if (index >= 0 && index < AOStruct.items.Count)
                return AOStruct.items[index].fVal;
            else return 0f;
        }
        public void Print(string text)
        {
         //   LogWriter.refMainWindow.Dispatcher.Invoke(LogWriter.refMainWindow.WriteLog,text);
           // LogWriter.AppendLog(text + Environment.NewLine);
        }

        /// <summary>
        /// возвращает время в мсек
        /// </summary>
        /// <returns></returns>
        public long GetTime()
        {
            DateTime now = DateTime.Now;
            return (now.Millisecond + now.Second * 1000 + now.Minute * 60000 + now.Hour * 24 * 60 * 1000);
        }

        public float GetDeltaTime()
        {
            return dt;
        }

     
    }

    [Serializable]
    public class ScriptInfo : BaseViewModel
    {
        /// <summary>
        /// ссылка на главное окно для вызова делегатов
        /// </summary>
        public static MainWindow refMainWindow;
        /// <summary>
        /// 
        /// </summary>
        public string Name
        { set; get;}

        private bool _en;
        public bool En
        { set { _en = value; OnPropertyChanged("En"); }
            get { return _en; }
        }

        public void Deactivate()
        {
            En = false;
        }

        public void Print(string text)
        {
            refMainWindow.Dispatcher.Invoke(refMainWindow.WriteLog, text);
        }

        public bool Ready
        { set; get; }


        private string _scripttxt="";
        /// <summary>
        /// текст скрипта
        /// </summary>
        public string ScriptTxt
        {
            set
            {
                _scripttxt = value;
                needInit = true;
            }
          get { return _scripttxt; }
        }

        private string _errorText = "";
        public string ErrorText
        { set { _errorText = value; OnPropertyChanged("ErrorText");  } get { return _errorText; } }

        /// <summary>
        /// флаг обозначает что при следующем вызове Run будет вызвана функци Init из скрипта
        /// </summary>
        private bool needInit = true;
        Lua lua = new Lua();
        private Utils utils = new Utils();
        private Utils sim = new Utils();
        LuaFunction funcInit;
        LuaFunction funcUpdate;

        public void Run(float dt, bool runOnce=false)
        {
            if (En || runOnce)
            {
                

                try
                {
                    string err = "";

                    lua["dt"] = dt;
                    utils.dt = dt;
                    
                    if (needInit)
                    {
                        //lua.LoadString(ScriptTxt, "");
                        lua.DoString(ScriptTxt);
                        needInit = false;
                        funcInit = lua.GetFunction("Init");
                        funcUpdate = lua.GetFunction("Update");

                        if (funcInit == null)
                        {
                            err += "Функция Init не определена; ";
                        }
                        else
                        {
                            funcInit.Call();
                            err += "Init ok; ";
                        }
                    }
                   // funcUpdate = lua.GetFunction("Update");
                    if (funcUpdate == null)
                    {
                        err += "Функция Update не определена";
                    }
                    else
                    {
                        funcUpdate.Call();
                        err += "Update ok;";
                    }
                    ErrorText = err;
                }
                catch (Exception err)
                {
                    // Обработка ошибки
                    En = false;
                    System.Windows.MessageBox.Show("Ошибка:" + Environment.NewLine + err.Message);

                }
            }
            
        }
        public ScriptInfo()
        {
            lua["Utils"] = utils;
            lua["Sim"] = utils;
            ScriptTxt = "function Init()\n\r" +
                        "end \n\r" +
                        "function Update()\n\r" +
                        "end";

            lua.RegisterFunction("SetDI", utils,typeof(Utils).GetMethod("SetDI"));
            lua.RegisterFunction("GetDI", utils, typeof(Utils).GetMethod("GetDI"));

            lua.RegisterFunction("SetAI", utils, typeof(Utils).GetMethod("SetAI"));
            lua.RegisterFunction("GetAI", utils, typeof(Utils).GetMethod("GetAI"));
            lua.RegisterFunction("ChangeAI", utils, typeof(Utils).GetMethod("ChangeAI"));

            lua.RegisterFunction("GetAO", utils, typeof(Utils).GetMethod("GetAO"));
            lua.RegisterFunction("GetDO", utils, typeof(Utils).GetMethod("GetDO"));

            lua.RegisterFunction("GetZDProc", utils, typeof(Utils).GetMethod("GetZDProc"));
            lua.RegisterFunction("GetVSState", utils, typeof(Utils).GetMethod("GetVSState"));
            lua.RegisterFunction("GetKLState", utils, typeof(Utils).GetMethod("GetKLState"));
            lua.RegisterFunction("GetMPNAState", utils, typeof(Utils).GetMethod("GetMPNAState"));

            lua.RegisterFunction("GetDeltaTime", utils, typeof(Utils).GetMethod("GetDeltaTime"));
            

            lua.RegisterFunction("Print", this, typeof(ScriptInfo).GetMethod("Print"));

            lua.RegisterFunction("Deactivate", this, typeof(ScriptInfo).GetMethod("Deactivate"));
        }

        /// <summary>
        /// подготовить к выполнению, в следующей итерации будет выполнена функция Init()
        /// </summary>
        public void Prepare()
        {
            needInit = true;
        }
     
    }
}
