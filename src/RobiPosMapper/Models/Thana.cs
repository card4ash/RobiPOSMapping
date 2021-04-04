using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;

namespace RobiPosMapper.Models
{
    public class Thana
    {
        public int ThanaId { get; set; }
        public String ThanaName { get; set; }
    }

    public class ThanaManager
    {
        private static Thana FillEntity(SqlDataReader reader)
        {
            return new Thana { ThanaId = Convert.ToInt32(reader["ThanaId"]), ThanaName = reader["ThanaName"].ToString() };
        }
        public static List<Thana> AreaSpecificThanaList(int areaId)
        {
            List<Thana> thanas = new List<Thana>();
            String CS = WebConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;
            using (SqlConnection connection = new SqlConnection(CS))
            {
                string sqlSelect = "SELECT ThanaId,ThanaName FROM Thana WHERE ThanaId >0 and AreaId = " + areaId + " ORDER BY ThanaName ASC";
                using (SqlCommand cmd = new SqlCommand(sqlSelect, connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        while (reader.Read())
                        {
                            thanas.Add(FillEntity(reader));
                        }

                        if (!reader.IsClosed)
                        {
                            reader.Close();
                        }
                    }
                }

            }
            return thanas;
        }
    }
}