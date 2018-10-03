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
    class ImportAOCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
            // throw new NotImplementedException();
        }

        public void Execute(object parameter)
        {
            AOStruct.EnableAutoIndex = false;
            if (MessageBox.Show("Стереть существующую таблицу AO?", "Подтверждение", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                AOStruct.items.Clear();
            }

            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
            ofd.Filter = "текстовый файл с разделителями (.csv)|*.csv";
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (ofd.FileName == null) return;

                System.IO.StreamReader reader = new System.IO.StreamReader(ofd.FileName);
                int count = 0;
                ReadTableAO(reader, out count);
            }//if OK

            AOStruct.EnableAutoIndex = true;
            System.Windows.Forms.MessageBox.Show("Импорт завершен");
        }

        void ReadTableAO(StreamReader reader, out int count)
        {
            count = 0;
            string line;
            //CultureInfo culture = new CultureInfo("ru-RU");
            CultureInfo culture = new CultureInfo("en-US");
          
            try
            {
                reader.ReadLine();//пропускаем строку с заголовками
                                  //    List<AOStruct> items = new List<AOStruct>();
                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine();

                  
                        string[] values = line.Split('\t');
                        if (values.Count() < 11)
                        {
                            continue;
                            //  System.Windows.Forms.MessageBox.Show("Ошибка чтения файла");
                        }
                        AOStruct item = new AOStruct();
                        item.En = bool.Parse(values[0]);
                        item.indx = int.Parse(values[1]);
                        item.OPCtag = values[2];
                        item.PLCAddr = int.Parse(values[3]);
                        item.PLCDestType = (EPLCDestType)Enum.Parse(typeof(EPLCDestType), values[4]);

                        item.Forced = bool.Parse(values[5]);
                        //item.ForcedValue = float.Parse(values[6], culture);
                        item.fVal = float.Parse(values[6], culture);
                        item.minACD = ushort.Parse(values[7], culture);
                        item.maxACD = ushort.Parse(values[8], culture);
                        item.minPhis = float.Parse(values[9], culture);
                        item.maxPhis = float.Parse(values[10], culture);


                        if (values.Length > 10)
                            item.TagName = values[11];

                        if (values.Length > 11)
                            item.Name = values[12];

                        //    items.Add(item);
                        AOStruct.items.Add(item);
                  
                }
                count = AOStruct.items.Count;

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
                System.Windows.Forms.MessageBox.Show("Ошибка импорта таблицы AO:\n\r" + ex.Message);
            }
        }

    }
}
