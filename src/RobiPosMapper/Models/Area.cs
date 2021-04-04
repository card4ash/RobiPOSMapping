using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;

namespace RobiPosMapper.Models
{
    public class Area
    {
        public Int32 AreaId { get; set; }
        public String AreaName { get; set; }
    }


    public class AreaManager
    {
        private static Area FillEntity(SqlDataReader reader)
        {
            return new Area { AreaId = Convert.ToInt32( reader["AreaId"]), AreaName = reader["AreaName"].ToString() };
        }
        public static List<Area> RegionSpecificAreaList(int regionId)
        {
            List<Area> areas = new List<Area>();
            String CS = WebConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;
            using (SqlConnection connection = new SqlConnection(CS))
            {
                string sqlSelect = "SELECT AreaId,AreaName FROM Area WHERE AreaId >0 and RegionId = " + regionId + " ORDER BY AreaName ASC";
                using (SqlCommand cmd = new SqlCommand(sqlSelect, connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        while (reader.Read())
                        {
                            areas.Add(FillEntity(reader));
                        }

                        if (!reader.IsClosed)
                        {
                            reader.Close();
                        }
                    }
                }

            }
            return areas;
        }
    }

}