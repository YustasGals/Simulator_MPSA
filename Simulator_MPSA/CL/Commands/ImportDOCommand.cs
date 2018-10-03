using Simulator_MPSA.CL.Signal;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace Simulator_MPSA.CL.Commands
{
    class ImportDOCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
            // throw new NotImplementedException();
        }

        public void Execute(object parameter)
        {
            DOStruct.EnableAutoIndex = false;

            if (MessageBox.Show("Стереть существующую таблицу DO?", "Подтверждение", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                DOStruct.items.Clear();
            }

            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
            ofd.Filter = "текстовый файл с разделителями (.csv)|*.csv";
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (ofd.FileName == null) return;

                System.IO.StreamReader reader = new System.IO.StreamReader(ofd.FileName);
                int count = 0;
                ReadTableDO(reader, out count);
            }//if OK

            DOStruct.EnableAutoIndex = true;
            System.Windows.Forms.MessageBox.Show("Импорт завершен");
        }

        void ReadTableDO(StreamReader reader, out int count)
        {
            count = 0;
            string line = "";
            CultureInfo culture = new CultureInfo("ru-RU");
            //CultureInfo culture = new CultureInfo("en-US");
            //считываем страницу DI
            try
            {
                line = reader.ReadLine();//пропускаем строку с заголовками
                List<DOStruct> items = new List<DOStruct>();
                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine();

                   
                        string[] values = line.Split('\t');
                        if (values.Count() < 10)
                        {
                            //  System.Windows.Forms.MessageBox.Show("Ошибка чтения файла");
                            //throw new Exception("недостаточно параметров в строке ("+values.Count()+")");
                            continue;
                        }
                        DOStruct item = new DOStruct();
                        item.En = bool.Parse(values[0]);
                        item.indxArrDO = int.Parse(values[1]);
                        item.OPCtag = values[2];
                        item.PLCAddr = int.Parse(values[3]);
                        item.indxBitDO = int.Parse(values[4]);

                        item.Forced = bool.Parse(values[5]);
                        item.ForcedValue = bool.Parse(values[6]);
                        item.ValDO = bool.Parse(values[7]);
                        item.InvertDO = bool.Parse(values[8]);

                        if (values.Length > 9)
                            item.TegDO = values[9];

                        if (values.Length > 10)
                            item.NameDO = values[10];

                        items.Add(item);
                  
                }

                count = items.Count;
             
             //   DOStruct.items.Clear();
                foreach (DOStruct item in items)
                    DOStruct.items.Add(item);

                //DITableViewModel.Instance.Init(DIStruct.items);
                //    dataGridDI.ItemsSource = DITableViewModel.Instance.viewSource.View;
                //обновление ссылок
                foreach (KLStruct kl in KLTableViewModel.KL)
                    kl.UpdateRefs();

                foreach (ZDStruct zd in ZDTableViewModel.ZDs)
                    zd.UpdateRefs();

                foreach (VSStruct vs in VSTableViewModel.VS)
                    vs.UpdateRefs();

                foreach (MPNAStruct mpna in MPNATableViewModel.MPNAs)
                    mpna.UpdateRefs();

                foreach (USOCounter counter in CountersTableViewModel.Counters)
                    counter.Refresh();

                foreach (WatchItem item in WatchItem.Items)
                    item.RefreshLink();
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("Ошибка импорта таблицы DO:\n\r" + ex.Message + "\n\r" + line);
                reader.Dispose();
            }
        }

    }
}
