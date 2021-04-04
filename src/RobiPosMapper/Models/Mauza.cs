using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;

namespace RobiPosMapper.Models
{
    public class Mauza
    {
        public int MauzaId { get; set; }
        public String MauzaName { get; set; }
    }

    public class MauzaManager
    {
        private static Mauza FillEntity(SqlDataReader reader)
        {
            return new Mauza { MauzaId = Convert.ToInt32(reader["MauzaId"]), MauzaName = reader["MauzaName"].ToString() };
        }
        public static List<Mauza> WardSpecificMauzaList(int wardId)
        {
            List<Mauza> Mauzas = new List<Mauza>();
            String CS = WebConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;
            using (SqlConnection connection = new SqlConnection(CS))
            {
                string sqlSelect = "SELECT MauzaId,MauzaName FROM Mauza WHERE MauzaId >0 and WardId = " + wardId + " ORDER BY MauzaName ASC";
                using (SqlCommand cmd = new SqlCommand(sqlSelect, connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        while (reader.Read())
                        {
                            Mauzas.Add(FillEntity(reader));
                        }

                        if (!reader.IsClosed)
                        {
                            reader.Close();
                        }
                    }
                }

            }
            return Mauzas;
        }
    }
}