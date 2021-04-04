using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Configuration;

namespace RobiPosMapper.Areas.RobiAdmin.Controllers
{
    public class OutletListController : Controller
    {
        #region Global Variables

        CultureInfo myCulture = new CultureInfo("bn-BD-robi");
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

        public ActionResult UpdatedOutletList()
        {
            //String queryDate = Request.QueryString["date"].ToString();
            //--> Variables
            String fromDate = Request.QueryString["fromdate"].ToString(); 
            String toDate = Request.QueryString["todate"].ToString(); 

            String rsp = Request.QueryString["rsp"].ToString();
            Int32 MoId = Convert.ToInt32(Request.QueryString["moid"]);


            String sqlSelectRetailers = String.Empty; String sqlSearchCountLog = String.Empty;
            String whereClause = " Where RetailerStatusId=2"; String whereClauseForSearchCount = " Where RetailerStatusId=2 ";
            //<-- Variables

            //check rsp
            if (!String.IsNullOrEmpty(rsp))
            { //-->if rsp wise search
                sqlSelectRetailers = @"SELECT Region.RegionId, Region.RegionName, A.AreaId, A.AreaName, Retailer.RetailerId, Retailer.RetailerName, Retailer.Address, Retailer.DefaultPhotoName AS PhotoName,                        Retailer.Latitude, Retailer.Longitude, Retailer.SurveyorActivityDateTime AS LastActivityDateTime, Retailer.IsVerifiedByRsp AS IsVerified, Retailer.RetailerStatusId,
                                          (SELECT COUNT(AppearId) AS Expr1 FROM dbo.RetailerAppearance WHERE (RetailerId = Retailer.RetailerId) AND (MoId=" + MoId + @")) AS SearchQuantity,
                                          (SELECT COUNT(DISTINCT SrId) AS Count FROM dbo.SrRetailerLog WHERE (RetailerId = Retailer.RetailerId)) AS VisitedSrQuantity,
                                          (SELECT COUNT(ElMsisdnId) AS Expr1 FROM dbo.ElMsisdn WHERE (RetailerId = Retailer.RetailerId)) AS ElQuantity,
                                          (SELECT COUNT(SimPosCodeId) AS Expr1 FROM dbo.SimPosCode WHERE (RetailerId = Retailer.RetailerId)) AS SimPosCodeQuantity,
                                          Retailer.RetailerStatusId, RSP.RspId, RSP.RspName, S.SurveyorId, S.LoginName
                                      FROM dbo.Area AS A INNER JOIN
                                           dbo.RSP AS RSP INNER JOIN
                                           dbo.Retailer AS Retailer ON RSP.RspId = Retailer.RspId INNER JOIN
                                           dbo.Region AS Region ON Retailer.RegionId = Region.RegionId ON A.AreaId = Retailer.AreaId INNER JOIN
                                           dbo.Surveyors AS S ON Retailer.SurveyorId = S.SurveyorId INNER JOIN
                                           dbo.MonitoringOfficerRegion AS MOR ON Region.RegionId = MOR.RegionId ";

                sqlSearchCountLog = @"SELECT  Retailer.RetailerId , MOR.MoId
                                      FROM dbo.RSP AS RSP INNER JOIN
                                              dbo.Retailer AS Retailer ON RSP.RspId = Retailer.RspId INNER JOIN
                                              dbo.Region AS Region ON Retailer.RegionId = Region.RegionId INNER JOIN
                                              dbo.MonitoringOfficerRegion AS MOR ON Region.RegionId = MOR.RegionId " ;
                                 

                Int32 rspId = Convert.ToInt32(rsp);

                if (String.IsNullOrEmpty(whereClause))
                {
                    whereClause = " RSP.RspId=" + rspId + "";
                }
                else
                {
                    whereClause += " AND RSP.RspId=" + rspId + "";
                }


                if (String.IsNullOrEmpty(whereClauseForSearchCount))
                {
                    whereClauseForSearchCount = " RSP.RspId=" + rspId + "";
                }
                else
                {
                    whereClauseForSearchCount += " AND RSP.RspId=" + rspId + "";
                }
            }
            else
            {
                sqlSelectRetailers = @"SELECT Region.RegionId, Region.RegionName, A.AreaId, A.AreaName, Retailer.RetailerId, Retailer.RetailerName, Retailer.Address, Retailer.DefaultPhotoName AS PhotoName,                        Retailer.Latitude, Retailer.Longitude, Retailer.SurveyorActivityDateTime AS LastActivityDateTime, Retailer.IsVerifiedByRsp AS IsVerified,
                                          (SELECT COUNT(AppearId) AS Expr1 FROM dbo.RetailerAppearance WHERE (RetailerId = Retailer.RetailerId) AND (MoId="+ MoId + @")) AS SearchQuantity,
                                          (SELECT COUNT(DISTINCT SrId) AS Count FROM dbo.SrRetailerLog WHERE (RetailerId = Retailer.RetailerId)) AS VisitedSrQuantity,
                                          (SELECT COUNT(ElMsisdnId) AS Expr1 FROM dbo.ElMsisdn WHERE (RetailerId = Retailer.RetailerId)) AS ElQuantity,
                                          (SELECT COUNT(SimPosCodeId) AS Expr1 FROM dbo.SimPosCode WHERE (RetailerId = Retailer.RetailerId)) AS SimPosCodeQuantity, 
                                          Retailer.RetailerStatusId, RSP.RspId, RSP.RspName, S.SurveyorId, S.LoginName
                                      FROM dbo.Area AS A INNER JOIN
                                           dbo.RSP AS RSP INNER JOIN
                                           dbo.Retailer AS Retailer ON RSP.RspId = Retailer.RspId INNER JOIN
                                           dbo.Region AS Region ON Retailer.RegionId = Region.RegionId ON A.AreaId = Retailer.AreaId INNER JOIN
                                           dbo.Surveyors AS S ON Retailer.SurveyorId = S.SurveyorId INNER JOIN
                                           dbo.MonitoringOfficerRegion AS MOR ON Region.RegionId = MOR.RegionId ";

                sqlSearchCountLog = @"SELECT Retailer.RetailerId,MOR.MoId
                                      FROM dbo.RSP AS RSP INNER JOIN
                                              dbo.Retailer AS Retailer ON RSP.RspId = Retailer.RspId INNER JOIN
                                              dbo.Region AS Region ON Retailer.RegionId = Region.RegionId INNER JOIN
                                              dbo.MonitoringOfficerRegion AS MOR ON Region.RegionId = MOR.RegionId ";
             

                if (String.IsNullOrEmpty(whereClause))
                {
                    whereClause = " MOR.MoId = "+ MoId +"";
                }
                else
                {
                    whereClause += " AND MOR.MoId = "+ MoId +"";
                }

                if (String.IsNullOrEmpty(whereClauseForSearchCount))
                {
                    whereClauseForSearchCount = " MOR.MoId = " + MoId + "";
                }
                else
                {
                    whereClauseForSearchCount += " AND MOR.MoId = " + MoId + "";
                }
            }

            if (!String.IsNullOrEmpty(fromDate))
            {
               // DateTime date;
               // DateTime.TryParseExact(queryDate, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out date);
                DateTime startDate;
                DateTime.TryParseExact(fromDate, InputDateFormatsAllowed, myCulture, DateTimeStyles.None, out startDate);
                DateTime endDate;
                DateTime.TryParseExact(toDate, InputDateFormatsAllowed, myCulture, DateTimeStyles.None, out endDate);

                whereClause += " AND CAST(Retailer.SurveyorActivityDateTime AS Date) Between '" + startDate.ToString("yyyy-MM-dd") + "' and '" + endDate.ToString("yyyy-MM-dd") + "'";

                whereClauseForSearchCount += " AND CAST(Retailer.SurveyorActivityDateTime AS Date) Between '" + startDate.ToString("yyyy-MM-dd") + "' and '" + endDate.ToString("yyyy-MM-dd") + "'";
            }

            sqlSelectRetailers += whereClause + "  ORDER BY Retailer.SurveyorActivityDateTime DESC";

            sqlSearchCountLog += whereClauseForSearchCount;

            DataTable dtRetailers = new DataTable();
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                using (SqlCommand command = new SqlCommand(sqlSelectRetailers, connection))
                {
                    using (SqlDataAdapter da = new SqlDataAdapter(command))
                    {
                        connection.Open();
                        da.Fill(dtRetailers);

                        command.CommandText = "Insert Into RetailerAppearance(RetailerId,MoId) "  + sqlSearchCountLog;
                        command.ExecuteNonQuery();
                        connection.Close();
                    }
                }
            }

            //--> View Bag
            ViewBag.MoId = MoId.ToString();
            ViewBag.UserName = Request.QueryString["user"].ToString(); //LoginName
            ViewBag.Retailers = dtRetailers;
            ViewBag.Title = "Updated retailers  ";
            ViewBag.MyCulture = myCulture;
            //<-- View Bag



            return View("OutletList");
        }

        public ActionResult NewOutletList()
        {
            //String queryDate = Request.QueryString["date"].ToString();
            //--> Variables
            String fromDate = Request.QueryString["fromdate"].ToString();
            String toDate = Request.QueryString["todate"].ToString();

            String rsp = Request.QueryString["rsp"].ToString();
            Int32 MoId = Convert.ToInt32(Request.QueryString["moid"]);


            String sqlSelectRetailers = String.Empty; String sqlSearchCountLog = String.Empty;
            String whereClause = " Where RetailerStatusId=3"; String whereClauseForSearchCount = " Where RetailerStatusId=3 ";
            //<-- Variables

            //check rsp
            if (!String.IsNullOrEmpty(rsp))
            {
                sqlSelectRetailers = @"SELECT Region.RegionId, Region.RegionName, A.AreaId, A.AreaName, Retailer.RetailerId, Retailer.RetailerName, Retailer.Address, Retailer.DefaultPhotoName AS PhotoName,                        Retailer.Latitude, Retailer.Longitude, Retailer.SurveyorActivityDateTime AS LastActivityDateTime, Retailer.IsVerifiedByRsp AS IsVerified, Retailer.RetailerStatusId,
                                          (SELECT COUNT(AppearId) AS Expr1 FROM dbo.RetailerAppearance WHERE (RetailerId = Retailer.RetailerId) AND (MoId=" + MoId + @")) AS SearchQuantity,
                                          (SELECT COUNT(DISTINCT SrId) AS Count FROM dbo.SrRetailerLog WHERE (RetailerId = Retailer.RetailerId)) AS VisitedSrQuantity,
                                          (SELECT COUNT(ElMsisdnId) AS Expr1 FROM dbo.ElMsisdn WHERE (RetailerId = Retailer.RetailerId)) AS ElQuantity,
                                          (SELECT COUNT(SimPosCodeId) AS Expr1 FROM dbo.SimPosCode WHERE (RetailerId = Retailer.RetailerId)) AS SimPosCodeQuantity,
                                          Retailer.RetailerStatusId, RSP.RspId, RSP.RspName, S.SurveyorId, S.LoginName
                                      FROM dbo.Area AS A INNER JOIN
                                           dbo.RSP AS RSP INNER JOIN
                                           dbo.Retailer AS Retailer ON RSP.RspId = Retailer.RspId INNER JOIN
                                           dbo.Region AS Region ON Retailer.RegionId = Region.RegionId ON A.AreaId = Retailer.AreaId INNER JOIN
                                           dbo.Surveyors AS S ON Retailer.SurveyorId = S.SurveyorId INNER JOIN
                                           dbo.MonitoringOfficerRegion AS MOR ON Region.RegionId = MOR.RegionId ";

                sqlSearchCountLog = @"SELECT  Retailer.RetailerId , MOR.MoId
                                      FROM dbo.RSP AS RSP INNER JOIN
                                              dbo.Retailer AS Retailer ON RSP.RspId = Retailer.RspId INNER JOIN
                                              dbo.Region AS Region ON Retailer.RegionId = Region.RegionId INNER JOIN
                                              dbo.MonitoringOfficerRegion AS MOR ON Region.RegionId = MOR.RegionId ";


                Int32 rspId = Convert.ToInt32(rsp);

                if (String.IsNullOrEmpty(whereClause))
                {
                    whereClause = " RSP.RspId=" + rspId + "";
                }
                else
                {
                    whereClause += " AND RSP.RspId=" + rspId + "";
                }


                if (String.IsNullOrEmpty(whereClauseForSearchCount))
                {
                    whereClauseForSearchCount = " RSP.RspId=" + rspId + "";
                }
                else
                {
                    whereClauseForSearchCount += " AND RSP.RspId=" + rspId + "";
                }
            }
            else
            {
                sqlSelectRetailers = @"SELECT Region.RegionId, Region.RegionName, A.AreaId, A.AreaName, Retailer.RetailerId, Retailer.RetailerName, Retailer.Address, Retailer.DefaultPhotoName AS PhotoName,                        Retailer.Latitude, Retailer.Longitude, Retailer.SurveyorActivityDateTime AS LastActivityDateTime, Retailer.IsVerifiedByRsp AS IsVerified,
                                          (SELECT COUNT(AppearId) AS Expr1 FROM dbo.RetailerAppearance WHERE (RetailerId = Retailer.RetailerId) AND (MoId=" + MoId + @")) AS SearchQuantity,
                                          (SELECT COUNT(DISTINCT SrId) AS Count FROM dbo.SrRetailerLog WHERE (RetailerId = Retailer.RetailerId)) AS VisitedSrQuantity,
                                          (SELECT COUNT(ElMsisdnId) AS Expr1 FROM dbo.ElMsisdn WHERE (RetailerId = Retailer.RetailerId)) AS ElQuantity,
                                          (SELECT COUNT(SimPosCodeId) AS Expr1 FROM dbo.SimPosCode WHERE (RetailerId = Retailer.RetailerId)) AS SimPosCodeQuantity, 
                                          Retailer.RetailerStatusId, RSP.RspId, RSP.RspName, S.SurveyorId, S.LoginName
                                      FROM dbo.Area AS A INNER JOIN
                                           dbo.RSP AS RSP INNER JOIN
                                           dbo.Retailer AS Retailer ON RSP.RspId = Retailer.RspId INNER JOIN
                                           dbo.Region AS Region ON Retailer.RegionId = Region.RegionId ON A.AreaId = Retailer.AreaId INNER JOIN
                                           dbo.Surveyors AS S ON Retailer.SurveyorId = S.SurveyorId INNER JOIN
                                           dbo.MonitoringOfficerRegion AS MOR ON Region.RegionId = MOR.RegionId ";

                sqlSearchCountLog = @"SELECT Retailer.RetailerId,MOR.MoId
                                      FROM dbo.RSP AS RSP INNER JOIN
                                              dbo.Retailer AS Retailer ON RSP.RspId = Retailer.RspId INNER JOIN
                                              dbo.Region AS Region ON Retailer.RegionId = Region.RegionId INNER JOIN
                                              dbo.MonitoringOfficerRegion AS MOR ON Region.RegionId = MOR.RegionId ";


                if (String.IsNullOrEmpty(whereClause))
                {
                    whereClause = " MOR.MoId = " + MoId + "";
                }
                else
                {
                    whereClause += " AND MOR.MoId = " + MoId + "";
                }

                if (String.IsNullOrEmpty(whereClauseForSearchCount))
                {
                    whereClauseForSearchCount = " MOR.MoId = " + MoId + "";
                }
                else
                {
                    whereClauseForSearchCount += " AND MOR.MoId = " + MoId + "";
                }
            }

            if (!String.IsNullOrEmpty(fromDate))
            {
                // DateTime date;
                // DateTime.TryParseExact(queryDate, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out date);
                DateTime startDate;
                DateTime.TryParseExact(fromDate, InputDateFormatsAllowed, myCulture, DateTimeStyles.None, out startDate);
                DateTime endDate;
                DateTime.TryParseExact(toDate, InputDateFormatsAllowed, myCulture, DateTimeStyles.None, out endDate);

                whereClause += " AND CAST(Retailer.SurveyorActivityDateTime AS Date) Between '" + startDate.ToString("yyyy-MM-dd") + "' and '" + endDate.ToString("yyyy-MM-dd") + "'";

                whereClauseForSearchCount += " AND CAST(Retailer.SurveyorActivityDateTime AS Date) Between '" + startDate.ToString("yyyy-MM-dd") + "' and '" + endDate.ToString("yyyy-MM-dd") + "'";
            }

            sqlSelectRetailers += whereClause + "  ORDER BY Retailer.SurveyorActivityDateTime DESC";

            sqlSearchCountLog += whereClauseForSearchCount;

            DataTable dtRetailers = new DataTable();
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                using (SqlCommand command = new SqlCommand(sqlSelectRetailers, connection))
                {
                    using (SqlDataAdapter da = new SqlDataAdapter(command))
                    {
                        connection.Open();
                        da.Fill(dtRetailers);

                        command.CommandText = "Insert Into RetailerAppearance(RetailerId,MoId) " + sqlSearchCountLog;
                        command.ExecuteNonQuery();
                        connection.Close();
                    }
                }
            }

            //--> View Bag
            ViewBag.MoId = MoId.ToString();
            ViewBag.UserName = Request.QueryString["user"].ToString(); //LoginName
            ViewBag.Retailers = dtRetailers;
            ViewBag.Title = "Newly added retailers  ";
            ViewBag.MyCulture = myCulture;
            //<-- View Bag



            return View("OutletList");
        }

        //this method was used in Phase-II
        public ActionResult NewOutletList_Old()
        {
            String queryDate = Request.QueryString["date"].ToString();
            String rsp = Request.QueryString["rsp"].ToString();

            String connectionString = WebConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;
            String sqlSelectRetailers = @"Select RegionName,AreaName,RspName,RetailerId, RetailerName, Address,Latitude,Longitude, SurveyorActivityDateTime as WorkDateTime, Photo from viewRetailerList ";
            String whereClause = " Where RetailerStatusId=3 and IsReevaluated=0 AND SurveyorActivityDateTime is not NULL ";
            if (!String.IsNullOrEmpty(queryDate))
            {
                DateTime date;
                DateTime.TryParseExact(queryDate, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out date);
                DateTime startDate = new DateTime(date.Year, date.Month, date.Day, 00, 00, 01);
                DateTime endDate = new DateTime(date.Year, date.Month, date.Day, 23, 59, 59);

                whereClause += " AND SurveyorActivityDateTime Between '" + startDate + "' and '" + endDate + "'";
            }

            //check rsp
            if (!String.IsNullOrEmpty(rsp))
            {
                Int32 rspId = Convert.ToInt32(rsp);
                if (String.IsNullOrEmpty(whereClause))
                {
                    whereClause = " RspId=" + rspId + "";
                }
                else
                {
                    whereClause += " AND RspId=" + rspId + "";
                }
            }

            sqlSelectRetailers += whereClause + " Order By RegionName,AreaName,RspName, RetailerName";

            DataTable dtRetailers = new DataTable();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(sqlSelectRetailers, connection))
                {
                    using (SqlDataAdapter da = new SqlDataAdapter(command))
                    {
                        connection.Open();
                        da.Fill(dtRetailers); connection.Close();
                    }
                }
            }

            ViewBag.UserName = Request.QueryString["user"].ToString();
            ViewBag.Retailers = dtRetailers;


            ViewBag.Title = "Newly created retailers ";
            return View("OutletList");
        }

        public ActionResult TotalOutletList()
        {
            String queryDate = Request.QueryString["date"].ToString();
            String rsp = Request.QueryString["rsp"].ToString();

            String connectionString = WebConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;
            String sqlSelectRetailers = @"Select RegionName,AreaName,RspName,RetailerId, RetailerName, Address,Latitude,Longitude, SurveyorActivityDateTime as WorkDateTime, Photo from viewRetailerList ";
            String whereClause = " Where SurveyorActivityDateTime is not NULL ";
            if (!String.IsNullOrEmpty(queryDate))
            {
                DateTime date;
                DateTime.TryParseExact(queryDate, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out date);
                DateTime startDate = new DateTime(date.Year, date.Month, date.Day, 00, 00, 01);
                DateTime endDate = new DateTime(date.Year, date.Month, date.Day, 23, 59, 59);

                whereClause += " AND SurveyorActivityDateTime Between '" + startDate + "' and '" + endDate + "'";
            }

            //check rsp
            if (!String.IsNullOrEmpty(rsp))
            {
                Int32 rspId = Convert.ToInt32(rsp);
                if (String.IsNullOrEmpty(whereClause))
                {
                    whereClause = " RspId=" + rspId + "";
                }
                else
                {
                    whereClause += " AND RspId=" + rspId + "";
                }
            }

            sqlSelectRetailers += whereClause + " Order By RegionName,AreaName,RspName, RetailerName";

            DataTable dtRetailers = new DataTable();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(sqlSelectRetailers, connection))
                {
                    using (SqlDataAdapter da = new SqlDataAdapter(command))
                    {
                        connection.Open();
                        da.Fill(dtRetailers); connection.Close();
                    }
                }
            }

            ViewBag.UserName = Request.QueryString["user"].ToString();
            ViewBag.Retailers = dtRetailers;
            ViewBag.Title = "Total retailers ";
            return View("OutletList");
        }

        
        public ActionResult OverlappingOutletList()
        {
            String connectionString = WebConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;

            int moid =Convert.ToInt32(Request.QueryString["moid"]);
            String sqlSelectRetailers = @"SELECT SrLog.RetailerId, COUNT(DISTINCT SrLog.SrId) AS MultipleSrQuantity
                                   FROM dbo.SrRetailerLog AS SrLog INNER JOIN
                                     dbo.Retailer ON SrLog.RetailerId = dbo.Retailer.RetailerId INNER JOIN
                                     dbo.MonitoringOfficerRegion AS MOR ON dbo.Retailer.RegionId = MOR.RegionId
                                   WHERE (MOR.MoId = "+ moid + @")
                                   GROUP BY SrLog.RetailerId
                                   HAVING (COUNT(DISTINCT SrLog.SrId) > 1)
                                   ORDER BY SrLog.RetailerId";


            DataTable dtRetailers = new DataTable();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(sqlSelectRetailers, connection))
                {
                    using (SqlDataAdapter da = new SqlDataAdapter(command))
                    {
                        connection.Open();
                        da.Fill(dtRetailers); connection.Close();
                    }

                    command.CommandText = sqlInsertMoLog;
                    command.Parameters.Clear();
                    command.Parameters.Add("@MoId", SqlDbType.Int).Value = Convert.ToInt32(moid);
                    command.Parameters.Add("@LogDescription", SqlDbType.VarChar).Value = "Clicked on Overlapping Retailers menu item.";
                    connection.Open(); command.ExecuteNonQuery(); connection.Close();
                }
            }



            ViewBag.UserName = Request.QueryString["user"].ToString();
            ViewBag.MoId = moid.ToString();

            ViewBag.Retailers = dtRetailers;
            ViewBag.ActiveMenuName = "Overlapping Retailers";
            ViewBag.Title = "Overlapping Retailers";
            return View();
        }

        //for inhosue use, from RETAILER table
        public ActionResult TotalOutletGreaterThanAnSpecificDateTimeList()
        {
            String queryDate = Request.QueryString["datetime"].ToString();
           
            String connectionString = WebConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;
            String sqlSelectRetailers = @"Select RegionName,AreaName,RspName,RetailerId, RetailerName, Address, Latitude,Longitude,SurveyorActivityDateTime as WorkDateTime, Photo from viewRetailerList ";
            String whereClause = " Where SurveyorActivityDateTime is not NULL ";
            if (!String.IsNullOrEmpty(queryDate))
            {
                DateTime date;
                DateTime.TryParseExact(queryDate, "yyyy-MM-dd HH:mm", null, System.Globalization.DateTimeStyles.None, out date);
               
                whereClause += " AND SurveyorActivityDateTime > '" + queryDate + "'";
            }


            sqlSelectRetailers += whereClause + " Order By SurveyorActivityDateTime ";
            DataTable dtRetailers = new DataTable();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(sqlSelectRetailers, connection))
                {
                    using (SqlDataAdapter da = new SqlDataAdapter(command))
                    {
                        connection.Open();
                        da.Fill(dtRetailers); connection.Close();
                    }
                }
            }

            ViewBag.UserName = Request.QueryString["user"].ToString();
            ViewBag.Retailers = dtRetailers;
            ViewBag.Title = "Total retailers ";
            return View("OutletList");
        }

        //for inhosue use, from SR Log table
        public ActionResult TotalOutletGreaterThanAnSpecificDateTimeListFromSrLog()
        {
            String queryDate = Request.QueryString["datetime"].ToString();
            DataTable dtRetailers = new DataTable();
            String connectionString = WebConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;
            String sqlSelectRetailers = @"SELECT R.RegionName, R.AreaName, R.RspName, R.RetailerId, R.RetailerName, R.Address, R.Latitude,R.Longitude, R.Photo, S.LogDateTime AS WorkDateTime FROM dbo.viewRetailerList AS R INNER JOIN dbo.SrRetailerLog AS S ON R.RetailerId = S.RetailerId WHERE S.LogDateTime> @LogDateTime Order BY  
              S.LogDateTime";

            if (!String.IsNullOrEmpty(queryDate))
            {
               DateTime date;
               Boolean isValidDateTime= DateTime.TryParseExact(queryDate, "yyyy-MM-dd HH:mm", null, System.Globalization.DateTimeStyles.None, out date);
               if (!isValidDateTime)
               {
                   throw new ArgumentNullException("Date time is invalid");
               }
               using (SqlConnection connection = new SqlConnection(connectionString))
               {
                   using (SqlCommand command = new SqlCommand(sqlSelectRetailers, connection))
                   {
                       using (SqlDataAdapter da = new SqlDataAdapter(command))
                       {
                           connection.Open();
                           command.Parameters.Clear();
                           command.Parameters.AddWithValue("@LogDateTime", date);
                           da.Fill(dtRetailers); connection.Close();
                       }
                   }
               }

            }
            else
            {
                throw new ArgumentNullException("Date time is invalid");
            }
            ViewBag.UserName = Request.QueryString["user"].ToString();
            ViewBag.Retailers = dtRetailers;
            ViewBag.Title = "Total retailers ";
            return View("OutletList");
        }

        //This accesssable from URL only, (for inhosue use), from SR Log table
        public ActionResult SrWiseRetailersFromSrLog()
        {
            Int32 surveyorId = Convert.ToInt32( Request.QueryString["srid"]);
            string strDate = String.Empty;

            if (Request.QueryString["date"]!=null)
            {
                strDate=   Request.QueryString["date"].ToString();
            }
            DataTable dtRetailers = new DataTable();
            String connectionString = WebConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;

            String sqlSelectRetailers = @"SELECT R.RegionName, R.AreaName, R.RspName, R.RetailerId, R.RetailerName, R.Address,R.Latitude,R.Longitude, R.Photo, S.LogDateTime as WorkDateTime FROM dbo.viewRetailerList AS R INNER JOIN dbo.SrRetailerLog AS S ON R.RetailerId = S.RetailerId  ";
            String whereClause = " WHERE S.SrId= @SrId ";
            DateTime date;
            if (!String.IsNullOrEmpty(strDate))
            {
                whereClause += " AND CAST (S.LogDateTime AS DATE)='" + strDate + "'";
            }

            String orderByClause = " Order BY  R.RegionName ASC, R.AreaName ASC, R.RspName ASC, S.LogDateTime DESC";
            String sql = sqlSelectRetailers + whereClause + orderByClause;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    using (SqlDataAdapter da = new SqlDataAdapter(command))
                    {
                        connection.Open();
                        command.Parameters.Clear();
                        command.Parameters.AddWithValue("@SrId", surveyorId);
                        da.Fill(dtRetailers); connection.Close();
                    }
                }
            }

            ViewBag.Date = strDate;
            ViewBag.UserName = Request.QueryString["user"].ToString();
            ViewBag.Retailers = dtRetailers;
            ViewBag.Title = "Total retailers ";
            return View("OutletList");
        }

        public ActionResult SrWiseRetailersFromRetailer()
        {
            //--> Variables
            int srId = Convert.ToInt32(Request.QueryString["srid"]);
            String fromDate=string.Empty;

            if (Request.QueryString["fromdate"] != null) { fromDate = Request.QueryString["fromdate"].ToString(); };
            String toDate=string.Empty;
            if (Request.QueryString["todate"] != null) { toDate = Request.QueryString["todate"].ToString(); };

            String rsp=string.Empty;

            if (Request.QueryString["rsp"] != null) { rsp = Request.QueryString["rsp"].ToString(); };
            Int32 MoId = Convert.ToInt32(Request.QueryString["moid"]);

            String sqlSelectRetailers = String.Empty; String sqlSearchCountLog = String.Empty;

            String whereClause = String.Empty; String whereClauseForSearchCount = string.Empty;
            if (Request.QueryString["statusid"]==null)
            {
                whereClause = " Where RetailerStatusId>1 AND S.SurveyorId=" + srId + " ";
                whereClauseForSearchCount = " Where RetailerStatusId>1 AND Retailer.SurveyorId=" + srId + " ";
            }
            else
            {
                int statusId = Convert.ToInt32(Request.QueryString["statusid"]);
                whereClause = " Where RetailerStatusId="+ statusId + " AND S.SurveyorId=" + srId + " ";
                whereClauseForSearchCount = " Where RetailerStatusId=" + statusId + " AND Retailer.SurveyorId=" + srId + " ";
            }
            //<-- Variables

            //check rsp
            if (!String.IsNullOrEmpty(rsp))
            { //-->if rsp wise search
                sqlSelectRetailers = @"SELECT Region.RegionId, Region.RegionName, A.AreaId, A.AreaName, Retailer.RetailerId, Retailer.RetailerName, Retailer.Address, Retailer.DefaultPhotoName AS PhotoName, Retailer.Latitude, Retailer.Longitude, Retailer.SurveyorActivityDateTime AS LastActivityDateTime, Retailer.IsVerifiedByRsp AS IsVerified, Retailer.RetailerStatusId,
                                          (SELECT COUNT(AppearId) AS Expr1 FROM dbo.RetailerAppearance WHERE (RetailerId = Retailer.RetailerId) AND (MoId=" + MoId + @")) AS SearchQuantity,
                                          (SELECT COUNT(DISTINCT SrId) AS Count FROM dbo.SrRetailerLog WHERE (RetailerId = Retailer.RetailerId)) AS VisitedSrQuantity,
                                          (SELECT COUNT(ElMsisdnId) AS Expr1 FROM dbo.ElMsisdn WHERE (RetailerId = Retailer.RetailerId)) AS ElQuantity,
                                          (SELECT COUNT(SimPosCodeId) AS Expr1 FROM dbo.SimPosCode WHERE (RetailerId = Retailer.RetailerId)) AS SimPosCodeQuantity,
                                          Retailer.RetailerStatusId, RSP.RspId, RSP.RspName, S.SurveyorId, S.LoginName
                                      FROM dbo.Area AS A INNER JOIN
                                           dbo.RSP AS RSP INNER JOIN
                                           dbo.Retailer AS Retailer ON RSP.RspId = Retailer.RspId INNER JOIN
                                           dbo.Region AS Region ON Retailer.RegionId = Region.RegionId ON A.AreaId = Retailer.AreaId INNER JOIN
                                           dbo.Surveyors AS S ON Retailer.SurveyorId = S.SurveyorId INNER JOIN
                                           dbo.MonitoringOfficerRegion AS MOR ON Region.RegionId = MOR.RegionId ";

                sqlSearchCountLog = @"SELECT  Retailer.RetailerId , MOR.MoId
                                      FROM dbo.RSP AS RSP INNER JOIN
                                              dbo.Retailer AS Retailer ON RSP.RspId = Retailer.RspId INNER JOIN
                                              dbo.Region AS Region ON Retailer.RegionId = Region.RegionId INNER JOIN
                                              dbo.MonitoringOfficerRegion AS MOR ON Region.RegionId = MOR.RegionId ";


                Int32 rspId = Convert.ToInt32(rsp);

                if (String.IsNullOrEmpty(whereClause))
                {
                    whereClause = " RSP.RspId=" + rspId + "";
                }
                else
                {
                    whereClause += " AND RSP.RspId=" + rspId + "";
                }


                if (String.IsNullOrEmpty(whereClauseForSearchCount))
                {
                    whereClauseForSearchCount = " RSP.RspId=" + rspId + "";
                }
                else
                {
                    whereClauseForSearchCount += " AND RSP.RspId=" + rspId + "";
                }
            }
            else
            {
                sqlSelectRetailers = @"SELECT Region.RegionId, Region.RegionName, A.AreaId, A.AreaName, Retailer.RetailerId, Retailer.RetailerName, Retailer.Address, Retailer.DefaultPhotoName AS PhotoName,                        Retailer.Latitude, Retailer.Longitude, Retailer.SurveyorActivityDateTime AS LastActivityDateTime, Retailer.IsVerifiedByRsp AS IsVerified,
                                          (SELECT COUNT(AppearId) AS Expr1 FROM dbo.RetailerAppearance WHERE (RetailerId = Retailer.RetailerId) AND (MoId=" + MoId + @")) AS SearchQuantity,
                                          (SELECT COUNT(DISTINCT SrId) AS Count FROM dbo.SrRetailerLog WHERE (RetailerId = Retailer.RetailerId)) AS VisitedSrQuantity,
                                          (SELECT COUNT(ElMsisdnId) AS Expr1 FROM dbo.ElMsisdn WHERE (RetailerId = Retailer.RetailerId)) AS ElQuantity,
                                          (SELECT COUNT(SimPosCodeId) AS Expr1 FROM dbo.SimPosCode WHERE (RetailerId = Retailer.RetailerId)) AS SimPosCodeQuantity, 
                                          Retailer.RetailerStatusId, RSP.RspId, RSP.RspName, S.SurveyorId, S.LoginName
                                      FROM dbo.Area AS A INNER JOIN
                                           dbo.RSP AS RSP INNER JOIN
                                           dbo.Retailer AS Retailer ON RSP.RspId = Retailer.RspId INNER JOIN
                                           dbo.Region AS Region ON Retailer.RegionId = Region.RegionId ON A.AreaId = Retailer.AreaId INNER JOIN
                                           dbo.Surveyors AS S ON Retailer.SurveyorId = S.SurveyorId INNER JOIN
                                           dbo.MonitoringOfficerRegion AS MOR ON Region.RegionId = MOR.RegionId ";

                sqlSearchCountLog = @"SELECT Retailer.RetailerId,MOR.MoId
                                      FROM dbo.RSP AS RSP INNER JOIN
                                              dbo.Retailer AS Retailer ON RSP.RspId = Retailer.RspId INNER JOIN
                                              dbo.Region AS Region ON Retailer.RegionId = Region.RegionId INNER JOIN
                                              dbo.MonitoringOfficerRegion AS MOR ON Region.RegionId = MOR.RegionId ";


                if (String.IsNullOrEmpty(whereClause))
                {
                    whereClause = " MOR.MoId = " + MoId + "";
                }
                else
                {
                    whereClause += " AND MOR.MoId = " + MoId + "";
                }

                if (String.IsNullOrEmpty(whereClauseForSearchCount))
                {
                    whereClauseForSearchCount = " MOR.MoId = " + MoId + "";
                }
                else
                {
                    whereClauseForSearchCount += " AND MOR.MoId = " + MoId + "";
                }
            }

            if (!String.IsNullOrEmpty(fromDate))
            {
                // DateTime date;
                // DateTime.TryParseExact(queryDate, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out date);
                DateTime startDate;
                DateTime.TryParseExact(fromDate, InputDateFormatsAllowed, myCulture, DateTimeStyles.None, out startDate);
                DateTime endDate;
                DateTime.TryParseExact(toDate, InputDateFormatsAllowed, myCulture, DateTimeStyles.None, out endDate);

                whereClause += " AND CAST(Retailer.SurveyorActivityDateTime AS Date) Between '" + startDate.ToString("yyyy-MM-dd") + "' and '" + endDate.ToString("yyyy-MM-dd") + "'";

                whereClauseForSearchCount += " AND CAST(Retailer.SurveyorActivityDateTime AS Date) Between '" + startDate.ToString("yyyy-MM-dd") + "' and '" + endDate.ToString("yyyy-MM-dd") + "'";
            }

            sqlSelectRetailers += whereClause + "  ORDER BY Retailer.SurveyorActivityDateTime DESC";

            sqlSearchCountLog += whereClauseForSearchCount;

            DataTable dtRetailers = new DataTable();
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                using (SqlCommand command = new SqlCommand(sqlSelectRetailers, connection))
                {
                    using (SqlDataAdapter da = new SqlDataAdapter(command))
                    {
                        connection.Open();
                        da.Fill(dtRetailers);

                        command.CommandText = "Insert Into RetailerAppearance(RetailerId,MoId) " + sqlSearchCountLog;
                        command.ExecuteNonQuery();
                        connection.Close();

                        command.CommandText = sqlInsertMoLog;
                        command.Parameters.Clear();
                        command.Parameters.Add("@MoId", SqlDbType.Int).Value = MoId;
                        command.Parameters.Add("@LogDescription", SqlDbType.VarChar).Value = "Viewed other retailers done by Surveyor-" + srId ;
                        connection.Open(); command.ExecuteNonQuery(); connection.Close();
                    }
                }
            }

            //--> View Bag
            ViewBag.MoId = MoId.ToString();
            ViewBag.UserName = Request.QueryString["user"].ToString(); //LoginName
            ViewBag.Retailers = dtRetailers;
            ViewBag.Title = "Updated retailers  ";
            ViewBag.MyCulture = myCulture;
            //<-- View Bag

            return View("OutletList");
        }

        //SR to Retailer count. It shows each SR-wise worked retailers count, From Retailer table
        public ActionResult SrToRetailersCount()
        {
          
            string strDate = Request.QueryString["date"].ToString();
            DataTable dtRetailers = new DataTable();
            String connectionString = WebConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;
            String sqlSelectRetailers = @"SELECT S.LoginName, S.SurveyorId, S.SurveyorName, S.ContactNo,  S.AreaName, Count( R.RetailerId) as WorkQuantity
                                         FROM dbo.Surveyors AS S INNER JOIN dbo.Retailer AS R ON S.SurveyorId = R.SurveyorId where Cast(R.SurveyorActivityDateTime as Date)=@Date group by S.SurveyorId, S.SurveyorName, S.ContactNo, S.LoginName, S.AreaName order by S.SurveyorId ASC";

            DateTime date;
            DateTime.TryParseExact(strDate, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out date);
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(sqlSelectRetailers, connection))
                {
                    using (SqlDataAdapter da = new SqlDataAdapter(command))
                    {
                        connection.Open();
                        command.Parameters.Clear();
                        command.Parameters.AddWithValue("@Date", date);
                        da.Fill(dtRetailers); connection.Close();
                    }
                }
            }

            ViewBag.UserName = Request.QueryString["user"].ToString();
            ViewBag.QueryDate = date.ToString("yyyy-MM-dd");
            ViewBag.Retailers = dtRetailers;
            ViewBag.Title = "Date-wise Surveyor's Activity Count : " + dtRetailers.Rows.Count.ToString() + " SR(s) found at field on "+ date.ToString("dd MMM yyyy") +".";
            return View("SrToRetailersCount");
        }
	}
}