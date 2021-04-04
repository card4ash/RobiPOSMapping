using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;

namespace RobiPosMapper.Models
{
    public class PosStructure
    {
        public Int32 PosStructureId { get; set; }
        public String PosStructureName { get; set; }
    }

    public class PosStructureManager
    {
        private static PosStructure FillEntity(SqlDataReader reader)
        {
            return new PosStructure { PosStructureId = Convert.ToInt32(reader["PosStructureId"]), PosStructureName = reader["PosStructureName"].ToString() };
        }

        public static List<PosStructure> PosStructureList()
        {
            List<PosStructure> PosCategories = new List<PosStructure>();
            String CS = WebConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;
            using (SqlConnection connection = new SqlConnection(CS))
            {
                string sqlSelect = "SELECT PosStructureId,PosStructureName FROM PosStructure WHERE PosStructureId >0  ORDER BY PosStructureName ASC";
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