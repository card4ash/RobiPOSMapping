using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;

namespace RobiPosMapper.Models
{
    public class Region
    {
        public Int32 RegionId { get; set; }
        public String RegionName { get; set; }
    }


    public class RegionManager
    {
        private static Region FillEntity(SqlDataReader reader)
        {
            return new Region { RegionId = Convert.ToInt32(reader["RegionId"]), RegionName = reader["RegionName"].ToString() };
        }

        public static List<Region> GetAllRegions()
        {
            List<Region> regions = new List<Region>();

            String ConnectionString = WebConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                string sqlSelect = "select RegionId, RegionName from REGION Where RegionId>0 order by RegionName ASC";
                using (SqlCommand command = new SqlCommand(sqlSelect, connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        while (reader.Read())
                        {
                            regions.Add(FillEntity(reader));
                        }

                        if (!reader.IsClosed)
                        {
                            reader.Close();
                        }
                    }
                }
            }
            return regions;
        }
    }
}