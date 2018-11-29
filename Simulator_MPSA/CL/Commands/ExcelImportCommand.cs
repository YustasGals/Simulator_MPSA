using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Simulator_MPSA.CL.Commands
{
    class ExcelImportCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
             return true;
        }

        public void Execute(object parameter)
        {
       
            System.Windows.Forms.OpenFileDialog fd = new System.Windows.Forms.OpenFileDialog();
            if (fd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string conString = ExcelImporter.GetConnectionString(fd.FileName);
                conString.Replace("\\", "\\\\");
                //LogViewModel.WriteLine("Открываю файл переменных: " + conString);

                LogWriter.AppendLog("Анализ файла переменных: " + conString + " ...");
                ExcelImporter.ReadExcelFile(conString);

            }
        }
    }
}
