using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using sCommonLib;
using System.Web.Configuration;
using System.Data;
using System.Data.SqlClient;
using RobiPosMapper.Models;
using System.IO;
using QueryStringEncryption;

namespace RobiPosMapper.Areas.Dsr.Controllers
{
    public class EditRetailerController : Controller
    {
        public ActionResult Index()
        {
            String LanguagePreference = "English"; 
            String ConnectionString = WebConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;

            SqlConnection connection = new SqlConnection(ConnectionString);
            SqlCommand command = new SqlCommand(); command.Connection = connection;

            Int32 RetailerStatusId=0;
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

                ViewBag.SessionId = sessionId; ViewBag.SessionDigest = encryptedSessionDigest;

                String sqlFindRetailer = "select RetailerId, RetailerName, Address, PosCategoryId, RegionId, AreaId, ThanaId, WardId, MauzaId, VillageId, IsElPos, IsSimPos, IsScPos, Latitude, Longitude, AccuracyLevel, VisitDayId, DefaultPhotoName, PosStructureId, ShopSignageId, ShopTypeId, RspId, DsrActivityDateTime, CreatedBy, CreateDateTime, IsActive, ModifiedBy, ModifiedDateTime, Remarks, RetailerStatusId, IsVerifiedByDsrs, DsrsVerificationDateTime, DsrId, IsApartments, IsSlums, IsSemiUrbunHousing, IsRuralHousing, IsShoppingMall, IsRetailHub, IsMobileDeviceMarket, IsBazaar, IsOfficeArea, IsGarmentsMajorityArea, IsGeneralIndustrialArea, IsUrbanTransitPoints, IsRuralTransitPoints, IsUrbanYouthHangouts, IsSemiUrbanYouthHangouts, IsRuralYouthHangouts, IsTouristDestinations, QrCodeId, IsReevaluated, HasTradeLicense FROM Retailer Where RetailerId=" + RetailerId + "";

                String sqlSelectElMsisdn = "SELECT ELMSISDN from ElMsisdn where RetailerId="+ RetailerId +"";
                String sqlSelectSimPosCode = "SELECT SimPosCode from SimPosCode where RetailerId=" + RetailerId + "";

                command.CommandText = sqlFindRetailer;
                DataTable dt = new DataTable(); DataTable dtElNumbers = new DataTable(); DataTable dtSimPosNumbers = new DataTable();

                String sqlLanguagePreference = "Select LanguagePreference from Person Where PersonId=" + DsrId + "";
               
                using (SqlDataAdapter da = new SqlDataAdapter(command))
                {
                    connection.Open(); da.Fill(dt);
                    command.CommandText = sqlSelectElMsisdn; da.Fill(dtElNumbers); command.CommandText = sqlSelectSimPosCode; da.Fill(dtSimPosNumbers);
                    command.CommandText = sqlLanguagePreference; LanguagePreference = command.ExecuteScalar().ToString(); 
                    connection.Close();
                }

                ViewBag.ElMsisdns = dtElNumbers;
                ViewBag.SimPosCodes = dtSimPosNumbers;

                RetailerStatusId = Convert.ToInt32(dt.Rows[0]["RetailerStatusId"].ToString());

                ViewBag.Retailer = dt;

                List<Region> regionList = RegionManager.GetAllRegions();
                ViewBag.RegionList = regionList;

               ViewBag.PosCategoryList = PosCategoryManager.PosCategoryList();
               ViewBag.AreaList = AreaManager.RegionSpecificAreaList(Convert.ToInt32(dt.Rows[0]["RegionId"]));
               ViewBag.ThanaList = ThanaManager.AreaSpecificThanaList(Convert.ToInt32(dt.Rows[0]["AreaId"]));
               ViewBag.WardList = WardManager.ThanaSpecificWardList(Convert.ToInt32(dt.Rows[0]["ThanaId"]));
               ViewBag.MauzaList = MauzaManager.WardSpecificMauzaList(Convert.ToInt32(dt.Rows[0]["WardId"]));
               ViewBag.VillageList = VillageManager.MauzaSpecificVillageList(Convert.ToInt32(dt.Rows[0]["MauzaId"]));
               ViewBag.PosStructureList = PosStructureManager.PosStructureList();
               ViewBag.VisitDayList = VisitDayManager.VisitDayList();
               ViewBag.ShopSignageList = ShopSignageManager.ShopSignageList();
               ViewBag.ShopTypeList = ShopTypeManager.ShopTypeList();
              
                
               // //---> Code to show/hide new QR textbox
               //Boolean isReevaluating = false;

               ////----->Check evaluatin by  DsrActivityDateTime
               //if (dt.Rows[0]["DsrActivityDateTime"] != DBNull.Value)
               //{
               //    isReevaluating = true;
               //}
               ////<--- Check already it is evaluated or not
               ////<--- Compare datetime
               ////<--- Code to show/hide new QR textbox

               //ViewBag.IsReevaluated = isReevaluating;
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


            switch (RetailerStatusId)
            {
                case 1:
                    return View("FullyEditableRetailer");
                  
                    //if (LanguagePreference == "English")
                    //{
                    //    return View("FullyEditableRetailer");
                    //}
                    //else
                    //{
                    //    return View("FullyEditableRetailer.bn");
                    //}
                   
                case 2:
                case 3:
                    return View("PartiallyEditableRetailer");

                    //if (LanguagePreference == "English")
                    //{
                    //    return View("PartiallyEditableRetailer");
                    //}
                    //else
                    //{
                    //    return View("PartiallyEditableRetailer");
                    //}
                default:
                    return View("PartiallyEditableRetailer");
                    //if (LanguagePreference == "English")
                    //{
                    //    return View("PartiallyEditableRetailer");
                    //}
                    //else
                    //{
                    //    return View("PartiallyEditableRetailer");
                    //}
            }
        }

        [HttpPost]
        public JsonResult RemoveElMsisdn(FormCollection data)
        {

            String ConnectionString = WebConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;

            SqlConnection connection = new SqlConnection(ConnectionString);
            SqlCommand command = new SqlCommand(); command.Connection = connection;
            SqlTransaction transaction = null;
           
            try
            {

                Int32 ElMsisdn = Convert.ToInt32(data["ElMsisdn"]);
                Int32 RetailerId = Convert.ToInt32(data["RetailerId"]);

                connection.Open();
                transaction = connection.BeginTransaction();
                command.Transaction = transaction;

               
                command.CommandText = "Delete from ElMsisdn where ElMsisdn=" + ElMsisdn + " and RetailerId=" + RetailerId + ""; 
                command.ExecuteNonQuery();
                command.CommandText = "select ElMsisdn from ElMsisdn Where RetailerId="+ RetailerId +"";
                Object findResult = command.ExecuteScalar();

                if (findResult==null)
                {
                    command.CommandText = "Update Retailer set IsElPos=0 Where RetailerId="+ RetailerId +"";
                    command.ExecuteNonQuery();
                }


                transaction.Commit();
                connection.Close();
                command.Dispose();
                transaction.Dispose();
                connection.Dispose();

                return Json(new { IsDeleted = true }, JsonRequestBehavior.AllowGet);
            }

            catch (Exception Ex)
            {
                if (transaction != null) { transaction.Rollback(); }
                if (connection.State == ConnectionState.Open) { connection.Close(); }
                transaction.Dispose(); connection.Dispose();

                var path = HttpContext.Server.MapPath("~/App_Data");
                ExceptionLogger.CreateLog(Ex, path);
                return Json(new { IsDeleted = false }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost]
        public JsonResult RemoveSimPosCode(FormCollection data)
        {

            String ConnectionString = WebConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;

            SqlConnection connection = new SqlConnection(ConnectionString);
            SqlCommand command = new SqlCommand(); command.Connection = connection;
            SqlTransaction transaction = null;

            try
            {

                String SimPosCode = data["SimPosCode"].ToString();
                Int32 RetailerId = Convert.ToInt32(data["RetailerId"]);

                connection.Open();
                transaction = connection.BeginTransaction();
                command.Transaction = transaction;

                command.CommandText = "Delete from SimPosCode where SimPosCode='" + SimPosCode + "' and RetailerId=" + RetailerId + "";
                command.ExecuteNonQuery();

                command.CommandText = "Select SimPosCode from SimPosCode Where RetailerId="+ RetailerId +"";
                Object findResult = command.ExecuteScalar();

                if (findResult==null)
                {
                    command.CommandText = "Update Retailer set IsSimPos=0 where RetailerId="+ RetailerId +"";
                    command.ExecuteNonQuery();
                }

                transaction.Commit();
                connection.Close();
                command.Dispose();
                transaction.Dispose();
                connection.Dispose();

                return Json(new { IsDeleted = true }, JsonRequestBehavior.AllowGet);

            }

            catch (Exception Ex)
            {
                if (transaction != null) { transaction.Rollback(); }
                if (connection.State == ConnectionState.Open) { connection.Close(); }
                transaction.Dispose(); connection.Dispose();

                var path = HttpContext.Server.MapPath("~/App_Data");
                ExceptionLogger.CreateLog(Ex, path);
                return Json(new { IsDeleted = false }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost]
        public JsonResult FullRetailerUpdate(FormCollection data)
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

                DateTime dtSessionCreated = Convert.ToDateTime(dtSessionInfo.Rows[0]["CreatedOn"]);
                DateTime currentTime = DateTime.Now;
                double hours = (currentTime - dtSessionCreated).TotalHours;

                if (hours > 12)
                {
                    command.Parameters.Clear();
                    command.CommandText = "Delete from SessionInfo Where SessionId=@SessionId";
                    command.Parameters.AddWithValue("@SessionId", sessionId);
                    connection.Open(); command.ExecuteNonQuery(); command.Parameters.Clear(); connection.Close();
                    return Json(new { IsError = true, IsSessionTimeOut = true, url = "/Dsr/Login/Index" }, JsonRequestBehavior.AllowGet);
                }


                Int32  CurrentDsrId = Convert.ToInt32(dtSessionInfo.Rows[0]["DsrId"]);
                Int32 surveyorId = Convert.ToInt32(dtSessionInfo.Rows[0]["SurveyorId"]);
                strSurveyorInfo = " SR" + surveyorId.ToString() + ".";
                Int32 RspId = Convert.ToInt32(dtSessionInfo.Rows[0]["RspId"]);
                Int32 RetailerId = Convert.ToInt32(data["RetailerId"]); strRetailerInfo = " Current Retailer ID-" + RetailerId.ToString() + "." ;
                //<-------------------------Session related checking

                connection.Open();
                transaction = connection.BeginTransaction();
                command.Transaction = transaction;

                String RetailerName = data["RetailerName"].ToString();
                String RetailerAddress = data["RetailerAddress"].ToString();
                Int32 PosCategoryId = Convert.ToInt32( data["PosCategoryId"]);
               
                Boolean IsElPos = Convert.ToBoolean(data["IsElMsisdn"]);

                String[] arrElMsisdn = null;

                if (IsElPos)
                {
                    String ElMsisdnData = data["ElMsisdn"].ToString();

                    if (!String.IsNullOrEmpty(ElMsisdnData))
                    {
                        if (ElMsisdnData.Contains(",")) //--> if multiple ElMsisdn
                        {
                            arrElMsisdn = ElMsisdnData.Split(',');

                            foreach (String strElNumber in arrElMsisdn)
                            {
                                Int32 ElMsisdn;
                                Boolean IsElValid = Int32.TryParse(strElNumber.Trim(), out ElMsisdn);

                                if (IsElValid)
                                {
                                    throw new InvalidInputException("EL MSISDN - " + strElNumber + " is invalid. It contains non-numeric character or value out of range.");
                                }
                            }
                        } //<-- if multiple ElMsisdn
                        else
                        {//--> if one ElMsisdn 

                            Int32 ElMsisdn;
                            Boolean IsElValid = Int32.TryParse(ElMsisdnData.Trim(), out ElMsisdn);

                            if (!IsElValid)
                            {
                                throw new InvalidInputException("EL MSISDN - " + ElMsisdnData + " is an invalid EL MSISDN.  It contains non-numeric character or value out of range.");
                            }

                        } //<-- if one ElMsisdn 
                    }
                }

                Boolean IsSimPos = Convert.ToBoolean(data["IsSimPos"]);
                Boolean IsScPos = Convert.ToBoolean(data["IsScPos"]);
                Boolean HasTradeLicense = Convert.ToBoolean(data["HasTradeLicense"]);

             
                Int32 RegionId = Convert.ToInt32(data["RegionId"]);
                
                Int32 AreaId;
                if (String.IsNullOrEmpty(data["AreaId"].ToString()))
                {
                    throw new InvalidInputException("Area information not found. Please check area combo." + strSurveyorInfo + strRetailerInfo);
                }
                else
                {
                    Boolean isValid = Int32.TryParse(data["AreaId"].ToString(), out AreaId);
                    if (!isValid)
                    {
                        throw new InvalidInputException("Area information not found. Please check area combo." + strSurveyorInfo + strRetailerInfo);
                    }
                }


                Int32 ThanaId;
                if (String.IsNullOrEmpty(data["ThanaId"].ToString()))
                {
                    throw new InvalidInputException("Thana information not found. Please check thana combo." + strSurveyorInfo + strRetailerInfo);
                }
                else
                {
                   // ThanaId = Convert.ToInt32(data["ThanaId"]);
                    Boolean isValid = Int32.TryParse(data["ThanaId"].ToString(), out ThanaId);
                    if (!isValid)
                    {
                        throw new InvalidInputException("Thana information not found. Please check thana combo." + strSurveyorInfo + strRetailerInfo);
                    }
                }
                
                Int32 WardId;
                if (String.IsNullOrEmpty(data["WardId"].ToString()))
                {
                    throw new InvalidInputException("Ward information not found. Please check ward combo." + strSurveyorInfo + strRetailerInfo);
                }
                else
                {
                   // WardId = Convert.ToInt32(data["WardId"]);
                    Boolean isValid = Int32.TryParse(data["WardId"].ToString(), out WardId);
                    if (!isValid)
                    {
                        throw new InvalidInputException("Ward information not found. Please check ward combo." + strSurveyorInfo + strRetailerInfo);
                    }
                }

                Int32 MauzaId;
                if (String.IsNullOrEmpty(data["MauzaId"].ToString()))
                {
                    throw new InvalidInputException("Mauza information not found. Please check Mauza combo." + strSurveyorInfo + strRetailerInfo);
                }
                else
                {
                   // MauzaId = Convert.ToInt32(data["MauzaId"]);
                    Boolean isValid = Int32.TryParse(data["MauzaId"].ToString(), out MauzaId);
                    if (!isValid)
                    {
                        throw new InvalidInputException("Mauza information not found. Please check mauza combo." + strSurveyorInfo + strRetailerInfo);
                    }
                }

                Int32 VillageId;
                if (String.IsNullOrEmpty(data["VillageId"].ToString()))
                {
                    throw new InvalidInputException("Village information not found. Please check Village combo." + strSurveyorInfo + strRetailerInfo);
                }
                else
                {
                    //VillageId = Convert.ToInt32(data["VillageId"]);
                    Boolean isValid = Int32.TryParse(data["VillageId"].ToString(), out VillageId);
                    if (!isValid)
                    {
                        throw new InvalidInputException("Village information not found. Please check village combo." + strSurveyorInfo + strRetailerInfo);
                    }
                }

                Int32 PosStructureId = Convert.ToInt32(data["PosStructureId"]);
                Int32 ShopSignageId = Convert.ToInt32(data["ShopSignageId"]);
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
                Boolean IsQrNumeric = Int32.TryParse(data["QrValue"].ToString(), out QrValue);
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
                    throw new InvalidInputException("This QR code "+ QrValue.ToString() +" does not exist in database. You can not tag it." + strSurveyorInfo + strRetailerInfo);
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
                    throw new InvalidInputException("This QR Code- "+ QrValue.ToString() +" already used for retailer-"+ IsQrDuplicate.ToString() +"." + strSurveyorInfo + strRetailerInfo);
                }
                //<-- in Retailer table
                //<-- check QrCode


                Decimal Latitude = Convert.ToDecimal(data["Latitude"]);
                Decimal Longitude = Convert.ToDecimal(data["Longitude"]);
                Decimal AccuracyLevel = Convert.ToDecimal(data["Accuracy"]);
                String Remarks = data["Remarks"].ToString();

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
                else
                {
                    throw new InvalidInputException("Please send retailer photo." + strSurveyorInfo + strRetailerInfo);
                }

                
                //--> update data in Retailer Table
                string sqlUpdateRetailer = "UPDATE Retailer SET RetailerName=@RetailerName, Address=@Address,PosCategoryId=@PosCategoryId, RegionId=@RegionId, AreaId=@AreaId, ThanaId=@ThanaId, WardId=@WardId, MauzaId=@MauzaId, VillageId=@VillageId,  IsElPos=@IsElPos, IsSimPos=@IsSimPos, IsScPos=@IsScPos, Latitude=@Latitude, Longitude=@Longitude, AccuracyLevel=@AccuracyLevel,  VisitDayId=@VisitDayId, DefaultPhotoName=@DefaultPhotoName, PosStructureId=@PosStructureId, ShopSignageId=@ShopSignageId, ShopTypeId=@ShopTypeId,  RspId=@RspId, DsrActivityDateTime=@DsrActivityDateTime, ModifiedBy=@ModifiedBy, ModifiedDateTime=@ModifiedDateTime, Remarks=@Remarks, RetailerStatusId=@RetailerStatusId, DsrId=@DsrId, IsApartments=@IsApartments,  IsSlums=@IsSlums, IsSemiUrbunHousing=@IsSemiUrbunHousing, IsRuralHousing=@IsRuralHousing, IsShoppingMall=@IsShoppingMall, IsRetailHub=@IsRetailHub, IsMobileDeviceMarket=@IsMobileDeviceMarket,  IsBazaar=@IsBazaar, IsOfficeArea=@IsOfficeArea, IsGarmentsMajorityArea=@IsGarmentsMajorityArea, IsGeneralIndustrialArea=@IsGeneralIndustrialArea,  IsUrbanTransitPoints=@IsUrbanTransitPoints, IsRuralTransitPoints=@IsRuralTransitPoints, IsUrbanYouthHangouts=@IsUrbanYouthHangouts, IsSemiUrbanYouthHangouts=@IsSemiUrbanYouthHangouts, IsRuralYouthHangouts=@IsRuralYouthHangouts, IsTouristDestinations=@IsTouristDestinations, QrCodeId=@QrCodeId, SurveyorId=@SurveyorId, SurveyorActivityDateTime=@SurveyorActivityDateTime, HasTradeLicense=@HasTradeLicense  Where RetailerId=" + RetailerId + "";

                command.CommandText = sqlUpdateRetailer;
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
                command.Parameters.AddWithValue("@IsElPos", IsElPos);
                command.Parameters.AddWithValue("@IsSimPos", IsSimPos);
                command.Parameters.AddWithValue("@IsScPos", IsScPos);
                command.Parameters.AddWithValue("@Latitude", Latitude);
                command.Parameters.AddWithValue("@Longitude", Longitude);
                command.Parameters.AddWithValue("@AccuracyLevel", AccuracyLevel);
                command.Parameters.AddWithValue("@VisitDayId", VisitDayId);
                command.Parameters.AddWithValue("@DefaultPhotoName", imageName);
                command.Parameters.AddWithValue("@PosStructureId", PosStructureId);
                command.Parameters.AddWithValue("@ShopSignageId", ShopSignageId);
                command.Parameters.AddWithValue("@ShopTypeId", ShopTypeId);
                command.Parameters.AddWithValue("@RspId", RspId);
                command.Parameters.AddWithValue("@DsrActivityDateTime", DateTime.Now);
                command.Parameters.AddWithValue("@ModifiedBy", CurrentDsrId);
                command.Parameters.AddWithValue("@ModifiedDateTime", DateTime.Now);
                command.Parameters.AddWithValue("@Remarks", Remarks);
                command.Parameters.AddWithValue("@RetailerStatusId", 2);
                command.Parameters.AddWithValue("@DsrId", CurrentDsrId);
                command.Parameters.AddWithValue("@IsApartments", IsApartments);
                command.Parameters.AddWithValue("@IsSlums", IsSlums);
                command.Parameters.AddWithValue("@IsSemiUrbunHousing", IsSemiUrbunHousing);
                command.Parameters.AddWithValue("@IsRuralHousing", IsRuralHousing);
                command.Parameters.AddWithValue("@IsShoppingMall", IsShoppingMall);
                command.Parameters.AddWithValue("@IsRetailHub", IsRetailHub);
                command.Parameters.AddWithValue("@IsMobileDeviceMarket", IsMobileDeviceMarket);
                command.Parameters.AddWithValue("@IsBazaar", IsBazaar);
                command.Parameters.AddWithValue("@IsOfficeArea", IsOfficeArea);
                command.Parameters.AddWithValue("@IsGarmentsMajorityArea", IsGarmentsMajorityArea);
                command.Parameters.AddWithValue("@IsGeneralIndustrialArea", IsGeneralIndustrialArea);
                command.Parameters.AddWithValue("@IsUrbanTransitPoints", IsUrbanTransitPoints);
                command.Parameters.AddWithValue("@IsRuralTransitPoints", IsRuralTransitPoints);
                command.Parameters.AddWithValue("@IsUrbanYouthHangouts", IsUrbanYouthHangouts);
                command.Parameters.AddWithValue("@IsSemiUrbanYouthHangouts", IsSemiUrbanYouthHangouts);
                command.Parameters.AddWithValue("@IsRuralYouthHangouts", IsRuralYouthHangouts);
                command.Parameters.AddWithValue("@IsTouristDestinations", IsTouristDestinations);
                command.Parameters.AddWithValue("@QrCodeId", QrValue);
                command.Parameters.AddWithValue("@SurveyorId", surveyorId);
                command.Parameters.AddWithValue("@SurveyorActivityDateTime", DateTime.Now);
                command.Parameters.AddWithValue("@HasTradeLicense", HasTradeLicense);

                command.ExecuteNonQuery();

                //--> Insert EL MSISDN in ElMsisdn table
                if (IsElPos)
                {
                    String ElMsisdnData = data["ElMsisdn"].ToString();

                    if (!String.IsNullOrEmpty(ElMsisdnData))
                    {
                        if (ElMsisdnData.Contains(","))
                        { //--> if multiple ElMsisdn
                            arrElMsisdn = ElMsisdnData.Split(',');
                            foreach (String strElNumber in arrElMsisdn)
                            {
                                Int32 ElMsisdn = Convert.ToInt32(strElNumber); //numeric validation has been done in few lines above.

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
                        } //<-- if multiple ElMsisdn
                        else
                        { //--> if one ElMsisdn

                            Int32 ElMsisdn = Convert.ToInt32(ElMsisdnData); //numeric validation has been done in few lines above.
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
                        }//<--- if one ElMsisdn
                    }
                }
                //<-- Insert EL MSISDN in ElMsisdn table


                //--> Insert SIM POS Code in SimPosCode table
                if (IsSimPos)
                {
                    String SimPosCodeData = data["SimPosCode"].ToString();

                    if (!String.IsNullOrEmpty(SimPosCodeData))
                    {
                        if (SimPosCodeData.Contains(","))
                        {
                            String[] arrSimPosCode = SimPosCodeData.Split(',');
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
                        else
                        {
                            string sqlInsertSimPosCode = "INSERT INTO SimPosCode(SimPosCode,RetailerId)  VALUES(@SimPosCode,@RetailerId)";
                            command.CommandText = sqlInsertSimPosCode;
                            command.Parameters.Clear();
                            command.Parameters.AddWithValue("@SimPosCode", SimPosCodeData);
                            command.Parameters.AddWithValue("@RetailerId", RetailerId);
                            command.ExecuteNonQuery();
                        }
                    }
                }
                //<-- Insert SIM POS Code in SimPosCode table

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
                return Json(new { IsError = true, IsSessionTimeOut = false, ErrorDetails = "Failed to save due to unknown error." }, JsonRequestBehavior.AllowGet);
            }
        }     
        
        [HttpPost]
        public JsonResult PartialRetailerUpdate(FormCollection data)
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
                String RetailerName = data["RetailerName"].ToString();
                String RetailerAddress = data["RetailerAddress"].ToString();
                Int32 PosCategoryId = Convert.ToInt32(data["PosCategoryId"]);

                Boolean IsElPos = Convert.ToBoolean(data["IsElMsisdn"]);
                String[] arrElMsisdn = null;
                if (IsElPos)
                {
                    String ElMsisdnData = data["ElMsisdn"].ToString();
                    if (!String.IsNullOrEmpty(ElMsisdnData))
                    {
                        if (ElMsisdnData.Contains(","))
                        {
                            arrElMsisdn = ElMsisdnData.Split(',');

                            foreach (String strElNumber in arrElMsisdn)
                            {
                                Int32 ElMsisdn;
                                Boolean IsElValid = Int32.TryParse(strElNumber.Trim(), out ElMsisdn);

                                if (!IsElValid)
                                {
                                    throw new InvalidInputException("EL MSISDN - " + strElNumber + " is invalid. It contains non-numeric character or value out of range.");
                                }
                            }
                        }
                        else
                        {
                            Int32 ElMsisdn;
                            Boolean IsElValid = Int32.TryParse(ElMsisdnData.Trim(), out ElMsisdn);

                            if (!IsElValid)
                            {
                                throw new InvalidInputException("EL MSISDN - " + ElMsisdnData + " is an invalid EL MSISDN.  It contains non-numeric character or value out of range.");
                            }

                        }
                    }
                }

                Boolean IsSimPos = Convert.ToBoolean(data["IsSimPos"]);
                Boolean IsScPos = Convert.ToBoolean(data["IsScPos"]);
                Boolean HasTradeLicense = Convert.ToBoolean(data["HasTradeLicense"]);

                Int32 RegionId = Convert.ToInt32(data["RegionId"]);
                Int32 AreaId;
                if (String.IsNullOrEmpty(data["AreaId"].ToString()))
                {
                    throw new InvalidInputException("Area information not found. Please check area combo." + strSurveyorInfo + strRetailerInfo);
                }
                else
                {
                    Boolean isValid = Int32.TryParse(data["AreaId"].ToString(), out AreaId);
                    if (!isValid)
                    {
                        throw new InvalidInputException("Area information not found. Please check area combo." + strSurveyorInfo + strRetailerInfo);
                    }
                }
                Int32 ThanaId;
                if (String.IsNullOrEmpty(data["ThanaId"].ToString()))
                {
                    throw new InvalidInputException("Thana information not found. Please check thana combo." + strSurveyorInfo + strRetailerInfo);
                }
                else
                {
                    // ThanaId = Convert.ToInt32(data["ThanaId"]);
                    Boolean isValid = Int32.TryParse(data["ThanaId"].ToString(), out ThanaId);
                    if (!isValid)
                    {
                        throw new InvalidInputException("Thana information not found. Please check thana combo." + strSurveyorInfo + strRetailerInfo);
                    }
                }

                Int32 WardId;
                if (String.IsNullOrEmpty(data["WardId"].ToString()))
                {
                    throw new InvalidInputException("Ward information not found. Please check ward combo." + strSurveyorInfo + strRetailerInfo);
                }
                else
                {
                    // WardId = Convert.ToInt32(data["WardId"]);
                    Boolean isValid = Int32.TryParse(data["WardId"].ToString(), out WardId);
                    if (!isValid)
                    {
                        throw new InvalidInputException("Ward information not found. Please check ward combo." + strSurveyorInfo + strRetailerInfo);
                    }
                }

                Int32 MauzaId;
                if (String.IsNullOrEmpty(data["MauzaId"].ToString()))
                {
                    throw new InvalidInputException("Mauza information not found. Please check Mauza combo." + strSurveyorInfo + strRetailerInfo);
                }
                else
                {
                    // MauzaId = Convert.ToInt32(data["MauzaId"]);
                    Boolean isValid = Int32.TryParse(data["MauzaId"].ToString(), out MauzaId);
                    if (!isValid)
                    {
                        throw new InvalidInputException("Mauza information not found. Please check mauza combo." + strSurveyorInfo + strRetailerInfo);
                    }
                }

                Int32 VillageId;
                if (String.IsNullOrEmpty(data["VillageId"].ToString()))
                {
                    throw new InvalidInputException("Village information not found. Please check Village combo." + strSurveyorInfo + strRetailerInfo);
                }
                else
                {
                    //VillageId = Convert.ToInt32(data["VillageId"]);
                    Boolean isValid = Int32.TryParse(data["VillageId"].ToString(), out VillageId);
                    if (!isValid)
                    {
                        throw new InvalidInputException("Village information not found. Please check village combo." + strSurveyorInfo + strRetailerInfo);
                    }
                }

                Int32 PosStructureId = Convert.ToInt32(data["PosStructureId"]);
                Int32 ShopSignageId = Convert.ToInt32(data["ShopSignageId"]);
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


                //---->  QR Code related
                //IsQrChanged
                Boolean isQrChanged = Convert.ToBoolean(data["IsQrChanged"]);
                if (isQrChanged)
                {
                    string strNewQrCode = data["NewQrCode"].ToString().Trim();
                    Int32 NewQrCode = 0;
                    //--> check QrCode
                    if (!string.IsNullOrEmpty(strNewQrCode))
                    {
                        Boolean IsQrNumeric = Int32.TryParse(strNewQrCode, out NewQrCode);
                        if (!IsQrNumeric)
                        {
                            throw new InvalidInputException("Incorrect QR format. You entered-" + NewQrCode.ToString() + strSurveyorInfo + strRetailerInfo);
                        }

                        //--> in QrCode table
                        string sqlFindQrCodeExistance = "SELECT QrCodeId FROM QrCode WHERE (QrCodeId = @QrCodeId)";
                        command.CommandText = sqlFindQrCodeExistance;
                        command.Parameters.Clear();
                        command.Parameters.AddWithValue("@QrCodeId", NewQrCode);

                        object IsQrFound = command.ExecuteScalar();
                        if (IsQrFound == null)
                        {
                            throw new InvalidInputException("This QR code-" + NewQrCode.ToString() + " does not exist in database. You can not tag it." + strSurveyorInfo + strRetailerInfo);
                        }
                        //<-- in QrCode table

                        //--> in Retailer table
                        string sqlFindQrCodeDuplicacy = "SELECT RetailerId FROM Retailer WHERE (QrCodeId = @QrCodeId)";
                        command.CommandText = sqlFindQrCodeDuplicacy;
                        command.Parameters.Clear();
                        command.Parameters.AddWithValue("@QrCodeId", NewQrCode);

                        object IsQrDuplicate = command.ExecuteScalar();
                        if (IsQrDuplicate != null)
                        {
                            throw new InvalidInputException("This QR Code-" + NewQrCode.ToString() + " already used for retailer-" + IsQrDuplicate.ToString() + "." + strSurveyorInfo + strRetailerInfo);
                        }
                        //<-- in Retailer table

                        //--> update QR data in Retailer Table
                        String sqlUpdateQr = "UPDATE Retailer SET QrCodeId=@QrCodeId Where RetailerId=" + RetailerId + "";

                        command.Parameters.Clear();
                        command.CommandText = sqlUpdateQr;
                        command.Parameters.AddWithValue("@QrCodeId", NewQrCode);
                        command.ExecuteNonQuery();
                    }
                    else
                    {
                        throw new InvalidInputException("Please enter new QR code." + strSurveyorInfo + strRetailerInfo);
                    }
                    //<-- check QrCode
                }

                //<--- QR Code related


                Decimal Latitude = Convert.ToDecimal(data["Latitude"]);
                Decimal Longitude = Convert.ToDecimal(data["Longitude"]);
                Decimal AccuracyLevel = Convert.ToDecimal(data["Accuracy"]);
                String Remarks = data["Remarks"].ToString();
                //Accuracy is handled in client side
                //if (AccuracyLevel>20)
                //{
                //    throw new InvalidInputException("Accuracy level is out of limit.");
                //}

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


                command.Parameters.Clear();
                command.CommandText = "select SurveyorId from Retailer Where RetailerId="+ RetailerId +"";
                Object isSurveyorExist = command.ExecuteScalar();
                

                //Only first SurveyorId will be assigned for a retailer. Any subsequent Surveyor may update this retailer, but he will not be recorded. 
                if (isSurveyorExist==DBNull.Value)
                {
                    // if surveyor id does not exist
                    //--> update data in Retailer Table
                    string sqlUpdateRetailer = "UPDATE Retailer SET RetailerName=@RetailerName, Address=@Address,PosCategoryId=@PosCategoryId, RegionId=@RegionId, AreaId=@AreaId, ThanaId=@ThanaId, WardId=@WardId, MauzaId=@MauzaId, VillageId=@VillageId,  IsElPos=@IsElPos, IsSimPos=@IsSimPos, IsScPos=@IsScPos, Latitude=@Latitude, Longitude=@Longitude, AccuracyLevel=@AccuracyLevel,  VisitDayId=@VisitDayId, PosStructureId=@PosStructureId, ShopSignageId=@ShopSignageId, ShopTypeId=@ShopTypeId,  RspId=@RspId,   ModifiedDateTime=@ModifiedDateTime, Remarks=@Remarks, DsrId=@DsrId, IsApartments=@IsApartments,  IsSlums=@IsSlums, IsSemiUrbunHousing=@IsSemiUrbunHousing, IsRuralHousing=@IsRuralHousing, IsShoppingMall=@IsShoppingMall, IsRetailHub=@IsRetailHub, IsMobileDeviceMarket=@IsMobileDeviceMarket,  IsBazaar=@IsBazaar, IsOfficeArea=@IsOfficeArea, IsGarmentsMajorityArea=@IsGarmentsMajorityArea, IsGeneralIndustrialArea=@IsGeneralIndustrialArea,  IsUrbanTransitPoints=@IsUrbanTransitPoints, IsRuralTransitPoints=@IsRuralTransitPoints, IsUrbanYouthHangouts=@IsUrbanYouthHangouts, IsSemiUrbanYouthHangouts=@IsSemiUrbanYouthHangouts, IsRuralYouthHangouts=@IsRuralYouthHangouts, IsTouristDestinations=@IsTouristDestinations, SurveyorId=@SurveyorId, IsReevaluated=@IsReevaluated, SurveyorActivityDateTime=@SurveyorActivityDateTime, HasTradeLicense=@HasTradeLicense Where RetailerId=" + RetailerId + "";

                    command.CommandText = sqlUpdateRetailer;
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
                    command.Parameters.AddWithValue("@IsElPos", IsElPos);
                    command.Parameters.AddWithValue("@IsSimPos", IsSimPos);
                    command.Parameters.AddWithValue("@IsScPos", IsScPos);
                    command.Parameters.AddWithValue("@Latitude", Latitude);
                    command.Parameters.AddWithValue("@Longitude", Longitude);
                    command.Parameters.AddWithValue("@AccuracyLevel", AccuracyLevel);
                    command.Parameters.AddWithValue("@VisitDayId", VisitDayId);
                    command.Parameters.AddWithValue("@PosStructureId", PosStructureId);
                    command.Parameters.AddWithValue("@ShopSignageId", ShopSignageId);
                    command.Parameters.AddWithValue("@ShopTypeId", ShopTypeId);
                    command.Parameters.AddWithValue("@RspId", RspId);
                    //command.Parameters.AddWithValue("@DsrActivityDateTime", DateTime.Now); // Removed intentionally
                    //command.Parameters.AddWithValue("@ModifiedBy", CurrentDsrId); 
                    command.Parameters.AddWithValue("@ModifiedDateTime", DateTime.Now);
                    command.Parameters.AddWithValue("@Remarks", Remarks);
                    command.Parameters.AddWithValue("@DsrId", CurrentDsrId);
                    command.Parameters.AddWithValue("@IsApartments", IsApartments);
                    command.Parameters.AddWithValue("@IsSlums", IsSlums);
                    command.Parameters.AddWithValue("@IsSemiUrbunHousing", IsSemiUrbunHousing);
                    command.Parameters.AddWithValue("@IsRuralHousing", IsRuralHousing);
                    command.Parameters.AddWithValue("@IsShoppingMall", IsShoppingMall);
                    command.Parameters.AddWithValue("@IsRetailHub", IsRetailHub);
                    command.Parameters.AddWithValue("@IsMobileDeviceMarket", IsMobileDeviceMarket);
                    command.Parameters.AddWithValue("@IsBazaar", IsBazaar);
                    command.Parameters.AddWithValue("@IsOfficeArea", IsOfficeArea);
                    command.Parameters.AddWithValue("@IsGarmentsMajorityArea", IsGarmentsMajorityArea);
                    command.Parameters.AddWithValue("@IsGeneralIndustrialArea", IsGeneralIndustrialArea);
                    command.Parameters.AddWithValue("@IsUrbanTransitPoints", IsUrbanTransitPoints);
                    command.Parameters.AddWithValue("@IsRuralTransitPoints", IsRuralTransitPoints);
                    command.Parameters.AddWithValue("@IsUrbanYouthHangouts", IsUrbanYouthHangouts);
                    command.Parameters.AddWithValue("@IsSemiUrbanYouthHangouts", IsSemiUrbanYouthHangouts);
                    command.Parameters.AddWithValue("@IsRuralYouthHangouts", IsRuralYouthHangouts);
                    command.Parameters.AddWithValue("@IsTouristDestinations", IsTouristDestinations);
                    command.Parameters.AddWithValue("@SurveyorId", surveyorId);
                    command.Parameters.AddWithValue("@IsReevaluated", false);
                    command.Parameters.AddWithValue("@SurveyorActivityDateTime", DateTime.Now); //This is required to easily show LastActivityDateTime.
                    command.Parameters.AddWithValue("@HasTradeLicense", HasTradeLicense);

                    command.ExecuteNonQuery(); 
                }
                else
                {  //--> if SurveyorId exist
                    //--> update data in Retailer Table
                    string sqlUpdateRetailer = "UPDATE Retailer SET RetailerName=@RetailerName, Address=@Address,PosCategoryId=@PosCategoryId, RegionId=@RegionId, AreaId=@AreaId, ThanaId=@ThanaId, WardId=@WardId, MauzaId=@MauzaId, VillageId=@VillageId,  IsElPos=@IsElPos, IsSimPos=@IsSimPos, IsScPos=@IsScPos, Latitude=@Latitude, Longitude=@Longitude, AccuracyLevel=@AccuracyLevel,  VisitDayId=@VisitDayId, PosStructureId=@PosStructureId, ShopSignageId=@ShopSignageId, ShopTypeId=@ShopTypeId, Remarks=@Remarks, IsApartments=@IsApartments,  IsSlums=@IsSlums, IsSemiUrbunHousing=@IsSemiUrbunHousing, IsRuralHousing=@IsRuralHousing, IsShoppingMall=@IsShoppingMall, IsRetailHub=@IsRetailHub, IsMobileDeviceMarket=@IsMobileDeviceMarket,  IsBazaar=@IsBazaar, IsOfficeArea=@IsOfficeArea, IsGarmentsMajorityArea=@IsGarmentsMajorityArea, IsGeneralIndustrialArea=@IsGeneralIndustrialArea,  IsUrbanTransitPoints=@IsUrbanTransitPoints, IsRuralTransitPoints=@IsRuralTransitPoints, IsUrbanYouthHangouts=@IsUrbanYouthHangouts, IsSemiUrbanYouthHangouts=@IsSemiUrbanYouthHangouts, IsRuralYouthHangouts=@IsRuralYouthHangouts, IsTouristDestinations=@IsTouristDestinations, SurveyorActivityDateTime=@SurveyorActivityDateTime, HasTradeLicense=@HasTradeLicense  Where RetailerId=" + RetailerId + "";

                    command.CommandText = sqlUpdateRetailer;
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
                    command.Parameters.AddWithValue("@IsElPos", IsElPos);
                    command.Parameters.AddWithValue("@IsSimPos", IsSimPos);
                    command.Parameters.AddWithValue("@IsScPos", IsScPos);
                    command.Parameters.AddWithValue("@Latitude", Latitude);
                    command.Parameters.AddWithValue("@Longitude", Longitude);
                    command.Parameters.AddWithValue("@AccuracyLevel", AccuracyLevel);
                    command.Parameters.AddWithValue("@VisitDayId", VisitDayId);
                    command.Parameters.AddWithValue("@PosStructureId", PosStructureId);
                    command.Parameters.AddWithValue("@ShopSignageId", ShopSignageId);
                    command.Parameters.AddWithValue("@ShopTypeId", ShopTypeId);
                    //command.Parameters.AddWithValue("@RspId", RspId); //Removed intentionally
                    //command.Parameters.AddWithValue("@DsrActivityDateTime", DateTime.Now); //Removed intentionally
                    //command.Parameters.AddWithValue("@ModifiedBy", CurrentDsrId);
                    //command.Parameters.AddWithValue("@ModifiedDateTime", DateTime.Now);
                    command.Parameters.AddWithValue("@Remarks", Remarks);
                    //command.Parameters.AddWithValue("@DsrId", CurrentDsrId);
                    command.Parameters.AddWithValue("@IsApartments", IsApartments);
                    command.Parameters.AddWithValue("@IsSlums", IsSlums);
                    command.Parameters.AddWithValue("@IsSemiUrbunHousing", IsSemiUrbunHousing);
                    command.Parameters.AddWithValue("@IsRuralHousing", IsRuralHousing);
                    command.Parameters.AddWithValue("@IsShoppingMall", IsShoppingMall);
                    command.Parameters.AddWithValue("@IsRetailHub", IsRetailHub);
                    command.Parameters.AddWithValue("@IsMobileDeviceMarket", IsMobileDeviceMarket);
                    command.Parameters.AddWithValue("@IsBazaar", IsBazaar);
                    command.Parameters.AddWithValue("@IsOfficeArea", IsOfficeArea);
                    command.Parameters.AddWithValue("@IsGarmentsMajorityArea", IsGarmentsMajorityArea);
                    command.Parameters.AddWithValue("@IsGeneralIndustrialArea", IsGeneralIndustrialArea);
                    command.Parameters.AddWithValue("@IsUrbanTransitPoints", IsUrbanTransitPoints);
                    command.Parameters.AddWithValue("@IsRuralTransitPoints", IsRuralTransitPoints);
                    command.Parameters.AddWithValue("@IsUrbanYouthHangouts", IsUrbanYouthHangouts);
                    command.Parameters.AddWithValue("@IsSemiUrbanYouthHangouts", IsSemiUrbanYouthHangouts);
                    command.Parameters.AddWithValue("@IsRuralYouthHangouts", IsRuralYouthHangouts);
                    command.Parameters.AddWithValue("@IsTouristDestinations", IsTouristDestinations);
                    command.Parameters.AddWithValue("@SurveyorActivityDateTime", DateTime.Now); //This is required to easily show LastActivityDateTime.
                    command.Parameters.AddWithValue("@HasTradeLicense", HasTradeLicense);
                    command.ExecuteNonQuery(); 
                }
                

                //--> Insert EL MSISDN in ElMsisdn table
                if (IsElPos)
                {
                    String ElMsisdnData = data["ElMsisdn"].ToString();

                    if (!String.IsNullOrEmpty(ElMsisdnData))
                    {
                        if (ElMsisdnData.Contains(","))
                        {
                            arrElMsisdn = ElMsisdnData.Split(',');
                            foreach (String strElNumber in arrElMsisdn)
                            {
                                Int32 ElMsisdn = Convert.ToInt32(strElNumber); //numeric validation has been done in few lines above.

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
                        else
                        {
                            //single data found
                            Int32 ElMsisdn = Convert.ToInt32(ElMsisdnData);//numeric validation has been done in few lines above.
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
                }
                //<-- Insert EL MSISDN in ElMsisdn table


                //--> Insert SIM POS Code in SimPosCode table
                if (IsSimPos)
                {
                    String SimPosCodeData = data["SimPosCode"].ToString();

                    if (!String.IsNullOrEmpty(SimPosCodeData))
                    {
                        if (SimPosCodeData.Contains(","))
                        {
                            String[] arrSimPosCode = SimPosCodeData.Split(',');
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
                        else
                        {
                            string sqlInsertSimPosCode = "INSERT INTO SimPosCode(SimPosCode,RetailerId)  VALUES(@SimPosCode,@RetailerId)";
                            command.CommandText = sqlInsertSimPosCode;
                            command.Parameters.Clear();
                            command.Parameters.AddWithValue("@SimPosCode", SimPosCodeData);
                            command.Parameters.AddWithValue("@RetailerId", RetailerId);
                            command.ExecuteNonQuery();
                        }
                    }
                }
                //<-- Insert SIM POS Code in SimPosCode table

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

        [HttpPost]
        public JsonResult DeleteRetailer(FormCollection data)
        {
            String encryptedSessionDigest = data["SessionId"].ToString();
            String sessionId = MyCrypto.GetDecryptedQueryString(encryptedSessionDigest);            

            String ConnectionString = WebConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;
            SqlConnection connection = new SqlConnection(ConnectionString);
            SqlCommand command = new SqlCommand(); command.Connection = connection;
            SqlTransaction transaction = null;

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
                    return Json(new { IsError = true, IsSessionTimeOut = true, url = "/Dsr/Login/Index" }, JsonRequestBehavior.AllowGet);
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
                    return Json(new { IsDeleted = false, IsSessionTimeOut = true, url = "/Dsr/Login/Index" }, JsonRequestBehavior.AllowGet);
                }


                Int32 DsrId = Convert.ToInt32(dtSessionInfo.Rows[0]["DsrId"]);
                Int32 surveyorId = Convert.ToInt32(dtSessionInfo.Rows[0]["SurveyorId"]);
                //<-------------------------Session related checking


                connection.Open();
                transaction = connection.BeginTransaction();
                command.Transaction = transaction;

                Int32 RetailerId = Convert.ToInt32(data["RetailerId"]);

                String sqlSelectRsp = "Select RspId from Person where PersonId=" + DsrId + "";
                command.CommandText = sqlSelectRsp;
                command.Parameters.Clear();
                Int32 RspId = Convert.ToInt32(command.ExecuteScalar());


                //--> update data in Retailer Table
                string sqlUpdateRetailer = "UPDATE Retailer SET RspId=@RspId, IsActive=@IsActive, ModifiedBy=@ModifiedBy,ModifiedDateTime=@ModifiedDateTime, DsrId=@DsrId,  RetailerStatusId=@RetailerStatusId,DsrActivityDateTime=@DsrActivityDateTime,SurveyorId=@SurveyorId Where RetailerId=@RetailerId";

                command.CommandText = sqlUpdateRetailer; command.Parameters.Clear();
                command.Parameters.AddWithValue("@RspId", RspId);
                command.Parameters.AddWithValue("@IsActive", false);
                command.Parameters.AddWithValue("@ModifiedBy", DsrId);
                command.Parameters.AddWithValue("@ModifiedDateTime",DateTime.Now);
                command.Parameters.AddWithValue("@DsrId",DsrId);
                command.Parameters.AddWithValue("@RetailerStatusId", 2);
                command.Parameters.AddWithValue("@DsrActivityDateTime", DateTime.Now);
                command.Parameters.AddWithValue("@SurveyorId", surveyorId);
                command.Parameters.AddWithValue("@RetailerId",RetailerId);

                command.ExecuteNonQuery();

                transaction.Commit();
                connection.Close();
                command.Dispose();
                transaction.Dispose();
                connection.Dispose();

                return Json(new { IsDeleted = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception Ex)
            {
                if (transaction != null) { transaction.Rollback(); }
                if (connection.State == ConnectionState.Open) { connection.Close(); }
                transaction.Dispose(); connection.Dispose();

                var path = HttpContext.Server.MapPath("~/App_Data");
                ExceptionLogger.CreateLog(Ex, path);
                return Json(new { IsDeleted = false, IsSessionTimeOut = false, url = "/Dsr/Login/Index" }, JsonRequestBehavior.AllowGet);
            }
        }
	}
}