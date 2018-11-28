using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Opc;
using OpcCom;
using OpcXml;
using Opc.Da;
using System.Diagnostics;
using Simulator_MPSA.CL.Signal;
namespace Simulator_MPSA.CL
{

    /// <summary>
    /// отдельный поток для чтения/записи данных по OPC, вычитываются теги DO
    /// </summary>
    class OPCThread
    {
        Thread thread;
        Opc.Da.Server srv;

        public MainWindow refMainWindow;

        List<Opc.Da.ItemValue> opcitems = new List<Opc.Da.ItemValue>();
        Opc.Da.ItemValue itm;

        int period;
        string fullServerName;
        int i;
        private DOStruct[] arrayDO;         //массив сигналов DO считываемых по OPC
        private AOStruct[] arrayAO;         //массив сигналов AO считываемых по OPC

        private Item[] opcDOItemsForRead;   //массив opc элементов соответствующих DO
        private Item[] opcAOItemsForRead;   //массив opc элементов соответствующих AO

        private ItemValueResult[] readResult; //массив результатов чтения opc элементов

        private bool isAbortRequested=false;  //требование остановить поток
        /// <summary>
        /// 
        /// </summary>
        /// <param name="serverName">Полное имя сервера</param>
        /// <param name="period">периодичность обмена</param>
        public OPCThread(string serverName, int period)
        {
            thread = new Thread(new ThreadStart(ThreadJob));
            fullServerName = serverName;
            this.period = period;

            List<Item> itm = new List<Item>();
            List<DOStruct> listDO = new List<DOStruct>();

            //получение списка сигналов DO которые будут читаться по OPC
            foreach (DOStruct d in DOStruct.items)
            {
                if (d.OPCtag != "" && d.En)
                {
                    listDO.Add(d);
                    itm.Add(new Item(new ItemIdentifier(d.OPCtag)));
                }
            }
           
            arrayDO = listDO.ToArray();            
            opcDOItemsForRead = itm.ToArray();

            itm = new List<Item>();
            //формирование списка сигналов AO для чтения по OPC 
            List<AOStruct> listAO = new List<AOStruct>();
            foreach (AOStruct item in AOStruct.items)
                if (item.OPCtag != "" && item.En)
                {
                    listAO.Add(item);
                    itm.Add(new Item(new ItemIdentifier(item.OPCtag)));
                }
            arrayAO = listAO.ToArray();
            opcAOItemsForRead = itm.ToArray();

            Debug.WriteLine("DO items: "+opcDOItemsForRead.Length.ToString());
            Debug.WriteLine("AO items: " + opcAOItemsForRead.Length.ToString());
        }


        void ThreadJob()
        {
            try
            {
                srv = new Opc.Da.Server(new OpcCom.Factory(), new Opc.URL(fullServerName));
                srv.Connect();
                if (!srv.IsConnected)
                {
                    throw new Exception("не удалось подключиться к серверу OPC");
                }
                    
            }
            catch (Exception ex)
            {
                if (refMainWindow != null)
                    refMainWindow.Dispatcher.Invoke(refMainWindow.ofsFail);
                return;
            }
            bool isFirstCycle = true;

            while (true)
            {
                //-------------- OPC ----------------
                opcitems.Clear();
                foreach (DIStruct di in DIStruct.items)
                    if ((di.OPCtag != "" && di.IsChanged && di.En)||(isFirstCycle))
                    {
                        itm = new Opc.Da.ItemValue(di.OPCtag);
                        itm.Value = di.ValDI ^ di.InvertDI;
                        opcitems.Add(itm);
                        di.IsChanged = false;
                    }

                foreach (AIStruct ai in AIStruct.items)
                    if ((ai.OPCtag != "" && ai.IsChanged && ai.En)|| (isFirstCycle))
                    {
                        itm = new Opc.Da.ItemValue(ai.OPCtag);
                        if (ai.PLCDestType == EPLCDestType.Float)
                            itm.Value = ai.fValAI;
                        else
                            itm.Value = ai.ValACD;

                        opcitems.Add(itm);
                        ai.IsChanged = false;
                    }
                try
                {
                    // Записываем сигналы DI AI
                    srv.Write(opcitems.ToArray());

                    //читаем сигналы DO для которых задан тег
                    if (opcDOItemsForRead.Length > 0)
                    {
                        readResult = srv.Read(opcDOItemsForRead);

                        for (int i = 0; i < readResult.Length; i++)
                        {
                            try
                            {
                                arrayDO[i].ValDO = (bool)readResult[i].Value;
                            }
                            catch (Exception ex)
                            {
                            }
                        }
                    }

                    //читаем сигналы AO для которых задан тег
                    if (opcAOItemsForRead.Length > 0)
                    {
                        readResult = srv.Read(opcAOItemsForRead);
                        for (int i = 0; i < readResult.Length; i++)
                        {
                            try
                            {
                                if (arrayAO[i].PLCDestType == EPLCDestType.ADC)
                                {
                                    object val = readResult[i].Value;
                                    if (val is short)
                                         arrayAO[i].ValACD = (ushort)((Int16)readResult[i].Value);
                                    if (val is ushort)
                                         arrayAO[i].ValACD = (UInt16)readResult[i].Value;
                                }                                    
                                else
                                    arrayAO[i].fVal = (float)readResult[i].Value;
                            }
                            catch (Exception ex)
                            {
                            }
                        }
                        Debug.WriteLine(opcAOItemsForRead.Length.ToString() + " AO tags read");
                    }
                }
                catch (ThreadAbortException abEx)
                {
                    //     Debug.WriteLine("data wasn't write cause thread aborted");
                }
                catch (Exception ex)
                {
                    //System.Windows.Forms.MessageBox.Show("OPC Thread exception:\n\r" + ex.Message);
                    //  LogWriter.AppendLog("Чтение по OPC прервано"+Environment.NewLine);
                    if (refMainWindow != null)
                        refMainWindow.Dispatcher.Invoke(refMainWindow.ofsFail);
                }
                isFirstCycle = false;
                Thread.Sleep(period);
                if (isAbortRequested) return;
            }//loop
        }

        public void Start()
        {
            if (thread != null)
            thread.Start();
        }

        public void Stop()
        {
            if (thread != null)
                isAbortRequested = true;
         //       thread.Abort();

        }
    }
}
