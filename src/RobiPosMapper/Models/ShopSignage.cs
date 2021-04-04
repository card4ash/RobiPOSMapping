using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;

namespace RobiPosMapper.Models
{

    public class ShopSignage
    {
        public Int32 ShopSignageId { get; set; }
        public String ShopSignageName { get; set; }
    }

    public class ShopSignageManager
    {
        private static ShopSignage FillEntity(SqlDataReader reader)
        {
            return new ShopSignage { ShopSignageId = Convert.ToInt32(reader["ShopSignageId"]), ShopSignageName = reader["ShopSignageName"].ToString() };
        }

        public static List<ShopSignage> ShopSignageList()
        {
            List<ShopSignage> list = new List<ShopSignage>();
            String CS = WebConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;
            using (SqlConnection connection = new SqlConnection(CS))
            {
                string sqlSelect = "SELECT ShopSignageId, ShopSignageName FROM ShopSignage WHERE ShopSignageId >0  ORDER BY ShopSignageId ASC";
                using (SqlCommand cmd = new SqlCommand(sqlSelect, connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        while (reader.Read())
                        {
                            list.Add(FillEntity(reader));
                        }

                        if (!reader.IsClosed)
                        {
                            reader.Close();
                        }
                    }
                }

            }
            return list;
        }
    }
}