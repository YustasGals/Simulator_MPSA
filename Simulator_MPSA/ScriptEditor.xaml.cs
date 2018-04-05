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

namespace Simulator_MPSA
{
    /// <summary>
    /// Логика взаимодействия для ScriptEditor.xaml
    /// </summary>
    public partial class ScriptEditor : Window
    {
        public ScriptEditor(Scripting.ScriptInfo script)
        {
            Editor.Text = script.ScriptTxt;
            InitializeComponent();
        }

        private void On_ButtonClose(object sender, RoutedEventArgs e)
        {

        }

        private void On_ButtonApply(object sender, RoutedEventArgs e)
        {

        }
    }
}
