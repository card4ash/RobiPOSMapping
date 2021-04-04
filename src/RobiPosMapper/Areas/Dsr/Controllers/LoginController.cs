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
    public class LoginController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public JsonResult Index(FormCollection data)
        {
            String ConnectionString = WebConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;
            SqlConnection connection = new SqlConnection(ConnectionString);
            try
            {
                //First check 
                Int32 DsrMsisdn;
                if (!Int32.TryParse(data["DsrMsisdn"].ToString(), out DsrMsisdn))
                {
                    throw new InvalidInputException("MSISDN invalid/out of range.");
                }


                String LoginName = data["LoginName"].ToString();

                String LoginPassword = data["LoginPassword"].ToString();

                string sqlSelectSurveyor = "select SurveyorId,LoginName,LoginPassword from Surveyors where LoginName = @LoginName AND IsActive=1 AND SurveyorId>0";

                DataTable dtResult = new DataTable();

                using (SqlCommand command=new SqlCommand(sqlSelectSurveyor,connection))
                {
                    command.Parameters.AddWithValue("@LoginName",LoginName);
                    using (SqlDataAdapter da = new SqlDataAdapter(command))
                    {
                        connection.Open();
                        da.Fill(dtResult); connection.Close();
                    }
                }

                if (dtResult.Rows.Count==0)
                {
                    return Json(new { result = "InvalidLogin"},JsonRequestBehavior.AllowGet);
                }
                else
                {
                    Int32 surveyorId =Convert.ToInt32(dtResult.Rows[0][0]);
                    String dbPassValue = dtResult.Rows[0][2].ToString();
                    Boolean isPasswordMatched = LoginPassword.Equals(dbPassValue, StringComparison.Ordinal);


                    if (isPasswordMatched)
                    {
                        //Get DSR & RSP Information
                        String sqlSelectDsr = "SELECT PersonId,RspId from Person WHERE PersonMsisdn=@PersonMsisdn";
                        DataTable dtDsrInfo = new DataTable();
                        using (SqlCommand command = new SqlCommand(sqlSelectDsr, connection))
                        {
                            command.Parameters.AddWithValue("@PersonMsisdn", DsrMsisdn);
                            using (SqlDataAdapter da = new SqlDataAdapter(command))
                            {
                                connection.Open();
                                da.Fill(dtDsrInfo); connection.Close();
                            }
                        }

                        if (dtDsrInfo.Rows.Count==0)
                        {
                            return Json(new { result = "InvalidDsr" }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            Int32 DsrId =Convert.ToInt32(  dtDsrInfo.Rows[0][0]);
                            Int32 RspId =Convert.ToInt32(  dtDsrInfo.Rows[0][1]);
                            string sessionId = Guid.NewGuid().ToString();
                            string sqlInsert = "INSERT INTO SessionInfo(SessionId,SurveyorId,DsrId,RspId,CreatedOn) values(@SessionId,@SurveyorId,@DsrId,@RspId,@CreatedOn)";
                            using (SqlCommand command = new SqlCommand(sqlInsert, connection))
                            {
                                command.Parameters.AddWithValue("@SessionId", sessionId);
                                command.Parameters.AddWithValue("@SurveyorId",surveyorId );
                                command.Parameters.AddWithValue("@DsrId", DsrId);
                                command.Parameters.AddWithValue("@RspId", RspId);
                                command.Parameters.AddWithValue("@CreatedOn", DateTime.Now);
                                connection.Open();
                                command.ExecuteNonQuery();
                                connection.Close();
                            }

                            String sessionDigest = MyCrypto.GetEncryptedQueryString(sessionId);
                            return Json(new { result = "Redirect", url = "/Dsr/Home/Index?sid=" + sessionId + "&sd=" + sessionDigest });
                        }
                    }
                    else
                    {
                        return Json(new { result = "InvalidPassword" }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (sCommonLib.InvalidInputException Ex)
            {
                if (connection.State == ConnectionState.Open) { connection.Close(); }
                var path = HttpContext.Server.MapPath("~/App_Data");
                ExceptionLogger.CreateLog(Ex, path);
                return Json(new { result = "Error" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception Ex)
            {
                if (connection.State == ConnectionState.Open) { connection.Close(); }
                var path = HttpContext.Server.MapPath("~/App_Data");
                ExceptionLogger.CreateLog(Ex, path);
                return Json(new { result = "Error" }, JsonRequestBehavior.AllowGet);
            }

        }


        public ActionResult LogOut()
        {
            
            String encryptedSessionDigest = Request.QueryString["sd"].ToString();
            String sessionId = MyCrypto.GetDecryptedQueryString(encryptedSessionDigest);

            String ConnectionString = WebConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;

            SqlConnection connection = new SqlConnection(ConnectionString);
            SqlCommand command = new SqlCommand(); command.Connection = connection;

            try
            {
                command.Parameters.Clear();
                command.CommandText = "Delete from SessionInfo Where SessionId=@SessionId";
                command.Parameters.AddWithValue("@SessionId", sessionId);
                connection.Open(); command.ExecuteNonQuery(); connection.Close();
               // return RedirectToAction("Index", "LogIn", new { area = "Dsr" });
            }
            catch (Exception Ex)
            {
                if (connection.State == ConnectionState.Open) { connection.Close(); }
                connection.Dispose();

                var path = HttpContext.Server.MapPath("~/App_Data");
                ExceptionLogger.CreateLog(Ex, path);
                //return View("index");
            }

            return RedirectToAction("Index", "LogIn", new { area = "Dsr" });
        }
	}
}