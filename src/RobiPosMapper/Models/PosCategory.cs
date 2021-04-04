using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;

namespace RobiPosMapper.Models
{
    public class PosCategory
    {
        public Int32 PosCategoryId { get; set; }
        public String PosCategoryName { get; set; }
    }

    public class PosCategoryManager
    {
        private static PosCategory FillEntity(SqlDataReader reader)
        {
            return new PosCategory { PosCategoryId = Convert.ToInt32(reader["PosCategoryId"]), PosCategoryName = reader["PosCategoryName"].ToString() };
        }
     
        public static List<PosCategory> PosCategoryList()
        {
            List<PosCategory> PosCategories = new List<PosCategory>();
            String CS = WebConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;
            using (SqlConnection connection = new SqlConnection(CS))
            {
                string sqlSelect = "SELECT PosCategoryId,PosCategoryName FROM PosCategory WHERE PosCategoryId >0  ORDER BY PosCategoryId ASC";
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