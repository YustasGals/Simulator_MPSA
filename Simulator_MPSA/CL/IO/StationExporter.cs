using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulator_MPSA.CL.IO
{
    public class StationExporter
    {
        /// <summary>
        /// разделитель таблиц
        /// </summary>
        string pageSeparator = "--EndPage--";
        /// <summary>
        /// разделитель списка аналогов
        /// </summary>
        string aiSeparator = "--EndOfAI--";

        string scriptSeparator = "---EndOfScript---";
        char separator='\t';
        char Separator
        {
            set { separator = value; }
        }
        /// <summary>
        /// экспорт в CSV - файл
        /// </summary>
        /// <param name="station"></param>
        /// <param name="filename"></param>
        public void exportCSV(Station station, string filename)
        {
            if (station == null || filename == null) return;

            System.IO.StreamWriter writer = new System.IO.StreamWriter(filename,false,Encoding.Unicode);
            CultureInfo culture = new CultureInfo("ru-RU");
            //CultureInfo ruProvider = new CultureInfo("ru-RU");
            //  System.Windows.Forms.MessageBox.Show("export");
            writer.WriteLine("------------------ Дискретные входы (DI)-------------------------------------");
            writer.WriteLine("Вкл." + separator + "Индекс" + separator + "OPC тэг" + separator +
                                "Адрес ModBus" + separator +
                                "Бит" + separator + "Принуд." + separator + "Принуд. знач" + separator +
                                "Значение" + separator + "Инвертировать" + separator + "Тэг" + separator + "Описание");
            if ((station.DIs != null) && (station.DIs.Count() > 0))
            {
            
                foreach (DIStruct di in station.DIs)
                    writer.WriteLine(   di.En.ToString(culture) +separator + 
                                        di.indxArrDI.ToString(culture) + separator + 
                                        di.OPCtag.ToString(culture) + separator+
                                        di.PLCAddr.ToString(culture) + separator +
                                        di.indxBitDI.ToString(culture) + separator +
                                        di.Forced.ToString(culture) + separator +
                                        di.ForcedValue.ToString(culture) + separator +
                                        di.ValDI.ToString(culture) + separator +
                                        di.InvertDI.ToString(culture) + separator +
                                        di.NameDI.ToString(culture));
                                
            }
            writer.WriteLine(pageSeparator);


            writer.WriteLine("------------------- Дискретные выходы (DO) ----------------------------------");
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
            if (station.DOs != null && station.DOs.Count()>0)
            {
                foreach (DOStruct dos in station.DOs)
                {
                    writer.WriteLine(   dos.En.ToString(culture) + separator+
                                        dos.indxArrDO.ToString(culture)+separator+
                                        dos.OPCtag.ToString(culture)+separator+
                                        dos.PLCAddr.ToString(culture)+separator+
                                        dos.indxBitDO.ToString(culture)+separator+
                                        dos.Forced.ToString(culture)+separator+
                                        dos.ForcedValue.ToString(culture)+separator+
                                        dos.ValDO.ToString(culture)+separator+
                                        dos.InvertDO.ToString(culture)+separator+
                                        dos.TegDO.ToString(culture)+separator+
                                        dos.NameDO.ToString(culture)+separator);
                }
            }
            writer.WriteLine(pageSeparator);

            writer.WriteLine("------------Аналоговые входы (AI) -----------------");

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
            if (station.AIs != null && station.AIs.Count() > 0)
            {                
                foreach (AIStruct ai in station.AIs)
                {
                    writer.WriteLine(   ai.En.ToString(culture)+separator+
                                        ai.indxAI.ToString(culture)+separator+
                                        ai.OPCtag.ToString(culture)+separator+
                                        ai.PLCAddr.ToString(culture)+separator+
                                        ai.PLCDestType.ToString()+separator+
                                        ai.Forced.ToString(culture) +separator+
                                        ai.ForcedValue.ToString(culture) +separator+
                                        ai.fValAI.ToString(culture) +separator+
                                        ai.minACD.ToString(culture) +separator+
                                        ai.maxACD.ToString(culture) +separator+
                                        ai.minPhis.ToString(culture) +separator+
                                        ai.maxPhis.ToString(culture) +separator+
                                        ai.TegAI.ToString(culture) +separator+
                                        ai.NameAI.ToString(culture)
                                        );
                }
                writer.WriteLine(pageSeparator);

                writer.WriteLine("----------- Настройки задвижек ----------------");
                writer.WriteLine("Вкл" + separator +
                                    "КВО" + separator +
                                    "КВЗ" + separator +
                                    "МПО" + separator +
                                    "МПЗ" + separator +
                                    "%открытия индекс" + separator +
                                    "авария привода" + separator +
                                    "время хода, сек" + separator +
                                    "Муфта" + separator +
                                    "состояине" + separator +
                                    "наличие напряжения" + separator +
                                    "Наличие напряжения на СШ" + separator +
                                    "команда - открыть" + separator +
                                    "команда - закрыть" + separator +
                                    "команда - остановить" + separator +
                                    "команда - стоп закрытия" + separator +
                                    "Наименование");
                if (station.ZDs != null && station.ZDs.Count() > 0)
                {
                 
                    foreach (ZDStruct zd in station.ZDs)
                    {
                        writer.WriteLine(   zd.En.ToString(culture)+separator+
                                            zd.OKCindxArrDI.ToString(culture) + separator +
                                            zd.CKCindxArrDI.ToString(culture) + separator +
                                            zd.ODCindxArrDI.ToString(culture) + separator +
                                            zd.CDCindxArrDI.ToString(culture) + separator +
                                            zd.ZD_Pos_index.ToString(culture) + separator +
                                            zd.OPCindxArrDI.ToString(culture) + separator +
                                            zd.TmoveZD.ToString(culture) + separator+
                                            zd.MCindxArrDI.ToString(culture) + separator+
                                            zd.StateZD.ToString() + separator +
                                            zd.VoltindxArrDI.ToString() + separator +
                                            zd.BSIndex.ToString(culture) + separator +
                                            zd.DOBindxArrDO.ToString() + separator +
                                            zd.DKBindxArrDO.ToString() + separator +
                                            zd.DCBindxArrDO.ToString() + separator +
                                            zd.DCBZindxArrDO.ToString() + separator + 
                                            zd.Description);
                    }                    
                }
                writer.WriteLine(pageSeparator);

                writer.WriteLine("------------ Настройки клапанов -----------");
                writer.WriteLine(   "Вкл." + separator+
                                    "Открыт" + separator+
                                    "Закрыт" + separator+
                                    "Открыть"+ separator+
                                    "Закрыть" + separator+
                                    "%открытия" + separator+
                                    "состояние" + separator+
                                    "Наименование");
                if (station.KLs != null && station.KLs.Count() > 0)
                {
                    foreach (KLStruct kl in station.KLs)
                    {
                        writer.WriteLine(   kl.En.ToString(culture) + separator +
                                            kl.OKCindxArrDI.ToString(culture) + separator + 
                                            kl.CKCindxArrDI.ToString(culture) + separator + 
                                            kl.DOBindxArrDO.ToString(culture)+ separator +
                                            kl.DKBindxArrDO.ToString(culture) + separator +
                                            kl.KLProc.ToString(culture) + separator +
                                            kl.State.ToString() + separator +
                                            kl.Description);
                    }
                }
                writer.WriteLine(pageSeparator);

                writer.WriteLine("--------- Настройки вспомсистем ---------------");
                writer.WriteLine(   "Вкл." + separator + "Команда-вкл" + separator +
                                    "Команда - откл." + separator + "Напряжение на СШ" + separator + 
                                    "Напряжение" + separator + "МП" + separator +
                                    "Состояине" + separator+ "АВОА" + separator + 
                                    "коэф задания частоты" + separator +
                                    "Адрес ModBus задания частоты" + separator +
                                    "Описание"
                                    );
                if (station.VSs != null && station.VSs.Count() > 0)
                {
                    foreach (VSStruct vs in station.VSs)
                    {
                        writer.WriteLine(   vs.En.ToString(culture) + separator +
                                            vs.ABBindxArrDO.ToString(culture) + separator +
                                            vs.ABOindxArrDO.ToString(culture) + separator +
                                            vs.BusSecIndex.ToString(culture) + separator +
                                            vs.ECindxArrDI.ToString(culture) + separator +
                                            vs.MPCindxArrDI.ToString(culture) + separator +
                                            vs.State.ToString() + separator + 
                                            vs.isAVOA.ToString() + separator +
                                            vs.SetRPM_Value.ToString() + separator+
                                            vs.SetRPM_Addr.ToString() + separator +
                                            vs.Description);
                        writer.WriteLine("--- VS AI ----");
                        writer.WriteLine("Индекс в таблице AI" + separator + "Номинальное значение"+ separator + "Интенсивность изменения");
                        if (vs.controledAIs != null && vs.controledAIs.Count() > 0)
                            foreach(AnalogIOItem io in vs.controledAIs)
                                writer.WriteLine(   io.Index.ToString(culture) + separator + 
                                                    io.ValueNom.ToString(culture) + separator + 
                                                    io.ValueSpd.ToString(culture));

                        writer.WriteLine(aiSeparator);
                    }
                }
                writer.WriteLine(pageSeparator);

                writer.WriteLine("---------------- настройки МПНА ------------------");
                writer.WriteLine("Вкл" + separator + "Исправность цепей вкл." + separator+
                                    "Исправность цепей выкл.1" + separator+ 
                                    "Исправность цепей выкл.2"+separator+
                                    "вв включен 1"+separator + "ВВ включен 2" + separator+
                                    "вв выключен 1" + separator + "ВВ выключен 2"+ separator+
                                    "команда - включить" + separator+
                                    "команда - откл.1"+ separator + "команда - откл.2"+separator+
                                    "Состояние" + separator+
                                    "ECx" + separator + "Наименование"
                                    );
                if (station.MPNAs != null && station.MPNAs.Count() > 0)
                    foreach (MPNAStruct mpna in station.MPNAs)
                    {
                        writer.WriteLine(   mpna.En.ToString(culture) + separator+
                                            mpna.ECBindxArrDI.ToString() + separator +
                                            mpna.ECO11indxArrDI.ToString(culture) + separator+
                                            mpna.ECO12indxArrDI.ToString(culture) + separator+
                                            mpna.MBC11indxArrDI.ToString(culture) + separator+
                                            mpna.MBC12indxArrDI.ToString(culture) + separator+
                                            mpna.MBC21indxArrDI.ToString(culture) +separator+
                                            mpna.MBC22indxArrDI.ToString(culture) + separator+
                                            mpna.ABBindxArrDO.ToString(culture) + separator+
                                            mpna.ABOindxArrDO.ToString(culture) + separator+
                                            mpna.ABO2indxArrDO.ToString(culture) + separator+
                                            mpna.State.ToString() + separator+
                                            mpna.ECxindxArrDI.ToString() + separator+
                                            mpna.Description
                                            );
                        writer.WriteLine("--- MPNA AI ----");
                        writer.WriteLine("Индекс в таблице AI" + separator + "Номинальное значение" + separator + "Интенсивность изменения");
                        if (mpna.controledAIs != null && mpna.controledAIs.Count() > 0)
                            foreach (AnalogIOItem io in mpna.controledAIs)
                                writer.WriteLine(io.Index.ToString(culture) + separator +
                                                    io.ValueNom.ToString(culture) + separator +
                                                    io.ValueSpd.ToString(culture));

                        writer.WriteLine(aiSeparator);
                    }

                writer.WriteLine(pageSeparator);

                writer.WriteLine("-------- Сигналы диагностики (DI) -------------");

                writer.WriteLine("Вкл" + separator +
                                    "Адрес ModBus" + separator +
                                    "Бит" + separator +
                                    "Описание");
                if (station.DiagSignals != null && station.DiagSignals.Count() > 0)
                {
                    foreach (DIStruct di in station.DiagSignals)
                    {
                        writer.WriteLine(di.En.ToString() + separator +
                                            di.PLCAddr.ToString() + separator +
                                            di.indxBitDI.ToString() + separator +
                                            di.NameDI);
                    }
                }
                writer.WriteLine(pageSeparator);

                writer.WriteLine("------ счетчики -------------");

                writer.WriteLine("Вкл."+separator+"адрес ModBus");

                if (station.Counters != null && station.Counters.Count() > 0)
                    foreach (USOCounter cnt in station.Counters)
                        writer.WriteLine(   cnt.En.ToString(culture) +separator +
                                            cnt.PLCAddr.ToString(culture));
                writer.WriteLine(pageSeparator);

                writer.WriteLine("------------скрипты ------------");
                if (station.scripts != null && station.scripts.Count()>0)
                    foreach(Scripting.ScriptInfo scr in station.scripts)
                    {
                        writer.WriteLine(scr.En.ToString() + separator + scr.Name + separator);
                        writer.Write(scr.ScriptTxt + Environment.NewLine);
                        writer.WriteLine(scriptSeparator);
                    }
                writer.WriteLine(pageSeparator);
            }//station != null



            writer.Close();
            System.Windows.Forms.MessageBox.Show("Экспорт завершен успешно");
        }
    }
}
