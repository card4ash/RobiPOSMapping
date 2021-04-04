using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Data;
using System.Data.SqlClient;

namespace RobiPosMapper.Areas.RobiAdmin.Controllers
{
    public class LoginController : Controller
    {
        //
        // GET: /Home/
        public ActionResult Index()
        {
            return View();
        }

        //Submits login info
        [HttpPost]
        public JsonResult Index(FormCollection data)
        {
            String LoginName = data["loginname"];
            String LoginPassword = data["password"];

            String ConnectionString = WebConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;
            DataTable dtUserInfo = new DataTable();

            String sqlSelectUser = @"SELECT MoId, MoName,LoginName from MonitoringOfficer Where LoginName=@LoginName and LoginPassword=@LoginPassword";
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    using (SqlCommand command = new SqlCommand(sqlSelectUser, connection))
                    {
                        command.Parameters.Add("@LoginName", SqlDbType.VarChar).Value = LoginName;
                        command.Parameters.Add("@LoginPassword", SqlDbType.VarChar).Value = LoginPassword;
                        connection.Open();
                        using (SqlDataAdapter da = new SqlDataAdapter(command))
                        {
                            da.Fill(dtUserInfo);
                            connection.Close();
                        }
                    }
                }


                if (dtUserInfo.Rows.Count == 1) //if user found
                {
                    Int32 moId =Convert.ToInt32( dtUserInfo.Rows[0]["MoId"]);
                    Session["MoId"] = moId;
                    Session["LoginName"] = dtUserInfo.Rows[0]["LoginName"].ToString();

                    //--> save to MonitoringOfficerLoginHistory
                    using (SqlConnection connection=new SqlConnection(ConnectionString))
                    {
                        using (SqlCommand command=new SqlCommand())
                        {
                            command.Connection = connection;
                            command.CommandText = "Insert into MontioringOfficerLoginHistory(MoId) Values(@MoId)";
                            command.Parameters.Clear();
                            command.Parameters.Add("@MoId", SqlDbType.Int).Value = moId;
                            connection.Open(); 
                            command.ExecuteNonQuery();
                           // command.CommandText = "SELECT RegionId FROM MonitoringOfficerRegion WHERE MoId";
                            connection.Close();
                        }
                    }
                    //<-- save to MonitoringOfficerLoginHistory

                    return Json(new { result = "Redirect", url = "/RobiAdmin/Home?user=" + LoginName + "&moid="+ moId.ToString() +"" });
                }
                else //if user not found
                {
                    return Json(new { result = "InvalidPassword" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception Ex)
            {
                return Json("Error", JsonRequestBehavior.AllowGet);
            }
        }
    }
}