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
using System.IO;

namespace RobiPosMapper.Areas.Dsr.Controllers
{
    public class ChangePhotoController : Controller
    {

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
                          
                 return View("index");

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
                //<-------------------------Session related checking

                Int32 retailerId;

                Boolean IsRetailerNumeric = Int32.TryParse(data["RetailerId"].ToString(), out retailerId);
                if (!IsRetailerNumeric)
                {
                    throw new InvalidInputException("Retailer Id is invalid/out of range.");
                }

                String sqlFindRetailer = "Select RetailerId from Retailer Where RetailerId=" + retailerId + " AND IsActive=1";
                command.CommandText = sqlFindRetailer;
                connection.Open();
                Object IsFoundRetailer = command.ExecuteScalar(); connection.Close();

                //if retailer not found
                if (IsFoundRetailer == null)
                {
                    return Json(new { SearchResult = "notfound" }, JsonRequestBehavior.AllowGet);
                }

                //Retailer found.
                return Json(new { SearchResult = "found", url = "../DSR/ChangePhoto/ChangePhotoContent?sid=" + sessionId + "&retailerid=" + retailerId.ToString() + "&sd=" + encryptedSessionDigest + "" }, JsonRequestBehavior.AllowGet);

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

        public ActionResult ChangePhotoContent()
        {
           
            String ConnectionString = WebConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;

            SqlConnection connection = new SqlConnection(ConnectionString);
            SqlCommand command = new SqlCommand(); command.Connection = connection;
           
            try
            {
                if (Request.QueryString["sid"] == null || Request.QueryString["sd"] == null) //sid = SessionId (string), sd = SessionDigest (string)
                {
                    return RedirectToAction("Index", "LogIn", new { area = "Dsr" });
                }

                String sessionId = Request.QueryString["sid"].ToString();
                String encryptedSessionDigest = Request.QueryString["sd"].ToString();
                String sessionDigest = MyCrypto.GetDecryptedQueryString(encryptedSessionDigest);

                if (!sessionId.Equals(sessionDigest, StringComparison.Ordinal))
                {
                    return RedirectToAction("Index", "LogIn", new { area = "Dsr" });
                }


                Int32 RetailerId = Convert.ToInt32(Request.QueryString["retailerid"].ToString());


                string sqlSelectSessionInfo = "Select * from SessionInfo where SessionId=@SessionId";
                command.CommandText = sqlSelectSessionInfo;
                command.Parameters.AddWithValue("@SessionId", sessionId);
                DataTable dtSessionInfo = new DataTable();
                using (SqlDataAdapter da = new SqlDataAdapter(command))
                {
                    connection.Open(); da.Fill(dtSessionInfo); connection.Close();
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

                Int32 DsrId = Convert.ToInt32(dtSessionInfo.Rows[0]["DsrId"]);
                Int32 surveyorId = Convert.ToInt32(dtSessionInfo.Rows[0]["SurveyorId"]);

                command.CommandText = "SELECT RetailerId,RetailerName,Address FROM Retailer WHERE RetailerId="+ RetailerId +"";
                DataTable tblRetailer = new DataTable();
                using (SqlDataAdapter da=new SqlDataAdapter(command))
                {
                    connection.Open(); da.Fill(tblRetailer); connection.Close();
                }

                //Note- No need to check RetailerId Existance. Its already done where it comes from.

                //ViewBag
                ViewBag.SessionId = sessionId; ViewBag.SessionDigest = encryptedSessionDigest;
                ViewBag.RetailerId = RetailerId;
                ViewBag.RetailerName = tblRetailer.Rows[0]["RetailerName"].ToString();
                ViewBag.Address= tblRetailer.Rows[0]["Address"].ToString();
                ViewBag.PhotoName = string.Format("{0}.png",RetailerId );

            }
            catch (sCommonLib.InvalidInputException Ex)
            {
                if (connection.State == ConnectionState.Open) { connection.Close(); }
                connection.Dispose();
                var path = HttpContext.Server.MapPath("~/App_Data");
                ExceptionLogger.CreateLog(Ex, path);
            }
            catch (Exception Ex)
            {
                if (connection.State == ConnectionState.Open) { connection.Close(); }
                connection.Dispose();

                var path = HttpContext.Server.MapPath("~/App_Data");
                ExceptionLogger.CreateLog(Ex, path);
            }

            return View();
        }

        [HttpPost]
        public JsonResult UpdatePhoto(FormCollection data)
        {
            string strSurveyorInfo = string.Empty; // this is only for include his id in error log.
            string strRetailerInfo = string.Empty; // this is only for include his id in error log.

            String encryptedSessionDigest = data["SessionId"].ToString();
            String sessionId = MyCrypto.GetDecryptedQueryString(encryptedSessionDigest);

            String ConnectionString = WebConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;
            SqlConnection connection = new SqlConnection(ConnectionString);
            SqlCommand command = new SqlCommand(); command.Connection = connection;
            SqlTransaction transaction = null;

            try
            {
                //---> Force to work between 8AM-6:30PM
                DateTime workStartTime = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, 08, 00, 00);
                DateTime workEndTime = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, 18, 30, 00);
                DateTime currentDateTime = DateTime.Now;


                if (((currentDateTime > workStartTime) && (currentDateTime < workEndTime)) == false)
                {
                    throw new InvalidInputException("You can work only from 8:00 am - 6:30 pm.");
                }
                //<--- Force to work between 8AM-8PM

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
                    return Json(new { IsError = true, IsSessionTimeOut = true, url = "/Dsr/Login/Index" }, JsonRequestBehavior.AllowGet);
                }

                DateTime SessionCreatedDateTime = Convert.ToDateTime(dtSessionInfo.Rows[0]["CreatedOn"]);
                DateTime currentTime = DateTime.Now;
                double hours = (currentTime - SessionCreatedDateTime).TotalHours;

                if (hours > 12)
                {
                    command.Parameters.Clear();
                    command.CommandText = "Delete from SessionInfo Where SessionId=@SessionId";
                    command.Parameters.AddWithValue("@SessionId", sessionId);
                    connection.Open(); command.ExecuteNonQuery(); command.Parameters.Clear(); connection.Close();
                    return Json(new { IsError = true, IsSessionTimeOut = true, url = "/Dsr/Login/Index" }, JsonRequestBehavior.AllowGet);
                }


                Int32 CurrentDsrId = Convert.ToInt32(dtSessionInfo.Rows[0]["DsrId"]);
                Int32 surveyorId = Convert.ToInt32(dtSessionInfo.Rows[0]["SurveyorId"]);
                strSurveyorInfo = " SR-" + surveyorId.ToString() + ".";
                Int32 RspId = Convert.ToInt32(dtSessionInfo.Rows[0]["RspId"]);
                //<-------------------------Session related checking

                connection.Open();
                transaction = connection.BeginTransaction();
                command.Transaction = transaction;

                Int32 RetailerId = Convert.ToInt32(data["RetailerId"]);
                strRetailerInfo = " Current Retailer ID-" + RetailerId.ToString() + ".";


                //RetailerPhoto
                Byte[] RetailerPhoto;
                String imageName = "default.png";
                if (Request.Files["RetailerPhoto"] != null)
                {
                    imageName = RetailerId.ToString() + ".png";

                    var directory = HttpContext.Server.MapPath("~/Photos");
                    string imagePath = Path.Combine(directory, "RetailerPhoto", imageName);

                    using (var binaryReader = new BinaryReader(Request.Files["RetailerPhoto"].InputStream))
                    {
                        RetailerPhoto = binaryReader.ReadBytes(Request.Files["RetailerPhoto"].ContentLength);//image
                    }

                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }

                    FileStream fs = new FileStream(imagePath, FileMode.CreateNew);
                    BinaryWriter bw = new BinaryWriter(fs);
                    bw.Write(RetailerPhoto);
                    bw.Close();
                }

                ////first get group id
                //string sqlSelectGroupId = "Select GroupId from DuplicatePhotos Where RetailerId="+ RetailerId +"";
                //command.CommandText = sqlSelectGroupId;
                //object objGroupId = command.ExecuteScalar();

                //now get oth

                //---> Update dupli info
                String sqlUpdateDupli = "Update DuplicatePhotos set IsRemovedLaterOn=1, SurveyorId=" + surveyorId + ", UploadDateTime='"+ DateTime.Now +"', RemovalDateTime='" + DateTime.Now + "' Where RetailerId=" + RetailerId + "";
                //command.Parameters.Clear();
                command.CommandText = sqlUpdateDupli;
                command.ExecuteNonQuery();
                //---> Update dupli info

                transaction.Commit();
                connection.Close();
                command.Dispose();
                transaction.Dispose();
                connection.Dispose();

                return Json(new { IsError = false, ErrorDetails = String.Empty }, JsonRequestBehavior.AllowGet);
            }

            catch (sCommonLib.InvalidInputException Ex)
            {
                if (transaction != null) { transaction.Rollback(); }
                if (connection.State == ConnectionState.Open) { connection.Close(); }
                transaction.Dispose(); connection.Dispose();
                var path = HttpContext.Server.MapPath("~/App_Data");
                ExceptionLogger.CreateLog(Ex, path);
                return Json(new { IsError = true, IsSessionTimeOut = false, ErrorDetails = Ex.Message }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception Ex)
            {
                if (transaction != null) { transaction.Rollback(); }
                if (connection.State == ConnectionState.Open) { connection.Close(); }
                transaction.Dispose(); connection.Dispose();

                var path = HttpContext.Server.MapPath("~/App_Data");
                ExceptionLogger.CreateLog(Ex, path);
                return Json(new { IsError = true, IsSessionTimeOut = false, ErrorDetails = "Failed to save due to unknown error." + strSurveyorInfo + strRetailerInfo }, JsonRequestBehavior.AllowGet);
            }
        }


	}
}