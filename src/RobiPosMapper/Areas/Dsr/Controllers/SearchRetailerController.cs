using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using sCommonLib;
using System.Web.Configuration;
using System.Data;
using System.Data.SqlClient;
using QueryStringEncryption;

namespace RobiPosMapper.Areas.Dsr.Controllers
{
    public class SearchRetailerController : Controller
    {
        //
        // GET: /Dsr/SearchRetailer/
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

                String sqlLanguagePreference = "Select LanguagePreference from Person Where PersonId=" + DsrId + "";
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
        public JsonResult SearchByRetailerId(FormCollection data)
        {

            String encryptedSessionDigest = data["SessionId"].ToString();
            String sessionId = MyCrypto.GetDecryptedQueryString(encryptedSessionDigest);

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
                    return Json(new { SearchResult = "sessiontimeout", url = "/Dsr/Login/Index" }, JsonRequestBehavior.AllowGet);
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
                    return Json(new { SearchResult = "sessiontimeout", url = "/Dsr/Login/Index" }, JsonRequestBehavior.AllowGet);
                }

               // Int32 DsrId = Convert.ToInt32(dtSessionInfo.Rows[0]["DsrId"]); // No need
               // Int32 surveyorId = Convert.ToInt32(dtSessionInfo.Rows[0]["SurveyorId"]); // No need.
                //<-------------------------Session related checking

                Int32 retailerId;

                Boolean IsRetailerNumeric = Int32.TryParse(data["RetailerId"].ToString(),out retailerId);
                if (!IsRetailerNumeric)
                {
                    throw new InvalidInputException("Retailer Id is invalid/out of range.");
                }

                String sqlFindRetailer = "Select RetailerId from Retailer Where RetailerId=" + retailerId + " AND IsActive=1";
                command.CommandText = sqlFindRetailer;
                connection.Open();
                Object IsFoundRetailer = command.ExecuteScalar(); connection.Close();

                //if retailer not found
                if (IsFoundRetailer==null)
                {
                    return Json(new { SearchResult = "notfound" }, JsonRequestBehavior.AllowGet);
                }

                //Retailer found.
                return Json(new { SearchResult = "found", url = "../DSR/EditRetailer/Index?sid=" + sessionId + "&retailerid=" + retailerId.ToString() + "&sd=" + encryptedSessionDigest + "" }, JsonRequestBehavior.AllowGet);

            }
            catch (sCommonLib.InvalidInputException Ex)
            {
                if (connection.State == ConnectionState.Open) { connection.Close(); }
                connection.Dispose();
                var path = HttpContext.Server.MapPath("~/App_Data");
                ExceptionLogger.CreateLog(Ex, path);
                return Json(new { SearchResult = "usererror",ErrorDetail=Ex.Message }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception Ex)
            {
                if (connection.State == ConnectionState.Open) { connection.Close(); }
                connection.Dispose();

                var path = HttpContext.Server.MapPath("~/App_Data");
                ExceptionLogger.CreateLog(Ex, path);
                return Json(new { SearchResult = "systemerror" }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult SearchByElMsisdn(FormCollection data)
        {
            String encryptedSessionDigest = data["SessionId"].ToString();
            String sessionId = MyCrypto.GetDecryptedQueryString(encryptedSessionDigest);

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
                    return Json(new { SearchResult = "sessiontimeout", url = "/Dsr/Login/Index" }, JsonRequestBehavior.AllowGet);
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
                    return Json(new { SearchResult = "sessiontimeout", url = "/Dsr/Login/Index" }, JsonRequestBehavior.AllowGet);
                }

                //No need
                //Int32 DsrId = Convert.ToInt32(dtSessionInfo.Rows[0]["DsrId"]);
                //Int32 surveyorId = Convert.ToInt32(dtSessionInfo.Rows[0]["SurveyorId"]);

                //<-------------------------Session related checking
                Int32 ElMsisdn;
                Boolean IsElMsisdnNumeric = Int32.TryParse(data["ElMsisdn"].ToString(), out ElMsisdn);
                if (!IsElMsisdnNumeric)
                {
                    throw new InvalidInputException("EL MSISDN is invalid or out of range. ");
                }

               // String sqlFindRetailer = "Select RetailerId from ElMsisdn Where ElMsisdn=" + ElMsisdn + "";

                String sqlFindRetailer = " SELECT R.RetailerId FROM ElMsisdn AS E INNER JOIN Retailer AS R ON E.RetailerId = R.RetailerId WHERE E.ElMsisdn = " + ElMsisdn + " AND R.IsActive=1";
                command.CommandText = sqlFindRetailer;
                connection.Open();

                DataTable dtRetailerInfo = new DataTable();
                using (SqlDataAdapter da=new SqlDataAdapter(command))
                {
                    da.Fill(dtRetailerInfo);
                }
                connection.Close();

                if (dtRetailerInfo.Rows.Count == 0)
                {
                    return Json(new { SearchResult = "notfound" }, JsonRequestBehavior.AllowGet);
                }

                if (dtRetailerInfo.Rows.Count > 1)
                {
                    return Json(new { SearchResult = "multiple" }, JsonRequestBehavior.AllowGet);
                }

                Int32 retailerId = Convert.ToInt32(dtRetailerInfo.Rows[0]["RetailerId"]);

                //No need to match tagged dsr with logged dsr
                //if (dtRetailerInfo.Rows[0]["DsrId"] != DBNull.Value)
                //{
                //    Int32 taggedDsrId = Convert.ToInt32(dtRetailerInfo.Rows[0]["DsrId"]); // Dsr Id that was primarily tagged with this retailer.
                //    if (DsrId != taggedDsrId)
                //    {
                //        throw new InvalidInputException("Unauthorized access. This retailer belongs to other DSR.");
                //    }
                //}
               

                return Json(new { SearchResult = "found", url = "../Dsr/EditRetailer/Index?sid=" + sessionId + "&retailerid=" + retailerId.ToString() + "&sd=" + encryptedSessionDigest + "" }, JsonRequestBehavior.AllowGet);
            }
            catch (sCommonLib.InvalidInputException Ex)
            {
                if (connection.State == ConnectionState.Open) { connection.Close(); }
                connection.Dispose();
                var path = HttpContext.Server.MapPath("~/App_Data");
                ExceptionLogger.CreateLog(Ex, path);
                return Json(new { SearchResult = "usererror", ErrorDetail = Ex.Message }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception Ex)
            {
                if (connection.State == ConnectionState.Open) { connection.Close(); }
                connection.Dispose();

                var path = HttpContext.Server.MapPath("~/App_Data");
                ExceptionLogger.CreateLog(Ex, path);
                return Json(new { SearchResult = "systemerror" }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult SearchBySimPosCode(FormCollection data)
        {
            String encryptedSessionDigest = data["SessionId"].ToString();
            String sessionId = MyCrypto.GetDecryptedQueryString(encryptedSessionDigest);

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
                    return Json(new { SearchResult = "sessiontimeout", url = "/Dsr/Login/Index" }, JsonRequestBehavior.AllowGet);
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
                    return Json(new { SearchResult = "sessiontimeout", url = "/Dsr/Login/Index" }, JsonRequestBehavior.AllowGet);
                }

                //No need
                //Int32 DsrId = Convert.ToInt32(dtSessionInfo.Rows[0]["DsrId"]);
                //Int32 surveyorId = Convert.ToInt32(dtSessionInfo.Rows[0]["SurveyorId"]);
                //<-------------------------Session related checking

                String SimPosCode=data["SimPosCode"].ToString().Trim();

               // String sqlFindRetailer = "Select RetailerId from SimPosCode Where SimPosCode='" + SimPosCode + "'";
                String sqlFindRetailer = "SELECT R.RetailerId FROM dbo.Retailer AS R INNER JOIN dbo.SimPosCode AS S ON R.RetailerId = S.RetailerId WHERE S.SimPosCode = '" + SimPosCode + "' AND R.IsActive=1";

                command.CommandText = sqlFindRetailer;
                DataTable dtRetailerInfo = new DataTable();
                using (SqlDataAdapter da=new SqlDataAdapter(command))
                {
                    connection.Open();
                    da.Fill(dtRetailerInfo);
                    connection.Close();
                }

                if (dtRetailerInfo.Rows.Count!=1)
                {
                    return Json(new { SearchResult = "notfound" }, JsonRequestBehavior.AllowGet);
                }

                Int32 retailerId = Convert.ToInt32(dtRetailerInfo.Rows[0]["RetailerId"]);

                //No need
                //Int32 taggedDsrId = Convert.ToInt32(dtRetailerInfo.Rows[0]["DsrId"]); // Dsr Id that was primarily tagged with this retailer.
                //if (DsrId != taggedDsrId)
                //{
                //    throw new InvalidInputException("Unauthorized access. This retailer belongs to other DSR.");
                //}

                return Json(new { SearchResult = "found", url = "../DSR/EditRetailer/Index?sid=" + sessionId + "&retailerid=" + retailerId.ToString() + "&sd=" + encryptedSessionDigest + "" }, JsonRequestBehavior.AllowGet);

            }
            catch (sCommonLib.InvalidInputException Ex)
            {
                if (connection.State == ConnectionState.Open) { connection.Close(); }
                connection.Dispose();
                var path = HttpContext.Server.MapPath("~/App_Data");
                ExceptionLogger.CreateLog(Ex, path);
                return Json(new { SearchResult = "usererror", ErrorDetail = Ex.Message }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception Ex)
            {
                if (connection.State == ConnectionState.Open) { connection.Close(); }
                connection.Dispose();

                var path = HttpContext.Server.MapPath("~/App_Data");
                ExceptionLogger.CreateLog(Ex, path);
                return Json(new { SearchResult = "systemerror" }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult SearchByQr(FormCollection data)
        {
            String encryptedSessionDigest = data["SessionId"].ToString();
            String sessionId = MyCrypto.GetDecryptedQueryString(encryptedSessionDigest);


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
                    return Json(new { SearchResult = "sessiontimeout", url = "/Dsr/Login/Index" }, JsonRequestBehavior.AllowGet);
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
                    return Json(new { SearchResult = "sessiontimeout", url = "/Dsr/Login/Index" }, JsonRequestBehavior.AllowGet);
                }

                //No need
                //Int32 DsrId = Convert.ToInt32(dtSessionInfo.Rows[0]["DsrId"]);
                //Int32 surveyorId = Convert.ToInt32(dtSessionInfo.Rows[0]["SurveyorId"]);
                //<-------------------------Session related checking

                Int32 QrValue;
                Boolean IsRetailerNumeric = Int32.TryParse(data["QrCode"].ToString(), out QrValue);
                if (!IsRetailerNumeric)
                {
                    throw new InvalidInputException("QR code invalid/out of range.");
                }

                String sqlFindRetailer = "Select RetailerId from Retailer Where QrCodeId=" + QrValue + " AND IsActive=1";
                command.CommandText = sqlFindRetailer;
                DataTable dtRetailerInfo = new DataTable();
                using (SqlDataAdapter da=new SqlDataAdapter(command))
                {
                    connection.Open(); da.Fill(dtRetailerInfo); connection.Close();
                }

                if (dtRetailerInfo.Rows.Count != 1)
                {
                    return Json(new { SearchResult = "notfound" }, JsonRequestBehavior.AllowGet);
                }

                Int32 retailerId = Convert.ToInt32(dtRetailerInfo.Rows[0]["RetailerId"]);
               
                //No need
                //Int32 taggedDsrId = Convert.ToInt32(dtRetailerInfo.Rows[0]["DsrId"]); // Dsr Id that was primarily tagged with this retailer.
                //if (DsrId != taggedDsrId)
                //{
                //    throw new InvalidInputException("Unauthorized access. This retailer belongs to other DSR.");
                //}

                return Json(new { SearchResult = "found", url = "../DSR/EditRetailer/Index?sid=" + sessionId + "&retailerid=" + retailerId.ToString() + "&sd=" + encryptedSessionDigest + "" }, JsonRequestBehavior.AllowGet);
            }
            catch (sCommonLib.InvalidInputException Ex)
            {
                if (connection.State == ConnectionState.Open) { connection.Close(); }
                connection.Dispose();
                var path = HttpContext.Server.MapPath("~/App_Data");
                ExceptionLogger.CreateLog(Ex, path);
                return Json(new { SearchResult = "usererror", ErrorDetail = Ex.Message }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception Ex)
            {
                if (connection.State == ConnectionState.Open) { connection.Close(); }
                connection.Dispose();

                var path = HttpContext.Server.MapPath("~/App_Data");
                ExceptionLogger.CreateLog(Ex, path);
                return Json(new { SearchResult = "systemerror" }, JsonRequestBehavior.AllowGet);
            }
        }

	}
}