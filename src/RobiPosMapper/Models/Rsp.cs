using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;

namespace RobiPosMapper.Models
{
    public class Rsp
    {
        public Int32 RspId { get; set; }
        public String RspName { get; set; }
    }


    public class RspManager
    {
        private static Rsp FillEntity(SqlDataReader reader)
        {
            return new Rsp { RspId = Convert.ToInt32(reader["RspId"]), RspName = reader["RspName"].ToString() };
        }
        public static List<Rsp> AreaSpecificRspList(int areaId)
        {
            List<Rsp> Rsps = new List<Rsp>();
            String CS = WebConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;
            using (SqlConnection connection = new SqlConnection(CS))
            {
                string sqlSelect = "SELECT RspId,RspName FROM RSP WHERE RspId >0 and AreaId = " + areaId + " ORDER BY RspName ASC";
                using (SqlCommand cmd = new SqlCommand(sqlSelect, connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        while (reader.Read())
                        {
                            Rsps.Add(FillEntity(reader));
                        }

                        if (!reader.IsClosed)
                        {
                            reader.Close();
                        }
                    }
                }

            }
            return Rsps;
        }
    }
}