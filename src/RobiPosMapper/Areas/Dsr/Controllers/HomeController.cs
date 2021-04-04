using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;
using sCommonLib;
using QueryStringEncryption;

namespace RobiPosMapper.Areas.Dsr.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Dsr/Home/
        public ActionResult Index()
        {
            
            if (Request.QueryString["sid"]==null || Request.QueryString["sd"]==null) //sid = SessionId (string), sd = SessionDigest (string)
            {
                return RedirectToAction("Index", "LogIn", new { area = "Dsr" });
            }
            
            String sessionId = Request.QueryString["sid"].ToString();
            String encryptedSessionDigest = Request.QueryString["sd"].ToString();
            String sessionDigest = MyCrypto.GetDecryptedQueryString(encryptedSessionDigest);

            if (!sessionId.Equals(sessionDigest,StringComparison.Ordinal))
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
                using (SqlDataAdapter da=new SqlDataAdapter(command))
                {
                    connection.Open(); da.Fill(dtSessionInfo); connection.Close();
                }

                if (dtSessionInfo.Rows.Count==0)
                {
                    return RedirectToAction("Index", "LogIn", new { area = "Dsr" });
                }

                DateTime dtSessionCreated = Convert.ToDateTime(dtSessionInfo.Rows[0]["CreatedOn"]);
                DateTime currentTime = DateTime.Now;
                double hours = (currentTime - dtSessionCreated).TotalHours;

                if (hours>12)
                {
                    command.Parameters.Clear();
                    command.CommandText = "Delete from SessionInfo Where SessionId=@SessionId";
                    command.Parameters.AddWithValue("@SessionId", sessionId);
                    connection.Open(); command.ExecuteNonQuery(); connection.Close();
                    return RedirectToAction("Index", "LogIn", new { area = "Dsr" });
                }

                ViewBag.SessionId = sessionId; ViewBag.SessionDigest = encryptedSessionDigest;

                Int32 DsrId =Convert.ToInt32( dtSessionInfo.Rows[0]["DsrId"]);

                if (Request.QueryString["changelang"]==null)
                {
                    String sqlLanguagePreference = "Select LanguagePreference from Person Where PersonId=" + DsrId + "";
                    command.CommandText = sqlLanguagePreference;
                    connection.Open();
                    ViewBag.LanguagePreference = command.ExecuteScalar().ToString(); connection.Close();
                }
                else
                {
                    String ChangeLanguageTo = Request.QueryString["changelang"].ToString();

                    String sqlUpdateLanguage = "Update Person Set LanguagePreference='"+ ChangeLanguageTo +"' Where PersonId=" + DsrId + "";
                    command.CommandText = sqlUpdateLanguage;
                    connection.Open();command.ExecuteScalar();connection.Close();
                    ViewBag.LanguagePreference = ChangeLanguageTo;
                }
            }
            catch (Exception Ex)
            {
                if (connection.State == ConnectionState.Open) { connection.Close(); }
                connection.Dispose();

                var path = HttpContext.Server.MapPath("~/App_Data");
                ExceptionLogger.CreateLog(Ex, path);
                return Json(new { SearchResult = "systemerror" }, JsonRequestBehavior.AllowGet);
            }


            return View();
        }
	}
}