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
    class ExportAICommand : ICommand
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
                if (AIStruct.items != null && AIStruct.items.Count() > 0)
                {
                    foreach (AIStruct ai in AIStruct.items)
                    {
                        writer.WriteLine(ai.En.ToString(culture) + separator +
                                            ai.indxAI.ToString(culture) + separator +
                                            ai.OPCtag.ToString(culture) + separator +
                                            ai.PLCAddr.ToString(culture) + separator +
                                            ai.PLCDestType.ToString() + separator +
                                            ai.Forced.ToString(culture) + separator +
                                            ai.ForcedValue.ToString(culture) + separator +
                                            ai.fValAI.ToString(culture) + separator +
                                            ai.minACD.ToString(culture) + separator +
                                            ai.maxACD.ToString(culture) + separator +
                                            ai.minPhis.ToString(culture) + separator +
                                            ai.maxPhis.ToString(culture) + separator +
                                            ai.TegAI.ToString(culture) + separator +
                                            ai.NameAI.ToString(culture)
                                            );
                    }

                }
                writer.Close();
                System.Windows.Forms.MessageBox.Show("Экспорт завершен");
            }//if ok

        }
    }
}
