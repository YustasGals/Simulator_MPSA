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
    /// Логика взаимодействия для StationSetup.xaml
    /// </summary>
    public partial class StationSetup : Window
    {
        public bool accepted = false;
        SetupViewModel viewModel;
        public StationSetup()
        {
            InitializeComponent();

            Sett s = new Sett();
            s.items = new Dictionary<string, SettingsItem>();

            foreach (KeyValuePair<string, SettingsItem> pair in Sett.Instance.items)
                s.items.Add(pair.Key, pair.Value);

            viewModel = new SetupViewModel(s);
            this.DataContext = viewModel;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Sett.Instance = viewModel.settings;
            RB.InitBuffer();
            WB.InitBuffers();
            accepted = true;
        }

        private void bClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

    class SetupViewModel
    {
        public Sett settings
        { set; get; }

        public SetupViewModel(Sett initSettings)
        {
            settings = initSettings;
        }
    }
}
