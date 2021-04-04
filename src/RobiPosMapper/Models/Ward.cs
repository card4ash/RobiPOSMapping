using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;

namespace RobiPosMapper.Models
{
    public class Ward
    {
        public int WardId { get; set; }
        public String WardName { get; set; }
    }

    public class WardManager
    {
        private static Ward FillEntity(SqlDataReader reader)
        {
            return new Ward { WardId = Convert.ToInt32(reader["WardId"]), WardName = reader["WardName"].ToString() };
        }
        public static List<Ward> ThanaSpecificWardList(int thanaId)
        {
            List<Ward> Wards = new List<Ward>();
            String CS = WebConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;
            using (SqlConnection connection = new SqlConnection(CS))
            {
                string sqlSelect = "SELECT WardId,WardName FROM Wards WHERE WardId >0 and ThanaId = " + thanaId + " ORDER BY WardName ASC";
                using (SqlCommand cmd = new SqlCommand(sqlSelect, connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        while (reader.Read())
                        {
                            Wards.Add(FillEntity(reader));
                        }

                        if (!reader.IsClosed)
                        {
                            reader.Close();
                        }
                    }
                }

            }
            return Wards;
        }
    }
}