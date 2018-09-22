using Simulator_MPSA.CL.Signal;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Simulator_MPSA.CL.Commands
{
    class ExportDICommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }
        char separator = '\t';
        public void Execute(object parameter)
        {
            System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            saveFileDialog.Filter = "текстовый файл с разделителями (.csv)|*.csv";
            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                System.IO.StreamWriter writer = new System.IO.StreamWriter(saveFileDialog.FileName, false, Encoding.Unicode);

                //CultureInfo culture = new CultureInfo("ru-RU");
                CultureInfo culture = new CultureInfo("en-US");
                //CultureInfo ruProvider = new CultureInfo("ru-RU");
                //  System.Windows.Forms.MessageBox.Show("export");
                //   writer.WriteLine(pageHeaderDI);
                writer.WriteLine("Вкл." + separator + "Индекс" + separator + "OPC тэг" + separator +
                                    "Адрес ModBus" + separator +
                                    "Бит" + separator + "Принуд." + separator + "Принуд. знач" + separator +
                                    "Значение" + separator + "Инвертировать" + separator + "Тэг" + separator + "Описание");
                if ((DIStruct.items != null) && (DIStruct.items.Count() > 0))
                {

                    foreach (DIStruct di in DIStruct.items)
                        writer.WriteLine(di.En.ToString(culture) + separator +
                                            di.indxArrDI.ToString(culture) + separator +
                                            di.OPCtag.ToString(culture) + separator +
                                            di.PLCAddr.ToString(culture) + separator +
                                            di.indxBitDI.ToString(culture) + separator +
                                            di.Forced.ToString(culture) + separator +
                                            di.ForcedValue.ToString(culture) + separator +
                                            di.ValDI.ToString(culture) + separator +
                                            di.InvertDI.ToString(culture) + separator +
                                            di.TegDI.ToString(culture) + separator +
                                            di.NameDI.ToString(culture));

                }
                writer.Close();
                System.Windows.Forms.MessageBox.Show("Экспорт завершен");
            }//if

           
        }
    }
}
