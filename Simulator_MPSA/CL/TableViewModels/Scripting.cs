using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simulator_MPSA.CL;
using System.Collections.ObjectModel;

using LuaInterface;
using LuaInterface.Exceptions;

namespace Simulator_MPSA.Scripting
{
    /// <summary>
    /// набор функции доступных из скрипта lua
    /// </summary>
    class Utils
    {
        float GetZDProc(int index)
        {
            return ZDTableViewModel.ZDs[index].ZDProc;
        }

        bool GetVSState(int index)
        {
            if (index < VSTableViewModel.VS.Count)
                return VSTableViewModel.VS[index].State == VSState.Work;
            else
                return false;
        }

        bool GetMPNAState(int index)
        {
            if (index < MPNATableViewModel.MPNAs.Count)
                return MPNATableViewModel.MPNAs[index].State == MPNAState.Work;
            else
                return false;
        }
        float GetKLState(int index)
        {
            if (index < KLTableViewModel.KL.Count)
                return KLTableViewModel.KL[index].KLProc;
            else return 0;
        }

        void SetAI(int index, float value)
        {
            AIStruct ai = AIStruct.FindByIndex(index);
            if (ai != null)
                ai.fValAI = value;
        }
        float GetAI(int index)
        {
            AIStruct ai = AIStruct.FindByIndex(index);
            if (ai != null)
                return ai.fValAI;
            else return 0;
        }

        void SetDI(int index, bool value)
        {
            DIStruct di = DIStruct.FindByIndex(index);
            if (di != null)
                di.ValDI = value;
        }
        
    }
    public class ScriptInfo
    {
        /// <summary>
        /// 
        /// </summary>
        public string Name
        { set; get;}


        public bool En
        { set; get; }

        public bool Ready
        { set; get; }

        /// <summary>
        /// текст скрипта
        /// </summary>
        public string ScriptTxt
        { set; get; }


        Lua lua = new Lua();
        public void Run(float dt)
        {
            if (En)
            {
                lua["dt"] = dt;

                try
                {
                    lua.DoString(ScriptTxt);
                }
                catch (LuaException err)
                {
                    // Обработка ошибки
                    En = false;
                    System.Windows.MessageBox.Show("Ошибка:" + Environment.NewLine + err.Message);
                   
                }
            }

        }
        public ScriptInfo()
        {
            lua["Utils"] = new Utils();
        }
        public void Prepare()
        {
        }
        private static ObservableCollection<ScriptInfo> _items = new ObservableCollection<ScriptInfo>();
        public static ObservableCollection<ScriptInfo> Items
            { set { _items = value; } get { return _items; } }

        public static void Init()
        {
            _items = new ObservableCollection<ScriptInfo>();
            _items.Add(new ScriptInfo());
        }
    }
}
