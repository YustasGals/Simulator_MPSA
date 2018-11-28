using Simulator_MPSA.CL.Signal;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Forms;
namespace Simulator_MPSA.CL.Commands
{
    class ImportAICommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
            // throw new NotImplementedException();
        }

        public void Execute(object parameter)
        {
            AIStruct.EnableAutoIndex = false;

            if (MessageBox.Show("Стереть существующую таблицу AI?", "Подтверждение", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                AIStruct.items.Clear();
            }

            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
            ofd.Filter = "текстовый файл с разделителями (.csv)|*.csv";
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (ofd.FileName == null) return;

                System.IO.StreamReader reader = new System.IO.StreamReader(ofd.FileName);
              //  int count = 0;
                ReadTableAI(reader);
            }//if OK

            AIStruct.EnableAutoIndex = true;
            System.Windows.Forms.MessageBox.Show("Импорт завершен");
        }

        void ReadTableAI(StreamReader reader)
        {
            int Nline = 2;
            string line;
            //CultureInfo culture = new CultureInfo("ru-RU");
            CultureInfo culture = new CultureInfo("en-US");
            //считываем страницу DI
            try
            {
                reader.ReadLine();//пропускаем строку с заголовками
                List<AIStruct> items = new List<AIStruct>();
                while (!reader.EndOfStream)
                {
                    
                    line = reader.ReadLine();

                    
                        string[] values = line.Split('\t');
                        if (values.Count() < 11)
                        {
                            continue;
                            //  System.Windows.Forms.MessageBox.Show("Ошибка чтения файла");
                        }
                        AIStruct item = new AIStruct();
                        item.En = bool.Parse(values[0]);
                        item.indxAI = int.Parse(values[1]);
                        item.OPCtag = values[2];
                        item.PLCAddr = int.Parse(values[3]);
                        item.PLCDestType = (EPLCDestType)Enum.Parse(typeof(EPLCDestType), values[4]);

                        item.Forced = bool.Parse(values[5]);
                        item.ForcedValue = float.Parse(values[6], culture);
                        item.fValAI = float.Parse(values[7], culture);
                        item.minACD = ushort.Parse(values[8], culture);
                        item.maxACD = ushort.Parse(values[9], culture);
                        item.minPhis = float.Parse(values[10], culture);
                        item.maxPhis = float.Parse(values[11], culture);


                        if (values.Length > 11)
                            item.TegAI = values[12];

                        if (values.Length > 12)
                            item.NameAI = values[13];

                        items.Add(item);

                    Nline++;
                }
          
            //    Debug.WriteLine(count.ToString() + " successfuly parsed");
           //     AIStruct.items.Clear();
                foreach (AIStruct item in items)
                    AIStruct.items.Add(item);

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
                System.Windows.Forms.MessageBox.Show("Ошибка импорта таблицы AI в строке "+Nline.ToString() + ":\n\r" + ex.Message);
            }
        }

    }
}
