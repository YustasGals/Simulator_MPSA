using System;
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

namespace Simulator_MPSA.Scripting
{
    /// <summary>
    /// набор функции доступных из скрипта lua
    /// </summary>
    class Utils
    {
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
            if (index < AIStruct.items.Length)
                AIStruct.items[index].fValAI = value;
        }
        public float GetAI(int index)
        {
            //  AIStruct ai = AIStruct.FindByIndex(index);
            //  if (ai != null)
            //      return ai.fValAI;
            //  else return 0;
            if (index < AIStruct.items.Length)
                return AIStruct.items[index].fValAI;
            else return 0;
        }
        public void ChangeAI(int index,  float delta)
        {
            //AIStruct ai = AIStruct.FindByIndex(index);
            //if (ai != null)
            //    ai.fValAI += delta;
            if (index < AIStruct.items.Length)
                AIStruct.items[index].fValAI += delta;
        }

        public void SetDI(int index, bool value)
        {
            //  DIStruct di = DIStruct.FindByIndex(index);
            //  if (di != null)
            //      di.ValDI = value;
            if (index < DIStruct.items.Length)
                DIStruct.items[index].ValDI = value;
        }
        public bool GetDO(int index)
        {
            //DOStruct d = DOStruct.FindByIndex(index);
            // if (d != null)
            //     return d.ValDO;
            // else return false;
            if (index < DOStruct.items.Length)
                return DOStruct.items[index].ValDO;
            else return false;
        }
        public void Test()
        {
            Debug.WriteLine("hello");
        //    System.Windows.MessageBox.Show("hello");
        }
    }

    [Serializable]
    public class ScriptInfo : BaseViewModel
    {
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
                    
                    lua.DoString(ScriptTxt);
                    if (needInit)
                    {
                        //lua.LoadString(ScriptTxt, "");
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
            ScriptTxt = "";
           
        }
        public void Prepare()
        {
            needInit = true;
        }
        private static ObservableCollection<ScriptInfo> _items = new ObservableCollection<ScriptInfo>();
        public static ObservableCollection<ScriptInfo> Items
            { set { _items = value; } get { return _items; } }

        public static void Init()
        {
            _items = new ObservableCollection<ScriptInfo>();
        }
    }
}
