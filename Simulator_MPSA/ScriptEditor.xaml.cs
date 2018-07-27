using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Simulator_MPSA.Scripting;
using System.Diagnostics;
using ICSharpCode.AvalonEdit;
using System.IO;
using System.Xml;

namespace Simulator_MPSA
{
    /// <summary>
    /// Логика взаимодействия для ScriptEditor.xaml
    /// </summary>
    public partial class ScriptEditor : Window
    {
        ScriptInfo script;
        bool textChanged = false;
        public ScriptEditor(Scripting.ScriptInfo script)
        {
            InitializeComponent();

            this.script = script;
            Editor.Text = script.ScriptTxt;

            Editor.TextChanged += Editor_TextChanged;
            using (Stream s = File.OpenRead("resources/lua.xshd"))
            {
                using (XmlTextReader reader = new XmlTextReader(s))
                {
                    Editor.SyntaxHighlighting = ICSharpCode.AvalonEdit.Highlighting.Xshd.HighlightingLoader.Load
                        (reader, ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance);
                }
            }
        }

        private void On_ButtonClose(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void On_ButtonApply(object sender, RoutedEventArgs e)
        {
            script.ScriptTxt = Editor.Text;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Process[] processes = Process.GetProcessesByName("hh");
            if (processes.Length > 0)
                processes[0].Kill();


        //    if (Process.GetProcessesByName("hh").Length == 0)
                Process.Start("SimHelp.chm");
        }

        private void On_Run(object sender, RoutedEventArgs e)
        {
            script.ScriptTxt = Editor.Text;
            script.Prepare();
            script.Run(0, true);
        }

        private void Editor_TextChanged(object sender, EventArgs e)
        {
            if (!textChanged)
            {
                editorWindow.Title = editorWindow.Title + " - изменен";
                textChanged = true;
            }

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (textChanged)
            {
                MessageBoxResult res = MessageBox.Show("Текст скрипта был изменен, сохранить изменения?", "Предупреждение", MessageBoxButton.YesNoCancel);
                switch (res)
                {
                    case MessageBoxResult.Yes:
                        On_ButtonApply(sender, new RoutedEventArgs());
                        break;
                    case MessageBoxResult.No:
                        break;
                    case MessageBoxResult.Cancel:
                        e.Cancel = true;
                        break;
                }
            }
        }
    }
}
