using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;
using sCommonLib;
using RobiPosMapper.Models;
using QueryStringEncryption;

namespace RobiPosMapper.Areas.Dsr.Controllers
{
    public class NewRetailerController : Controller
    {

        public ActionResult Index()
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

                ViewBag.SessionId = sessionId; ViewBag.SessionDigest = encryptedSessionDigest;

                Int32 DsrId = Convert.ToInt32(dtSessionInfo.Rows[0]["DsrId"]);
                
                DataTable dtUserData = new DataTable();

                String sqlSelectUserData = "SELECT P.LanguagePreference, A.AreaId, A.AreaName, R.RegionId, R.RegionName FROM dbo.Person AS P INNER JOIN dbo.Area AS A ON P.AreaId = A.AreaId INNER JOIN dbo.Region AS R ON A.RegionId = R.RegionId WHERE (P.PersonId = "+ Convert.ToInt32(DsrId) +")";

                using (SqlDataAdapter da=new SqlDataAdapter(command))
                {
                    connection.Open(); command.CommandText = sqlSelectUserData; da.Fill(dtUserData); connection.Close();
                }

                String LanguagePreference = dtUserData.Rows[0]["LanguagePreference"].ToString();
                Int32 RegionId = Convert.ToInt32(dtUserData.Rows[0]["RegionId"]); ViewBag.RegionId = RegionId;
                String RegionName = dtUserData.Rows[0]["RegionName"].ToString(); ViewBag.RegionName = RegionName;
                Int32 AreaId = Convert.ToInt32(dtUserData.Rows[0]["AreaId"]); ViewBag.AreaId = AreaId;
                String AreaName = dtUserData.Rows[0]["AreaName"].ToString(); ViewBag.AreaName = AreaName;
                ViewBag.ThanaList = ThanaManager.AreaSpecificThanaList(AreaId);
                ViewBag.PosCategoryList = PosCategoryManager.PosCategoryList();
                ViewBag.PosStructureList = PosStructureManager.PosStructureList();
                ViewBag.VisitDayList = VisitDayManager.VisitDayList();
                ViewBag.ShopSignageList = ShopSignageManager.ShopSignageList();
                ViewBag.ShopTypeList = ShopTypeManager.ShopTypeList();

                return View("index");

                //if (LanguagePreference=="English")
                //{
                //    return View("index");
                //}
                //else
                //{
                //    return View("index.bn");
                //}
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
        public JsonResult CreateNewRetailer(FormCollection data)
        {
            string strSurveyorInfo = string.Empty; // this is only for include his id in error log.
            string strRetailerInfo = string.Empty; // this is only for include his id in error log.

            String encryptedSessionDigest =  data["SessionId"].ToString();
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

                if (dtSessionInfo.Rows.Count == 0)
                {
                    return Json(new { IsError = true, IsSessionError = true, url = "/Dsr/Login/Index" }, JsonRequestBehavior.AllowGet);
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
                    return Json(new { IsError = true, IsSessionError = true, url = "/Dsr/Login/Index" }, JsonRequestBehavior.AllowGet);
                }


                Int32 DsrId = Convert.ToInt32(dtSessionInfo.Rows[0]["DsrId"]);
                Int32 surveyorId = Convert.ToInt32(dtSessionInfo.Rows[0]["SurveyorId"]);
                strSurveyorInfo = " SR-" + surveyorId.ToString() + ".";

                //<-------------------------Session related checking


                connection.Open();
                transaction = connection.BeginTransaction();
                command.Transaction = transaction;

                String RetailerName = data["RetailerName"].ToString();
                String RetailerAddress = data["RetailerAddress"].ToString();
                Int32 PosCategoryId = Convert.ToInt32(data["PosCategoryId"]);

                Boolean IsElPos = Convert.ToBoolean(data["IsElMsisdn"]);

                String[] arrElMsisdn = null;

                //-->validate whether it is numenric
                if (IsElPos)
                {
                    arrElMsisdn = data["ElMsisdn"].Split(',');

                    foreach (String strElNumber in arrElMsisdn)
                    {
                        Int32 ElMsisdn;
                        Boolean IsElValid = Int32.TryParse(strElNumber, out ElMsisdn);

                        if (!IsElValid)
                        {
                            throw new InvalidInputException(strElNumber + " is an invalid EL MSISDN. It must be numeric and have 10 digits .");
                        }
                    }
                }
                //--> Just validate whether it is numenric


                Boolean IsSimPos = Convert.ToBoolean(data["IsSimPos"]);
                Boolean IsScPos = Convert.ToBoolean(data["IsScPos"]);
                Boolean HasTradeLicense = Convert.ToBoolean(data["HasTradeLicense"]);

                Int32 RegionId = Convert.ToInt32(data["RegionId"]);
                //Int32 AreaId = Convert.ToInt32(data["AreaId"]);
                Int32 AreaId;
                if (String.IsNullOrEmpty(data["AreaId"].ToString()))
                {
                    throw new InvalidInputException("Area information not found. Please check area combo." + strSurveyorInfo);
                }
                else
                {
                    Boolean isValid = Int32.TryParse(data["AreaId"].ToString(), out AreaId);
                    if (!isValid)
                    {
                        throw new InvalidInputException("Area information not found. Please check area combo." + strSurveyorInfo);
                    }
                }

                //Int32 ThanaId = Convert.ToInt32(data["ThanaId"]);
                Int32 ThanaId;
                if (String.IsNullOrEmpty(data["ThanaId"].ToString()))
                {
                    throw new InvalidInputException("Thana information not found. Please check thana combo." + strSurveyorInfo);
                }
                else
                {
                    Boolean isValid = Int32.TryParse(data["ThanaId"].ToString(), out ThanaId);
                    if (!isValid)
                    {
                        throw new InvalidInputException("Thana information not found. Please check thana combo." + strSurveyorInfo);
                    }
                }

                //Int32 WardId = Convert.ToInt32(data["WardId"]);
                Int32 WardId;
                if (String.IsNullOrEmpty(data["WardId"].ToString()))
                {
                    throw new InvalidInputException("Ward information not found. Please check ward combo." + strSurveyorInfo );
                }
                else
                {
                    Boolean isValid = Int32.TryParse(data["WardId"].ToString(), out WardId);
                    if (!isValid)
                    {
                        throw new InvalidInputException("Ward information not found. Please check ward combo." + strSurveyorInfo );
                    }
                }

                //Int32 MauzaId = Convert.ToInt32(data["MauzaId"]);
                Int32 MauzaId;
                if (String.IsNullOrEmpty(data["MauzaId"].ToString()))
                {
                    throw new InvalidInputException("Mauza information not found. Please check Mauza combo." + strSurveyorInfo );
                }
                else
                {
                    Boolean isValid = Int32.TryParse(data["MauzaId"].ToString(), out MauzaId);
                    if (!isValid)
                    {
                        throw new InvalidInputException("Mauza information not found. Please check mauza combo." + strSurveyorInfo );
                    }
                }

                //Int32 VillageId = Convert.ToInt32(data["VillageId"]);
                Int32 VillageId;
                if (String.IsNullOrEmpty(data["VillageId"].ToString()))
                {
                    throw new InvalidInputException("Village information not found. Please check Village combo." + strSurveyorInfo );
                }
                else
                {
                    Boolean isValid = Int32.TryParse(data["VillageId"].ToString(), out VillageId);
                    if (!isValid)
                    {
                        throw new InvalidInputException("Village information not found. Please check village combo." + strSurveyorInfo );
                    }
                }

                Int32 PosStructureId = Convert.ToInt32(data["PosStructureId"]);
                Int32 ShopSignageId=Convert.ToInt32(data["ShopSignageId"]);
                Int32 ShopTypeId = Convert.ToInt32(data["ShopTypeId"]);
                Int32 VisitDayId = Convert.ToInt32(data["VisitDayId"]);
                Boolean IsApartments = Convert.ToBoolean(data["IsApartments"]);
                Boolean IsSlums = Convert.ToBoolean(data["IsSlums"]);
                Boolean IsSemiUrbunHousing = Convert.ToBoolean(data["IsSemiUrbunHousing"]);
                Boolean IsRuralHousing = Convert.ToBoolean(data["IsRuralHousing"]);
                Boolean IsShoppingMall = Convert.ToBoolean(data["IsShoppingMall"]);
                Boolean IsRetailHub = Convert.ToBoolean(data["IsRetailHub"]);
                Boolean IsMobileDeviceMarket = Convert.ToBoolean(data["IsMobileDeviceMarket"]);
                Boolean IsBazaar = Convert.ToBoolean(data["IsBazaar"]);
                Boolean IsOfficeArea = Convert.ToBoolean(data["IsOfficeArea"]);
                Boolean IsGarmentsMajorityArea = Convert.ToBoolean(data["IsGarmentsMajorityArea"]);
                Boolean IsGeneralIndustrialArea = Convert.ToBoolean(data["IsGeneralIndustrialArea"]);
                Boolean IsUrbanTransitPoints = Convert.ToBoolean(data["IsUrbanTransitPoints"]);
                Boolean IsRuralTransitPoints = Convert.ToBoolean(data["IsRuralTransitPoints"]);
                Boolean IsUrbanYouthHangouts = Convert.ToBoolean(data["IsUrbanYouthHangouts"]);
                Boolean IsSemiUrbanYouthHangouts = Convert.ToBoolean(data["IsSemiUrbanYouthHangouts"]);
                Boolean IsRuralYouthHangouts = Convert.ToBoolean(data["IsRuralYouthHangouts"]);
                Boolean IsTouristDestinations = Convert.ToBoolean(data["IsTouristDestinations"]);
                

                Int32 QrValue;
                Boolean IsQrNumeric = Int32.TryParse(data["QrValue"].ToString(),out QrValue);
                if (!IsQrNumeric)
                {
                    throw new InvalidInputException("QR Code is not in correct format.");
                }


                //--> check QrCode
                //--> in QrCode table
                string sqlFindQrCodeExistance = "SELECT QrCodeId FROM dbo.QrCode WHERE (QrCodeId = @QrCodeId)";
                command.CommandText = sqlFindQrCodeExistance;
                command.Parameters.Clear();
                command.Parameters.AddWithValue("@QrCodeId", QrValue);

                object IsQrFound = command.ExecuteScalar();
                if (IsQrFound == null)
                {
                   // throw new InvalidInputException("You submitted an unknown QR code.");

                    throw new InvalidInputException("This QR code-" + QrValue.ToString() + " does not exist in database. You can not tag it." + strSurveyorInfo);
                }
                //<-- in QrCode table

                //--> in Retailer table
                string sqlFindQrCodeDuplicacy = "SELECT RetailerId FROM Retailer WHERE (QrCodeId = @QrCodeId)";
                command.CommandText = sqlFindQrCodeDuplicacy;
                command.Parameters.Clear();
                command.Parameters.AddWithValue("@QrCodeId", QrValue);

                object IsQrDuplicate = command.ExecuteScalar();
                if (IsQrDuplicate != null)
                {
                    //throw new InvalidInputException("This QR Code already in use.");
                    throw new InvalidInputException("This QR Code-" + QrValue.ToString() + " already used for retailer-" + IsQrDuplicate.ToString() + "." + strSurveyorInfo);
                }
                //<-- in Retailer table

                //<-- check QrCode


                Decimal Latitude=Convert.ToDecimal(data["Latitude"]);
                Decimal Longitude=Convert.ToDecimal(data["Longitude"]);
                Decimal AccuracyLevel = Convert.ToDecimal(data["Accuracy"]);
                String remarks = data["Remarks"].ToString();
            

                String sqlSelectRsp = "Select RspId from Person where PersonId="+ DsrId +"";
                command.CommandText = sqlSelectRsp;
                command.Parameters.Clear();
                Int32 RspId = Convert.ToInt32(command.ExecuteScalar());

                //--> insert data in Retailer Table
                string sqlInsertRetailer = "insert into Retailer(RetailerName,Address,PosCategoryId,RegionId,AreaId,ThanaId,WardId,MauzaId,VillageId,IsElPos,IsSimPos,IsScPos,Latitude,Longitude,AccuracyLevel,VisitDayId,DefaultPhotoName,PosStructureId,ShopSignageId,ShopTypeId,RspId,DsrActivityDateTime,CreatedBy,CreateDateTime,IsActive,ModifiedBy,ModifiedDateTime,Remarks,RetailerStatusId,IsVerifiedByDsrs,DsrId,IsApartments,IsSlums,IsSemiUrbunHousing,IsRuralHousing,IsShoppingMall,IsRetailHub,IsMobileDeviceMarket,IsBazaar,IsOfficeArea,IsGarmentsMajorityArea,IsGeneralIndustrialArea,IsUrbanTransitPoints,IsRuralTransitPoints,IsUrbanYouthHangouts,IsSemiUrbanYouthHangouts,IsRuralYouthHangouts,IsTouristDestinations,QrCodeId,SurveyorId,IsReevaluated,SurveyorActivityDateTime,HasTradeLicense) values (@RetailerName, @Address,@PosCategoryId, @RegionId, @AreaId, @ThanaId,@WardId,@MauzaId,@VillageId,@IsElPos,@IsSimPos,@IsScPos,@Latitude,@Longitude,@AccuracyLevel,@VisitDayId,@DefaultPhotoName,@PosStructureId,@ShopSignageId,@ShopTypeId,@RspId,@DsrActivityDateTime,@CreatedBy,@CreateDateTime,@IsActive,@ModifiedBy,@ModifiedDateTime,@Remarks,@RetailerStatusId,@IsVerifiedByDsrs,@DsrId,@IsApartments,@IsSlums,@IsSemiUrbunHousing,@IsRuralHousing,@IsShoppingMall,@IsRetailHub,@IsMobileDeviceMarket,@IsBazaar,@IsOfficeArea,@IsGarmentsMajorityArea,@IsGeneralIndustrialArea,@IsUrbanTransitPoints,@IsRuralTransitPoints,@IsUrbanYouthHangouts,@IsSemiUrbanYouthHangouts,@IsRuralYouthHangouts, @IsTouristDestinations, @QrCodeId,@SurveyorId,@IsReevaluated,@SurveyorActivityDateTime,@HasTradeLicense);select SCOPE_IDENTITY();";
                
                command.CommandText = sqlInsertRetailer;
                command.Parameters.Clear();
                command.Parameters.AddWithValue("@RetailerName", RetailerName);
                command.Parameters.AddWithValue("@Address", RetailerAddress);
                command.Parameters.AddWithValue("@PosCategoryId", PosCategoryId);
                command.Parameters.AddWithValue("@RegionId", RegionId);
                command.Parameters.AddWithValue("@AreaId", AreaId);
                command.Parameters.AddWithValue("@ThanaId", ThanaId);
                command.Parameters.AddWithValue("@WardId", WardId);
                command.Parameters.AddWithValue("@MauzaId", MauzaId);
                command.Parameters.AddWithValue("@VillageId", VillageId);
                command.Parameters.AddWithValue("@IsElPos",IsElPos);
                command.Parameters.AddWithValue("@IsSimPos", IsSimPos);
                command.Parameters.AddWithValue("@IsScPos", IsScPos);
                command.Parameters.AddWithValue("@Latitude", Latitude);
                command.Parameters.AddWithValue("@Longitude", Longitude);
                command.Parameters.AddWithValue("@AccuracyLevel", AccuracyLevel);
                command.Parameters.AddWithValue("@VisitDayId", VisitDayId);
                command.Parameters.AddWithValue("@DefaultPhotoName", "default.png" );
                command.Parameters.AddWithValue("@PosStructureId", PosStructureId);
                command.Parameters.AddWithValue("@ShopSignageId", ShopSignageId);
                command.Parameters.AddWithValue("@ShopTypeId", ShopTypeId);
                command.Parameters.AddWithValue("@RspId", RspId);
                command.Parameters.AddWithValue("@DsrActivityDateTime", DateTime.Now);
                command.Parameters.AddWithValue("@CreatedBy", DsrId);
                command.Parameters.AddWithValue("@CreateDateTime", DateTime.Now);
                command.Parameters.AddWithValue("@IsActive", true);
                command.Parameters.AddWithValue("@ModifiedBy", DsrId);
                command.Parameters.AddWithValue("@ModifiedDateTime", DateTime.Now);
                command.Parameters.AddWithValue("@Remarks", remarks);
                command.Parameters.AddWithValue("@RetailerStatusId", 3);
                command.Parameters.AddWithValue("@IsVerifiedByDsrs", false);
                command.Parameters.AddWithValue("@DsrId", DsrId);
                command.Parameters.AddWithValue("@IsApartments", IsApartments);
                command.Parameters.AddWithValue("@IsSlums", IsSlums);
                command.Parameters.AddWithValue("@IsSemiUrbunHousing",IsSemiUrbunHousing );
                command.Parameters.AddWithValue("@IsRuralHousing", IsRuralHousing);
                command.Parameters.AddWithValue("@IsShoppingMall", IsShoppingMall);
                command.Parameters.AddWithValue("@IsRetailHub",IsRetailHub );
                command.Parameters.AddWithValue("@IsMobileDeviceMarket", IsMobileDeviceMarket);
                command.Parameters.AddWithValue("@IsBazaar",IsBazaar );
                command.Parameters.AddWithValue("@IsOfficeArea", IsOfficeArea);
                command.Parameters.AddWithValue("@IsGarmentsMajorityArea", IsGarmentsMajorityArea);
                command.Parameters.AddWithValue("@IsGeneralIndustrialArea",IsGeneralIndustrialArea );
                command.Parameters.AddWithValue("@IsUrbanTransitPoints", IsUrbanTransitPoints);
                command.Parameters.AddWithValue("@IsRuralTransitPoints", IsRuralTransitPoints);
                command.Parameters.AddWithValue("@IsUrbanYouthHangouts", IsUrbanYouthHangouts);
                command.Parameters.AddWithValue("@IsSemiUrbanYouthHangouts", IsSemiUrbanYouthHangouts);
                command.Parameters.AddWithValue("@IsRuralYouthHangouts", IsRuralYouthHangouts);
                command.Parameters.AddWithValue("@IsTouristDestinations", IsTouristDestinations);
                command.Parameters.AddWithValue("@QrCodeId", QrValue);
                command.Parameters.AddWithValue("@SurveyorId", surveyorId);
                command.Parameters.AddWithValue("@IsReevaluated", false);
                command.Parameters.AddWithValue("@SurveyorActivityDateTime", DateTime.Now);
                command.Parameters.AddWithValue("@HasTradeLicense", HasTradeLicense);
                Int32 RetailerId = Convert.ToInt32(command.ExecuteScalar());

                //--> Update/Insert EL MSISDN in ElMsisdn table
                if (IsElPos)
                {
                    foreach (String strElNumber in arrElMsisdn)
                    {
                        Int32 ElMsisdn=Convert.ToInt32(strElNumber); //numeric validation has been done in few lines above.

                        //--> check ElMsisdn in ElMsisdn table
                        string sqlFindElMsisdn = "SELECT ElMsisdnId FROM ElMsisdn WHERE (ElMsisdn = @ElMsisdn)";
                        command.CommandText = sqlFindElMsisdn;
                        command.Parameters.Clear();
                        command.Parameters.Add("@ElMsisdn", SqlDbType.Int).Value = ElMsisdn;

                        object elMsisdnId = command.ExecuteScalar();
                        if (elMsisdnId != null) //if found
                        {
                            //-->Update in ElMsisdn Table
                            string sqlUpdateElMsisdn = "Update ElMsisdn Set RetailerId=@RetailerId Where ElMsisdnId=@ElMsisdnId";
                            command.CommandText = sqlUpdateElMsisdn;
                            command.Parameters.Clear();
                            command.Parameters.Add("@RetailerId", SqlDbType.Int).Value = RetailerId;
                            command.Parameters.Add("@ElMsisdnId", SqlDbType.Int).Value = Convert.ToInt32(elMsisdnId);
                            command.ExecuteNonQuery();
                            //<--Update in ElMsisdn Table
                        }
                        else
                        { //if not found.
                            //--> Insert in ElMsisdn table
                            string sqlInsertElMsisdn = "Insert into ElMsisdn(ElMsisdn,RetailerId) VALUES(@ElMsisdn,@RetailerId)";
                            command.CommandText = sqlInsertElMsisdn;
                            command.Parameters.Clear();
                            command.Parameters.AddWithValue("@ElMsisdn", ElMsisdn);
                            command.Parameters.AddWithValue("@RetailerId", RetailerId);
                            command.ExecuteNonQuery();
                            //<-- Insert in ElMsisdn table
                        }
                        //<-- check ElMsisdn in ElMsisdn table
                    }
                }
                //<-- Update/Insert EL MSISDN in ElMsisdn table

               
                //--> Insert SIM POS Code in SimPosCode table
                if (IsSimPos)
                {
                    String[]  arrSimPosCode = data["SimPosCode"].Split(',');
                    foreach (String strSimPosCode in arrSimPosCode)
                    {
                        string sqlInsertSimPosCode = "INSERT INTO SimPosCode(SimPosCode,RetailerId)  VALUES(@SimPosCode,@RetailerId)";
                        command.CommandText = sqlInsertSimPosCode;
                        command.Parameters.Clear();
                        command.Parameters.AddWithValue("@SimPosCode", strSimPosCode);
                        command.Parameters.AddWithValue("@RetailerId", RetailerId);
                        command.ExecuteNonQuery();
                    }
                }
                //<-- Insert SIM POS Code in SimPosCode table


                //RetailerPhoto
                Byte[] RetailerPhoto;

                if (Request.Files["RetailerPhoto"] != null)
                {
                    String imageName = RetailerId.ToString() + ".png";

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
                else
                {
                    throw new InvalidInputException("Please send retailer photo");
                }


             command.Parameters.Clear(); command.CommandText = "Update Retailer Set DefaultPhotoName='"+ RetailerId.ToString() +".png' where RetailerId="+ RetailerId +"";
             command.ExecuteNonQuery();

             //---> Insert into SrRetailerLog
             String sqlInsertIntoSrRetailerLog = "Insert Into SrRetailerLog(RetailerId,SrId) Values(@RetailerId,@SrId)";
             command.Parameters.Clear();
             command.CommandText = sqlInsertIntoSrRetailerLog;
             command.Parameters.AddWithValue("@RetailerId", RetailerId);
             command.Parameters.AddWithValue("@SrId", surveyorId);
             command.ExecuteNonQuery();
             //<--- Insert into SrRetailerLog

                transaction.Commit();
                connection.Close();
                command.Dispose();
                transaction.Dispose();
                connection.Dispose();

                return Json(new { IsError = false, ErrorDetails = String.Empty }, JsonRequestBehavior.AllowGet);
            }

            catch(sCommonLib.InvalidInputException Ex)
            {
                if (transaction != null) { transaction.Rollback(); }
                if (connection.State == ConnectionState.Open) { connection.Close(); }
                transaction.Dispose(); connection.Dispose();
                var path = HttpContext.Server.MapPath("~/App_Data");
                ExceptionLogger.CreateLog(Ex, path);
                return Json(new { IsError = true, IsSessionError = false , ErrorDetails = Ex.Message}, JsonRequestBehavior.AllowGet);
            }
            catch (Exception Ex)
            {
                if (transaction != null) { transaction.Rollback(); }
                if (connection.State == ConnectionState.Open) { connection.Close(); }
                transaction.Dispose(); connection.Dispose();

                var path = HttpContext.Server.MapPath("~/App_Data");
                ExceptionLogger.CreateLog(Ex, path);
                return Json(new { IsError = true, IsSessionError = false, ErrorDetails = "Failed to save due to unknown error." }, JsonRequestBehavior.AllowGet);
            }
        }

	}
}