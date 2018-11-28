using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.OleDb;
using System.Data;
using Simulator_MPSA.CL;
using Simulator_MPSA.CL.Signal;
namespace Simulator_MPSA
{
    class ExcelImporter
    {
        public static string GetConnectionString(string filename)
        {
            Dictionary<string, string> props = new Dictionary<string, string>();

            // XLSX - Excel 2007, 2010, 2012, 2013
            props["Provider"] = "Microsoft.ACE.OLEDB.12.0;";
            props["Extended Properties"] = "Excel 12.0 XML";
            props["Data Source"] = filename;

            // XLS - Excel 2003 and Older
            //props["Provider"] = "Microsoft.Jet.OLEDB.4.0";
            //props["Extended Properties"] = "Excel 8.0";
            //props["Data Source"] = "C:\\MyExcel.xls";

            StringBuilder sb = new StringBuilder();

            foreach (KeyValuePair<string, string> prop in props)
            {
                sb.Append(prop.Key);
                sb.Append('=');
                sb.Append(prop.Value);
                sb.Append(';');
            }

            return sb.ToString();
        }

        public static void ReadExcelFile(string connectionString)
        {
            DataSet ds = new DataSet();

            //   string connectionString = connectionString;

                using (OleDbConnection conn = new OleDbConnection(connectionString))
                {
                try
                {
                    conn.Open();
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show("Ошибка подключения:" + Environment.NewLine + ex.Message);
                    return;
                }

                    ReadSignals(conn);
                    ReadZD(conn);
                    ReadVS(conn);
                    ReadKL(conn);


                    conn.Close();
                    System.Windows.Forms.MessageBox.Show("импорт завершен");
                }
 

        //    return ds;
        }
        private static void ReadSignals(OleDbConnection conn)
        {
            OleDbCommand cmd = new OleDbCommand();
            cmd.Connection = conn;

            // Get all Sheets in Excel File
            DataTable dtSheet = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);

            // Loop through all Sheets to get data
            string sheetName = "";

            //==================================================== ЧТЕНИЕ ТАБЛИЦЫ СИГНАЛОВ ==========================================================
            foreach (DataRow dr in dtSheet.Rows)
            {
                sheetName = dr["TABLE_NAME"].ToString();
                if (sheetName.Contains("Таблица сигналов"))
                {
                    //LogViewModel.WriteLine("Таблица сигналов найдена, чтение данных...");
                    LogWriter.AppendLog("Таблица сигналов найдена, чтение данных...");
                    break;
                }
            }

            // Get all rows from the Sheet
            cmd.CommandText = "SELECT * FROM [" + sheetName + "]";

            DataTable dt = new DataTable();
            dt.TableName = sheetName;

            OleDbDataAdapter da = new OleDbDataAdapter(cmd);
            da.Fill(dt);

            //       ds.Tables.Add(dt);

            foreach (DataColumn col in dt.Columns)
                LogWriter.AppendLog(col.ColumnName);
                //LogViewModel.WriteLine(col.ColumnName);

            //--- тип сигнала ai,di,do ---
            string signalType = "";
            //---адрес сигнала ---
            double currentAddr = 0;
            //---- номер бита если попадется
            int bit = 0;
            //----
            string tagName = "";
            string signalName = "";

            //--- номер сигнала, пригодится---
            int NSign = 0;

            for (int i = 0; i < dt.Rows.Count; i++)
            {

                //  mod =(string)dt.Rows[i].ItemArray[1];
                if (!(dt.Rows[i].ItemArray[1] is DBNull))
                    signalType = (string)dt.Rows[i].ItemArray[1];

                // addr = (string)dt.Rows[i].ItemArray[6];
                // if (!(dt.Rows[i].ItemArray[6] is DBNull))
                if (dt.Rows[i].ItemArray[6] is double)
                    currentAddr = (double)dt.Rows[i].ItemArray[6];
                else if (dt.Rows[i].ItemArray[6] is string)
                    currentAddr = double.Parse((string)dt.Rows[i].ItemArray[6]);

                try
                {
                    if (dt.Rows[i].ItemArray[7] is string)

                        bit = int.Parse((string)dt.Rows[i].ItemArray[7]);
                    else if (dt.Rows[i].ItemArray[7] is double)
                        bit = (int)((double)dt.Rows[i].ItemArray[7]);
                    else bit = 0;

                    if (dt.Rows[i].ItemArray[8] is string)
                        NSign = int.Parse((string)dt.Rows[i].ItemArray[8]);
                    else if (dt.Rows[i].ItemArray[8] is double)
                        NSign = (int)((double)dt.Rows[i].ItemArray[8]);
                    else NSign = 0;
                }
                catch (Exception ex)
                {
                }

                try
                {
                    tagName = (string)dt.Rows[i].ItemArray[4];
                }
                catch (Exception ex)
                {
                    tagName = "";
                }

                try
                {
                    signalName = (string)dt.Rows[i].ItemArray[5];
                }
                catch (Exception ex)
                {
                    signalName = "";
                }

                if (signalType.ToLower() == "ai" && signalName != "")
                {
                    AIStruct ai = new AIStruct();
                    ai.En = true;
                    ai.NameAI = signalName;
                    ai.TegAI = tagName;
                    ai.PLCAddr = (int)currentAddr - 300000 + 40000;
                    AIStruct.items.Add(ai);
                    //LogViewModel.WriteLine("прочитан сигнал AI: " + ai.NameAI);

                }

                if (signalType.ToLower() == "di" && signalName != "")
                {
                    DIStruct di = new DIStruct();
                    di.En = true;

                    di.NameDI = signalName;
                    di.TegDI = tagName;

                    di.PLCAddr = (int)currentAddr - 300000 + 40000;

                    di.indxBitDI = bit;

                    di.Nsign = NSign;
                    DIStruct.items.Add(di);
                }

                if (signalType.ToLower() == "do" && signalName != "")
                {
                    DOStruct _do = new DOStruct();
                    _do.En = true;
                    _do.TegDO = tagName;
                    _do.NameDO = signalName;

                    _do.PLCAddr = (int)currentAddr-400000;
                    _do.indxBitDO = bit;
                    _do.Nsign = NSign;
                    DOStruct.items.Add(_do);
                }
            }
            cmd = null;
        }

        /// <summary>
        /// чтение настроек задвижек
        /// </summary>
        /// <param name="conn"></param>
        private static void ReadZD(OleDbConnection conn)
        {
            OleDbCommand cmd = new OleDbCommand();
            cmd.Connection = conn;

            // Get all Sheets in Excel File
            DataTable dtSheet = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);


            //======================================================================= чтение таблицы DI задвижек =====================================
            string sheetName = "";
            foreach (DataRow dr in dtSheet.Rows)
            {
                sheetName = dr["TABLE_NAME"].ToString();
                if (sheetName.Contains("DI задвижек"))
                {
                    //LogViewModel.WriteLine("Таблица задвижек найдена, чтение данных...");
                    LogWriter.AppendLog("Таблица задвижек найдена, чтение данных...");
                    break;
                }
            }
            // Get all rows from the Sheet
            cmd.CommandText = "SELECT * FROM [" + sheetName + "]";

            DataTable dt = new DataTable();
            dt.TableName = sheetName;

            OleDbDataAdapter da = new OleDbDataAdapter(cmd);

            ZDTableViewModel.ZDs.Clear();
            da.Fill(dt);
            object[] zdparams = new object[7];
            //=================
            for (int i = 0; i < dt.Rows.Count / 8; i++)
            {
                int rowindex = i * 8 + 4;
                ZDStruct zd;
                if (dt.Rows[rowindex].ItemArray[0] is DBNull) //название задвижки пустое, прекращаем чтение
                {
                    break;
                }
                else
                {
                    zd = new ZDStruct();
                    zd.Description = (string)dt.Rows[rowindex].ItemArray[0];
                    if (zd.Description.Length < 2) break;
                    zd.En = true;
                    ZDTableViewModel.ZDs.Add(zd);
                }

                for (int j = 0; j < 8; j++)
                {
                    object zdParam = dt.Rows[rowindex + j].ItemArray[8];
                    int iParam = 0;

                    if (zdParam is string)
                        iParam = int.Parse((string)(zdParam));
                    else if (zdParam is double)
                        iParam = (int)((double)zdParam);


                    if (iParam == 0) continue;

                    switch (j)
                    {
                        case 0: zd.OKCindxArrDI = DIStruct.FindByNsign(iParam + 1000); break; //КВО
                        case 1: zd.CKCindxArrDI = DIStruct.FindByNsign(iParam + 1000); break; //КВЗ
                        case 2: zd.ODCindxArrDI = DIStruct.FindByNsign(iParam + 1000); break; //МПО
                        case 3: zd.CDCindxArrDI = DIStruct.FindByNsign(iParam + 1000); break; //МПЗ
                        case 4: zd.DCindxArrDI = DIStruct.FindByNsign(iParam + 1000); break; //Дист
                        case 5: zd.VoltindxArrDI = DIStruct.FindByNsign(iParam + 1000); break; //напряж
                        case 6: zd.MCindxArrDI = DIStruct.FindByNsign(iParam + 1000); break; //муфта
                        case 7: zd.OPCindxArrDI = DIStruct.FindByNsign(iParam + 1000); break; //авария
                    }
                }
            }


            foreach (DataRow dr in dtSheet.Rows)
            {
                sheetName = dr["TABLE_NAME"].ToString();
                if (sheetName.Contains("DO задвижек"))
                {
                    //LogViewModel.WriteLine("Таблица DO задвижек найдена, чтение данных...");
                    LogWriter.AppendLog("Таблица DO задвижек найдена, чтение данных...");
                    break;
                }
            }

            // Get all rows from the Sheet
            cmd.CommandText = "SELECT * FROM [" + sheetName + "]";

            dt = new DataTable();
            dt.TableName = sheetName;

            //OleDbDataAdapter da = new OleDbDataAdapter(cmd);

            da.Fill(dt);

            for (int i = 0; i < ZDTableViewModel.ZDs.Count; i++)
            {
                int rowindex = i * 4 + 4;

                for (int j = 0; j < 4; j++)
                {
                    object zdParam = dt.Rows[rowindex + j].ItemArray[5];
                    int iParam = 0;

                    if (zdParam is string)
                        iParam = int.Parse((string)(zdParam));
                    else if (zdParam is double)
                        iParam = (int)((double)zdParam);


                    if (iParam == 0) continue;

                    switch (j)
                    {
                        case 0: ZDTableViewModel.ZDs[i].DOBindxArrDO = DOStruct.FindByNsign(iParam + 4000); break; //открыть
                        case 1: ZDTableViewModel.ZDs[i].DKBindxArrDO = DOStruct.FindByNsign(iParam + 4000); break;  //закрыть
                        case 2: ZDTableViewModel.ZDs[i].DCBindxArrDO = DOStruct.FindByNsign(iParam + 4000); break;  //стоп
                        case 3: ZDTableViewModel.ZDs[i].DCBZindxArrDO = DOStruct.FindByNsign(iParam + 4000); break;  //стоп закрытия                
                    }
                }

            }


        }//readzd
        /// <summary>
        /// чтение настроек вспомсистем
        /// </summary>
        /// <param name="conn"></param>
        private static void ReadVS(OleDbConnection conn)
        {
            //---------- настройки чтения ------
            int skipLines = 4; //количество пропускаемых строк заголовка
            int nNameCol = 0; //номер столбца с именем

            int nDICol = 8; //номер столбца со ссылками на DI
            int strideDI = 3; //количество строк на одну вспомку
            int shiftDIlink = 1000; //ссылки на DI имеют смещение на 1000

            int nDOCol = 5; //номер столбца со ссылками на DO
            int strideDO = 2; //количество строк на одну вспомку
            int shiftDOlink = 4000; //ссылки на DO смещаются на 4000

            //наименования используемых таблиц
            string vsDITableName = "DI вспомсистем";
            string vsDOTableName = "DO вспомсистем";

            OleDbCommand cmd = new OleDbCommand();
            cmd.Connection = conn;

            // Get all Sheets in Excel File
            DataTable dtSheet = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);


            //======================================================================= чтение таблицы DI задвижек =====================================
            string sheetName = "";
            foreach (DataRow dr in dtSheet.Rows)
            {
                sheetName = dr["TABLE_NAME"].ToString();
                if (sheetName.Contains(vsDITableName))
                {
                    //LogViewModel.WriteLine("Таблица DI вспомсистем найдена, чтение данных...");
                    LogWriter.AppendLog("Таблица DI вспомсистем найдена, чтение данных...");
                    break;
                }
            }
            // Get all rows from the Sheet
            cmd.CommandText = "SELECT * FROM [" + sheetName + "]";

            DataTable dataTable = new DataTable();
            dataTable.TableName = sheetName;

            OleDbDataAdapter dataAdapter = new OleDbDataAdapter(cmd);

            dataAdapter.Fill(dataTable);

            VSTableViewModel.VS.Clear();
            //=================
            for (int i = 0; i < dataTable.Rows.Count / strideDI; i++)
            {
                int rowindex = i * strideDI + skipLines;
                VSStruct vs;
                if (dataTable.Rows[rowindex].ItemArray[nNameCol] is DBNull) //название пустое, прекращаем чтение
                {
                    break;
                }
                else
                {
                    vs = new VSStruct();
                    vs.Description = (string)dataTable.Rows[rowindex].ItemArray[0];
                    if (vs.Description.Length < 2) break;
                    vs.En = true;
                    VSTableViewModel.VS.Add(vs);
                }

                for (int j = 0; j < strideDI; j++)
                {
                    object param = dataTable.Rows[rowindex + j].ItemArray[nDICol];
                    int iParam = 0;

                    if (param is string)
                        iParam = int.Parse((string)(param));
                    else if (param is double)
                        iParam = (int)((double)param);


                    if (iParam == 0) continue;

                    switch (j)
                    {
                        case 0: vs.ECindxArrDI = DIStruct.FindByNsign(iParam + shiftDIlink); break; //EC
                        case 1: vs.MPCindxArrDI = DIStruct.FindByNsign(iParam + shiftDIlink); break; //MPC
                        case 2: vs.PCindxArrDI = DIStruct.FindByNsign(iParam + shiftDIlink); break; //PC
                    }
                }
            }


            foreach (DataRow dr in dtSheet.Rows)
            {
                sheetName = dr["TABLE_NAME"].ToString();
                if (sheetName.Contains(vsDOTableName))
                {
                    //LogViewModel.WriteLine("Таблица DO вспомсистем найдена, чтение данных...");
                    LogWriter.AppendLog("Таблица DO вспомсистем найдена, чтение данных...");
                    break;
                }
            }

            // Get all rows from the Sheet
            cmd.CommandText = "SELECT * FROM [" + sheetName + "]";

            dataTable = new DataTable();
            dataTable.TableName = sheetName;
            dataAdapter.Fill(dataTable);

           

            for (int i = 0; i < VSTableViewModel.VS.Count; i++)
            {
                int rowindex = i * strideDO + skipLines;

                for (int j = 0; j < strideDO; j++)
                {
                    object param = dataTable.Rows[rowindex + j].ItemArray[nDOCol];
                    int iParam = 0;

                    if (param is string)
                        iParam = int.Parse((string)(param));
                    else if (param is double)
                        iParam = (int)((double)param);


                    if (iParam == 0) continue;

                    switch (j)
                    {
                        case 0: VSTableViewModel.VS[i].ABBindxArrDO = DOStruct.FindByNsign(iParam + shiftDOlink); break; //вкл
                        case 1: VSTableViewModel.VS[i].ABOindxArrDO = DOStruct.FindByNsign(iParam + shiftDOlink); break; //выкл

                    }
                }

            }


        }//readvs

        /// <summary>
        /// чтение настроек клапанов
        /// </summary>
        /// <param name="conn"></param>
        private static void ReadKL(OleDbConnection conn)
        {
            //---------- настройки чтения ------
            int skipLines = 4; //количество пропускаемых строк заголовка
            int nNameCol = 0; //номер столбца с именем

            int nDICol = 8; //номер столбца со ссылками на DI
            int strideDI = 2; //количество строк на однин клапан
            int shiftDIlink = 1000; //ссылки на DI имеют смещение на 1000

            int nDOCol = 5; //номер столбца со ссылками на DO
            int strideDO = 2; //количество строк на однин клпан
            int shiftDOlink = 4000; //ссылки на DO смещаются на 4000

            //наименования используемых таблиц
            string tableName = "Настр. клапанов";
            string DITableName = "DI клапанов";
            string DOTableName = "DO клапанов";

            OleDbCommand cmd = new OleDbCommand();
            cmd.Connection = conn;

            // Get all Sheets in Excel File
            DataTable dtSheet = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);


            //======================================================================= чтение таблицы DI задвижек =====================================
            string sheetName = "";
            foreach (DataRow dr in dtSheet.Rows)
            {
                sheetName = dr["TABLE_NAME"].ToString();
                if (sheetName.Contains(DITableName))
                {
                    //LogViewModel.WriteLine("Таблица DI вспомсистем найдена, чтение данных...");
                    LogWriter.AppendLog("Таблица DI вспомсистем найдена, чтение данных...");
                    break;
                }
            }
            // Get all rows from the Sheet
            cmd.CommandText = "SELECT * FROM [" + sheetName + "]";

            DataTable dataTable = new DataTable();
            dataTable.TableName = sheetName;

            OleDbDataAdapter dataAdapter = new OleDbDataAdapter(cmd);

            dataAdapter.Fill(dataTable);

            KLTableViewModel.KL.Clear();
            //=================
            for (int i = 0; i < dataTable.Rows.Count / strideDI; i++)
            {
                int rowindex = i * strideDI + skipLines;
                KLStruct kl;
                if (dataTable.Rows[rowindex].ItemArray[nNameCol] is DBNull) //название пустое, прекращаем чтение
                {
                    break;
                }
                else
                {
                    kl = new KLStruct();
                    kl.Description = (string)dataTable.Rows[rowindex].ItemArray[0];
                    if (kl.Description.Length < 2) break;
                    kl.En = true;
                    KLTableViewModel.KL.Add(kl);
                }

                for (int j = 0; j < strideDI; j++)
                {
                    object param = dataTable.Rows[rowindex + j].ItemArray[nDICol];
                    int iParam = 0;

                    if (param is string)
                        iParam = int.Parse((string)(param));
                    else if (param is double)
                        iParam = (int)((double)param);


                    if (iParam == 0) continue;
                    int refValue = DIStruct.FindByNsign(iParam + shiftDIlink);
                    switch (j)
                    {
                        case 0: kl.OKCindxArrDI = refValue;  break; //клапан открыт
                        case 1: kl.CKCindxArrDI = refValue; break; //клапан закрыт
                            
                    }
                }
            }


            foreach (DataRow dr in dtSheet.Rows)
            {
                sheetName = dr["TABLE_NAME"].ToString();
                if (sheetName.Contains(DOTableName))
                {
                    //LogViewModel.WriteLine("Таблица DO вспомсистем найдена, чтение данных...");
                    LogWriter.AppendLog("Таблица DO вспомсистем найдена, чтение данных...");
                    break;
                }
            }

            // Get all rows from the Sheet
            cmd.CommandText = "SELECT * FROM [" + sheetName + "]";

            dataTable = new DataTable();
            dataTable.TableName = sheetName;
            dataAdapter.Fill(dataTable);



            for (int i = 0; i < KLTableViewModel.KL.Count; i++)
            {
                int rowindex = i * strideDO + skipLines;

                for (int j = 0; j < strideDO; j++)
                {
                    object param = dataTable.Rows[rowindex + j].ItemArray[nDOCol];
                    int iParam = 0;

                    if (param is string)
                        iParam = int.Parse((string)(param));
                    else if (param is double)
                        iParam = (int)((double)param);


                    if (iParam == 0) continue;
                    int refValue = DOStruct.FindByNsign(iParam + shiftDOlink);
                    switch (j)
                    {
                        case 0: KLTableViewModel.KL[i].DOBindxArrDO = refValue;  break; //открыть
                        case 1: KLTableViewModel.KL[i].DKBindxArrDO = refValue; break; //закрыть                       
                    }
                }

            }


        }//readkl
    }
}
