using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Configuration;
using System.Data;
using System.Data.SqlClient;
using sCommonLib;
using QueryStringEncryption;

namespace RobiPosMapper.Areas.Dsr.Controllers
{
    public class ChangePasswordController : Controller
    {
        //
        // GET: /Dsr/ChangePassword/
        public ActionResult Index()
        {

            String sessionId = Request.QueryString["sid"].ToString();
            String encryptedSessionDigest = Request.QueryString["sd"].ToString();
            String sessionDigest = MyCrypto.GetDecryptedQueryString(encryptedSessionDigest);


            if (!sessionId.Equals(sessionDigest, StringComparison.Ordinal))
            {
                return RedirectToAction("Index", "LogIn", new { area = "Dsr" });
            }

           

            String ConnectionString = WebConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;

            SqlConnection connection = new SqlConnection(ConnectionString);
            SqlCommand command = new SqlCommand(); command.Connection = connection;

            try
            {

                string sqlSelectSessionInfo = "Select * from SessionInfo where SessionId=@SessionId";
                command.CommandText = sqlSelectSessionInfo;
                command.Parameters.AddWithValue("@SessionId", sessionId);
                DataTable dtSessionInfo = new DataTable();
                using (SqlDataAdapter da = new SqlDataAdapter(command))
                {
                    connection.Open(); da.Fill(dtSessionInfo); command.Parameters.Clear(); connection.Close();
                }

                if (dtSessionInfo.Rows.Count != 1)
                {
                    return RedirectToAction("Index", "LogIn", new { area = "Dsr" });
                }

                DateTime dtSessionCreated = Convert.ToDateTime(dtSessionInfo.Rows[0]["CreatedOn"]);
                DateTime currentTime = DateTime.Now;
                double hours = (currentTime - dtSessionCreated).TotalHours;

                if (hours > 12)
                {
                    command.Parameters.Clear();
                    command.CommandText = "Delete from SessionInfo Where SessionId=@SessionId";
                    command.Parameters.AddWithValue("@SessionId", sessionId);
                    connection.Open(); command.ExecuteNonQuery(); connection.Close();
                    return RedirectToAction("Index", "LogIn", new { area = "Dsr" });
                }

                ViewBag.SessionId = sessionId; ViewBag.SessionDigest = encryptedSessionDigest;

                Int32 DsrId = Convert.ToInt32(dtSessionInfo.Rows[0]["DsrId"]);

                String sqlLanguagePreference = "Select LanguagePreference from Person Where PersonId=" + Convert.ToInt32(DsrId) + "";
                command.CommandText = sqlLanguagePreference;
                connection.Open();
                String LanguagePreference = command.ExecuteScalar().ToString(); connection.Close();

                if (LanguagePreference == "English")
                {
                    return View("index");
                }
                else
                {
                    return View("index.bn");
                }
            }
            catch (Exception Ex)
            {
                if (connection.State == ConnectionState.Open) { connection.Close(); }
                connection.Dispose();

                var path = HttpContext.Server.MapPath("~/App_Data");
                ExceptionLogger.CreateLog(Ex, path);
                return View("index");
            }

        }

        [HttpPost]
        public JsonResult Change(FormCollection data)
        {
            String encryptedSessionDigest = data["SessionId"].ToString();
            String sessionId = MyCrypto.GetDecryptedQueryString(encryptedSessionDigest);
            
            String newPassword = data["NewPassword"].ToString().Trim();
            String ConnectionString = WebConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;

            SqlConnection connection = new SqlConnection(ConnectionString);
            SqlCommand command = new SqlCommand(); command.Connection = connection;

            try
            {
                //-----------> Session Related Checking
                string sqlSelectSessionInfo = "Select * from SessionInfo where SessionId=@SessionId";
                command.CommandText = sqlSelectSessionInfo;
                command.Parameters.AddWithValue("@SessionId", sessionId);
                DataTable dtSessionInfo = new DataTable();
                using (SqlDataAdapter da = new SqlDataAdapter(command))
                {
                    connection.Open(); da.Fill(dtSessionInfo); command.Parameters.Clear(); connection.Close();
                }

                if (dtSessionInfo.Rows.Count != 1)
                {
                    return Json(new { Changed = "failed", IsSessionTimeOut = true, url = "/Dsr/Login/Index" }, JsonRequestBehavior.AllowGet);
                }

                DateTime dtSessionCreated = Convert.ToDateTime(dtSessionInfo.Rows[0]["CreatedOn"]);
                DateTime currentTime = DateTime.Now;
                double hours = (currentTime - dtSessionCreated).TotalHours;

                if (hours > 12)
                {
                    command.Parameters.Clear();
                    command.CommandText = "Delete from SessionInfo Where SessionId=@SessionId";
                    command.Parameters.AddWithValue("@SessionId", sessionId);
                    connection.Open(); command.ExecuteNonQuery(); command.Parameters.Clear(); connection.Close();
                    return Json(new { Changed = "failed", IsSessionTimeOut = true, url = "/Dsr/Login/Index" }, JsonRequestBehavior.AllowGet);
                }

                Int32 DsrId = Convert.ToInt32(dtSessionInfo.Rows[0]["DsrId"]);
                Int32 surveyorId = Convert.ToInt32(dtSessionInfo.Rows[0]["SurveyorId"]);

                //<-------------------------Session related checking
                command.Parameters.Clear();
                command.CommandText = "Update Surveyors Set LoginPassword=@NewPassword Where SurveyorId=" + surveyorId+ "";
                command.Parameters.AddWithValue("@NewPassword",newPassword);
                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
                return Json(new {Changed="ok" },JsonRequestBehavior.AllowGet);
            }
            catch (Exception Ex)
            {
                if (connection.State == ConnectionState.Open) { connection.Close(); }
                connection.Dispose();

                var path = HttpContext.Server.MapPath("~/App_Data");
                ExceptionLogger.CreateLog(Ex, path);
                return Json(new { Changed = "failed", IsSessionTimeOut = false }, JsonRequestBehavior.AllowGet);
            }
        }
	}
}