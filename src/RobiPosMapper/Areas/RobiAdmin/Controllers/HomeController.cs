using Newtonsoft.Json;
using OfficeOpenXml;
using RobiPosMapper.Areas.RobiAdmin.Models;
using sCommonLib;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RobiPosMapper.Areas.RobiAdmin.Controllers
{
    public class HomeController : Controller
    {

        #region Global Variables


        CultureInfo ci = new CultureInfo("bn-BD-robi");

        string[] InputDateFormatsAllowed = {
                                                       "d/M/yy", "d/M/yyyy",
                                                       "d/MM/yy", "d/MM/yyyy", 
                                                       "dd/M/yy", "dd/M/yyyy", 
                                                       "dd/MM/yy", "dd/MM/yyyy",

                                                       "d-M-yy", "d-M-yyyy",
                                                       "d-MM-yy", "d-MM-yyyy", 
                                                       "dd-M-yy", "dd-M-yyyy", 
                                                       "dd-MM-yy", "dd-MM-yyyy",

                                                       "d.M.yy", "d.M.yyyy",
                                                       "d.MM.yy", "d.MM.yyyy", 
                                                       "dd.M.yy", "dd.M.yyyy", 
                                                       "dd.MM.yy", "dd.MM.yyyy"

                                                    };

         String ConnectionString = ConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;
         string sqlInsertMoLog = "INSERT INTO MonitoringOfficerActivityLog(MoId,LogDescription) VALUES (@MoId,@LogDescription)";
        #endregion

         public ActionResult Index()
        {
            try
            {
               Int32 MoId= Convert.ToInt32(Request.QueryString["moid"]); //Monitoring Officer Id
                //--> Variables
                DateTime today = DateTime.Today.Date;

                DataTable dtRspList = new DataTable(); DataTable dtRetailerList = new DataTable(); DataTable tblOverlappingRetailers = new DataTable();

                Int32 totalRetailersQuantity = 0; // 194305;
                Object objAllNewRetailersCount; Object objAllUpdatedRetailersCount; Object objAllNotFoundRetailersCount; Int32 AllPendingRetailersQuantity; Object objAllVerifiedCount;
                Object objTodaysUpdatedRetailersCount; Object objTodaysNewRetailersCount; Object objTodaysNotFoundRetailersCount; Object objTodaysVerifiedCount;
                DataTable dtRegions = new DataTable();
                DataTable tblDupli = new DataTable();

                DataTable tblSurveyorsQuantityPerDate = new DataTable(); //date-wise koto gulo Surveyor field-e chhilo tar data
                DataTable tblRetailersQuantityPerDate = new DataTable(); //protidin koto gulo retailer-e kaj hoechhe tar data
                DataTable tblVerifiedQuantyPerDate = new DataTable(); //date-wise koto gulo verified holo tar data. Shown on line graph.
                DataTable tblSurveyorRetailerOfToday = new DataTable(); //Ajke kon Surveyor kotota kaj korlo tar data
                DataTable tblRegionStatusBar = new DataTable(); //Region-wise Total & Updated bar chart
                //<-- Variables


                //---> SQL Statements
//                String sqlTotalOutletsOfThisMo = @"SELECT COUNT(Retailer.RetailerId) AS RetailerCount
//                                              FROM Region INNER JOIN
//                                                 Retailer ON Region.RegionId = Retailer.RegionId INNER JOIN
//                                                 MonitoringOfficerRegion AS MOR ON Region.RegionId = MOR.RegionId
//                                              WHERE (MOR.MoId = "+ MoId +")";

                String sqlTotalOutletsOfThisMo = @"SELECT COUNT(Retailer.RetailerId) AS RetailerCount
                                              FROM Region INNER JOIN
                                                 Retailer ON Region.RegionId = Retailer.RegionId INNER JOIN
                                                 MonitoringOfficerRegion AS MOR ON Region.RegionId = MOR.RegionId
                                              WHERE (MOR.MoId = " + MoId + ") AND (Retailer.RetailerStatusId<3)";


               String sqlCountAllUpdatedRetailers = @"SELECT COUNT(Retailer.RetailerId) AS RetailerCount
                                              FROM Region INNER JOIN
                                                 Retailer ON Region.RegionId = Retailer.RegionId INNER JOIN
                                                 MonitoringOfficerRegion AS MOR ON Region.RegionId = MOR.RegionId
                                   WHERE (CAST(Retailer.SurveyorActivityDateTime AS DATE) > '2015-04-01') AND (Retailer.RetailerStatusId = 2) AND (Retailer.IsActive = 1) AND (MOR.MoId = "+ MoId +")";



               String sqlCountAllNewRetailers = @"SELECT COUNT(Retailer.RetailerId) AS RetailerCount
                                           FROM Region INNER JOIN
                                                 Retailer ON Region.RegionId = Retailer.RegionId INNER JOIN
                                                 MonitoringOfficerRegion AS MOR ON Region.RegionId = MOR.RegionId
                                           WHERE (CAST(Retailer.SurveyorActivityDateTime AS DATE) > '2015-04-01') AND (Retailer.RetailerStatusId=3) AND (Retailer.IsActive=1) AND (MOR.MoId=" + MoId + ")";


               String sqlCountAllNotFoundRetailers = @"SELECT COUNT(Retailer.RetailerId) AS RetailerCount
                                                FROM Region INNER JOIN
                                                  Retailer ON Region.RegionId = Retailer.RegionId INNER JOIN
                                                  MonitoringOfficerRegion AS MOR ON Region.RegionId = MOR.RegionId
                                                 Where (Cast(Retailer.InactiveDateTime as DATE)>'2015-04-01') and (Retailer.IsActive=0) AND (MOR.MoId=" + MoId + ")";

               string sqlCountAllVerifiedRetailers = @"SELECT COUNT(dbo.Retailer.RetailerId) AS VerifiedQuantity
                                                       FROM dbo.Retailer INNER JOIN
                                                            dbo.MonitoringOfficerRegion ON dbo.Retailer.RegionId = dbo.MonitoringOfficerRegion.RegionId
                                                       WHERE (dbo.Retailer.IsVerifiedByRsp = 1) AND (dbo.MonitoringOfficerRegion.MoId = "+ MoId +")";

                String sqlCountTodaysUpdatedRetailers = @"SELECT COUNT(Retailer.RetailerId) AS RetailerCount
                                                FROM Region INNER JOIN
                                                  Retailer ON Region.RegionId = Retailer.RegionId INNER JOIN
                                                  MonitoringOfficerRegion AS MOR ON Region.RegionId = MOR.RegionId
                                               Where Cast(Retailer.SurveyorActivityDateTime as DATE)='" + today.ToString("yyyy-MM-dd") + "' and (Retailer.RetailerStatusId=2) And (Retailer.IsActive=1) AND (MOR.MoId=" + MoId + ")";

                String sqlCountTodaysNewRetailers = @"SELECT COUNT(Retailer.RetailerId) AS RetailerCount
                                              FROM Region INNER JOIN
                                                  Retailer ON Region.RegionId = Retailer.RegionId INNER JOIN
                                                  MonitoringOfficerRegion AS MOR ON Region.RegionId = MOR.RegionId
                                              Where (Cast(Retailer.SurveyorActivityDateTime as DATE)='" + today.ToString("yyyy-MM-dd") + "') and (Retailer.RetailerStatusId=3) AND (Retailer.IsActive=1)  AND (MOR.MoId=" + MoId + ")";


                String sqlCountTodaysNotFoundRetailers = @"SELECT COUNT(Retailer.RetailerId) AS RetailerCount
                                              FROM Region INNER JOIN
                                                  Retailer ON Region.RegionId = Retailer.RegionId INNER JOIN
                                                  MonitoringOfficerRegion AS MOR ON Region.RegionId = MOR.RegionId
                                                 Where (Cast(Retailer.InactiveDateTime as DATE)='" + today.ToString("yyyy-MM-dd") + "') AND (Retailer.IsActive=0) AND (MOR.MoId=" + MoId + ")";

                //Use SurveyorActivityDateTime instead of RspVerificationDateTime. Otherwise Quantity mismatched with some other places.
                string sqlCountTodaysVerifiedRetailers = @"SELECT COUNT(R.RetailerId) AS VerifiedQuantity
                                                           FROM dbo.Retailer AS R INNER JOIN
                                                                dbo.MonitoringOfficerRegion AS M ON R.RegionId = M.RegionId
                                                           WHERE (R.IsVerifiedByRsp = 1) AND (M.MoId = " + MoId + ") AND (CAST(R.SurveyorActivityDateTime AS Date) = '" + DateTime.Today.Date.ToString("yyyy-MM-dd") + "')";

                String sqlSelectRegion = @"SELECT Region.RegionId, dbo.Region.RegionName
                                   FROM dbo.Region INNER JOIN
                                         dbo.MonitoringOfficerRegion ON dbo.Region.RegionId = dbo.MonitoringOfficerRegion.RegionId
                                   WHERE (dbo.MonitoringOfficerRegion.MoId = "+ MoId +") ORDER BY dbo.Region.RegionName";

                String sqlSelectRsp = @"SELECT Region.RegionName, Area.AreaName, RSP.RspId, RSP.RspName
                                FROM Area INNER JOIN
                                dbo.RSP ON dbo.Area.AreaId = dbo.RSP.AreaId INNER JOIN
                                dbo.Region ON dbo.Area.RegionId = dbo.Region.RegionId INNER JOIN
                                dbo.MonitoringOfficerRegion ON dbo.Region.RegionId = dbo.MonitoringOfficerRegion.RegionId
                                WHERE (dbo.MonitoringOfficerRegion.MoId = "+ MoId + @")
                                ORDER BY dbo.Region.RegionName, dbo.Area.AreaName, dbo.RSP.RspName";

                String sqlSelectRetailers = @"SELECT Retailer.RetailerId, Retailer.RetailerStatusId, Retailer.IsActive, dbo.Retailer.RspId, dbo.Retailer.SurveyorId,      
                                           Cast(SurveyorActivityDateTime as DATE) as SurveyorDate
                                     FROM dbo.Retailer INNER JOIN
                                       dbo.Region ON dbo.Retailer.RegionId = dbo.Region.RegionId INNER JOIN
                                       dbo.MonitoringOfficerRegion ON dbo.Region.RegionId = dbo.MonitoringOfficerRegion.RegionId
                                     WHERE (dbo.MonitoringOfficerRegion.MoId = "+ MoId +")";


                String sqlSelectOverlappingRetailers = @"SELECT dbo.SrRetailerLog.RetailerId, COUNT(DISTINCT dbo.SrRetailerLog.SrId) AS MultipleSrQuantity
                                                         FROM dbo.Retailer INNER JOIN
                                                            dbo.SrRetailerLog ON dbo.Retailer.RetailerId = dbo.SrRetailerLog.RetailerId INNER JOIN
                                                            dbo.MonitoringOfficerRegion ON dbo.Retailer.RegionId = dbo.MonitoringOfficerRegion.RegionId
                                                         WHERE (dbo.MonitoringOfficerRegion.MoId = "+ MoId + @")
                                                         GROUP BY dbo.SrRetailerLog.RetailerId
                                                         HAVING (COUNT(DISTINCT dbo.SrRetailerLog.SrId) > 1)
                                                         ORDER BY dbo.SrRetailerLog.RetailerId";



                string sqlDuplicateImages = @"SELECT     TOP (100) PERCENT Photo.GroupId, Photo.RetailerId, Photo.PhotoDateTime, R.SurveyorActivityDateTime, R.RetailerName, R.Address, R.DefaultPhotoName, dbo.RSP.RspName, S.LoginName AS SR,
                       S.SurveyorName, R.DsrId, P.PersonName, Photo.IsRemovedLaterOn, Photo.ThisIsNotDuplicate, dbo.Region.RegionName
FROM         dbo.Surveyors AS S INNER JOIN
                      dbo.Region ON S.RegionId = dbo.Region.RegionId RIGHT OUTER JOIN
                      dbo.DuplicatePhotos AS Photo INNER JOIN
                      dbo.Retailer AS R ON Photo.RetailerId = R.RetailerId INNER JOIN
                      dbo.RSP ON R.RspId = dbo.RSP.RspId INNER JOIN
                      dbo.Person AS P ON R.DsrId = P.PersonId ON S.SurveyorId = R.SurveyorId
WHERE      (Photo.ThisIsNotDuplicate = 0) AND (S.SurveyorId IS NOT NULL) AND (S.RegionId IN
                          (SELECT     RegionId
                            FROM          dbo.MonitoringOfficerRegion
                            WHERE      (MoId = "+ MoId +"))) ORDER BY Photo.GroupId";


                //protidin koto gulo Surveyor field-e chhilo tar data
                string sqlSurveyorsQuantityPerDate = @"SELECT COUNT(Distinct(R.SurveyorId)) as SRQuantity,Cast(R.SurveyorActivityDateTime as Date) AS WorkDate
                                                       FROM dbo.Retailer AS R INNER JOIN
                                                            dbo.Surveyors AS S ON R.SurveyorId = S.SurveyorId INNER JOIN
                                                            dbo.MonitoringOfficerRegion AS M ON S.RegionId = M.RegionId
                                                      Where  Cast(R.SurveyorActivityDateTime as Date)>'2015-05-01' AND M.MoId="+ MoId+ @"
                                                      Group By Cast(R.SurveyorActivityDateTime as Date)
                                                      Order By Cast(R.SurveyorActivityDateTime as Date) DESC";

                //date-wise koto gulo retailer-e kaj hoechhe tar data
                string sqlRetailersQuantityPerDate = @"SELECT  COUNT(R.RetailerId) as RetailerQuantity, Cast(R.SurveyorActivityDateTime as Date) AS WorkDate
                                                       FROM dbo.Retailer AS R INNER JOIN
                                                            dbo.Surveyors AS S ON R.SurveyorId = S.SurveyorId INNER JOIN
                                                            dbo.MonitoringOfficerRegion AS M ON S.RegionId = M.RegionId
                                                       Where  Cast(R.SurveyorActivityDateTime as Date)>'2015-05-01' AND M.MoId="+ MoId+ @"
                                                       Group By Cast(R.SurveyorActivityDateTime as Date)
                                                       Order By Cast(R.SurveyorActivityDateTime as Date) DESC";

                //date-wise koto gulo retailer verify holo tar data. Shown on line graph.
                string sqlVerifiedQuantityPerDate = @"SELECT  COUNT(R.RetailerId) as RetailerQuantity, Cast(R.SurveyorActivityDateTime as Date) AS WorkDate
                                                       FROM dbo.Retailer AS R INNER JOIN
                                                            dbo.MonitoringOfficerRegion AS M ON R.RegionId = M.RegionId
                                                       Where  Cast(R.SurveyorActivityDateTime as Date)>'2015-05-01' AND M.MoId="+ MoId + @" AND R.IsVerifiedByRsp=1
                                                       Group By Cast(R.SurveyorActivityDateTime as Date)
                                                       Order By Cast(R.SurveyorActivityDateTime as Date) DESC";

                //Ajke Kon Surveyor kotota kaj korlo tar data
                string sqlSurveyorRetailerOfToday = @"SELECT  S.SurveyorId, COUNT(R.RetailerId) as RetailerQuantity
                                                     FROM dbo.Retailer AS R INNER JOIN
                                                          dbo.Surveyors AS S ON R.SurveyorId = S.SurveyorId INNER JOIN
                                                          dbo.MonitoringOfficerRegion AS M ON S.RegionId = M.RegionId
                                                     Where  Cast(R.SurveyorActivityDateTime as Date)='" + DateTime.Today.Date.ToString("yyyy-MM-dd") + @"' AND M.MoId=" + MoId + @"
                                                     Group By S.SurveyorId
                                                     Order By S.SurveyorId ASC";

                //Region-wise Total & Updated bar chart
                string sqlRegionStatusBar = @"SELECT     dbo.Region.RegionName, (Select Count(RetailerId) from dbo.Retailer where RegionId=dbo.Region.RegionId AND dbo.Retailer.RetailerStatusId<3)  AS                                             TotalRetailer, ((Select Count(RetailerId) from dbo.Retailer where RegionId=dbo.Region.RegionId AND dbo.Retailer.RetailerStatusId=2 AND                                                      dbo.Retailer.IsActive=1) + (Select Count(RetailerId) from dbo.Retailer where RegionId=dbo.Region.RegionId AND dbo.Retailer.IsActive=0)) AS                                                UpdatedRetailer
                                              FROM dbo.Region INNER JOIN
                                                      dbo.MonitoringOfficerRegion ON dbo.Region.RegionId = dbo.MonitoringOfficerRegion.RegionId
                                              WHERE (dbo.MonitoringOfficerRegion.MoId = "+ MoId +")";
                //<--- SQL Statements
 
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = connection;
                        using (SqlDataAdapter da = new SqlDataAdapter(command))
                        {
                            
                            connection.Open();
                            //Count all retailer quantity
                            command.CommandText = sqlTotalOutletsOfThisMo; totalRetailersQuantity = Convert.ToInt32(command.ExecuteScalar());
                            //Count all updated retailers
                            command.CommandText = sqlCountAllUpdatedRetailers; objAllUpdatedRetailersCount = command.ExecuteScalar();
                            //Count all new retailers
                            command.CommandText = sqlCountAllNewRetailers; objAllNewRetailersCount = command.ExecuteScalar();
                            //Count all not found retailers
                            command.CommandText = sqlCountAllNotFoundRetailers; objAllNotFoundRetailersCount = command.ExecuteScalar();
                            //Count all verified
                            command.CommandText = sqlCountAllVerifiedRetailers; objAllVerifiedCount = command.ExecuteScalar();
                            //Count todays updated retailes
                            command.CommandText = sqlCountTodaysUpdatedRetailers; objTodaysUpdatedRetailersCount = command.ExecuteScalar();
                            //Count todays new retailes
                            command.CommandText = sqlCountTodaysNewRetailers; objTodaysNewRetailersCount = command.ExecuteScalar();
                            //Count todays not found retailes
                            command.CommandText = sqlCountTodaysNotFoundRetailers; objTodaysNotFoundRetailersCount = command.ExecuteScalar();
                            //Count todays verified retailers
                            command.CommandText = sqlCountTodaysVerifiedRetailers; objTodaysVerifiedCount = command.ExecuteScalar();
                            // fill RSP table
                            command.CommandText = sqlSelectRsp;   da.Fill(dtRspList);
                            //fill Retailer table
                            command.CommandText = sqlSelectRetailers; da.Fill(dtRetailerList);
                            //fill Region table.
                            command.CommandText = sqlSelectRegion; da.Fill(dtRegions); 
                            //fill Overlapping Retailer table
                            command.CommandText = sqlSelectOverlappingRetailers; da.Fill(tblOverlappingRetailers);
                            //fill duplicate images
                            command.CommandText = sqlDuplicateImages; da.Fill(tblDupli);
                            //protidin koto gulo Surveyor field-e chhilo tar data
                            command.CommandText = sqlSurveyorsQuantityPerDate; da.Fill(tblSurveyorsQuantityPerDate);
                            //protidin koto gulo retailer-e kaj hoechhe tar data
                            command.CommandText = sqlRetailersQuantityPerDate; da.Fill(tblRetailersQuantityPerDate);
                            //date-wise kotogulo verify holo tar data. Shown on line graph.
                            command.CommandText = sqlVerifiedQuantityPerDate; da.Fill(tblVerifiedQuantyPerDate);
                            //kon surveyor kotota kaj korlo tar data
                            command.CommandText = sqlSurveyorRetailerOfToday; da.Fill(tblSurveyorRetailerOfToday);
                            //Region-wise Total & Updated bar chart
                            command.CommandText = sqlRegionStatusBar; da.Fill(tblRegionStatusBar);
                            connection.Close();

                            //Insert MO Log
                            command.CommandText = sqlInsertMoLog;
                            command.Parameters.Clear();
                            command.Parameters.Add("@MoId", SqlDbType.Int).Value = MoId;
                            command.Parameters.Add("@LogDescription", SqlDbType.VarChar).Value = "Comes on Dashboard.";
                            connection.Open(); command.ExecuteNonQuery(); connection.Close();
                        }
                    }
                }

                string overLappingInformation = string.Empty;
                if (tblOverlappingRetailers.Rows.Count>0)
                {
                    string overLappedRetailers = tblOverlappingRetailers.Rows.Count.ToString();
                    string overLappedSr = tblOverlappingRetailers.Compute("SUM(MultipleSrQuantity)","").ToString();
                    overLappingInformation = overLappedRetailers + " retailers overlapped by " + overLappedSr + " different surveyors.";
                }

                //--> For search panel
                ViewBag.Regions = dtRegions;
                //<-- For search panel

                //--> Forcasting
                int avgOutletsPerday = Convert.ToInt32(objAllUpdatedRetailersCount) / tblRetailersQuantityPerDate.Rows.Count;
                int requiredDays = totalRetailersQuantity - (Convert.ToInt32(objAllUpdatedRetailersCount) + Convert.ToInt32(objAllNotFoundRetailersCount));
                requiredDays = requiredDays / avgOutletsPerday;
                string forcastMessage = string.Format("{0} more days required to complete. Current average rate {1} outlets/day.",requiredDays, avgOutletsPerday.ToString("N",ci));
                //<-- Forcasting

                //--> Line graph in workflow progress
                    string[] chartLabels = new string[tblRetailersQuantityPerDate.Rows.Count];
                    for (int i = tblRetailersQuantityPerDate.Rows.Count-1; i >0 ; i--)
                    {
                        chartLabels[i] = Convert.ToDateTime(tblRetailersQuantityPerDate.Rows[i]["WorkDate"]).ToString("dd-MMM") ;
                    }

                    chartLabels[0]= Convert.ToDateTime(tblRetailersQuantityPerDate.Rows[0]["WorkDate"]).ToString("dd-MMM") ;

                    //--> Retailer quantity
                    int[] chartData = new int[tblRetailersQuantityPerDate.Rows.Count];
                    for (int i = tblRetailersQuantityPerDate.Rows.Count - 1 ; i > 0; i--)
                    {
                        chartData[i] = Convert.ToInt32(tblRetailersQuantityPerDate.Rows[i]["RetailerQuantity"]);
                    }

                    chartData[0] = Convert.ToInt32(tblRetailersQuantityPerDate.Rows[0]["RetailerQuantity"]);
                    //<-- retailer quantity

                    //--> verified quantity
                    int[] verifiedData = new int[tblVerifiedQuantyPerDate.Rows.Count];
                    for (int i = tblVerifiedQuantyPerDate.Rows.Count - 1; i > 0; i--)
                    {
                        verifiedData[i] = Convert.ToInt32(tblVerifiedQuantyPerDate.Rows[i]["RetailerQuantity"]); //Verified quantity
                    }

                    verifiedData[0] = Convert.ToInt32(tblVerifiedQuantyPerDate.Rows[0]["RetailerQuantity"]);
                    //<-- verified quantity

                //<-- Line graph in workflow progress

                //--> Region Chart - Chart2
                int chart2DataQuantity = tblRegionStatusBar.Rows.Count;
                string[] chart2Regions = new string[chart2DataQuantity];
                int[] chart2TotalRetailerQuantity = new int[chart2DataQuantity];
                int[] chart2UpdatedRetailerQuantity = new int[chart2DataQuantity]; // updated + not found. Calculated in sql statement
                for (int i = 0; i < chart2DataQuantity; i++)
                {
                    string regionName = tblRegionStatusBar.Rows[i]["RegionName"].ToString();
                    int totalQuantity = Convert.ToInt32(tblRegionStatusBar.Rows[i]["TotalRetailer"]);
                    int updatedQuantity=Convert.ToInt32(  tblRegionStatusBar.Rows[i]["UpdatedRetailer"]);
                    double percentage = totalQuantity!=0?(updatedQuantity * 100) / totalQuantity:0;

                    chart2Regions[i] = string.Format("{0} - {1}%", regionName, percentage.ToString());
                    chart2TotalRetailerQuantity[i] = totalQuantity;
                    chart2UpdatedRetailerQuantity[i] = updatedQuantity;
                }

                //<--Region Chart - Chart2
                //-->ViewBag
                ViewBag.MoId = MoId.ToString();
                ViewBag.UserName = Request.QueryString["user"].ToString(); //LoginName

                ViewBag.TotalRetailersQuantity = totalRetailersQuantity;
                ViewBag.AllUpdatedRetailersCount = Convert.ToInt32( objAllUpdatedRetailersCount); //All updated retailers count.
                ViewBag.AllNewRetailersCount = Convert.ToInt32(objAllNewRetailersCount); //All new retailers count.
                ViewBag.AllNotFoundRetailersCount = Convert.ToInt32(objAllNotFoundRetailersCount); // All not found retailers count.
                ViewBag.AllPendingRetailersCount= totalRetailersQuantity - (Convert.ToInt32(objAllUpdatedRetailersCount) + Convert.ToInt32(objAllNotFoundRetailersCount));
                ViewBag.AllVerifiedRetailersCount = Convert.ToInt32(objAllVerifiedCount); //all verified
                ViewBag.TodaysUpdatedRetailersCount = Convert.ToInt32(objTodaysUpdatedRetailersCount); //Todays updated retailers count.
                ViewBag.TodaysNewRetailersCount = Convert.ToInt32(objTodaysNewRetailersCount); // Todays new retailer count.
                ViewBag.TodaysNotFoundRetailersCount = Convert.ToInt32(objTodaysNotFoundRetailersCount); // Todays not found retailser count.
                ViewBag.TodaysVerifiedCount = Convert.ToInt32(objTodaysVerifiedCount);

                ViewBag.OverLappingInformation = overLappingInformation;
                ViewBag.MyCulture = ci;
                ViewBag.ActiveMenuItem = "Dashboard";
                ViewBag.DuplicateImagesCount = tblDupli.Rows.Count;
                ViewBag.SurveyorsQuantityPerDate = tblSurveyorsQuantityPerDate;
                ViewBag.RetailersQuantityPerDate = tblRetailersQuantityPerDate;
                ViewBag.SurveyorRetailerOfToday = tblSurveyorRetailerOfToday;
                ViewBag.ChartLabel = chartLabels;
                ViewBag.ChartData = chartData;
                ViewBag.VerifiedData = verifiedData;
                ViewBag.ChartTwoRegion = chart2Regions;
                ViewBag.ChartTwoTotalRetailer = chart2TotalRetailerQuantity;
                ViewBag.ChartTwoUpdateRetailer = chart2UpdatedRetailerQuantity;
                ViewBag.ForcastMessage = forcastMessage;
                //<-- ViewBag

                return View();
            }
            catch (Exception ex)
            {
                var path = HttpContext.Server.MapPath("~/App_Data");
                ExceptionLogger.CreateLog(ex, path);
                return View("ErrorContent");
            }


            return View();
        }

        private static DataRow AddAsOverallRecord(DataTable dtRetailers, DataTable dtOverallRspStatistics, Int32 rspId, String rspName, String regionName, String areaName)
        {
            DataRow row;
            row = dtOverallRspStatistics.NewRow();
            row["rspId"] = rspId;
            row["RspName"] = rspName;
            row["RegionName"] = regionName;
            row["AreaName"] = areaName;
            Int32 totalRetailersQuantity = Convert.ToInt32(dtRetailers.Compute("Count(RetailerId)", "RspId=" + rspId + " and IsActive=1"));
           //row["TotalRetailersQuantity"] = totalRetailersQuantity;
            Int32 workDone = Convert.ToInt32(dtRetailers.Compute("Count(RetailerId)", "RspId=" + rspId + " and SurveyorDate is not null"));
            row["WorkDone"] = workDone;

            Int32 pending = totalRetailersQuantity - workDone;
            row["Pending"] = pending;
            Int32 newRetailer = Convert.ToInt32(dtRetailers.Compute("Count(RetailerId)", "RspId=" + rspId + " and RetailerStatusId=3 AND IsReevaluated=0 and SurveyorDate is not null"));
            row["New"] = newRetailer;

            row["TotalRetailersQuantity"] = totalRetailersQuantity-newRetailer;

            return row;
        }
       
        private static DataRow AddAsTodaysRecord(DataTable dtRetailers, DataTable dtTodaysRspStatistics, String today, Int32 rspId, String rspName, String regionName, String areaName)
        {
            DataRow row;
            row = dtTodaysRspStatistics.NewRow();
            row["RspId"] = rspId;
            row["RspName"] = rspName;
            row["RegionName"] = regionName;
            row["AreaName"] = areaName;
            Int32 reevaluated = Convert.ToInt32(dtRetailers.Compute("Count(RetailerId)", "RspId=" + rspId + " and RetailerStatusId=2 and IsReevaluated=1 AND SurveyorDate='" + today + "'"));
            row["Reevaluated"] = reevaluated;
            Int32 updated = Convert.ToInt32(dtRetailers.Compute("Count(RetailerId)", "RspId=" + rspId + " and RetailerStatusId=2 and IsReevaluated=0 AND SurveyorDate='" + today + "'"));
            row["Updated"] = updated;
            Int32 newRetailer = Convert.ToInt32(dtRetailers.Compute("Count(RetailerId)", "RspId=" + rspId + " and RetailerStatusId=3 AND IsReevaluated=0 AND SurveyorDate='" + today + "'"));
            row["New"] = newRetailer;
            Int32 total = reevaluated + updated + newRetailer;
            row["Total"] = total;
            return row;
        }

        public ActionResult SrAtField()
        {
            DateTime date2, date1;
            date1 = DateTime.Now.Date;
            TimeSpan time1 = new TimeSpan(23, 59, 59);
            date2 = date1.Add(time1);
            System.Globalization.CultureInfo ci = System.Globalization.CultureInfo.InvariantCulture;
            string d1 = date1.ToString("yyyy-MM-dd HH:mm:ss.f", ci);
            string d2 = date2.ToString("yyyy-MM-dd HH:mm:ss.f", ci);
            String sqlSRList = String.Format("select dsrId,Person.PersonName,Person.PersonTypeId,Person.PersonMsisdn,PersonType.PersonTypeName,Area.AreaId,Area.AreaName,Retailer.RspId,RSP.RspName,Count(RetailerId) as POSNo  FROM            dbo.Retailer INNER JOIN "
                         + " dbo.RSP ON dbo.Retailer.RspId = dbo.RSP.RspId INNER JOIN"
                         + " dbo.Area ON dbo.Retailer.AreaId = dbo.Area.AreaId INNER JOIN"
                         + " dbo.Person ON dbo.Retailer.DsrId = dbo.Person.PersonId INNER JOIN"
                         +" dbo.PersonType ON dbo.Person.PersonTypeId = dbo.PersonType.PersonTypeId"
                         + " where DsrActivityDateTime between '{0}' and '{1}' group by dsrId,Person.PersonName,Person.PersonMsisdn,Person.PersonTypeId,PersonType.PersonTypeName,Area.AreaId,Area.AreaName,Retailer.RspId,RSP.RspName", d1, d2);

            String ConnectionString = ConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;
            DataTable dtSRList = new DataTable();
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    using (SqlCommand command = new SqlCommand(sqlSRList, connection))
                    {
                        using (SqlDataAdapter da = new SqlDataAdapter(command))
                        {
                            connection.Open();
                            da.Fill(dtSRList);
                            
                            connection.Close();
                        }
                    }
                }

                ViewBag.SRList = dtSRList;
                return View("sratfield");
            }
            catch (Exception ex)
            {
                var path = HttpContext.Server.MapPath("~/App_Data");
                ExceptionLogger.CreateLog(ex, path);
                return View("ErrorContent");
            }
            return View("sratfield");
        }


        public ActionResult AdvanceSearch() 
        {
            String ConnectionString = ConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;
            String sqlSelectArea = "select [AreaId],[AreaName] from [Area]";
            DataTable dtAreaList = new DataTable();
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                using (SqlCommand command = new SqlCommand(sqlSelectArea, connection))
                {
                    using (SqlDataAdapter da = new SqlDataAdapter(command))
                    {
                        connection.Open();
                        da.Fill(dtAreaList);
                        connection.Close();
                    }
                }
            }

            ViewBag.AreaList = dtAreaList;
            return View("advanceSearch");
        }
        public JsonResult GetRSP(int areaId)
        {
            using (DataAccess.RobiPosMappingEntities db = new DataAccess.RobiPosMappingEntities())
            {
                var rsps = (from a in db.RSPs where a.AreaId == areaId select new { RSPId = a.RspId, RSPName = a.RspName }).ToList();
                return Json(rsps);
            }
        }

        public JsonResult GetSR(int rspId)
        {
            using (DataAccess.RobiPosMappingEntities db = new DataAccess.RobiPosMappingEntities())
            {
                var srs = (from a in db.People where a.RspId==rspId  select new { SRId = a.PersonId, SRName = a.PersonName }).ToList();
                return Json(srs);
            }
        }
        public ActionResult GetElmsisdn(int id)
        {
            using (DataAccess.RobiPosMappingEntities db = new DataAccess.RobiPosMappingEntities())
            {
                var elmsisdns = (from a in db.ElMsisdns where a.RetailerId == id select new { ID = a.ElMsisdnId, Name = a.ElMsisdn1 }).ToList();
                return Json(new { msg = "success", Rdata = elmsisdns });
            }
        }
        public ActionResult GetSimpos(int id)
        {
            using (DataAccess.RobiPosMappingEntities db = new DataAccess.RobiPosMappingEntities())
            {
                var simpos = (from a in db.SimPosCodes where a.RetailerId == id select new { ID = a.SimPosCodeId, Name = a.SimPosCode1 }).ToList();
                return Json(new { msg = "success", Rdata = simpos });
            }
        }
        public ActionResult ExportToCSV(int areId = 0, int rspId = 0, int srId = 0, string sDate = "", string eDate = "")
        {
            DateTime StartDate = DateTime.Now.Date; DateTime EndDate = DateTime.Now.Date;
            if (sDate != "")
            {
                Boolean isStartDateValid = DateTime.TryParseExact(sDate, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out StartDate);
                if (!isStartDateValid)
                {
                    return Json(new { status = "error", details = "Start date is invalid. It must be in dd/MM/yyyy format." }, JsonRequestBehavior.AllowGet);
                }


            }
            if (eDate != "")
            {
                Boolean isEndDateValid = DateTime.TryParseExact(eDate, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out EndDate);
                if (!isEndDateValid)
                {
                    return Json(new { status = "error", details = "End date is invalid. It must be in dd/MM/yyyy format." }, JsonRequestBehavior.AllowGet);
                }
            }
            TimeSpan ts = new TimeSpan(23, 59, 59);
            EndDate = EndDate.Add(ts);
            using (DataAccess.RobiPosMappingEntities db = new DataAccess.RobiPosMappingEntities())
            {
                var retailers = from a in db.Retailers where ((a.IsActive == true) || ((a.IsActive == false) && (a.RetailerStatusId == 2))) select a;
                if (areId > 0)
                {
                    retailers = from a in retailers where a.AreaId == areId select a;
                }
                if (rspId > 0)
                {
                    retailers = from a in retailers where a.RspId == rspId select a;
                }
                if (srId > 0)
                {
                    retailers = from a in retailers where a.DsrId == srId select a;
                }
                if (StartDate <= EndDate)
                {
                    retailers = from a in retailers where a.DsrActivityDateTime != null && a.DsrActivityDateTime >= StartDate && a.DsrActivityDateTime <= EndDate select a;
                }
                var retailerList = (from a in retailers
                                    select new
                                    {
                                        RetailerID = a.RetailerId,
                                        Region = a.Area.Region.RegionName,
                                        Area = a.Area.AreaName,
                                        Thana = a.Thana.ThanaName,
                                        Union = a.Ward.WardName,
                                        Mauza = a.Mauza.MauzaName,
                                        Village = a.Village.VillageName,
                                        RetailerName = a.RetailerName,
                                        RetailerAddress = a.Address,
                                        ELPOS = a.IsElPos == true ? "Yes" : "No",
                                        SIMPOS = a.IsSimPos == true ? "Yes" : "No",
                                        ScratchCardPOS = a.IsScPos == true ? "Yes" : "No",
                                        Longitude = a.Longitude.ToString(),
                                        Latitude = a.Latitude.ToString(),
                                        DSRMSISDN = (int)a.Person.PersonMsisdn,
                                        VisitDays = a.VisitDay.VisitDays,
                                        DSRRouteID = a.Person.PersonMsisdn.ToString() + a.VisitDay.VisitDayCode,
                                        POSStructure = a.PosStructure.PosStructureName,
                                        ShopSignage = a.ShopSignage.ShopSignageName,
                                        ShopType = a.ShopType.ShopTypeName,
                                        Apartments = a.IsApartments == true ? "Yes" : "No",
                                        Slums = a.IsSlums == true ? "Yes" : "No",
                                        SemiUrbanHousing = a.IsSemiUrbunHousing == true ? "Yes" : "No",
                                        RuralHousing = a.IsRuralHousing == true ? "Yes" : "No",
                                        ShoppingMall = a.IsShoppingMall == true ? "Yes" : "No",
                                        RetailHub = a.IsRetailHub == true ? "Yes" : "No",
                                        MobileDeviceMarket = a.IsMobileDeviceMarket == true ? "Yes" : "No",
                                        Bazaar = a.IsBazaar == true ? "Yes" : "No",
                                        OfficeArea = a.IsOfficeArea == true ? "Yes" : "No",
                                        GarmentsMajorityArea = a.IsGarmentsMajorityArea == true ? "Yes" : "No",
                                        GeneralIndustrialArea = a.IsGeneralIndustrialArea == true ? "Yes" : "No",
                                        UrbanTransitPoints = a.IsUrbanTransitPoints == true ? "Yes" : "No",
                                        RuralTransitPoints = a.IsRuralTransitPoints == true ? "Yes" : "No",
                                        UrbanYouthHangouts = a.IsUrbanYouthHangouts == true ? "Yes" : "No",
                                        SemiUrbanYouthHangouts = a.IsSemiUrbanYouthHangouts == true ? "Yes" : "No",
                                        RuralYouthHangouts = a.IsRuralYouthHangouts == true ? "Yes" : "No",
                                        TouristDestinations = a.IsTouristDestinations == true ? "Yes" : "No"
                                    }).ToList();
                string retailerStr = "RetailerID;,Region;,Area;,Thana;,Union;,Mauza;,Villag;,Retailer Name;,Retailer Address;,EL POS;,SIM POS;,Scratch Card POS;,Longitude;,Latitude;,DSR MSISDN;,Visit Days;,DSR Route ID;,POS Structure;,Shop Signage;,ShopType;,Apartments;,Slums;,Semi-Urban Housing;,Rural Housing;,Shopping Mall;,Retail Hub;,Mobile Device Market;,Bazaar;,Office Area;,Garments Majority Area;,General Industrial Area;,Urban Transit Points;,Rural Transit Points;,Urban Youth Hangouts;,Semi Urban Youth Hangouts;,Rural Youth Hangouts;,Tourist Destinations \n";
                string csv = string.Concat(from e in retailerList
                                           select string.Format("{0};,{1};,{2};,{3};,{4};,{5};,{6};,{7};,{8};,{9};,{10};,{11};,{12};,{13};,{14};,{15};,{16};,{17};,{18};,{19};,{20};,{21};,{22};,{23};,{24};,{25};,{26};,{27};,{28};,{29};,{30};,{31};,{32};,{33};,{34};,{35};,{36}\n", e.RetailerID, e.Region, e.Area, e.Thana, e.Union, e.Mauza, e.Village, e.RetailerName, e.RetailerAddress, e.ELPOS, e.SIMPOS, e.ScratchCardPOS, e.Longitude, e.Latitude, e.DSRMSISDN, e.VisitDays, e.DSRRouteID, e.POSStructure, e.ShopSignage,  e.ShopType, e.Apartments, e.Slums, e.SemiUrbanHousing, e.RuralHousing, e.ShoppingMall, e.RetailHub, e.MobileDeviceMarket, e.Bazaar, e.OfficeArea, e.GarmentsMajorityArea, e.GeneralIndustrialArea, e.UrbanTransitPoints, e.RuralTransitPoints, e.UrbanYouthHangouts, e.SemiUrbanYouthHangouts, e.RuralYouthHangouts,e.TouristDestinations));
                retailerStr += csv;
                Response.Clear();
                Response.AddHeader("content-disposition", "attachment;  filename=List.csv");
                Response.ContentType = "text/csv";
                Response.Write(retailerStr);
                Response.End();
                return View("Index");
            }
        }
        public ActionResult ExportToExcel(int areId = 0, int rspId = 0, int srId = 0, string sDate = "", string eDate = "")
        {
            DateTime StartDate = DateTime.Now.Date; DateTime EndDate = DateTime.Now.Date;
            if (sDate != "")
            {
                Boolean isStartDateValid = DateTime.TryParseExact(sDate, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out StartDate);
                if (!isStartDateValid)
                {
                    return Json(new { status = "error", details = "Start date is invalid. It must be in dd/MM/yyyy format." }, JsonRequestBehavior.AllowGet);
                }


            }
            if (eDate != "")
            {
                Boolean isEndDateValid = DateTime.TryParseExact(eDate, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out EndDate);
                if (!isEndDateValid)
                {
                    return Json(new { status = "error", details = "End date is invalid. It must be in dd/MM/yyyy format." }, JsonRequestBehavior.AllowGet);
                }
            }
            TimeSpan ts = new TimeSpan(23, 59, 59);
            EndDate = EndDate.Add(ts);
            using (DataAccess.RobiPosMappingEntities db = new DataAccess.RobiPosMappingEntities())
            {
                var retailers = from a in db.Retailers where ((a.IsActive == true) || ((a.IsActive == false) && (a.RetailerStatusId == 2))) select a;
                if (areId > 0)
                {
                    retailers = from a in retailers where a.AreaId == areId select a;
                }
                if (rspId > 0)
                {
                    retailers = from a in retailers where a.RspId == rspId select a;
                }
                if (srId > 0)
                {
                    retailers = from a in retailers where a.DsrId == srId select a;
                }
                if (StartDate <= EndDate)
                {
                    retailers = from a in retailers where a.DsrActivityDateTime != null && a.DsrActivityDateTime >= StartDate && a.DsrActivityDateTime <= EndDate select a;
                }
                var retailerList = (from a in retailers
                                    select new
                                    {
                                        RetailerID = a.RetailerId,
                                        Region = a.Area.Region.RegionName,
                                        Area = a.Area.AreaName,
                                        Thana = a.Thana.ThanaName,
                                        Union = a.Ward.WardName,
                                        Mauza = a.Mauza.MauzaName,
                                        Village = a.Village.VillageName,
                                        RetailerName = a.RetailerName,
                                        RetailerAddress = a.Address,
                                        ELPOS = a.IsElPos == true ? "Yes" : "No",
                                        SIMPOS = a.IsSimPos == true ? "Yes" : "No",
                                        ScratchCardPOS = a.IsScPos == true ? "Yes" : "No",
                                        Longitude = a.Longitude.ToString(),
                                        Latitude = a.Latitude.ToString(),
                                        DSRMSISDN = (int)a.Person.PersonMsisdn,
                                        VisitDays = a.VisitDay.VisitDays,
                                        DSRRouteID = a.Person.PersonMsisdn.ToString() + a.VisitDay.VisitDayCode,
                                        POSStructure = a.PosStructure.PosStructureName,
                                        ShopSignage = a.ShopSignage.ShopSignageName,
                                        ShopType = a.ShopType.ShopTypeName,
                                        Apartments = a.IsApartments == true ? "Yes" : "No",
                                        Slums = a.IsSlums == true ? "Yes" : "No",
                                        SemiUrbanHousing = a.IsSemiUrbunHousing == true ? "Yes" : "No",
                                        RuralHousing = a.IsRuralHousing == true ? "Yes" : "No",
                                        ShoppingMall = a.IsShoppingMall == true ? "Yes" : "No",
                                        RetailHub = a.IsRetailHub == true ? "Yes" : "No",
                                        MobileDeviceMarket = a.IsMobileDeviceMarket == true ? "Yes" : "No",
                                        Bazaar = a.IsBazaar == true ? "Yes" : "No",
                                        OfficeArea = a.IsOfficeArea == true ? "Yes" : "No",
                                        GarmentsMajorityArea = a.IsGarmentsMajorityArea == true ? "Yes" : "No",
                                        GeneralIndustrialArea = a.IsGeneralIndustrialArea == true ? "Yes" : "No",
                                        UrbanTransitPoints = a.IsUrbanTransitPoints == true ? "Yes" : "No",
                                        RuralTransitPoints = a.IsRuralTransitPoints == true ? "Yes" : "No",
                                        UrbanYouthHangouts = a.IsUrbanYouthHangouts == true ? "Yes" : "No",
                                        SemiUrbanYouthHangouts = a.IsSemiUrbanYouthHangouts == true ? "Yes" : "No",
                                        RuralYouthHangouts = a.IsRuralYouthHangouts == true ? "Yes" : "No",
                                        TouristDestinations = a.IsTouristDestinations == true ? "Yes" : "No"
                                    });
                var simPOSList = (from a in retailerList join b in db.SimPosCodes on a.RetailerID equals b.RetailerId select new { RetailerId = a.RetailerID, SimPosCode = b.SimPosCode1 });
                var elList = (from a in retailerList join b in db.ElMsisdns on a.RetailerID equals b.RetailerId select new { RetailerId = a.RetailerID, ELCode = b.ElMsisdn1 });
                DataTable dt1 = new DataTable();
                dt1 = retailerList.CopyToAnyDataTable();
                DataTable dt2 = elList.CopyToAnyDataTable();
                DataTable dt3 = simPOSList.CopyToAnyDataTable();
                using (ExcelPackage pck = new ExcelPackage())
                {
                    ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Retailer Info");
                    ExcelWorksheet ws2 = pck.Workbook.Worksheets.Add("EL MSISDN");
                    ExcelWorksheet ws3 = pck.Workbook.Worksheets.Add("Sim POS Info");

                    ws.Cells["A1"].LoadFromDataTable(dt1, true);
                    ws2.Cells["A1"].LoadFromDataTable(dt2, true);
                    ws3.Cells["A1"].LoadFromDataTable(dt3, true);

                    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    Response.AddHeader("content-disposition", "attachment;  filename=RetailerList.xlsx");
                    Response.BinaryWrite(pck.GetAsByteArray());
                    Response.Flush();
                    Response.End();
                }
                return View("Index");
            }


        }

        [HttpPost]
        public JsonResult SearchResult(FormCollection data)
        {
            String sDate = data["startdate"]; String eDate = data["enddate"];

            DateTime StartDate = DateTime.Now.Date; DateTime EndDate = DateTime.Now.Date;
            if (sDate != "")
            {
                Boolean isStartDateValid = DateTime.TryParseExact(sDate, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out StartDate);
                if (!isStartDateValid)
                {
                    return Json(new { status = "error", details = "Start date is invalid. It must be in dd/MM/yyyy format." }, JsonRequestBehavior.AllowGet);
                }

                
            }
            if (eDate != "")
            {
                Boolean isEndDateValid = DateTime.TryParseExact(eDate, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out EndDate);
                if (!isEndDateValid)
                {
                    return Json(new { status = "error", details = "End date is invalid. It must be in dd/MM/yyyy format." }, JsonRequestBehavior.AllowGet);
                }
            }
            TimeSpan ts = new TimeSpan(23, 59, 59);
            EndDate = EndDate.Add(ts);
            int areId = (data["area"] == string.Empty) ? 0 : Convert.ToInt32(data["area"]) ;
            int rspId = (data["rsp"] == string.Empty) ? 0 : Convert.ToInt32(data["rsp"]); 
            int srId = (data["sr"] == string.Empty) ? 0 : Convert.ToInt32(data["sr"]);
            using (DataAccess.RobiPosMappingEntities db = new DataAccess.RobiPosMappingEntities())
            {
                var retailers = from a in db.Retailers where ((a.IsActive == true) || ((a.IsActive==false) && (a.RetailerStatusId==2))) select a;
                if (areId > 0)
                {
                    retailers = from a in retailers where a.AreaId == areId select a;
                }
                if (rspId > 0)
                {
                    retailers = from a in retailers where a.RspId == rspId select a;
                }
                if (srId > 0)
                {
                    retailers = from a in retailers where a.DsrId == srId select a;
                }
                if (StartDate <= EndDate)
                {
                    retailers = from a in retailers where a.DsrActivityDateTime!=null && a.DsrActivityDateTime >= StartDate && a.DsrActivityDateTime <= EndDate select a;
                }
                var retailerList = (from a in retailers
                                    select new
                                        {
                                            ID = a.RetailerId,
                                            Name = a.RetailerName,
                                            AreaId=a.Area.AreaId,
                                            AreaName=a.Area.AreaName,
                                            RSPID=a.RspId,
                                            RSPName=a.RSP.RspName,
                                            DsrId=a.DsrId,
                                            DsrName=a.Person.PersonName,
                                            RetailerStatusId=a.RetailerStatusId,
                                            Active=a.IsActive,
                                            Lat=a.Latitude,
                                            Lon=a.Longitude,
                                            DSRMSISDN=a.Person.PersonMsisdn,
                                            Photo=a.DefaultPhotoName,



                                            Region = a.Region.RegionName,
                                            Area = a.Area.AreaName,
                                            Thana = a.Thana.ThanaName,
                                            Ward = a.Ward.WardName,
                                            Mauza = a.Mauza.MauzaName,
                                            Village =a.Village.VillageName,
                                            RetailerName = a.RetailerName,
                                            ThanaId = a.ThanaId,
                                            WardId = a.WardId,
                                            MauzaId = a.MauzaId,
                                            VillageId = a.VillageId,
                                            POSStatus = a.RetailerStatusId,
                                            Verified = a.IsVerifiedByDsrs,
                                            Address = a.Address,
                                            VisitDayId = a.VisitDayId,
                                            VisitDay = a.VisitDay.VisitDays,
                                            VisitDayCode = a.VisitDay.VisitDayCode,
                                            PosStructure = a.PosStructure.PosStructureName,
                                            PosStructureId = a.PosStructureId,
                                            PosImage = a.DefaultPhotoName,
                                            ShopSignageId = a.ShopSignageId,
                                            ShopSignage = a.ShopSignage.ShopSignageName,
                                            ShopTypeId = a.ShopTypeId,
                                            ShopType = a.ShopType.ShopTypeName,
                                            Dsr = a.Person.PersonName,
                                            DsrMsisdn = a.Person.PersonMsisdn,
                                            a.QrCodeId,
                                            a.IsElPos,
                                            a.IsSimPos,
                                            a.IsScPos,
                                            a.IsApartments,
                                            a.IsSlums,
                                            a.IsSemiUrbunHousing,
                                            a.IsRuralHousing,
                                            a.IsShoppingMall,
                                            a.IsRetailHub,
                                            a.IsMobileDeviceMarket,
                                            a.IsBazaar,
                                            a.IsOfficeArea,
                                            a.IsGarmentsMajorityArea,
                                            a.IsGeneralIndustrialArea,
                                            a.IsUrbanTransitPoints,
                                            a.IsRuralTransitPoints,
                                            a.IsUrbanYouthHangouts,
                                            a.IsSemiUrbanYouthHangouts,
                                            a.IsRuralYouthHangouts
                                        }).ToList();
                return Json(new {status="success",data=retailerList});
            }
        }



        //Region Area Rsp wise group quantity. Comes from dashboad.
        [HttpPost]
        public JsonResult RspGroupResult(FormCollection data)
        {
            try
            {
                //--> Variables
                Int32 moId = 0;
                if (data.AllKeys.Contains("MoId"))
                {
                    moId = Convert.ToInt32(data["MoId"]);
                }
                String strRegionId = data["RegionId"].ToString();
                String strAreaId = data["AreaId"].ToString();
                String strRspId = data["RspId"].ToString();
                Boolean IsAllDate = Convert.ToBoolean(data["IsAllDate"]);
                String strFromDate = data["FromDate"].ToString();
                String strToDate = data["ToDate"].ToString();

                String sqlSelectRegionAreaRsp = String.Empty; String sqlRspWhereClause = String.Empty; String sqlSelectRetailers = String.Empty; String sqlRetailersWhereClause = String.Empty;
                DateTime FromDate=new DateTime(); DateTime ToDate=new DateTime();
                DataTable tblResult = new DataTable();
                //<-- Variables

                //RegionId must be select. So dont check for RegionId
                sqlRspWhereClause = " Where Region.RegionId=" + Convert.ToInt32(strRegionId) + " ";
                sqlRetailersWhereClause = " Where RegionId=" + Convert.ToInt32(strRegionId) + " ";

                if (!strAreaId.Equals("-1"))
                {
                    Int32 intAreaId = Convert.ToInt32(strAreaId);
                    sqlRspWhereClause += " AND Area.AreaId=" + intAreaId + "";
                    sqlRetailersWhereClause += " AND AreaId=" + intAreaId + "";

                    //RspId comes in scene if only areaid is there.
                    if (!strRspId.Equals("-1"))
                    {
                        Int32 intRspId = Convert.ToInt32(strRspId);
                        sqlRspWhereClause += " AND RSP.RspId=" + intRspId + "";
                        sqlRetailersWhereClause += " AND RspId=" + intRspId + "";

                    }
                }


                sqlSelectRegionAreaRsp = @"SELECT Region.RegionName, Area.AreaName, RSP.RspId, RSP.RspName
                                       FROM Area INNER JOIN RSP ON dbo.Area.AreaId = dbo.RSP.AreaId INNER JOIN Region ON dbo.Area.RegionId = dbo.Region.RegionId " + sqlRspWhereClause + @"
                                       ORDER BY dbo.Region.RegionName, dbo.Area.AreaName, dbo.RSP.RspName";

                sqlSelectRetailers = @"SELECT RetailerId,RetailerStatusId, IsActive, RspId, Cast(SurveyorActivityDateTime as DATE) AS SurveyorDate FROM Retailer " + sqlRetailersWhereClause;

                //Datatable to store search result and send back to user.
                tblResult.Columns.Add("RegionName", typeof(System.String));
                tblResult.Columns.Add("AreaName", typeof(System.String));
                tblResult.Columns.Add("RspId", typeof(System.Int32));
                tblResult.Columns.Add("RspName", typeof(System.String));
                tblResult.Columns.Add("TotalRetailers", typeof(System.Int32));
                tblResult.Columns.Add("Updated", typeof(System.Int32));
                tblResult.Columns.Add("NotFound", typeof(System.Int32));
                tblResult.Columns.Add("New", typeof(System.Int32));
                tblResult.Columns.Add("Pending", typeof(System.Int32)); //this pending is as per search
                tblResult.Columns.Add("OverallPending", typeof(System.Int32)); //this pending is for overall


                DataTable tblRsp=new DataTable(); //contains all Region-Area-Rsp
                DataTable tblRetailers = new DataTable(); //to hold all retailers from database. 

                //--> Database
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    using (SqlCommand command=new SqlCommand())
                    {
                        command.Connection = connection;
                        using (SqlDataAdapter da=new SqlDataAdapter(command))
                        {
                            connection.Open();
                            command.CommandText = sqlSelectRegionAreaRsp; da.Fill(tblRsp);
                            command.CommandText = sqlSelectRetailers; da.Fill(tblRetailers);
                            connection.Close();
                        }

                        //Insert MO Log
                        command.CommandText = sqlInsertMoLog;
                        command.Parameters.Clear();
                        command.Parameters.Add("@MoId", SqlDbType.Int).Value = moId;
                        command.Parameters.Add("@LogDescription", SqlDbType.VarChar).Value = "Clicked on Dashboard Grouped Status Search Button.";
                        connection.Open(); command.ExecuteNonQuery(); connection.Close();
                    }
                }
                //<-- Database

                if (!IsAllDate)
                {
                    Boolean isFromDateValid = DateTime.TryParseExact(strFromDate, InputDateFormatsAllowed, ci, DateTimeStyles.None, out FromDate);
                    Boolean isToDateValid = DateTime.TryParseExact(strToDate, InputDateFormatsAllowed, ci, DateTimeStyles.None, out ToDate);
                }

                //---> read every RSP
                foreach (DataRow rsp in tblRsp.Rows)
                {
                    Int32 rspId = Convert.ToInt32(rsp["RspId"]);
                    String rspName = rsp["RspName"].ToString();
                    String regionName = rsp["RegionName"].ToString();
                    String areaName = rsp["AreaName"].ToString();


                    DataRow row = tblResult.NewRow();
                    row["RspId"] = rspId; row["RspName"] = rspName; row["RegionName"] = regionName;  row["AreaName"] = areaName;

                    Int32 totalRetailersQuantity = Convert.ToInt32(tblRetailers.Compute("Count(RetailerId)", "RspId=" + rspId + " AND RetailerStatusId<>3"));

                    Int32 updatedQuantity = 0; Int32 notFoundQuantity = 0; Int32 newQuantity = 0; Int32 overallPendingQuantity = 0;

                    //Check Date
                    if (!IsAllDate)
                    {
                        var updatedRetailers = (from DataRow r in tblRetailers.Rows
                                       where r.Field<int>("RspId") == rspId && r.Field<int>("RetailerStatusId") == 2 && r.Field<DateTime>("SurveyorDate") >= FromDate 
                                       && r.Field<DateTime>("SurveyorDate") <= ToDate
                                       select r.Field<int>("RetailerId")).ToList();

                        updatedQuantity = updatedRetailers.Count();

                        var newRetailers = (from DataRow r in tblRetailers.Rows
                                                where r.Field<int>("RspId") == rspId && r.Field<int>("RetailerStatusId") == 3 && r.Field<DateTime>("SurveyorDate") >= FromDate
                                                && r.Field<DateTime>("SurveyorDate") <= ToDate
                                                select r.Field<int>("RetailerId")).ToList();
                        newQuantity = newRetailers.Count();
                        var notFoundRetailers = (from DataRow r in tblRetailers.Rows
                                            where r.Field<int>("RspId") == rspId && r.Field<Boolean>("IsActive") ==false && r.Field<DateTime>("SurveyorDate") >= FromDate
                                            && r.Field<DateTime>("SurveyorDate") <= ToDate
                                            select r.Field<int>("RetailerId")).ToList();
                        notFoundQuantity = notFoundRetailers.Count();
                    }
                    else
                    {
                        updatedQuantity = Convert.ToInt32(tblRetailers.Compute("Count(RetailerId)", "RspId=" + rspId + " AND RetailerStatusId=2 AND SurveyorDate is not null"));
                        notFoundQuantity = Convert.ToInt32(tblRetailers.Compute("Count(RetailerId)", "RspId=" + rspId + " AND IsActive=false AND SurveyorDate is not null"));
                        newQuantity = Convert.ToInt32(tblRetailers.Compute("Count(RetailerId)", "RspId=" + rspId + " AND RetailerStatusId=3 AND SurveyorDate is not null"));
                    }

                    overallPendingQuantity = Convert.ToInt32(tblRetailers.Compute("Count(RetailerId)", "RspId=" + rspId + " AND SurveyorDate is null AND IsActive=1"));

                    //Int32 workDone = Convert.ToInt32(dtRetailers.Compute("Count(RetailerId)", "RspId=" + rspId + " and SurveyorDate is not null"));

                    row["TotalRetailers"] = totalRetailersQuantity;
                    row["Updated"] = updatedQuantity;
                    row["NotFound"] = notFoundQuantity;
                    row["New"] = newQuantity;
                    row["Pending"] = totalRetailersQuantity - (updatedQuantity + notFoundQuantity);  //this pending is as per search
                    row["OverallPending"] = overallPendingQuantity;
                    tblResult.Rows.Add(row);
                }
                //<--- read every RSP

                String SerializedDatatable = JsonConvert.SerializeObject(tblResult, Formatting.None);
                return Json(new { result = "Success", DataTable = SerializedDatatable },
                                JsonRequestBehavior.AllowGet);
            }
            catch ( Exception ex)
            {
                return Json(new { result = "Error" }, JsonRequestBehavior.AllowGet);
            }


        }

        public ActionResult BrProfile()
        {
            try
            {
                Int32 MoId = Convert.ToInt32(Request.QueryString["moid"]); //Monitoring Officer Id
                //--> Variables
                DateTime today = DateTime.Today.Date;

                DataTable dtRspList = new DataTable(); DataTable dtRetailerList = new DataTable(); DataTable tblOverlappingRetailers = new DataTable();

                Int32 totalRetailersQuantity = 0; // 194305;
                Object objAllNewRetailersCount; Object objAllUpdatedRetailersCount; Object objAllNotFoundRetailersCount; Int32 AllPendingRetailersQuantity; Object objAllVerifiedCount;
                Object objTodaysUpdatedRetailersCount; Object objTodaysNewRetailersCount; Object objTodaysNotFoundRetailersCount; Object objTodaysVerifiedCount;
                DataTable dtRegions = new DataTable();
                DataTable tblDupli = new DataTable();

                DataTable tblSurveyorsQuantityPerDate = new DataTable(); //date-wise koto gulo Surveyor field-e chhilo tar data
                DataTable tblRetailersQuantityPerDate = new DataTable(); //protidin koto gulo retailer-e kaj hoechhe tar data
                DataTable tblVerifiedQuantyPerDate = new DataTable(); //date-wise koto gulo verified holo tar data. Shown on line graph.
                DataTable tblSurveyorRetailerOfToday = new DataTable(); //Ajke kon Surveyor kotota kaj korlo tar data
                DataTable tblRegionStatusBar = new DataTable(); //Region-wise Total & Updated bar chart
                //<-- Variables


                //---> SQL Statements
                //                String sqlTotalOutletsOfThisMo = @"SELECT COUNT(Retailer.RetailerId) AS RetailerCount
                //                                              FROM Region INNER JOIN
                //                                                 Retailer ON Region.RegionId = Retailer.RegionId INNER JOIN
                //                                                 MonitoringOfficerRegion AS MOR ON Region.RegionId = MOR.RegionId
                //                                              WHERE (MOR.MoId = "+ MoId +")";

                String sqlTotalOutletsOfThisMo = @"SELECT COUNT(Retailer.RetailerId) AS RetailerCount
                                              FROM Region INNER JOIN
                                                 Retailer ON Region.RegionId = Retailer.RegionId INNER JOIN
                                                 MonitoringOfficerRegion AS MOR ON Region.RegionId = MOR.RegionId
                                              WHERE (MOR.MoId = " + MoId + ") AND (Retailer.RetailerStatusId<3)";


                String sqlCountAllUpdatedRetailers = @"SELECT COUNT(Retailer.RetailerId) AS RetailerCount
                                              FROM Region INNER JOIN
                                                 Retailer ON Region.RegionId = Retailer.RegionId INNER JOIN
                                                 MonitoringOfficerRegion AS MOR ON Region.RegionId = MOR.RegionId
                                   WHERE (CAST(Retailer.SurveyorActivityDateTime AS DATE) > '2015-04-01') AND (Retailer.RetailerStatusId = 2) AND (Retailer.IsActive = 1) AND (MOR.MoId = " + MoId + ")";



                String sqlCountAllNewRetailers = @"SELECT COUNT(Retailer.RetailerId) AS RetailerCount
                                           FROM Region INNER JOIN
                                                 Retailer ON Region.RegionId = Retailer.RegionId INNER JOIN
                                                 MonitoringOfficerRegion AS MOR ON Region.RegionId = MOR.RegionId
                                           WHERE (CAST(Retailer.SurveyorActivityDateTime AS DATE) > '2015-04-01') AND (Retailer.RetailerStatusId=3) AND (Retailer.IsActive=1) AND (MOR.MoId=" + MoId + ")";


                String sqlCountAllNotFoundRetailers = @"SELECT COUNT(Retailer.RetailerId) AS RetailerCount
                                                FROM Region INNER JOIN
                                                  Retailer ON Region.RegionId = Retailer.RegionId INNER JOIN
                                                  MonitoringOfficerRegion AS MOR ON Region.RegionId = MOR.RegionId
                                                 Where (Cast(Retailer.InactiveDateTime as DATE)>'2015-04-01') and (Retailer.IsActive=0) AND (MOR.MoId=" + MoId + ")";

                string sqlCountAllVerifiedRetailers = @"SELECT COUNT(dbo.Retailer.RetailerId) AS VerifiedQuantity
                                                       FROM dbo.Retailer INNER JOIN
                                                            dbo.MonitoringOfficerRegion ON dbo.Retailer.RegionId = dbo.MonitoringOfficerRegion.RegionId
                                                       WHERE (dbo.Retailer.IsVerifiedByRsp = 1) AND (dbo.MonitoringOfficerRegion.MoId = " + MoId + ")";

                String sqlCountTodaysUpdatedRetailers = @"SELECT COUNT(Retailer.RetailerId) AS RetailerCount
                                                FROM Region INNER JOIN
                                                  Retailer ON Region.RegionId = Retailer.RegionId INNER JOIN
                                                  MonitoringOfficerRegion AS MOR ON Region.RegionId = MOR.RegionId
                                               Where Cast(Retailer.SurveyorActivityDateTime as DATE)='" + today.ToString("yyyy-MM-dd") + "' and (Retailer.RetailerStatusId=2) And (Retailer.IsActive=1) AND (MOR.MoId=" + MoId + ")";

                String sqlCountTodaysNewRetailers = @"SELECT COUNT(Retailer.RetailerId) AS RetailerCount
                                              FROM Region INNER JOIN
                                                  Retailer ON Region.RegionId = Retailer.RegionId INNER JOIN
                                                  MonitoringOfficerRegion AS MOR ON Region.RegionId = MOR.RegionId
                                              Where (Cast(Retailer.SurveyorActivityDateTime as DATE)='" + today.ToString("yyyy-MM-dd") + "') and (Retailer.RetailerStatusId=3) AND (Retailer.IsActive=1)  AND (MOR.MoId=" + MoId + ")";


                String sqlCountTodaysNotFoundRetailers = @"SELECT COUNT(Retailer.RetailerId) AS RetailerCount
                                              FROM Region INNER JOIN
                                                  Retailer ON Region.RegionId = Retailer.RegionId INNER JOIN
                                                  MonitoringOfficerRegion AS MOR ON Region.RegionId = MOR.RegionId
                                                 Where (Cast(Retailer.InactiveDateTime as DATE)='" + today.ToString("yyyy-MM-dd") + "') AND (Retailer.IsActive=0) AND (MOR.MoId=" + MoId + ")";

                //Use SurveyorActivityDateTime instead of RspVerificationDateTime. Otherwise Quantity mismatched with some other places.
                string sqlCountTodaysVerifiedRetailers = @"SELECT COUNT(R.RetailerId) AS VerifiedQuantity
                                                           FROM dbo.Retailer AS R INNER JOIN
                                                                dbo.MonitoringOfficerRegion AS M ON R.RegionId = M.RegionId
                                                           WHERE (R.IsVerifiedByRsp = 1) AND (M.MoId = " + MoId + ") AND (CAST(R.SurveyorActivityDateTime AS Date) = '" + DateTime.Today.Date.ToString("yyyy-MM-dd") + "')";

                String sqlSelectRegion = @"SELECT Region.RegionId, dbo.Region.RegionName
                                   FROM dbo.Region INNER JOIN
                                         dbo.MonitoringOfficerRegion ON dbo.Region.RegionId = dbo.MonitoringOfficerRegion.RegionId
                                   WHERE (dbo.MonitoringOfficerRegion.MoId = " + MoId + ") ORDER BY dbo.Region.RegionName";

                String sqlSelectRsp = @"SELECT Region.RegionName, Area.AreaName, RSP.RspId, RSP.RspName
                                FROM Area INNER JOIN
                                dbo.RSP ON dbo.Area.AreaId = dbo.RSP.AreaId INNER JOIN
                                dbo.Region ON dbo.Area.RegionId = dbo.Region.RegionId INNER JOIN
                                dbo.MonitoringOfficerRegion ON dbo.Region.RegionId = dbo.MonitoringOfficerRegion.RegionId
                                WHERE (dbo.MonitoringOfficerRegion.MoId = " + MoId + @")
                                ORDER BY dbo.Region.RegionName, dbo.Area.AreaName, dbo.RSP.RspName";

                String sqlSelectRetailers = @"SELECT Retailer.RetailerId, Retailer.RetailerStatusId, Retailer.IsActive, dbo.Retailer.RspId, dbo.Retailer.SurveyorId,      
                                           Cast(SurveyorActivityDateTime as DATE) as SurveyorDate
                                     FROM dbo.Retailer INNER JOIN
                                       dbo.Region ON dbo.Retailer.RegionId = dbo.Region.RegionId INNER JOIN
                                       dbo.MonitoringOfficerRegion ON dbo.Region.RegionId = dbo.MonitoringOfficerRegion.RegionId
                                     WHERE (dbo.MonitoringOfficerRegion.MoId = " + MoId + ")";


                String sqlSelectOverlappingRetailers = @"SELECT dbo.SrRetailerLog.RetailerId, COUNT(DISTINCT dbo.SrRetailerLog.SrId) AS MultipleSrQuantity
                                                         FROM dbo.Retailer INNER JOIN
                                                            dbo.SrRetailerLog ON dbo.Retailer.RetailerId = dbo.SrRetailerLog.RetailerId INNER JOIN
                                                            dbo.MonitoringOfficerRegion ON dbo.Retailer.RegionId = dbo.MonitoringOfficerRegion.RegionId
                                                         WHERE (dbo.MonitoringOfficerRegion.MoId = " + MoId + @")
                                                         GROUP BY dbo.SrRetailerLog.RetailerId
                                                         HAVING (COUNT(DISTINCT dbo.SrRetailerLog.SrId) > 1)
                                                         ORDER BY dbo.SrRetailerLog.RetailerId";



                string sqlDuplicateImages = @"SELECT     TOP (100) PERCENT Photo.GroupId, Photo.RetailerId, Photo.PhotoDateTime, R.SurveyorActivityDateTime, R.RetailerName, R.Address, R.DefaultPhotoName, dbo.RSP.RspName, S.LoginName AS SR,
                       S.SurveyorName, R.DsrId, P.PersonName, Photo.IsRemovedLaterOn, Photo.ThisIsNotDuplicate, dbo.Region.RegionName
FROM         dbo.Surveyors AS S INNER JOIN
                      dbo.Region ON S.RegionId = dbo.Region.RegionId RIGHT OUTER JOIN
                      dbo.DuplicatePhotos AS Photo INNER JOIN
                      dbo.Retailer AS R ON Photo.RetailerId = R.RetailerId INNER JOIN
                      dbo.RSP ON R.RspId = dbo.RSP.RspId INNER JOIN
                      dbo.Person AS P ON R.DsrId = P.PersonId ON S.SurveyorId = R.SurveyorId
WHERE      (Photo.ThisIsNotDuplicate = 0) AND (S.SurveyorId IS NOT NULL) AND (S.RegionId IN
                          (SELECT     RegionId
                            FROM          dbo.MonitoringOfficerRegion
                            WHERE      (MoId = " + MoId + "))) ORDER BY Photo.GroupId";


                //protidin koto gulo Surveyor field-e chhilo tar data
                string sqlSurveyorsQuantityPerDate = @"SELECT COUNT(Distinct(R.SurveyorId)) as SRQuantity,Cast(R.SurveyorActivityDateTime as Date) AS WorkDate
                                                       FROM dbo.Retailer AS R INNER JOIN
                                                            dbo.Surveyors AS S ON R.SurveyorId = S.SurveyorId INNER JOIN
                                                            dbo.MonitoringOfficerRegion AS M ON S.RegionId = M.RegionId
                                                      Where  Cast(R.SurveyorActivityDateTime as Date)>'2015-05-01' AND M.MoId=" + MoId + @"
                                                      Group By Cast(R.SurveyorActivityDateTime as Date)
                                                      Order By Cast(R.SurveyorActivityDateTime as Date) DESC";

                //date-wise koto gulo retailer-e kaj hoechhe tar data
                string sqlRetailersQuantityPerDate = @"SELECT  COUNT(R.RetailerId) as RetailerQuantity, Cast(R.SurveyorActivityDateTime as Date) AS WorkDate
                                                       FROM dbo.Retailer AS R INNER JOIN
                                                            dbo.Surveyors AS S ON R.SurveyorId = S.SurveyorId INNER JOIN
                                                            dbo.MonitoringOfficerRegion AS M ON S.RegionId = M.RegionId
                                                       Where  Cast(R.SurveyorActivityDateTime as Date)>'2015-05-01' AND M.MoId=" + MoId + @"
                                                       Group By Cast(R.SurveyorActivityDateTime as Date)
                                                       Order By Cast(R.SurveyorActivityDateTime as Date) DESC";

                //date-wise koto gulo retailer verify holo tar data. Shown on line graph.
                string sqlVerifiedQuantityPerDate = @"SELECT  COUNT(R.RetailerId) as RetailerQuantity, Cast(R.SurveyorActivityDateTime as Date) AS WorkDate
                                                       FROM dbo.Retailer AS R INNER JOIN
                                                            dbo.MonitoringOfficerRegion AS M ON R.RegionId = M.RegionId
                                                       Where  Cast(R.SurveyorActivityDateTime as Date)>'2015-05-01' AND M.MoId=" + MoId + @" AND R.IsVerifiedByRsp=1
                                                       Group By Cast(R.SurveyorActivityDateTime as Date)
                                                       Order By Cast(R.SurveyorActivityDateTime as Date) DESC";

                //Ajke Kon Surveyor kotota kaj korlo tar data
                string sqlSurveyorRetailerOfToday = @"SELECT  S.SurveyorId, COUNT(R.RetailerId) as RetailerQuantity
                                                     FROM dbo.Retailer AS R INNER JOIN
                                                          dbo.Surveyors AS S ON R.SurveyorId = S.SurveyorId INNER JOIN
                                                          dbo.MonitoringOfficerRegion AS M ON S.RegionId = M.RegionId
                                                     Where  Cast(R.SurveyorActivityDateTime as Date)='" + DateTime.Today.Date.ToString("yyyy-MM-dd") + @"' AND M.MoId=" + MoId + @"
                                                     Group By S.SurveyorId
                                                     Order By S.SurveyorId ASC";

                //Region-wise Total & Updated bar chart
                string sqlRegionStatusBar = @"SELECT     dbo.Region.RegionName, (Select Count(RetailerId) from dbo.Retailer where RegionId=dbo.Region.RegionId AND dbo.Retailer.RetailerStatusId<3)  AS                                             TotalRetailer, ((Select Count(RetailerId) from dbo.Retailer where RegionId=dbo.Region.RegionId AND dbo.Retailer.RetailerStatusId=2 AND                                                      dbo.Retailer.IsActive=1) + (Select Count(RetailerId) from dbo.Retailer where RegionId=dbo.Region.RegionId AND dbo.Retailer.IsActive=0)) AS                                                UpdatedRetailer
                                              FROM dbo.Region INNER JOIN
                                                      dbo.MonitoringOfficerRegion ON dbo.Region.RegionId = dbo.MonitoringOfficerRegion.RegionId
                                              WHERE (dbo.MonitoringOfficerRegion.MoId = " + MoId + ")";
                //<--- SQL Statements

                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = connection;
                        using (SqlDataAdapter da = new SqlDataAdapter(command))
                        {

                            connection.Open();
                            //Count all retailer quantity
                            command.CommandText = sqlTotalOutletsOfThisMo; totalRetailersQuantity = Convert.ToInt32(command.ExecuteScalar());
                            //Count all updated retailers
                            command.CommandText = sqlCountAllUpdatedRetailers; objAllUpdatedRetailersCount = command.ExecuteScalar();
                            //Count all new retailers
                            command.CommandText = sqlCountAllNewRetailers; objAllNewRetailersCount = command.ExecuteScalar();
                            //Count all not found retailers
                            command.CommandText = sqlCountAllNotFoundRetailers; objAllNotFoundRetailersCount = command.ExecuteScalar();
                            //Count all verified
                            command.CommandText = sqlCountAllVerifiedRetailers; objAllVerifiedCount = command.ExecuteScalar();
                            //Count todays updated retailes
                            command.CommandText = sqlCountTodaysUpdatedRetailers; objTodaysUpdatedRetailersCount = command.ExecuteScalar();
                            //Count todays new retailes
                            command.CommandText = sqlCountTodaysNewRetailers; objTodaysNewRetailersCount = command.ExecuteScalar();
                            //Count todays not found retailes
                            command.CommandText = sqlCountTodaysNotFoundRetailers; objTodaysNotFoundRetailersCount = command.ExecuteScalar();
                            //Count todays verified retailers
                            command.CommandText = sqlCountTodaysVerifiedRetailers; objTodaysVerifiedCount = command.ExecuteScalar();
                            // fill RSP table
                            command.CommandText = sqlSelectRsp; da.Fill(dtRspList);
                            //fill Retailer table
                            command.CommandText = sqlSelectRetailers; da.Fill(dtRetailerList);
                            //fill Region table.
                            command.CommandText = sqlSelectRegion; da.Fill(dtRegions);
                            //fill Overlapping Retailer table
                            command.CommandText = sqlSelectOverlappingRetailers; da.Fill(tblOverlappingRetailers);
                            //fill duplicate images
                            command.CommandText = sqlDuplicateImages; da.Fill(tblDupli);
                            //protidin koto gulo Surveyor field-e chhilo tar data
                            command.CommandText = sqlSurveyorsQuantityPerDate; da.Fill(tblSurveyorsQuantityPerDate);
                            //protidin koto gulo retailer-e kaj hoechhe tar data
                            command.CommandText = sqlRetailersQuantityPerDate; da.Fill(tblRetailersQuantityPerDate);
                            //date-wise kotogulo verify holo tar data. Shown on line graph.
                            command.CommandText = sqlVerifiedQuantityPerDate; da.Fill(tblVerifiedQuantyPerDate);
                            //kon surveyor kotota kaj korlo tar data
                            command.CommandText = sqlSurveyorRetailerOfToday; da.Fill(tblSurveyorRetailerOfToday);
                            //Region-wise Total & Updated bar chart
                            command.CommandText = sqlRegionStatusBar; da.Fill(tblRegionStatusBar);
                            connection.Close();

                            //Insert MO Log
                            command.CommandText = sqlInsertMoLog;
                            command.Parameters.Clear();
                            command.Parameters.Add("@MoId", SqlDbType.Int).Value = MoId;
                            command.Parameters.Add("@LogDescription", SqlDbType.VarChar).Value = "Comes on Dashboard.";
                            connection.Open(); command.ExecuteNonQuery(); connection.Close();
                        }
                    }
                }

                string overLappingInformation = string.Empty;
                if (tblOverlappingRetailers.Rows.Count > 0)
                {
                    string overLappedRetailers = tblOverlappingRetailers.Rows.Count.ToString();
                    string overLappedSr = tblOverlappingRetailers.Compute("SUM(MultipleSrQuantity)", "").ToString();
                    overLappingInformation = overLappedRetailers + " retailers overlapped by " + overLappedSr + " different surveyors.";
                }

                //--> For search panel
                ViewBag.Regions = dtRegions;
                //<-- For search panel

                //--> Forcasting
                int avgOutletsPerday = Convert.ToInt32(objAllUpdatedRetailersCount) / tblRetailersQuantityPerDate.Rows.Count;
                int requiredDays = totalRetailersQuantity - (Convert.ToInt32(objAllUpdatedRetailersCount) + Convert.ToInt32(objAllNotFoundRetailersCount));
                requiredDays = requiredDays / avgOutletsPerday;
                string forcastMessage = string.Format("{0} more days required to complete. Current average rate {1} outlets/day.", requiredDays, avgOutletsPerday.ToString("N", ci));
                //<-- Forcasting

                //--> Line graph in workflow progress
                string[] chartLabels = new string[tblRetailersQuantityPerDate.Rows.Count];
                for (int i = tblRetailersQuantityPerDate.Rows.Count - 1; i > 0; i--)
                {
                    chartLabels[i] = Convert.ToDateTime(tblRetailersQuantityPerDate.Rows[i]["WorkDate"]).ToString("dd-MMM");
                }

                chartLabels[0] = Convert.ToDateTime(tblRetailersQuantityPerDate.Rows[0]["WorkDate"]).ToString("dd-MMM");

                //--> Retailer quantity
                int[] chartData = new int[tblRetailersQuantityPerDate.Rows.Count];
                for (int i = tblRetailersQuantityPerDate.Rows.Count - 1; i > 0; i--)
                {
                    chartData[i] = Convert.ToInt32(tblRetailersQuantityPerDate.Rows[i]["RetailerQuantity"]);
                }

                chartData[0] = Convert.ToInt32(tblRetailersQuantityPerDate.Rows[0]["RetailerQuantity"]);
                //<-- retailer quantity

                //--> verified quantity
                int[] verifiedData = new int[tblVerifiedQuantyPerDate.Rows.Count];
                for (int i = tblVerifiedQuantyPerDate.Rows.Count - 1; i > 0; i--)
                {
                    verifiedData[i] = Convert.ToInt32(tblVerifiedQuantyPerDate.Rows[i]["RetailerQuantity"]); //Verified quantity
                }

                verifiedData[0] = Convert.ToInt32(tblVerifiedQuantyPerDate.Rows[0]["RetailerQuantity"]);
                //<-- verified quantity

                //<-- Line graph in workflow progress

                //--> Region Chart - Chart2
                int chart2DataQuantity = tblRegionStatusBar.Rows.Count;
                string[] chart2Regions = new string[chart2DataQuantity];
                int[] chart2TotalRetailerQuantity = new int[chart2DataQuantity];
                int[] chart2UpdatedRetailerQuantity = new int[chart2DataQuantity]; // updated + not found. Calculated in sql statement
                for (int i = 0; i < chart2DataQuantity; i++)
                {
                    string regionName = tblRegionStatusBar.Rows[i]["RegionName"].ToString();
                    int totalQuantity = Convert.ToInt32(tblRegionStatusBar.Rows[i]["TotalRetailer"]);
                    int updatedQuantity = Convert.ToInt32(tblRegionStatusBar.Rows[i]["UpdatedRetailer"]);
                    double percentage = totalQuantity != 0 ? (updatedQuantity * 100) / totalQuantity : 0;

                    chart2Regions[i] = string.Format("{0} - {1}%", regionName, percentage.ToString());
                    chart2TotalRetailerQuantity[i] = totalQuantity;
                    chart2UpdatedRetailerQuantity[i] = updatedQuantity;
                }

                //<--Region Chart - Chart2
                //-->ViewBag
                ViewBag.MoId = MoId.ToString();
                ViewBag.UserName = Request.QueryString["user"].ToString(); //LoginName

                ViewBag.TotalRetailersQuantity = totalRetailersQuantity;
                ViewBag.AllUpdatedRetailersCount = Convert.ToInt32(objAllUpdatedRetailersCount); //All updated retailers count.
                ViewBag.AllNewRetailersCount = Convert.ToInt32(objAllNewRetailersCount); //All new retailers count.
                ViewBag.AllNotFoundRetailersCount = Convert.ToInt32(objAllNotFoundRetailersCount); // All not found retailers count.
                ViewBag.AllPendingRetailersCount = totalRetailersQuantity - (Convert.ToInt32(objAllUpdatedRetailersCount) + Convert.ToInt32(objAllNotFoundRetailersCount));
                ViewBag.AllVerifiedRetailersCount = Convert.ToInt32(objAllVerifiedCount); //all verified
                ViewBag.TodaysUpdatedRetailersCount = Convert.ToInt32(objTodaysUpdatedRetailersCount); //Todays updated retailers count.
                ViewBag.TodaysNewRetailersCount = Convert.ToInt32(objTodaysNewRetailersCount); // Todays new retailer count.
                ViewBag.TodaysNotFoundRetailersCount = Convert.ToInt32(objTodaysNotFoundRetailersCount); // Todays not found retailser count.
                ViewBag.TodaysVerifiedCount = Convert.ToInt32(objTodaysVerifiedCount);

                ViewBag.OverLappingInformation = overLappingInformation;
                ViewBag.MyCulture = ci;
                ViewBag.ActiveMenuItem = "Dashboard";
                ViewBag.DuplicateImagesCount = tblDupli.Rows.Count;
                ViewBag.SurveyorsQuantityPerDate = tblSurveyorsQuantityPerDate;
                ViewBag.RetailersQuantityPerDate = tblRetailersQuantityPerDate;
                ViewBag.SurveyorRetailerOfToday = tblSurveyorRetailerOfToday;
                ViewBag.ChartLabel = chartLabels;
                ViewBag.ChartData = chartData;
                ViewBag.VerifiedData = verifiedData;
                ViewBag.ChartTwoRegion = chart2Regions;
                ViewBag.ChartTwoTotalRetailer = chart2TotalRetailerQuantity;
                ViewBag.ChartTwoUpdateRetailer = chart2UpdatedRetailerQuantity;
                ViewBag.ForcastMessage = forcastMessage;
                //<-- ViewBag

                return View();
            }
            catch (Exception ex)
            {
                var path = HttpContext.Server.MapPath("~/App_Data");
                ExceptionLogger.CreateLog(ex, path);
                return View("ErrorContent");
            }


            return View();
        }

        public ActionResult CallCenterReport()
        {
            try
            {
                Int32 MoId = Convert.ToInt32(Request.QueryString["moid"]); //Monitoring Officer Id
                //--> Variables
                DateTime today = DateTime.Today.Date;

                DataTable dtRspList = new DataTable(); DataTable dtRetailerList = new DataTable(); DataTable tblOverlappingRetailers = new DataTable();

                Int32 totalRetailersQuantity = 0; // 194305;
                Object objAllNewRetailersCount; Object objAllUpdatedRetailersCount; Object objAllNotFoundRetailersCount; Int32 AllPendingRetailersQuantity; Object objAllVerifiedCount;
                Object objTodaysUpdatedRetailersCount; Object objTodaysNewRetailersCount; Object objTodaysNotFoundRetailersCount; Object objTodaysVerifiedCount;
                DataTable dtRegions = new DataTable();
                DataTable tblDupli = new DataTable();

                DataTable tblSurveyorsQuantityPerDate = new DataTable(); //date-wise koto gulo Surveyor field-e chhilo tar data
                DataTable tblRetailersQuantityPerDate = new DataTable(); //protidin koto gulo retailer-e kaj hoechhe tar data
                DataTable tblVerifiedQuantyPerDate = new DataTable(); //date-wise koto gulo verified holo tar data. Shown on line graph.
                DataTable tblSurveyorRetailerOfToday = new DataTable(); //Ajke kon Surveyor kotota kaj korlo tar data
                DataTable tblRegionStatusBar = new DataTable(); //Region-wise Total & Updated bar chart
                //<-- Variables


                //---> SQL Statements
                //                String sqlTotalOutletsOfThisMo = @"SELECT COUNT(Retailer.RetailerId) AS RetailerCount
                //                                              FROM Region INNER JOIN
                //                                                 Retailer ON Region.RegionId = Retailer.RegionId INNER JOIN
                //                                                 MonitoringOfficerRegion AS MOR ON Region.RegionId = MOR.RegionId
                //                                              WHERE (MOR.MoId = "+ MoId +")";

                String sqlTotalOutletsOfThisMo = @"SELECT COUNT(Retailer.RetailerId) AS RetailerCount
                                              FROM Region INNER JOIN
                                                 Retailer ON Region.RegionId = Retailer.RegionId INNER JOIN
                                                 MonitoringOfficerRegion AS MOR ON Region.RegionId = MOR.RegionId
                                              WHERE (MOR.MoId = " + MoId + ") AND (Retailer.RetailerStatusId<3)";


                String sqlCountAllUpdatedRetailers = @"SELECT COUNT(Retailer.RetailerId) AS RetailerCount
                                              FROM Region INNER JOIN
                                                 Retailer ON Region.RegionId = Retailer.RegionId INNER JOIN
                                                 MonitoringOfficerRegion AS MOR ON Region.RegionId = MOR.RegionId
                                   WHERE (CAST(Retailer.SurveyorActivityDateTime AS DATE) > '2015-04-01') AND (Retailer.RetailerStatusId = 2) AND (Retailer.IsActive = 1) AND (MOR.MoId = " + MoId + ")";



                String sqlCountAllNewRetailers = @"SELECT COUNT(Retailer.RetailerId) AS RetailerCount
                                           FROM Region INNER JOIN
                                                 Retailer ON Region.RegionId = Retailer.RegionId INNER JOIN
                                                 MonitoringOfficerRegion AS MOR ON Region.RegionId = MOR.RegionId
                                           WHERE (CAST(Retailer.SurveyorActivityDateTime AS DATE) > '2015-04-01') AND (Retailer.RetailerStatusId=3) AND (Retailer.IsActive=1) AND (MOR.MoId=" + MoId + ")";


                String sqlCountAllNotFoundRetailers = @"SELECT COUNT(Retailer.RetailerId) AS RetailerCount
                                                FROM Region INNER JOIN
                                                  Retailer ON Region.RegionId = Retailer.RegionId INNER JOIN
                                                  MonitoringOfficerRegion AS MOR ON Region.RegionId = MOR.RegionId
                                                 Where (Cast(Retailer.InactiveDateTime as DATE)>'2015-04-01') and (Retailer.IsActive=0) AND (MOR.MoId=" + MoId + ")";

                string sqlCountAllVerifiedRetailers = @"SELECT COUNT(dbo.Retailer.RetailerId) AS VerifiedQuantity
                                                       FROM dbo.Retailer INNER JOIN
                                                            dbo.MonitoringOfficerRegion ON dbo.Retailer.RegionId = dbo.MonitoringOfficerRegion.RegionId
                                                       WHERE (dbo.Retailer.IsVerifiedByRsp = 1) AND (dbo.MonitoringOfficerRegion.MoId = " + MoId + ")";

                String sqlCountTodaysUpdatedRetailers = @"SELECT COUNT(Retailer.RetailerId) AS RetailerCount
                                                FROM Region INNER JOIN
                                                  Retailer ON Region.RegionId = Retailer.RegionId INNER JOIN
                                                  MonitoringOfficerRegion AS MOR ON Region.RegionId = MOR.RegionId
                                               Where Cast(Retailer.SurveyorActivityDateTime as DATE)='" + today.ToString("yyyy-MM-dd") + "' and (Retailer.RetailerStatusId=2) And (Retailer.IsActive=1) AND (MOR.MoId=" + MoId + ")";

                String sqlCountTodaysNewRetailers = @"SELECT COUNT(Retailer.RetailerId) AS RetailerCount
                                              FROM Region INNER JOIN
                                                  Retailer ON Region.RegionId = Retailer.RegionId INNER JOIN
                                                  MonitoringOfficerRegion AS MOR ON Region.RegionId = MOR.RegionId
                                              Where (Cast(Retailer.SurveyorActivityDateTime as DATE)='" + today.ToString("yyyy-MM-dd") + "') and (Retailer.RetailerStatusId=3) AND (Retailer.IsActive=1)  AND (MOR.MoId=" + MoId + ")";


                String sqlCountTodaysNotFoundRetailers = @"SELECT COUNT(Retailer.RetailerId) AS RetailerCount
                                              FROM Region INNER JOIN
                                                  Retailer ON Region.RegionId = Retailer.RegionId INNER JOIN
                                                  MonitoringOfficerRegion AS MOR ON Region.RegionId = MOR.RegionId
                                                 Where (Cast(Retailer.InactiveDateTime as DATE)='" + today.ToString("yyyy-MM-dd") + "') AND (Retailer.IsActive=0) AND (MOR.MoId=" + MoId + ")";

                //Use SurveyorActivityDateTime instead of RspVerificationDateTime. Otherwise Quantity mismatched with some other places.
                string sqlCountTodaysVerifiedRetailers = @"SELECT COUNT(R.RetailerId) AS VerifiedQuantity
                                                           FROM dbo.Retailer AS R INNER JOIN
                                                                dbo.MonitoringOfficerRegion AS M ON R.RegionId = M.RegionId
                                                           WHERE (R.IsVerifiedByRsp = 1) AND (M.MoId = " + MoId + ") AND (CAST(R.SurveyorActivityDateTime AS Date) = '" + DateTime.Today.Date.ToString("yyyy-MM-dd") + "')";

                String sqlSelectRegion = @"SELECT Region.RegionId, dbo.Region.RegionName
                                   FROM dbo.Region INNER JOIN
                                         dbo.MonitoringOfficerRegion ON dbo.Region.RegionId = dbo.MonitoringOfficerRegion.RegionId
                                   WHERE (dbo.MonitoringOfficerRegion.MoId = " + MoId + ") ORDER BY dbo.Region.RegionName";

                String sqlSelectRsp = @"SELECT Region.RegionName, Area.AreaName, RSP.RspId, RSP.RspName
                                FROM Area INNER JOIN
                                dbo.RSP ON dbo.Area.AreaId = dbo.RSP.AreaId INNER JOIN
                                dbo.Region ON dbo.Area.RegionId = dbo.Region.RegionId INNER JOIN
                                dbo.MonitoringOfficerRegion ON dbo.Region.RegionId = dbo.MonitoringOfficerRegion.RegionId
                                WHERE (dbo.MonitoringOfficerRegion.MoId = " + MoId + @")
                                ORDER BY dbo.Region.RegionName, dbo.Area.AreaName, dbo.RSP.RspName";

                String sqlSelectRetailers = @"SELECT Retailer.RetailerId, Retailer.RetailerStatusId, Retailer.IsActive, dbo.Retailer.RspId, dbo.Retailer.SurveyorId,      
                                           Cast(SurveyorActivityDateTime as DATE) as SurveyorDate
                                     FROM dbo.Retailer INNER JOIN
                                       dbo.Region ON dbo.Retailer.RegionId = dbo.Region.RegionId INNER JOIN
                                       dbo.MonitoringOfficerRegion ON dbo.Region.RegionId = dbo.MonitoringOfficerRegion.RegionId
                                     WHERE (dbo.MonitoringOfficerRegion.MoId = " + MoId + ")";


                String sqlSelectOverlappingRetailers = @"SELECT dbo.SrRetailerLog.RetailerId, COUNT(DISTINCT dbo.SrRetailerLog.SrId) AS MultipleSrQuantity
                                                         FROM dbo.Retailer INNER JOIN
                                                            dbo.SrRetailerLog ON dbo.Retailer.RetailerId = dbo.SrRetailerLog.RetailerId INNER JOIN
                                                            dbo.MonitoringOfficerRegion ON dbo.Retailer.RegionId = dbo.MonitoringOfficerRegion.RegionId
                                                         WHERE (dbo.MonitoringOfficerRegion.MoId = " + MoId + @")
                                                         GROUP BY dbo.SrRetailerLog.RetailerId
                                                         HAVING (COUNT(DISTINCT dbo.SrRetailerLog.SrId) > 1)
                                                         ORDER BY dbo.SrRetailerLog.RetailerId";



                string sqlDuplicateImages = @"SELECT     TOP (100) PERCENT Photo.GroupId, Photo.RetailerId, Photo.PhotoDateTime, R.SurveyorActivityDateTime, R.RetailerName, R.Address, R.DefaultPhotoName, dbo.RSP.RspName, S.LoginName AS SR,
                       S.SurveyorName, R.DsrId, P.PersonName, Photo.IsRemovedLaterOn, Photo.ThisIsNotDuplicate, dbo.Region.RegionName
FROM         dbo.Surveyors AS S INNER JOIN
                      dbo.Region ON S.RegionId = dbo.Region.RegionId RIGHT OUTER JOIN
                      dbo.DuplicatePhotos AS Photo INNER JOIN
                      dbo.Retailer AS R ON Photo.RetailerId = R.RetailerId INNER JOIN
                      dbo.RSP ON R.RspId = dbo.RSP.RspId INNER JOIN
                      dbo.Person AS P ON R.DsrId = P.PersonId ON S.SurveyorId = R.SurveyorId
WHERE      (Photo.ThisIsNotDuplicate = 0) AND (S.SurveyorId IS NOT NULL) AND (S.RegionId IN
                          (SELECT     RegionId
                            FROM          dbo.MonitoringOfficerRegion
                            WHERE      (MoId = " + MoId + "))) ORDER BY Photo.GroupId";


                //protidin koto gulo Surveyor field-e chhilo tar data
                string sqlSurveyorsQuantityPerDate = @"SELECT COUNT(Distinct(R.SurveyorId)) as SRQuantity,Cast(R.SurveyorActivityDateTime as Date) AS WorkDate
                                                       FROM dbo.Retailer AS R INNER JOIN
                                                            dbo.Surveyors AS S ON R.SurveyorId = S.SurveyorId INNER JOIN
                                                            dbo.MonitoringOfficerRegion AS M ON S.RegionId = M.RegionId
                                                      Where  Cast(R.SurveyorActivityDateTime as Date)>'2015-05-01' AND M.MoId=" + MoId + @"
                                                      Group By Cast(R.SurveyorActivityDateTime as Date)
                                                      Order By Cast(R.SurveyorActivityDateTime as Date) DESC";

                //date-wise koto gulo retailer-e kaj hoechhe tar data
                string sqlRetailersQuantityPerDate = @"SELECT  COUNT(R.RetailerId) as RetailerQuantity, Cast(R.SurveyorActivityDateTime as Date) AS WorkDate
                                                       FROM dbo.Retailer AS R INNER JOIN
                                                            dbo.Surveyors AS S ON R.SurveyorId = S.SurveyorId INNER JOIN
                                                            dbo.MonitoringOfficerRegion AS M ON S.RegionId = M.RegionId
                                                       Where  Cast(R.SurveyorActivityDateTime as Date)>'2015-05-01' AND M.MoId=" + MoId + @"
                                                       Group By Cast(R.SurveyorActivityDateTime as Date)
                                                       Order By Cast(R.SurveyorActivityDateTime as Date) DESC";

                //date-wise koto gulo retailer verify holo tar data. Shown on line graph.
                string sqlVerifiedQuantityPerDate = @"SELECT  COUNT(R.RetailerId) as RetailerQuantity, Cast(R.SurveyorActivityDateTime as Date) AS WorkDate
                                                       FROM dbo.Retailer AS R INNER JOIN
                                                            dbo.MonitoringOfficerRegion AS M ON R.RegionId = M.RegionId
                                                       Where  Cast(R.SurveyorActivityDateTime as Date)>'2015-05-01' AND M.MoId=" + MoId + @" AND R.IsVerifiedByRsp=1
                                                       Group By Cast(R.SurveyorActivityDateTime as Date)
                                                       Order By Cast(R.SurveyorActivityDateTime as Date) DESC";

                //Ajke Kon Surveyor kotota kaj korlo tar data
                string sqlSurveyorRetailerOfToday = @"SELECT  S.SurveyorId, COUNT(R.RetailerId) as RetailerQuantity
                                                     FROM dbo.Retailer AS R INNER JOIN
                                                          dbo.Surveyors AS S ON R.SurveyorId = S.SurveyorId INNER JOIN
                                                          dbo.MonitoringOfficerRegion AS M ON S.RegionId = M.RegionId
                                                     Where  Cast(R.SurveyorActivityDateTime as Date)='" + DateTime.Today.Date.ToString("yyyy-MM-dd") + @"' AND M.MoId=" + MoId + @"
                                                     Group By S.SurveyorId
                                                     Order By S.SurveyorId ASC";

                //Region-wise Total & Updated bar chart
                string sqlRegionStatusBar = @"SELECT     dbo.Region.RegionName, (Select Count(RetailerId) from dbo.Retailer where RegionId=dbo.Region.RegionId AND dbo.Retailer.RetailerStatusId<3)  AS                                             TotalRetailer, ((Select Count(RetailerId) from dbo.Retailer where RegionId=dbo.Region.RegionId AND dbo.Retailer.RetailerStatusId=2 AND                                                      dbo.Retailer.IsActive=1) + (Select Count(RetailerId) from dbo.Retailer where RegionId=dbo.Region.RegionId AND dbo.Retailer.IsActive=0)) AS                                                UpdatedRetailer
                                              FROM dbo.Region INNER JOIN
                                                      dbo.MonitoringOfficerRegion ON dbo.Region.RegionId = dbo.MonitoringOfficerRegion.RegionId
                                              WHERE (dbo.MonitoringOfficerRegion.MoId = " + MoId + ")";
                //<--- SQL Statements

                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = connection;
                        using (SqlDataAdapter da = new SqlDataAdapter(command))
                        {

                            connection.Open();
                            //Count all retailer quantity
                            command.CommandText = sqlTotalOutletsOfThisMo; totalRetailersQuantity = Convert.ToInt32(command.ExecuteScalar());
                            //Count all updated retailers
                            command.CommandText = sqlCountAllUpdatedRetailers; objAllUpdatedRetailersCount = command.ExecuteScalar();
                            //Count all new retailers
                            command.CommandText = sqlCountAllNewRetailers; objAllNewRetailersCount = command.ExecuteScalar();
                            //Count all not found retailers
                            command.CommandText = sqlCountAllNotFoundRetailers; objAllNotFoundRetailersCount = command.ExecuteScalar();
                            //Count all verified
                            command.CommandText = sqlCountAllVerifiedRetailers; objAllVerifiedCount = command.ExecuteScalar();
                            //Count todays updated retailes
                            command.CommandText = sqlCountTodaysUpdatedRetailers; objTodaysUpdatedRetailersCount = command.ExecuteScalar();
                            //Count todays new retailes
                            command.CommandText = sqlCountTodaysNewRetailers; objTodaysNewRetailersCount = command.ExecuteScalar();
                            //Count todays not found retailes
                            command.CommandText = sqlCountTodaysNotFoundRetailers; objTodaysNotFoundRetailersCount = command.ExecuteScalar();
                            //Count todays verified retailers
                            command.CommandText = sqlCountTodaysVerifiedRetailers; objTodaysVerifiedCount = command.ExecuteScalar();
                            // fill RSP table
                            command.CommandText = sqlSelectRsp; da.Fill(dtRspList);
                            //fill Retailer table
                            command.CommandText = sqlSelectRetailers; da.Fill(dtRetailerList);
                            //fill Region table.
                            command.CommandText = sqlSelectRegion; da.Fill(dtRegions);
                            //fill Overlapping Retailer table
                            command.CommandText = sqlSelectOverlappingRetailers; da.Fill(tblOverlappingRetailers);
                            //fill duplicate images
                            command.CommandText = sqlDuplicateImages; da.Fill(tblDupli);
                            //protidin koto gulo Surveyor field-e chhilo tar data
                            command.CommandText = sqlSurveyorsQuantityPerDate; da.Fill(tblSurveyorsQuantityPerDate);
                            //protidin koto gulo retailer-e kaj hoechhe tar data
                            command.CommandText = sqlRetailersQuantityPerDate; da.Fill(tblRetailersQuantityPerDate);
                            //date-wise kotogulo verify holo tar data. Shown on line graph.
                            command.CommandText = sqlVerifiedQuantityPerDate; da.Fill(tblVerifiedQuantyPerDate);
                            //kon surveyor kotota kaj korlo tar data
                            command.CommandText = sqlSurveyorRetailerOfToday; da.Fill(tblSurveyorRetailerOfToday);
                            //Region-wise Total & Updated bar chart
                            command.CommandText = sqlRegionStatusBar; da.Fill(tblRegionStatusBar);
                            connection.Close();

                            //Insert MO Log
                            command.CommandText = sqlInsertMoLog;
                            command.Parameters.Clear();
                            command.Parameters.Add("@MoId", SqlDbType.Int).Value = MoId;
                            command.Parameters.Add("@LogDescription", SqlDbType.VarChar).Value = "Comes on Dashboard.";
                            connection.Open(); command.ExecuteNonQuery(); connection.Close();
                        }
                    }
                }

                string overLappingInformation = string.Empty;
                if (tblOverlappingRetailers.Rows.Count > 0)
                {
                    string overLappedRetailers = tblOverlappingRetailers.Rows.Count.ToString();
                    string overLappedSr = tblOverlappingRetailers.Compute("SUM(MultipleSrQuantity)", "").ToString();
                    overLappingInformation = overLappedRetailers + " retailers overlapped by " + overLappedSr + " different surveyors.";
                }

                //--> For search panel
                ViewBag.Regions = dtRegions;
                //<-- For search panel

                //--> Forcasting
                int avgOutletsPerday = Convert.ToInt32(objAllUpdatedRetailersCount) / tblRetailersQuantityPerDate.Rows.Count;
                int requiredDays = totalRetailersQuantity - (Convert.ToInt32(objAllUpdatedRetailersCount) + Convert.ToInt32(objAllNotFoundRetailersCount));
                requiredDays = requiredDays / avgOutletsPerday;
                string forcastMessage = string.Format("{0} more days required to complete. Current average rate {1} outlets/day.", requiredDays, avgOutletsPerday.ToString("N", ci));
                //<-- Forcasting

                //--> Line graph in workflow progress
                string[] chartLabels = new string[tblRetailersQuantityPerDate.Rows.Count];
                for (int i = tblRetailersQuantityPerDate.Rows.Count - 1; i > 0; i--)
                {
                    chartLabels[i] = Convert.ToDateTime(tblRetailersQuantityPerDate.Rows[i]["WorkDate"]).ToString("dd-MMM");
                }

                chartLabels[0] = Convert.ToDateTime(tblRetailersQuantityPerDate.Rows[0]["WorkDate"]).ToString("dd-MMM");

                //--> Retailer quantity
                int[] chartData = new int[tblRetailersQuantityPerDate.Rows.Count];
                for (int i = tblRetailersQuantityPerDate.Rows.Count - 1; i > 0; i--)
                {
                    chartData[i] = Convert.ToInt32(tblRetailersQuantityPerDate.Rows[i]["RetailerQuantity"]);
                }

                chartData[0] = Convert.ToInt32(tblRetailersQuantityPerDate.Rows[0]["RetailerQuantity"]);
                //<-- retailer quantity

                //--> verified quantity
                int[] verifiedData = new int[tblVerifiedQuantyPerDate.Rows.Count];
                for (int i = tblVerifiedQuantyPerDate.Rows.Count - 1; i > 0; i--)
                {
                    verifiedData[i] = Convert.ToInt32(tblVerifiedQuantyPerDate.Rows[i]["RetailerQuantity"]); //Verified quantity
                }

                verifiedData[0] = Convert.ToInt32(tblVerifiedQuantyPerDate.Rows[0]["RetailerQuantity"]);
                //<-- verified quantity

                //<-- Line graph in workflow progress

                //--> Region Chart - Chart2
                int chart2DataQuantity = tblRegionStatusBar.Rows.Count;
                string[] chart2Regions = new string[chart2DataQuantity];
                int[] chart2TotalRetailerQuantity = new int[chart2DataQuantity];
                int[] chart2UpdatedRetailerQuantity = new int[chart2DataQuantity]; // updated + not found. Calculated in sql statement
                for (int i = 0; i < chart2DataQuantity; i++)
                {
                    string regionName = tblRegionStatusBar.Rows[i]["RegionName"].ToString();
                    int totalQuantity = Convert.ToInt32(tblRegionStatusBar.Rows[i]["TotalRetailer"]);
                    int updatedQuantity = Convert.ToInt32(tblRegionStatusBar.Rows[i]["UpdatedRetailer"]);
                    double percentage = totalQuantity != 0 ? (updatedQuantity * 100) / totalQuantity : 0;

                    chart2Regions[i] = string.Format("{0} - {1}%", regionName, percentage.ToString());
                    chart2TotalRetailerQuantity[i] = totalQuantity;
                    chart2UpdatedRetailerQuantity[i] = updatedQuantity;
                }

                //<--Region Chart - Chart2
                //-->ViewBag
                ViewBag.MoId = MoId.ToString();
                ViewBag.UserName = Request.QueryString["user"].ToString(); //LoginName

                ViewBag.TotalRetailersQuantity = totalRetailersQuantity;
                ViewBag.AllUpdatedRetailersCount = Convert.ToInt32(objAllUpdatedRetailersCount); //All updated retailers count.
                ViewBag.AllNewRetailersCount = Convert.ToInt32(objAllNewRetailersCount); //All new retailers count.
                ViewBag.AllNotFoundRetailersCount = Convert.ToInt32(objAllNotFoundRetailersCount); // All not found retailers count.
                ViewBag.AllPendingRetailersCount = totalRetailersQuantity - (Convert.ToInt32(objAllUpdatedRetailersCount) + Convert.ToInt32(objAllNotFoundRetailersCount));
                ViewBag.AllVerifiedRetailersCount = Convert.ToInt32(objAllVerifiedCount); //all verified
                ViewBag.TodaysUpdatedRetailersCount = Convert.ToInt32(objTodaysUpdatedRetailersCount); //Todays updated retailers count.
                ViewBag.TodaysNewRetailersCount = Convert.ToInt32(objTodaysNewRetailersCount); // Todays new retailer count.
                ViewBag.TodaysNotFoundRetailersCount = Convert.ToInt32(objTodaysNotFoundRetailersCount); // Todays not found retailser count.
                ViewBag.TodaysVerifiedCount = Convert.ToInt32(objTodaysVerifiedCount);

                ViewBag.OverLappingInformation = overLappingInformation;
                ViewBag.MyCulture = ci;
                ViewBag.ActiveMenuItem = "Dashboard";
                ViewBag.DuplicateImagesCount = tblDupli.Rows.Count;
                ViewBag.SurveyorsQuantityPerDate = tblSurveyorsQuantityPerDate;
                ViewBag.RetailersQuantityPerDate = tblRetailersQuantityPerDate;
                ViewBag.SurveyorRetailerOfToday = tblSurveyorRetailerOfToday;
                ViewBag.ChartLabel = chartLabels;
                ViewBag.ChartData = chartData;
                ViewBag.VerifiedData = verifiedData;
                ViewBag.ChartTwoRegion = chart2Regions;
                ViewBag.ChartTwoTotalRetailer = chart2TotalRetailerQuantity;
                ViewBag.ChartTwoUpdateRetailer = chart2UpdatedRetailerQuantity;
                ViewBag.ForcastMessage = forcastMessage;
                //<-- ViewBag

                return View();
            }
            catch (Exception ex)
            {
                var path = HttpContext.Server.MapPath("~/App_Data");
                ExceptionLogger.CreateLog(ex, path);
                return View("ErrorContent");
            }


            return View();
        }
	}
}