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
    /// Логика взаимодействия для Log.xaml
    /// </summary>
    public partial class Log : Window
    {
        

        public Log()
        {
            InitializeComponent();
            DataContext = LogViewModel.Instance;
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            LogViewModel.Instance.LogText = "";  
        }

        private void btnTopmost_Click(object sender, RoutedEventArgs e)
        {
            this.Topmost = !Topmost;    
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox_log.ScrollToEnd();
        }
    }

    public class LogViewModel : BaseViewModel
    {
        private static LogViewModel instance;
        public static LogViewModel Instance
        {
            get
            {
                if (instance == null)
                    instance = new LogViewModel();

                return instance;
            }
            set { }
        }

        private string logText;
        public string LogText
        {
            set
            {               
                logText = value;
                OnPropertyChanged("LogText");
            }
            get { return logText; }
        }
        public static void WriteLine(string line)
        {
            Instance.LogText += DateTime.Now.TimeOfDay.ToString() + ": " + line + Environment.NewLine;
        }
       /* public void WriteLine(string line)
        {
            LogText += DateTime.Now.TimeOfDay.ToString() + ": " + line + Environment.NewLine;
        }*/
    }
}
