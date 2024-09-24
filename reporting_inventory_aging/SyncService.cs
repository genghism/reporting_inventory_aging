using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.ServiceProcess;
using System.Threading;
using System.Configuration;

namespace reporting_inventory_aging
{
    public partial class SyncService : ServiceBase
    {
        Thread Worker;
        bool DoJob;
        Job job;
        List<Stock> stocks;
        List<Stock> existingStocks;
        List<Aging> agings;

        List<string> periods = new List<string>
        {
            "0_30",
            "31_60",
            "61_90",
            "91_120",
            "121_150",
            "151_180",
            "181_210",
            "211_240",
            "241_270",
            "271_300",
            "301_330",
            "331_360",
            "361_720",
            "721_1080",
            "1081_1800",
            "1801_plus"
        };

        public SyncService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            DoJob = true;
            Worker = new Thread(DoWork);
            Worker.Start();
        }

        protected override void OnStop()
        {
            DoJob = false;
            Worker.Join();
        }

        internal void TestStartupAndStop(string[] args)
        {
            OnStart(args);
            Console.ReadLine();
            OnStop();
        }

        private void DoWork(object arg)
        {
            while (DoJob)
            {
                try
                {
                    job = DbHandler.GetJob(ConfigurationManager.AppSettings.Get("job"));
                    if (job.Next_dt < DateTime.Now)
                    {
                        stocks = DbHandler.GetStocks();
                        existingStocks = DbHandler.GetExistingStocks();
                        agings = DbHandler.GetAgings();

                        foreach (Stock st in stocks)
                        {
                            Aging aging = agings.FirstOrDefault(a => a.material == st.Material);

                            aging.quantity_current = st.Quantity;
                            aging.weight_current = st.Weight;

                            decimal remainingQuantity = st.Quantity;
                            decimal remainingWeight = st.Weight;

                            for (int i = 0; i < periods.Count; i++)
                            {
                                string period = periods[i];

                                decimal periodQuantity = (decimal)aging.GetType().GetProperty($"quantity_{period}").GetValue(aging);
                                decimal adjustedQuantity = Math.Min(Math.Max(remainingQuantity, 0), periodQuantity);
                                aging.GetType().GetProperty($"quantity_{period}").SetValue(aging, adjustedQuantity);
                                remainingQuantity -= adjustedQuantity;

                                decimal periodWeight = (decimal)aging.GetType().GetProperty($"weight_{period}").GetValue(aging);
                                decimal adjustedWeight = Math.Min(Math.Max(remainingWeight, 0), periodWeight);
                                aging.GetType().GetProperty($"weight_{period}").SetValue(aging, adjustedWeight);
                                remainingWeight -= adjustedWeight;

                                // If both remaining quantity and weight are 0, stop
                                if (remainingQuantity <= 0 && remainingWeight <= 0)
                                {
                                    for (int j = i + 1; j < periods.Count; j++)
                                    {
                                        aging.GetType().GetProperty($"quantity_{periods[j]}").SetValue(aging, 0m);
                                        aging.GetType().GetProperty($"weight_{periods[j]}").SetValue(aging, 0m);
                                    }
                                    break;
                                }
                            }



                            if ((from est in existingStocks where est.Material == st.Material select est).Count() == 1)
                            {
                                DbHandler.ModifyReport(aging, "UPDATE");
                            }
                            else
                            {
                                DbHandler.ModifyReport(aging, "INSERT");
                            }
                        }


                        foreach (Stock est in existingStocks)
                        {
                            if ((from st in stocks where st.Material == est.Material select st).Count() < 1)
                            {
                                Aging aging = agings.FirstOrDefault(a => a.material == est.Material);

                                if (aging == null)
                                {
                                    var agingToBeDeleted = new Aging(est.Material, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
                                    DbHandler.ModifyReport(agingToBeDeleted, "DELETE");
                                }
                                else
                                {
                                    DbHandler.ModifyReport(aging, "DELETE");
                                }
                            }
                        }

                        DbHandler.UpdateJobStatus(job);
                    }


                    else
                    {
                        Thread.Sleep(1000);
                    }
                }
                catch (Exception ex)
                {
                    //Thread.Sleep(10000);
                    throw ex;
                }
            }
        }
    }
}
