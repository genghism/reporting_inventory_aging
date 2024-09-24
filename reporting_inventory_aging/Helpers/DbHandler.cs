using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Configuration;

namespace reporting_inventory_aging
{
    class DbHandler
    {
        public static Job GetJob(string job_name)
        {
            Job job = null;
            string queryForJob = @"select id,method,definition,status,period,last_dt,next_dt,log_path from jobs where status=0 and method='" + job_name + "'";
            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(ConfigurationManager.AppSettings.Get("connStringDwh")))
                {
                    using (SqlCommand sqlCommand = new SqlCommand(queryForJob, sqlConnection))
                    {
                        sqlConnection.Open();
                        using (SqlDataReader sqlDataReader = sqlCommand.ExecuteReader())
                        {
                            if (sqlDataReader.Read())
                            {
                                job = new Job(sqlDataReader.GetByte(sqlDataReader.GetOrdinal("id")), sqlDataReader.GetString(sqlDataReader.GetOrdinal("method")), sqlDataReader.GetString(sqlDataReader.GetOrdinal("definition")),
                                   sqlDataReader.GetByte(sqlDataReader.GetOrdinal("status")), sqlDataReader.GetInt32(sqlDataReader.GetOrdinal("period")), sqlDataReader.GetDateTime(sqlDataReader.GetOrdinal("last_dt")),
                                   sqlDataReader.GetDateTime(sqlDataReader.GetOrdinal("next_dt")), sqlDataReader.GetString(sqlDataReader.GetOrdinal("log_path")));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.LogError(ConfigurationManager.AppSettings.Get("logPath") + DateTime.Now.Year + "_" + DateTime.Now.Month + "_" + DateTime.Now.Day + ".txt", "DbHandler.GetJob", ex.Message != null ? ex.Message.ToString() : ex.ToString());
                throw ex;
            }
            return job;
        }

        public static void UpdateJobStatus(Job job)
        {
            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(ConfigurationManager.AppSettings.Get("connStringDwh")))
                {
                    SqlCommand sqlCommand = new SqlCommand(@"update jobs set next_dt=DATEADD(MINUTE,period,current_timestamp),last_dt=current_timestamp where id=" + job.Id, sqlConnection);
                    sqlCommand.Connection.Open();
                    if (sqlCommand.ExecuteNonQuery() != 1)
                    {
                        throw new Exception("Job Not Correctly Updated");
                    }
                    sqlCommand.Connection.Close();
                }
            }
            catch (Exception ex)
            {
                Logging.LogError(ConfigurationManager.AppSettings.Get("logPath") + DateTime.Now.Year + "_" + DateTime.Now.Month + "_" + DateTime.Now.Day + ".txt", "DbHandler.UpdateJobStatus", ex.Message != null ? ex.Message.ToString() : ex.ToString());
                throw ex;
            }
        }
        public static List<Stock> GetStocks()
        {
            List<Stock> stocks = new List<Stock>();
            string queryForStocks = @"SELECT II.MATERIAL AS material,
                                               SUM(II.StockKeepingQuantity * (1 - 2 * II.QPOSTWAY)) AS quantity,
                                               SUM(II.TheoreticWeight * (1 - 2 * II.QPOSTWAY)) AS weight
                                        FROM METAK803.DBO.IASINVITEM II
                                        WHERE II.CLIENT = '00'
                                          AND II.COMPANY = '01'
                                          AND II.ISCANCELED = 0
                                          AND II.WAREHOUSE != 'YOL'
                                          AND II.DocumentDate <= CURRENT_TIMESTAMP
                                        GROUP BY II.MATERIAL
                                        HAVING SUM(II.StockKeepingQuantity * (1 - 2 * II.QPOSTWAY)) >= 1
                                        ORDER BY II.MATERIAL";
            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(ConfigurationManager.AppSettings.Get("connStringErp")))
                {
                    using (SqlCommand sqlCommand = new SqlCommand(queryForStocks, sqlConnection))
                    {
                        sqlConnection.Open();
                        using (SqlDataReader sqlDataReader = sqlCommand.ExecuteReader())
                        {
                            while (sqlDataReader.Read())
                            {
                                stocks.Add(new Stock(
                                    sqlDataReader.GetString(sqlDataReader.GetOrdinal("material")),
                                    sqlDataReader.GetDecimal(sqlDataReader.GetOrdinal("quantity")),
                                    sqlDataReader.GetDecimal(sqlDataReader.GetOrdinal("weight"))));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.LogError(ConfigurationManager.AppSettings.Get("logPath") + DateTime.Now.Year + "_" + DateTime.Now.Month + "_" + DateTime.Now.Day + ".txt", "DbHandler.GetStocks", ex.Message != null ? ex.Message.ToString() : ex.ToString());
                throw ex;
            }
            return stocks;
        }


        public static List<Stock> GetExistingStocks()
        {
            List<Stock> stocks = new List<Stock>();
            string queryForStocks = @"SELECT material,quantity_current,weight_current FROM reporting_inventory_aging";
            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(ConfigurationManager.AppSettings.Get("connStringDwh")))
                {
                    using (SqlCommand sqlCommand = new SqlCommand(queryForStocks, sqlConnection))
                    {
                        sqlConnection.Open();
                        using (SqlDataReader sqlDataReader = sqlCommand.ExecuteReader())
                        {
                            while (sqlDataReader.Read())
                            {
                                stocks.Add(new Stock(
                                    sqlDataReader.GetString(sqlDataReader.GetOrdinal("material")),
                                    sqlDataReader.GetDecimal(sqlDataReader.GetOrdinal("quantity_current")),
                                    sqlDataReader.GetDecimal(sqlDataReader.GetOrdinal("weight_current"))));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.LogError(ConfigurationManager.AppSettings.Get("logPath") + DateTime.Now.Year + "_" + DateTime.Now.Month + "_" + DateTime.Now.Day + ".txt", "DbHandler.GetExistingStocks", ex.Message != null ? ex.Message.ToString() : ex.ToString());
                throw ex;
            }
            return stocks;
        }


        public static List<Aging> GetAgings()
        {
            List<Aging> agings = new List<Aging>();
            string queryForStocks = @"SELECT
                                        MATERIAL as material,
                                        SUM(CASE WHEN DocumentDate >= DATEADD(DAY, -30, CURRENT_TIMESTAMP) THEN StockKeepingQuantity ELSE 0 END) AS quantity_0_30,
                                        SUM(CASE WHEN DocumentDate >= DATEADD(DAY, -60, CURRENT_TIMESTAMP) AND DocumentDate < DATEADD(DAY, -30, CURRENT_TIMESTAMP) THEN StockKeepingQuantity ELSE 0 END) AS quantity_31_60,
                                        SUM(CASE WHEN DocumentDate >= DATEADD(DAY, -90, CURRENT_TIMESTAMP) AND DocumentDate < DATEADD(DAY, -60, CURRENT_TIMESTAMP) THEN StockKeepingQuantity ELSE 0 END) AS quantity_61_90,
                                        SUM(CASE WHEN DocumentDate >= DATEADD(DAY, -120, CURRENT_TIMESTAMP) AND DocumentDate < DATEADD(DAY, -90, CURRENT_TIMESTAMP) THEN StockKeepingQuantity ELSE 0 END) AS quantity_91_120,
                                        SUM(CASE WHEN DocumentDate >= DATEADD(DAY, -150, CURRENT_TIMESTAMP) AND DocumentDate < DATEADD(DAY, -120, CURRENT_TIMESTAMP) THEN StockKeepingQuantity ELSE 0 END) AS quantity_121_150,
                                        SUM(CASE WHEN DocumentDate >= DATEADD(DAY, -180, CURRENT_TIMESTAMP) AND DocumentDate < DATEADD(DAY, -150, CURRENT_TIMESTAMP) THEN StockKeepingQuantity ELSE 0 END) AS quantity_151_180,
                                        SUM(CASE WHEN DocumentDate >= DATEADD(DAY, -210, CURRENT_TIMESTAMP) AND DocumentDate < DATEADD(DAY, -180, CURRENT_TIMESTAMP) THEN StockKeepingQuantity ELSE 0 END) AS quantity_181_210,
                                        SUM(CASE WHEN DocumentDate >= DATEADD(DAY, -240, CURRENT_TIMESTAMP) AND DocumentDate < DATEADD(DAY, -210, CURRENT_TIMESTAMP) THEN StockKeepingQuantity ELSE 0 END) AS quantity_211_240,
                                        SUM(CASE WHEN DocumentDate >= DATEADD(DAY, -270, CURRENT_TIMESTAMP) AND DocumentDate < DATEADD(DAY, -240, CURRENT_TIMESTAMP) THEN StockKeepingQuantity ELSE 0 END) AS quantity_241_270,
                                        SUM(CASE WHEN DocumentDate >= DATEADD(DAY, -300, CURRENT_TIMESTAMP) AND DocumentDate < DATEADD(DAY, -270, CURRENT_TIMESTAMP) THEN StockKeepingQuantity ELSE 0 END) AS quantity_271_300,
                                        SUM(CASE WHEN DocumentDate >= DATEADD(DAY, -330, CURRENT_TIMESTAMP) AND DocumentDate < DATEADD(DAY, -300, CURRENT_TIMESTAMP) THEN StockKeepingQuantity ELSE 0 END) AS quantity_301_330,
                                        SUM(CASE WHEN DocumentDate >= DATEADD(DAY, -360, CURRENT_TIMESTAMP) AND DocumentDate < DATEADD(DAY, -330, CURRENT_TIMESTAMP) THEN StockKeepingQuantity ELSE 0 END) AS quantity_331_360,
                                        SUM(CASE WHEN DocumentDate >= DATEADD(DAY, -720, CURRENT_TIMESTAMP) AND DocumentDate < DATEADD(DAY, -360, CURRENT_TIMESTAMP) THEN StockKeepingQuantity ELSE 0 END) AS quantity_361_720,
                                        SUM(CASE WHEN DocumentDate >= DATEADD(DAY, -1080, CURRENT_TIMESTAMP) AND DocumentDate < DATEADD(DAY, -720, CURRENT_TIMESTAMP) THEN StockKeepingQuantity ELSE 0 END) AS quantity_721_1080,
                                        SUM(CASE WHEN DocumentDate >= DATEADD(DAY, -1800, CURRENT_TIMESTAMP) AND DocumentDate < DATEADD(DAY, -1080, CURRENT_TIMESTAMP) THEN StockKeepingQuantity ELSE 0 END) AS quantity_1081_1800,
                                        SUM(CASE WHEN DocumentDate < DATEADD(DAY, -1800, CURRENT_TIMESTAMP) THEN StockKeepingQuantity ELSE 0 END) AS quantity_1801_plus,
	                                    SUM(CASE WHEN DocumentDate >= DATEADD(DAY, -30, CURRENT_TIMESTAMP) THEN TheoreticWeight ELSE 0 END) AS weight_0_30,
                                        SUM(CASE WHEN DocumentDate >= DATEADD(DAY, -60, CURRENT_TIMESTAMP) AND DocumentDate < DATEADD(DAY, -30, CURRENT_TIMESTAMP) THEN TheoreticWeight ELSE 0 END) AS weight_31_60,
                                        SUM(CASE WHEN DocumentDate >= DATEADD(DAY, -90, CURRENT_TIMESTAMP) AND DocumentDate < DATEADD(DAY, -60, CURRENT_TIMESTAMP) THEN TheoreticWeight ELSE 0 END) AS weight_61_90,
                                        SUM(CASE WHEN DocumentDate >= DATEADD(DAY, -120, CURRENT_TIMESTAMP) AND DocumentDate < DATEADD(DAY, -90, CURRENT_TIMESTAMP) THEN TheoreticWeight ELSE 0 END) AS weight_91_120,
                                        SUM(CASE WHEN DocumentDate >= DATEADD(DAY, -150, CURRENT_TIMESTAMP) AND DocumentDate < DATEADD(DAY, -120, CURRENT_TIMESTAMP) THEN TheoreticWeight ELSE 0 END) AS weight_121_150,
                                        SUM(CASE WHEN DocumentDate >= DATEADD(DAY, -180, CURRENT_TIMESTAMP) AND DocumentDate < DATEADD(DAY, -150, CURRENT_TIMESTAMP) THEN TheoreticWeight ELSE 0 END) AS weight_151_180,
                                        SUM(CASE WHEN DocumentDate >= DATEADD(DAY, -210, CURRENT_TIMESTAMP) AND DocumentDate < DATEADD(DAY, -180, CURRENT_TIMESTAMP) THEN TheoreticWeight ELSE 0 END) AS weight_181_210,
                                        SUM(CASE WHEN DocumentDate >= DATEADD(DAY, -240, CURRENT_TIMESTAMP) AND DocumentDate < DATEADD(DAY, -210, CURRENT_TIMESTAMP) THEN TheoreticWeight ELSE 0 END) AS weight_211_240,
                                        SUM(CASE WHEN DocumentDate >= DATEADD(DAY, -270, CURRENT_TIMESTAMP) AND DocumentDate < DATEADD(DAY, -240, CURRENT_TIMESTAMP) THEN TheoreticWeight ELSE 0 END) AS weight_241_270,
                                        SUM(CASE WHEN DocumentDate >= DATEADD(DAY, -300, CURRENT_TIMESTAMP) AND DocumentDate < DATEADD(DAY, -270, CURRENT_TIMESTAMP) THEN TheoreticWeight ELSE 0 END) AS weight_271_300,
                                        SUM(CASE WHEN DocumentDate >= DATEADD(DAY, -330, CURRENT_TIMESTAMP) AND DocumentDate < DATEADD(DAY, -300, CURRENT_TIMESTAMP) THEN TheoreticWeight ELSE 0 END) AS weight_301_330,
                                        SUM(CASE WHEN DocumentDate >= DATEADD(DAY, -360, CURRENT_TIMESTAMP) AND DocumentDate < DATEADD(DAY, -330, CURRENT_TIMESTAMP) THEN TheoreticWeight ELSE 0 END) AS weight_331_360,
                                        SUM(CASE WHEN DocumentDate >= DATEADD(DAY, -720, CURRENT_TIMESTAMP) AND DocumentDate < DATEADD(DAY, -360, CURRENT_TIMESTAMP) THEN TheoreticWeight ELSE 0 END) AS weight_361_720,
                                        SUM(CASE WHEN DocumentDate >= DATEADD(DAY, -1080, CURRENT_TIMESTAMP) AND DocumentDate < DATEADD(DAY, -720, CURRENT_TIMESTAMP) THEN TheoreticWeight ELSE 0 END) AS weight_721_1080,
                                        SUM(CASE WHEN DocumentDate >= DATEADD(DAY, -1800, CURRENT_TIMESTAMP) AND DocumentDate < DATEADD(DAY, -1080, CURRENT_TIMESTAMP) THEN TheoreticWeight ELSE 0 END) AS weight_1081_1800,
                                        SUM(CASE WHEN DocumentDate < DATEADD(DAY, -1800, CURRENT_TIMESTAMP) THEN TheoreticWeight ELSE 0 END) AS weight_1801_plus
                                    FROM InventoryTransactionsTable
                                    WHERE IsDeleted = 0
                                        AND TransactionType != 'Transfer'";
            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(ConfigurationManager.AppSettings.Get("connStringErp")))
                {
                    using (SqlCommand sqlCommand = new SqlCommand(queryForStocks, sqlConnection))
                    {
                        sqlConnection.Open();
                        using (SqlDataReader sqlDataReader = sqlCommand.ExecuteReader())
                        {
                            while (sqlDataReader.Read())
                            {
                                agings.Add(new Aging(
                                    sqlDataReader.GetString(sqlDataReader.GetOrdinal("material")),
                                    0,
                                    sqlDataReader.GetDecimal(sqlDataReader.GetOrdinal("quantity_0_30")),
                                    sqlDataReader.GetDecimal(sqlDataReader.GetOrdinal("quantity_31_60")),
                                    sqlDataReader.GetDecimal(sqlDataReader.GetOrdinal("quantity_61_90")),
                                    sqlDataReader.GetDecimal(sqlDataReader.GetOrdinal("quantity_91_120")),
                                    sqlDataReader.GetDecimal(sqlDataReader.GetOrdinal("quantity_121_150")),
                                    sqlDataReader.GetDecimal(sqlDataReader.GetOrdinal("quantity_151_180")),
                                    sqlDataReader.GetDecimal(sqlDataReader.GetOrdinal("quantity_181_210")),
                                    sqlDataReader.GetDecimal(sqlDataReader.GetOrdinal("quantity_211_240")),
                                    sqlDataReader.GetDecimal(sqlDataReader.GetOrdinal("quantity_241_270")),
                                    sqlDataReader.GetDecimal(sqlDataReader.GetOrdinal("quantity_271_300")),
                                    sqlDataReader.GetDecimal(sqlDataReader.GetOrdinal("quantity_301_330")),
                                    sqlDataReader.GetDecimal(sqlDataReader.GetOrdinal("quantity_331_360")),
                                    sqlDataReader.GetDecimal(sqlDataReader.GetOrdinal("quantity_361_720")),
                                    sqlDataReader.GetDecimal(sqlDataReader.GetOrdinal("quantity_721_1080")),
                                    sqlDataReader.GetDecimal(sqlDataReader.GetOrdinal("quantity_1081_1800")),
                                    sqlDataReader.GetDecimal(sqlDataReader.GetOrdinal("quantity_1801_plus")),
                                    0,
                                    sqlDataReader.GetDecimal(sqlDataReader.GetOrdinal("weight_0_30")),
                                    sqlDataReader.GetDecimal(sqlDataReader.GetOrdinal("weight_31_60")),
                                    sqlDataReader.GetDecimal(sqlDataReader.GetOrdinal("weight_61_90")),
                                    sqlDataReader.GetDecimal(sqlDataReader.GetOrdinal("weight_91_120")),
                                    sqlDataReader.GetDecimal(sqlDataReader.GetOrdinal("weight_121_150")),
                                    sqlDataReader.GetDecimal(sqlDataReader.GetOrdinal("weight_151_180")),
                                    sqlDataReader.GetDecimal(sqlDataReader.GetOrdinal("weight_181_210")),
                                    sqlDataReader.GetDecimal(sqlDataReader.GetOrdinal("weight_211_240")),
                                    sqlDataReader.GetDecimal(sqlDataReader.GetOrdinal("weight_241_270")),
                                    sqlDataReader.GetDecimal(sqlDataReader.GetOrdinal("weight_271_300")),
                                    sqlDataReader.GetDecimal(sqlDataReader.GetOrdinal("weight_301_330")),
                                    sqlDataReader.GetDecimal(sqlDataReader.GetOrdinal("weight_331_360")),
                                    sqlDataReader.GetDecimal(sqlDataReader.GetOrdinal("weight_361_720")),
                                    sqlDataReader.GetDecimal(sqlDataReader.GetOrdinal("weight_721_1080")),
                                    sqlDataReader.GetDecimal(sqlDataReader.GetOrdinal("weight_1081_1800")),
                                    sqlDataReader.GetDecimal(sqlDataReader.GetOrdinal("weight_1801_plus"))));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.LogError(ConfigurationManager.AppSettings.Get("logPath") + DateTime.Now.Year + "_" + DateTime.Now.Month + "_" + DateTime.Now.Day + ".txt", "DbHandler.GetAgings", ex.Message != null ? ex.Message.ToString() : ex.ToString());
                throw ex;
            }
            return agings;
        }
        public static void ModifyReport(Aging aging, string action)
        {
            string connectionString = ConfigurationManager.AppSettings.Get("connStringDwh");
            string sqlCommand = "";

            switch (action)
            {
                case "INSERT":
                    sqlCommand = @"
                INSERT INTO reporting_inventory_aging (
                    material, quantity_current, quantity_0_30, quantity_31_60, quantity_61_90, quantity_91_120, 
                    quantity_121_150, quantity_151_180, quantity_181_210, quantity_211_240, 
                    quantity_241_270, quantity_271_300, quantity_301_330, quantity_331_360, 
                    quantity_361_720, quantity_721_1080, quantity_1081_1800, quantity_1801_plus, 
                    weight_current, weight_0_30, weight_31_60, weight_61_90, weight_91_120, weight_121_150, 
                    weight_151_180, weight_181_210, weight_211_240, weight_241_270, weight_271_300, 
                    weight_301_330, weight_331_360, weight_361_720, weight_721_1080, 
                    weight_1081_1800, weight_1801_plus, changed_at
                ) VALUES (
                    @material, @quantity_current, @quantity_0_30, @quantity_31_60, @quantity_61_90, @quantity_91_120, 
                    @quantity_121_150, @quantity_151_180, @quantity_181_210, @quantity_211_240, 
                    @quantity_241_270, @quantity_271_300, @quantity_301_330, @quantity_331_360, 
                    @quantity_361_720, @quantity_721_1080, @quantity_1081_1800, @quantity_1801_plus, 
                    @weight_current, @weight_0_30, @weight_31_60, @weight_61_90, @weight_91_120, @weight_121_150, 
                    @weight_151_180, @weight_181_210, @weight_211_240, @weight_241_270, @weight_271_300, 
                    @weight_301_330, @weight_331_360, @weight_361_720, @weight_721_1080, 
                    @weight_1081_1800, @weight_1801_plus, @changed_at
                )";
                    break;

                case "UPDATE":
                    sqlCommand = @"
                UPDATE reporting_inventory_aging SET
                    quantity_current = @quantity_current,
                    quantity_0_30 = @quantity_0_30, quantity_31_60 = @quantity_31_60, 
                    quantity_61_90 = @quantity_61_90, quantity_91_120 = @quantity_91_120, 
                    quantity_121_150 = @quantity_121_150, quantity_151_180 = @quantity_151_180, 
                    quantity_181_210 = @quantity_181_210, quantity_211_240 = @quantity_211_240, 
                    quantity_241_270 = @quantity_241_270, quantity_271_300 = @quantity_271_300, 
                    quantity_301_330 = @quantity_301_330, quantity_331_360 = @quantity_331_360, 
                    quantity_361_720 = @quantity_361_720, quantity_721_1080 = @quantity_721_1080, 
                    quantity_1081_1800 = @quantity_1081_1800, quantity_1801_plus = @quantity_1801_plus, 
                    weight_current = @weight_current,
                    weight_0_30 = @weight_0_30, weight_31_60 = @weight_31_60, 
                    weight_61_90 = @weight_61_90, weight_91_120 = @weight_91_120, 
                    weight_121_150 = @weight_121_150, weight_151_180 = @weight_151_180, 
                    weight_181_210 = @weight_181_210, weight_211_240 = @weight_211_240, 
                    weight_241_270 = @weight_241_270, weight_271_300 = @weight_271_300, 
                    weight_301_330 = @weight_301_330, weight_331_360 = @weight_331_360, 
                    weight_361_720 = @weight_361_720, weight_721_1080 = @weight_721_1080, 
                    weight_1081_1800 = @weight_1081_1800, weight_1801_plus = @weight_1801_plus,
                    changed_at = @changed_at
                WHERE material = @material";
                    break;

                case "DELETE":
                    sqlCommand = "DELETE FROM reporting_inventory_aging WHERE material = @material";
                    break;

                default:
                    throw new ArgumentException("Invalid command. Use 'INSERT', 'UPDATE', or 'DELETE'.");
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    using (SqlCommand command = new SqlCommand(sqlCommand, connection))
                    {
                        command.Parameters.AddWithValue("@material", aging.material);

                        if (action != "DELETE")
                        {
                            command.Parameters.AddWithValue("@quantity_current", aging.quantity_current);
                            command.Parameters.AddWithValue("@quantity_0_30", aging.quantity_0_30);
                            command.Parameters.AddWithValue("@quantity_31_60", aging.quantity_31_60);
                            command.Parameters.AddWithValue("@quantity_61_90", aging.quantity_61_90);
                            command.Parameters.AddWithValue("@quantity_91_120", aging.quantity_91_120);
                            command.Parameters.AddWithValue("@quantity_121_150", aging.quantity_121_150);
                            command.Parameters.AddWithValue("@quantity_151_180", aging.quantity_151_180);
                            command.Parameters.AddWithValue("@quantity_181_210", aging.quantity_181_210);
                            command.Parameters.AddWithValue("@quantity_211_240", aging.quantity_211_240);
                            command.Parameters.AddWithValue("@quantity_241_270", aging.quantity_241_270);
                            command.Parameters.AddWithValue("@quantity_271_300", aging.quantity_271_300);
                            command.Parameters.AddWithValue("@quantity_301_330", aging.quantity_301_330);
                            command.Parameters.AddWithValue("@quantity_331_360", aging.quantity_331_360);
                            command.Parameters.AddWithValue("@quantity_361_720", aging.quantity_361_720);
                            command.Parameters.AddWithValue("@quantity_721_1080", aging.quantity_721_1080);
                            command.Parameters.AddWithValue("@quantity_1081_1800", aging.quantity_1081_1800);
                            command.Parameters.AddWithValue("@quantity_1801_plus", aging.quantity_1801_plus);
                            command.Parameters.AddWithValue("@weight_current", aging.weight_current);
                            command.Parameters.AddWithValue("@weight_0_30", aging.weight_0_30);
                            command.Parameters.AddWithValue("@weight_31_60", aging.weight_31_60);
                            command.Parameters.AddWithValue("@weight_61_90", aging.weight_61_90);
                            command.Parameters.AddWithValue("@weight_91_120", aging.weight_91_120);
                            command.Parameters.AddWithValue("@weight_121_150", aging.weight_121_150);
                            command.Parameters.AddWithValue("@weight_151_180", aging.weight_151_180);
                            command.Parameters.AddWithValue("@weight_181_210", aging.weight_181_210);
                            command.Parameters.AddWithValue("@weight_211_240", aging.weight_211_240);
                            command.Parameters.AddWithValue("@weight_241_270", aging.weight_241_270);
                            command.Parameters.AddWithValue("@weight_271_300", aging.weight_271_300);
                            command.Parameters.AddWithValue("@weight_301_330", aging.weight_301_330);
                            command.Parameters.AddWithValue("@weight_331_360", aging.weight_331_360);
                            command.Parameters.AddWithValue("@weight_361_720", aging.weight_361_720);
                            command.Parameters.AddWithValue("@weight_721_1080", aging.weight_721_1080);
                            command.Parameters.AddWithValue("@weight_1081_1800", aging.weight_1081_1800);
                            command.Parameters.AddWithValue("@weight_1801_plus", aging.weight_1801_plus);
                            command.Parameters.AddWithValue("@changed_at", DateTime.Now);
                        }

                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.LogError(ConfigurationManager.AppSettings.Get("logPath") + DateTime.Now.Year + "_" + DateTime.Now.Month + "_" + DateTime.Now.Day + ".txt", "DbHandler.ModifyReport", ex.Message != null ? ex.Message.ToString() : ex.ToString());
                throw;
            }
        }
    }

    
}
