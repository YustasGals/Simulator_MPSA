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
using Simulator_MPSA.CL;
namespace Simulator_MPSA
{
    /// <summary>
    /// Логика взаимодействия для SetupDialog.xaml
    /// </summary>
    public partial class SetupDialog : Window
    {
        SetupTableModel model;
        VSStruct _agr;

        public SetupDialog(VSStruct agr)
        {
            InitializeComponent();
            _agr = agr;
            model = new SetupTableModel(agr);
           // textBox_name.DataContext = model;
           // textBox_name.DataContext = model;
           // dataGrid.DataContext = model;
            this.DataContext = model;
        }

        private void button_OK_Click(object sender, RoutedEventArgs e)
        {
            _agr.ECindxArrDI = model.Items[0].Index;
            _agr.isECAnalog = model.Items[0].IsAnalog;
            _agr.valueEC = model.Items[0].ActivationValue;

            _agr.MPCindxArrDI = model.Items[1].Index;
            _agr.isMPCAnalog = model.Items[1].IsAnalog;
            _agr.valueMPC = model.Items[1].ActivationValue;

            _agr.PCindxArrDI = model.Items[2].Index;
            _agr.isPCAnalog = model.Items[2].IsAnalog;
            _agr.valuePC = model.Items[2].ActivationValue;

            _agr.Description = model.VSName;
            _agr.Group = model.VSGroup;
            // this.Close();
            //чтобы обновить табличку заново вызовем конструктор модели
            model = new SetupTableModel(_agr);
            this.DataContext = model;
        }

        private void button_cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
