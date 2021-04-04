using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;

namespace RobiPosMapper.Models
{

    public class ShopType
    {
        public Int32 ShopTypeId { get; set; }
        public String ShopTypeName { get; set; }
    }

    public class ShopTypeManager
    {
        private static ShopType FillEntity(SqlDataReader reader)
        {
            return new ShopType { ShopTypeId = Convert.ToInt32(reader["ShopTypeId"]), ShopTypeName = reader["ShopTypeName"].ToString() };
        }

        public static List<ShopType> ShopTypeList()
        {
            List<ShopType> PosCategories = new List<ShopType>();
            String CS = WebConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;
            using (SqlConnection connection = new SqlConnection(CS))
            {
                string sqlSelect = "SELECT ShopTypeId,ShopTypeName FROM ShopType WHERE ShopTypeId >0  ORDER BY ShopTypeName ASC";
                using (SqlCommand cmd = new SqlCommand(sqlSelect, connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        while (reader.Read())
                        {
                            PosCategories.Add(FillEntity(reader));
                        }

                        if (!reader.IsClosed)
                        {
                            reader.Close();
                        }
                    }
                }

            }
            return PosCategories;
        }
    }
}