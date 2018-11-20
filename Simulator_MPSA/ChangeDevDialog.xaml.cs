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
using Simulator_MPSA.CL.Signal;

namespace Simulator_MPSA
{
    /// <summary>
    /// Логика взаимодействия для ChangeDevDialog.xaml
    /// </summary>
    public partial class ChangeDevDialog : Window
    {
        Context context;
        public ChangeDevDialog()
        {
            InitializeComponent();
            string tagName="";
            if (DIStruct.items.Count > 0)
            {
                foreach (DIStruct item in DIStruct.items)
                {
                    if (item.OPCtag != "")
                    {
                        tagName = item.OPCtag;
                        
                    }
                }
            }

            string deviceName="";
            for (int i = 0; i < tagName.Length; i++)
            {
                if (tagName[i] == '!')
                    break;

                deviceName += tagName[i];
            }

            context = new Context();

            context.CurrentName = deviceName;
            this.DataContext = context;
            //textBox_current.Text = 
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (context.NewName != "")
            {
                string newName = context.NewName;
                int count = 0;
                int errorCount = 0;
                string errorMessage = "----- Ошибка переименования -----"+Environment.NewLine;
                errorMessage += "Тэги не содержат корректное название устройства:"+Environment.NewLine;

                System.IO.StreamWriter writer = new System.IO.StreamWriter("RenameError.txt");
                writer.Write(errorMessage);
                foreach (DIStruct item in DIStruct.items)
                    if (item.OPCtag != "")
                    {
                        if (item.OPCtag.Contains(context.CurrentName))
                        {
                            item.OPCtag = newName + item.OPCtag.Remove(0, context.CurrentName.Length);
                            count++;
                        }
                        else
                        {
                            errorCount++;
                            writer.Write("DI "+item.indxArrDI + "  " + item.OPCtag + Environment.NewLine);
                        }
                    }

                foreach (AIStruct item in AIStruct.items)
                    if (item.OPCtag != "")
                    {
                        if (item.OPCtag.Contains(context.CurrentName))
                        {
                            item.OPCtag = newName + item.OPCtag.Remove(0, context.CurrentName.Length);
                            count++;
                        }
                        else
                        {
                            errorCount++;
                            writer.Write("AI " + item.indxAI + "  " + item.OPCtag + Environment.NewLine);
                        }
                    }

                foreach (DOStruct item in DOStruct.items)
                    if (item.OPCtag != "")
                    {
                        if (item.OPCtag.Contains(context.CurrentName))
                        {
                            item.OPCtag = newName + item.OPCtag.Remove(0, context.CurrentName.Length);
                            count++;
                        }
                        else
                        {
                            errorCount++;
                            writer.Write("DO " + item.indxArrDO + "  " + item.OPCtag + Environment.NewLine);
                        }
                    }

                foreach (AOStruct item in AOStruct.items)
                    if (item.OPCtag != "")
                    {
                        if (item.OPCtag.Contains(context.CurrentName+'!'))
                        {
                            item.OPCtag = newName + item.OPCtag.Remove(0, context.CurrentName.Length);
                            count++;
                        }
                        else
                        {
                            errorCount++;
                            writer.Write("AO " + item.indx + "  " + item.OPCtag + Environment.NewLine);
                        }
                    }
                writer.Close();
                context.CurrentName = newName;
                if (errorCount == 0)
                    MessageBox.Show("Переименовано тэгов: " + count.ToString());
                else
                    if (MessageBox.Show("Переименовано тэгов: " + count.ToString() + Environment.NewLine + "Некоторые тэги содержали ошибки, показать отчет?", "Завершено с ошибками", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                        {
                    System.Diagnostics.Process.Start("RenameError.txt");
                        }
            }

           
        }
    }

    public class Context
    {
        public string CurrentName
        { set; get; }
        public string NewName
        { set; get; }
    }
}
