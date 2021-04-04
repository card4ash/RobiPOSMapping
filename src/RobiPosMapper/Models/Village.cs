using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;

namespace RobiPosMapper.Models
{
    public class Village
    {
        public int VillageId { get; set; }
        public String VillageName { get; set; }
    }

    public class VillageManager
    {
        private static Village FillEntity(SqlDataReader reader)
        {
            return new Village { VillageId = Convert.ToInt32(reader["VillageId"]), VillageName = reader["VillageName"].ToString() };
        }
        public static List<Village> MauzaSpecificVillageList(int mauzaId)
        {
            List<Village> Villages = new List<Village>();
            String CS = WebConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;
            using (SqlConnection connection = new SqlConnection(CS))
            {
                string sqlSelect = "SELECT VillageId,VillageName FROM Village WHERE VillageId >0 and MauzaId = " + mauzaId + " ORDER BY VillageName ASC";
                using (SqlCommand cmd = new SqlCommand(sqlSelect, connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        while (reader.Read())
                        {
                            Villages.Add(FillEntity(reader));
                        }

                        if (!reader.IsClosed)
                        {
                            reader.Close();
                        }
                    }
                }

            }
            return Villages;
        }
    }
}