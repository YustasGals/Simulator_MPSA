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
    class ExportAOCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
          //  throw new NotImplementedException();
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

                writer.WriteLine("Вкл" + separator +
                                   "Индекс" + separator +
                                   "OPC тэг" + separator +
                                   "Адрес ModBus" + separator +
                                   "Тип" + separator +
                                   "Принудительно" + separator +
                                   "Принуд. значение" + separator +
                                   "Значение физ." + separator +
                                   "АЦП мин." + separator +
                                   "АЦП макс." + separator +
                                   "Физ мин." + separator +
                                   "Физ макс." + separator +
                                   "Тэг" + separator +
                                   "Описание"
                                   );

                if (AOStruct.items != null && AOStruct.items.Count() > 0)
                {
                    foreach (AOStruct item in AOStruct.items)
                    {
                        writer.WriteLine(item.En.ToString(culture) + separator +
                                            item.indx.ToString(culture) + separator +
                                            item.OPCtag.ToString(culture) + separator +
                                            item.PLCAddr.ToString(culture) + separator +
                                            item.PLCDestType.ToString() + separator +
                                            item.Forced.ToString(culture) + separator +
                                            //item.ForcedValue.ToString(culture) + separator +
                                            item.fVal.ToString(culture) + separator +
                                            item.minACD.ToString(culture) + separator +
                                            item.maxACD.ToString(culture) + separator +
                                            item.minPhis.ToString(culture) + separator +
                                            item.maxPhis.ToString(culture) + separator +
                                            item.TagName.ToString(culture) + separator +
                                            item.Name.ToString(culture)
                                            );
                    }

                }

                System.Windows.Forms.MessageBox.Show("Экспорт завершен");
            }//if ok

           
        }
    }
}
