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
    /// Логика взаимодействия для AppSettings.xaml
    /// </summary>
    public partial class AppSettings : Window
    {
        public AppSettings()
        {
            InitializeComponent();
        }
        SettingsContext context;
        private void Window_Initialized(object sender, EventArgs e)
        {
            context = new SettingsContext();

            context.ModbusHost = Properties.Settings.Default.ModbusIPAddr;
            context.OPCServerName = Properties.Settings.Default.OPCServerName;
            context.OPCServerPrefix = Properties.Settings.Default.OPCAddr;
            context.OPCDeviceName = Properties.Settings.Default.OPCDevice;
            this.DataContext = context;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.ModbusIPAddr = context.ModbusHost;
            Properties.Settings.Default.OPCServerName = context.OPCServerName;
            Properties.Settings.Default.OPCAddr = context.OPCServerPrefix;
            Properties.Settings.Default.OPCDevice = context.OPCDeviceName;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }

    public class SettingsContext
    {
        public string ModbusHost
        { set; get; }

        public string OPCServerName
        { set; get; }

        public string OPCServerPrefix
        { set; get; }

        public string OPCDeviceName
        { set; get; }
    }
}
