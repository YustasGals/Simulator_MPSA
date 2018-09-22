using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simulator_MPSA.CL.Signal;
using System.Windows.Input;
using System.Globalization;

namespace Simulator_MPSA.CL.Commands
{
    class ExportDOCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        char separator = '\t';
        public bool CanExecute(object parameter)
        {
            return true;
           // throw new NotImplementedException();
        }

        public void Execute(object parameter)
        {
            System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            saveFileDialog.Filter = "текстовый файл с разделителями (.csv)|*.csv";
            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                System.IO.StreamWriter writer = new System.IO.StreamWriter(saveFileDialog.FileName, false, Encoding.Unicode);

                //CultureInfo culture = new CultureInfo("ru-RU");
                CultureInfo culture = new CultureInfo("en-US");

                writer.WriteLine("Вкл" + separator +
                                 "Индекс" + separator +
                                 "OPC тэг" + separator +
                                 "Адрес ModBus" + separator +
                                 "Бит" + separator +
                                 "Принуд." + separator +
                                 "Принуд. знач." + separator +
                                 "Значение" + separator +
                                 "Инвертировать" + separator +
                                 "Тэг" + separator +
                                 "Описание" + separator
                                 );
                if (DOStruct.items != null && DOStruct.items.Count() > 0)
                {
                    foreach (DOStruct dos in DOStruct.items)
                    {
                        writer.WriteLine(dos.En.ToString(culture) + separator +
                                            dos.indxArrDO.ToString(culture) + separator +
                                            dos.OPCtag.ToString(culture) + separator +
                                            dos.PLCAddr.ToString(culture) + separator +
                                            dos.indxBitDO.ToString(culture) + separator +
                                            dos.Forced.ToString(culture) + separator +
                                            dos.ForcedValue.ToString(culture) + separator +
                                            dos.ValDO.ToString(culture) + separator +
                                            dos.InvertDO.ToString(culture) + separator +
                                            dos.TegDO.ToString(culture) + separator +
                                            dos.NameDO.ToString(culture) + separator);
                    }
                }
                writer.Close();
                System.Windows.Forms.MessageBox.Show("Экспорт завершен");
            }//if

           
        }
    }
}
