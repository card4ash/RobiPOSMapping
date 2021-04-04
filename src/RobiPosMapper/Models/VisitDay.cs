using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;

namespace RobiPosMapper.Models
{
    public class VisitDay
    {
        public Int32 VisitDayId { get; set; }
        public String VisitDayName { get; set; }
    }

    public class VisitDayManager
    {
        private static VisitDay FillEntity(SqlDataReader reader)
        {
            return new VisitDay { VisitDayId = Convert.ToInt32(reader["VisitDayId"]), VisitDayName = reader["VisitDayName"].ToString() };
        }

        public static List<VisitDay> VisitDayList()
        {
            List<VisitDay> PosCategories = new List<VisitDay>();
            String CS = WebConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;
            using (SqlConnection connection = new SqlConnection(CS))
            {
                string sqlSelect = "SELECT VisitDayId, VisitDays as VisitDayName FROM VisitDay WHERE VisitDayId >0  ORDER BY VisitDayId ASC";
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