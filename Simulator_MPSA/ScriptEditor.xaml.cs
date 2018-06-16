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
namespace Simulator_MPSA
{
    /// <summary>
    /// Логика взаимодействия для ScriptEditor.xaml
    /// </summary>
    public partial class ScriptEditor : Window
    {
        ScriptInfo script;
        public ScriptEditor(Scripting.ScriptInfo script)
        {
            InitializeComponent();

            this.script = script;
            Editor.Text = script.ScriptTxt;

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
            Process.Start("SimHelp.chm");
        }

        private void On_Run(object sender, RoutedEventArgs e)
        {
            script.ScriptTxt = Editor.Text;
            script.Prepare();
            script.Run(0, true);
        }
    }
}
