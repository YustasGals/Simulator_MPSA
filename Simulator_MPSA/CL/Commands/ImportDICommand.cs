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

namespace Simulator_MPSA.CL
{
    class ImportDICommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
            //throw new NotImplementedException();
        }

        public void Execute(object parameter)
        {
            DIStruct.EnableAutoIndex = false;

            if (MessageBox.Show("Стереть существующую таблицу DI?", "Подтверждение", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                DIStruct.items.Clear();
            }

            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
            ofd.Filter = "текстовый файл с разделителями (.csv)|*.csv";
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (ofd.FileName == null) return;

                System.IO.StreamReader reader = new System.IO.StreamReader(ofd.FileName);
                int count=0;
                ReadTableDI(reader, out count);
            }//if OK

            DIStruct.EnableAutoIndex = true;
            System.Windows.Forms.MessageBox.Show("Импорт завершен");
        }



        void ReadTableDI(StreamReader reader, out int count)
        {
            count = 0;
            string line;
            // CultureInfo culture = new CultureInfo("ru-RU");
            CultureInfo culture = new CultureInfo("en-US");
            //считываем страницу DI
            try
            {
                reader.ReadLine();//пропускаем строку с заголовками
                List<DIStruct> listDI = new List<DIStruct>();
                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine();

                 //   if (!line.Contains(pageSeparator))
               //     {
                        string[] values = line.Split('\t');
                        if (values.Count() < 11)
                        {
                            System.Windows.Forms.MessageBox.Show("Ошибка чтения файла");
                        }
                        DIStruct di = new DIStruct();
                        di.En = bool.Parse(values[0]);
                        di.indxArrDI = int.Parse(values[1]);
                        di.OPCtag = values[2];
                        di.PLCAddr = int.Parse(values[3]);
                        di.indxBitDI = int.Parse(values[4]);

                        di.Forced = bool.Parse(values[5]);
                        di.ForcedValue = bool.Parse(values[6]);
                        di.ValDI = bool.Parse(values[7]);
                        di.InvertDI = bool.Parse(values[8]);

                        if (values.Length > 9)
                            di.TegDI = values[9];

                        if (values.Length > 10)
                            di.NameDI = values[10];

                        listDI.Add(di);
               //     }
               //     else
               //     {                      
             //           break;
               //     }
                }

                count = listDI.Count;
              
              ///  DIStruct.items.Clear();
                foreach (DIStruct di in listDI)
                    DIStruct.items.Add(di);

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
                System.Windows.Forms.MessageBox.Show("Ошибка импорта таблицы DI:\n\r" + ex.Message);
            }
        }

    }






}
