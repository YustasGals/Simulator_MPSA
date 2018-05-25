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

namespace Simulator_MPSA.CL
{

    /// <summary>
    /// отдельный поток для чтения/записи данных по OPC, вычитываются теги DO
    /// </summary>
    class OPCThread
    {
        Thread thread;
        Opc.Da.Server srv;

        List<Opc.Da.ItemValue> opcitems = new List<Opc.Da.ItemValue>();
        Opc.Da.ItemValue itm;

        int period;
        string fullServerName;
        int i;
        private DOStruct[] arrayDO;         //массив сигналов DO считываемых по OPC
        private Item[] opcDOItemsForRead;   //массив opc элементов соответствующих DO
        private ItemValueResult[] readResult; //массив результатов чтения opc элементов
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
                if (d.OPCtag != "")
                    listDO.Add(d);
            }

            arrayDO = listDO.ToArray();

            foreach (DOStruct d in listDO)
            {
                itm.Add(new Item(new ItemIdentifier(d.OPCtag)));
            }
            opcDOItemsForRead = itm.ToArray();
        }


        void ThreadJob()
        {
            try
            {
                srv = new Opc.Da.Server(new OpcCom.Factory(), new Opc.URL(fullServerName));
                srv.Connect();
                if (!srv.IsConnected)
                    throw new Exception("не удалось подключиться к серверу OPC");
            }
            catch (Exception ex)
            {
                //
                return;
            }
            while (true)
            {
                //-------------- OPC ----------------
                opcitems.Clear();
                foreach (DIStruct di in DIStruct.items)
                    if (di.OPCtag != "" && di.IsChanged)
                    {
                        itm = new Opc.Da.ItemValue(di.OPCtag);
                        itm.Value = di.ValDI ^ di.InvertDI;
                        opcitems.Add(itm);
                        di.IsChanged = false;
                    }

                foreach (AIStruct ai in AIStruct.items)
                    if (ai.OPCtag != "" && ai.IsChanged)
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
                    srv.Write(opcitems.ToArray());
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
                catch (ThreadAbortException abEx)
                {
                    //     Debug.WriteLine("data wasn't write cause thread aborted");
                }

                Thread.Sleep(period);
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
                thread.Abort();

        }
    }
}
