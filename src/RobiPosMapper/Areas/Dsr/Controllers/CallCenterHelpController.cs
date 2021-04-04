using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using QueryStringEncryption;
using System.Web.Configuration;
using System.Data;
using System.Data.SqlClient;
using sCommonLib;

namespace RobiPosMapper.Areas.Dsr.Controllers
{
    public class CallCenterHelpController : Controller
    {
        //
        // GET: /Dsr/CallCenterHelp/
        public ActionResult Index()
        {

            if (Request.QueryString["userid"] == null || Request.QueryString["ud"] == null)
            {
               // return RedirectToAction("/LogIn", new { area = "Dsr" });
                return RedirectToAction("Index", "Login", new { area = "Dsr" });
            }

            String UserId = Request.QueryString["userid"].ToString();
            String encryptedUserIdDigest = Request.QueryString["ud"].ToString();
            String userDigest = MyCrypto.GetDecryptedQueryString(encryptedUserIdDigest);

            if (!UserId.Equals(userDigest, StringComparison.Ordinal))
            {
                return RedirectToAction("LogIn",  new { area = "Dsr" });
            }

            ViewBag.UserId = UserId; ViewBag.UserIdDigest = encryptedUserIdDigest;





            String ConnectionString = WebConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;

            SqlConnection connection = new SqlConnection(ConnectionString);
            SqlCommand command = new SqlCommand(); command.Connection = connection;

            try
            {

                String sqlLanguagePreference = "Select LanguagePreference from Person Where PersonId=" + Convert.ToInt32(UserId) + "";
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
	}
}