using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;

namespace RobiPosMapper.Areas.RobiAdmin.Controllers
{
    public class SurveyorManagementController : Controller
    {
        CultureInfo ci = new CultureInfo("bn-BD-robi");

        String ConnectionString =  ConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;
        string sqlInsertMoLog = "INSERT INTO MonitoringOfficerActivityLog(MoId,LogDescription) VALUES (@MoId,@LogDescription)";
       

        public ActionResult Index()
        {

            Int32 MoId = Convert.ToInt32(Request.QueryString["MoId"].ToString()); //Monitoring Officer Id
            ViewBag.MoId = MoId;
            ViewBag.UserName = Request.QueryString["user"].ToString(); //LoginName
            ViewBag.Title = "Surveyor Management";
            ViewBag.ActiveMenuName = "Surveyor Management";
            DataTable tblSurveyorRetailerOfToday = new DataTable(); //Ajke kon Surveyor kotota kaj korlo tar data
            DataTable tblSurveyorsQuantityPerDate = new DataTable(); //protidin koto gulo Surveyor field-e chhilo tar data
            int activeSrQuantity = 0;
            DataTable tblTodayNotWorkingSrList = new DataTable(); //those who are not working today
            DataTable tblSurveyorAverageData = new DataTable();  //Data for showing top10 and bottom10 sr

            //Ajke Kon Surveyor kotota kaj korlo tar data
            string sqlSurveyorRetailerOfToday = @"SELECT     dbo.Surveyors.SurveyorId,
(select Count(RetailerId) from Retailer Where (CAST(SurveyorActivityDateTime AS Date)='"+ DateTime.Today.Date.ToString("yyyy-MM-dd") + @"') AND SurveyorId= dbo.Surveyors.SurveyorId) aS RetailerQuantity

FROM         dbo.Region INNER JOIN
                      dbo.MonitoringOfficerRegion ON dbo.Region.RegionId = dbo.MonitoringOfficerRegion.RegionId INNER JOIN
                      dbo.Surveyors ON dbo.Region.RegionId = dbo.Surveyors.RegionId
WHERE     (dbo.MonitoringOfficerRegion.MoId = " + MoId + ") AND (dbo.Surveyors.IsActive = 1) AND (dbo.Surveyors.SurveyorId <> 419) AND (dbo.Surveyors.SurveyorId IN (select SurveyorId from Retailer Where CAST(SurveyorActivityDateTime AS Date)='" + DateTime.Today.Date.ToString("yyyy-MM-dd") + @"'))
Order By dbo.Surveyors.SurveyorId Desc";

            //protidin koto gulo Surveyor field-e chhilo tar data
            string sqlSurveyorsQuantityPerDate = @"SELECT COUNT(Distinct(R.SurveyorId)) as SRQuantity,Cast(R.SurveyorActivityDateTime as Date) AS WorkDate
                                                       FROM dbo.Retailer AS R INNER JOIN
                                                            dbo.Surveyors AS S ON R.SurveyorId = S.SurveyorId INNER JOIN
                                                            dbo.MonitoringOfficerRegion AS M ON S.RegionId = M.RegionId
                                                      Where  Cast(R.SurveyorActivityDateTime as Date)>'2015-05-01' AND M.MoId=" + MoId + @"
                                                      Group By Cast(R.SurveyorActivityDateTime as Date)
                                                      Order By Cast(R.SurveyorActivityDateTime as Date) DESC";
            //Active Surveyrors of this MO
            string sqlActiveSrQuantity = @"SELECT COUNT(dbo.Surveyors.SurveyorId) AS ActiveSrQuantity
                                           FROM dbo.MonitoringOfficerRegion INNER JOIN
                                                 dbo.Surveyors ON dbo.MonitoringOfficerRegion.RegionId = dbo.Surveyors.RegionId
                                           WHERE  dbo.Surveyors.SurveyorId<>419 AND (dbo.MonitoringOfficerRegion.MoId = " + MoId + ") AND (dbo.Surveyors.IsActive = 1)";
            //Those who are not working today
            string sqlTodayNotWorkingSrList = @"SELECT dbo.Surveyors.SurveyorId
                                                FROM  dbo.MonitoringOfficerRegion INNER JOIN
                                                     dbo.Surveyors ON dbo.MonitoringOfficerRegion.RegionId = dbo.Surveyors.RegionId
                                                WHERE (dbo.Surveyors.SurveyorId<>419) AND (dbo.MonitoringOfficerRegion.MoId = " + MoId + @") AND (dbo.Surveyors.IsActive = 1) AND (dbo.Surveyors.SurveyorId NOT IN
                          (SELECT     SurveyorId
                            FROM          dbo.Retailer
                            WHERE      (CAST(SurveyorActivityDateTime AS Date) = '" + DateTime.Today.Date.ToString("yyyy-MM-dd") +"')))";

            //Surveyor average data
            string sqlSurveyorAverageData = @"SELECT dbo.Surveyors.SurveyorId, dbo.Surveyors.SurveyorName, 
                                                (Select Count(RetailerId) from Retailer Where SurveyorId=dbo.Surveyors.SurveyorId) AS WorkQuantity,
                                                (Select  Count(Distinct CAST(SurveyorActivityDateTime AS Date)) from Retailer Where SurveyorId=dbo.Surveyors.SurveyorId) AS DayQuantity
                                             FROM dbo.Surveyors INNER JOIN
                                                 dbo.MonitoringOfficerRegion ON dbo.Surveyors.RegionId = dbo.MonitoringOfficerRegion.RegionId
                                             WHERE (dbo.MonitoringOfficerRegion.MoId = "+ MoId +") and dbo.Surveyors.IsActive=1";

            using (SqlConnection connection=new SqlConnection(ConnectionString))
            {
                using (SqlCommand command=new SqlCommand(sqlInsertMoLog,connection))
                {
                    command.Parameters.Clear();
                    command.Parameters.Add("@MoId", SqlDbType.Int).Value = Convert.ToInt32(Request.QueryString["MoId"]);
                    command.Parameters.Add("@LogDescription", SqlDbType.VarChar).Value = "Clicked on Surveyor Management menu";
                    connection.Open(); command.ExecuteNonQuery();
                    command.Parameters.Clear();
                    command.CommandText = sqlSurveyorRetailerOfToday;
                    using (SqlDataAdapter da=new SqlDataAdapter(command))
                    {
                        da.Fill(tblSurveyorRetailerOfToday);
                        command.CommandText = sqlSurveyorsQuantityPerDate; da.Fill(tblSurveyorsQuantityPerDate);
                        command.CommandText = sqlTodayNotWorkingSrList; da.Fill(tblTodayNotWorkingSrList);
                        command.CommandText = sqlSurveyorAverageData; da.Fill(tblSurveyorAverageData); //Surveyor average data
                    }

                    command.CommandText = sqlActiveSrQuantity; activeSrQuantity = Convert.ToInt32(command.ExecuteScalar());
                    connection.Close();
                }
            }

            //--> protidin koto gulo Surveyor field-e chhilo tar graph data
            string[] surveyorWorkDateLabels = new string[tblSurveyorsQuantityPerDate.Rows.Count];
            for (int i = tblSurveyorsQuantityPerDate.Rows.Count - 1; i > 0; i--)
            {
                surveyorWorkDateLabels[i] = Convert.ToDateTime(tblSurveyorsQuantityPerDate.Rows[i]["WorkDate"]).ToString("dd-MMM");
            }

            surveyorWorkDateLabels[0] = Convert.ToDateTime(tblSurveyorsQuantityPerDate.Rows[0]["WorkDate"]).ToString("dd-MMM");

            int[] surveyorQuantityData = new int[tblSurveyorsQuantityPerDate.Rows.Count];
            int totalSurveyorQuantity = 0;
            for (int i = tblSurveyorsQuantityPerDate.Rows.Count - 1; i > 0; i--)
            {
                surveyorQuantityData[i] = Convert.ToInt32(tblSurveyorsQuantityPerDate.Rows[i]["SRQuantity"]);
                totalSurveyorQuantity += Convert.ToInt32(tblSurveyorsQuantityPerDate.Rows[i]["SRQuantity"]);
            }

            surveyorQuantityData[0] = Convert.ToInt32(tblSurveyorsQuantityPerDate.Rows[0]["SRQuantity"]);
            totalSurveyorQuantity += Convert.ToInt32(tblSurveyorsQuantityPerDate.Rows[0]["SRQuantity"]);
            int avgSurveyorPerDate = totalSurveyorQuantity / tblSurveyorsQuantityPerDate.Rows.Count;
            string avgSrPerDate = string.Format("Average Presence on Field: {0} surveyors/day.", avgSurveyorPerDate);
            //<--  protidin koto gulo Surveyor field-e chhilo tar graph data

            //--> Ascending surveyors todays updated retailer data
            DataView view = tblSurveyorRetailerOfToday.DefaultView;
            view.Sort = "RetailerQuantity ASC";
            
            DataTable sortedWorkProgressOfToday = view.ToTable();

            //--> Ascending Surveyors Average Data
            tblSurveyorAverageData.Columns.Add("Average", typeof(double));

            foreach (System.Data.DataRow row in tblSurveyorAverageData.Rows)
            {
                double avg = 0;
                int workQuantity = Convert.ToInt32(row["WorkQuantity"]);
                int dayQuantity = Convert.ToInt32(row["DayQuantity"]);
                if (dayQuantity > 0)
                {
                    avg = Convert.ToDouble(workQuantity/dayQuantity);
                }
                row["Average"] = Math.Round(avg, 2);
            }

            DataView averageView = tblSurveyorAverageData.DefaultView;
            averageView.Sort = "Average DESC";
           // averageView.RowFilter = "DayQuantity > 3";
            DataTable sortedSurveyorAverageData = averageView.ToTable();
            //<-- Ascending Surveyors Average Data

            ViewBag.MyCulture = ci;
            ViewBag.SurveyorRetailerOfToday = sortedWorkProgressOfToday; // tblSurveyorRetailerOfToday;
            ViewBag.AvgSrPerDate = avgSrPerDate;
            ViewBag.SurveyorWorkDateLabels = surveyorWorkDateLabels;
            ViewBag.SurveyorQuantityData = surveyorQuantityData;
            ViewBag.ActiveSrQuantity = activeSrQuantity;
            ViewBag.TodayNotWorkingSrList = tblTodayNotWorkingSrList;
            ViewBag.SurveyorAverageData = sortedSurveyorAverageData;
            return View("SurveyorManagement");
        }

        public ActionResult AddNewSurveyor()
        {
            Int32 moId=Convert.ToInt32( Request.QueryString["MoId"]); //Monitoring Officer Id
            DataTable tblRegions = new DataTable();
            String sqlSelectRegion = @"SELECT Region.RegionId, dbo.Region.RegionName
                                   FROM dbo.Region INNER JOIN
                                         dbo.MonitoringOfficerRegion ON dbo.Region.RegionId = dbo.MonitoringOfficerRegion.RegionId
                                   WHERE (dbo.MonitoringOfficerRegion.MoId = " + moId + ") ORDER BY dbo.Region.RegionName";
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = sqlSelectRegion;
                    using (SqlDataAdapter da = new SqlDataAdapter(command))
                    {
                        connection.Open(); da.Fill(tblRegions); connection.Close();
                    }

                    command.CommandText = sqlInsertMoLog;
                    command.Parameters.Clear();
                    command.Parameters.Add("@MoId", SqlDbType.Int).Value = moId;
                    command.Parameters.Add("@LogDescription", SqlDbType.VarChar).Value = "Clicked on Add New Surveyor";
                    connection.Open(); command.ExecuteNonQuery(); connection.Close();
                }
            }

            ViewBag.MoId = moId;
            ViewBag.UserName = Request.QueryString["user"].ToString(); //LoginName
            ViewBag.Title = "Add New Surveyor"; ViewBag.ActiveMenuName = "Surveyor Management";
            ViewBag.Regions = tblRegions;
            return View();
        }

        [HttpPost]
        public ActionResult AddNewSurveyor(FormCollection data)
        {
            Int32 moId = Convert.ToInt32(Request.QueryString["MoId"]); //Monitoring Officer Id
            String fullName = data["FullName"].ToString().Trim();
            String contactNo = data["ContactNo"].ToString().Trim();
            Int32 regionId = Convert.ToInt32(data["RegionId"]);
            String description = data["Description"].ToString().Trim();

            Random rand = new Random();
            Int32 loginPassword = rand.Next(100000, 999999);

            String sqlInsert = @"INSERT INTO Surveyors(SurveyorName,ContactNo,LoginPassword,RegionId,Description) 
                                 VALUES(@SurveyorName,@ContactNo,@LoginPassword,@RegionId,@Description); select SCOPE_IDENTITY();";

            String sqlUpdate = "UPDATE Surveyors SET LoginName=@LoginName  WHERE SurveyorId=@SurveyorId";

            String sqlSelectRegion = @"SELECT RegionName FROM dbo.Region WHERE RegionId="+ regionId +"";

            Object autoIdValue; DataTable tblRegions = new DataTable();
            String regionName = "";
            using (SqlConnection connection=new SqlConnection(ConnectionString))
            {
                using (SqlCommand command=new SqlCommand(sqlInsert,connection))
                {
                    command.Parameters.Clear();
                    command.Parameters.Add("@SurveyorName", SqlDbType.VarChar).Value = fullName;
                    command.Parameters.Add("@ContactNo", SqlDbType.VarChar).Value = contactNo;
                    command.Parameters.Add("@LoginPassword", SqlDbType.VarChar).Value = loginPassword.ToString();
                    command.Parameters.Add("@RegionId", SqlDbType.Int).Value = regionId;
                    command.Parameters.Add("@Description", SqlDbType.VarChar).Value = description;
                    connection.Open(); autoIdValue = command.ExecuteScalar();
                    command.Parameters.Clear();
                    command.CommandText = sqlUpdate;
                    command.Parameters.Add("@LoginName", SqlDbType.VarChar).Value = "sr" + autoIdValue.ToString();
                    command.Parameters.Add("@SurveyorId", SqlDbType.Int).Value = Convert.ToInt32(autoIdValue);
                    command.ExecuteNonQuery();

                    command.CommandText = sqlSelectRegion; regionName = command.ExecuteScalar().ToString();
                    connection.Close();


                    command.CommandText = sqlInsertMoLog;
                    command.Parameters.Clear();
                    command.Parameters.Add("@MoId", SqlDbType.Int).Value = moId;
                    command.Parameters.Add("@LogDescription", SqlDbType.VarChar).Value = "Added new surveyor- " + autoIdValue.ToString();
                    connection.Open(); command.ExecuteNonQuery(); connection.Close();

                }
            }

            ViewBag.MoId = moId;
            ViewBag.UserName = Request.QueryString["user"].ToString(); //LoginName
            ViewBag.Title = "Add New Surveyor";
            ViewBag.Region = regionName;
            ViewBag.FullName = fullName; ViewBag.ContactNo = contactNo; ViewBag.RegionId = regionId; ViewBag.LoginName = "SR" + autoIdValue.ToString(); ViewBag.LoginPassword = loginPassword;
            ViewBag.Description = description;
            ViewBag.ActiveMenuName = "Surveyor Management";
            return View("SurveyorAdded");
        }

        public ActionResult ShowActiveSurveyors()
        {
            Int32 moId= Convert.ToInt32( Request.QueryString["moid"].ToString()); //Monitoring Officer Id
            DataTable dtSurveyors = new DataTable();
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = @"SELECT S.SurveyorId, S.LoginName, S.SurveyorName, S.ContactNo, S.Description, R.RegionName, S.IsActive, 
                                                (SELECT COUNT(RetailerId) FROM Retailer WHERE SurveyorId=S.SurveyorId AND RetailerStatusId=2) AS UpdatedQuantity,
                                                (SELECT COUNT(RetailerId) FROM Retailer WHERE SurveyorId=S.SurveyorId AND RetailerStatusId=3) AS NewQuantity,
                                                (SELECT COUNT(RetailerId) FROM Retailer WHERE SurveyorId=S.SurveyorId AND IsVerifiedByRsp=1) AS VerifiedQuantity,
                                                (SELECT COUNT(DISTINCT RA.RetailerId) FROM dbo.Retailer AS R INNER JOIN dbo.RetailerAppearance AS RA ON R.RetailerId = RA.RetailerId
                                                    WHERE (R.SurveyorId = S.SurveyorId) GROUP BY R.SurveyorId) AS ViewedQuantity,

                                                (SELECT COUNT(RetailerId) FROM Retailer WHERE SurveyorId=S.SurveyorId AND RetailerStatusId=2 AND Cast(SurveyorActivityDateTime AS Date)='" + DateTime.Today.Date.ToString("yyyy-MM-dd") + @"') AS TodaysUpdatedQuantity,
                                                (SELECT COUNT(RetailerId) FROM Retailer WHERE SurveyorId=S.SurveyorId AND RetailerStatusId=3 AND Cast(SurveyorActivityDateTime AS Date)='" + DateTime.Today.Date.ToString("yyyy-MM-dd") + @"') AS TodaysNewQuantity,
                                                (SELECT COUNT(RetailerId) FROM Retailer WHERE SurveyorId=S.SurveyorId AND IsVerifiedByRsp=1 AND Cast(SurveyorActivityDateTime AS Date)='" + DateTime.Today.Date.ToString("yyyy-MM-dd") + @"') AS TodaysVerifiedQuantity

                                           FROM dbo.MonitoringOfficerRegion AS MOR INNER JOIN
                                               dbo.Region AS R ON MOR.RegionId = R.RegionId INNER JOIN
                                               dbo.Surveyors AS S ON R.RegionId = S.RegionId
                                           WHERE     (MOR.MoId = " + moId + @") AND (S.IsActive = 1) AND (S.SurveyorId > 0)
                                           ORDER BY S.LoginName";
                    command.CommandTimeout = 60;
                
                    using (SqlDataAdapter da = new SqlDataAdapter(command))
                    {
                        connection.Open(); da.Fill(dtSurveyors); connection.Close();
                    }

                    command.CommandText = sqlInsertMoLog;
                    command.Parameters.Clear();
                    command.Parameters.Add("@MoId", SqlDbType.Int).Value = moId;
                    command.Parameters.Add("@LogDescription", SqlDbType.VarChar).Value = "Clicked on Show Active Surveyors";
                    connection.Open(); command.ExecuteNonQuery(); connection.Close();

                }
            }

            ViewBag.Surveyors = dtSurveyors;
            ViewBag.MoId = moId;
            ViewBag.UserName = Request.QueryString["user"].ToString(); //LoginName
            ViewBag.Title = "Show All Surveyors"; ViewBag.ActiveMenuName = "Surveyor Management";
            ViewBag.ShowInactiveImage = 1;
            ViewBag.ActiveInactiveText = "Active";
            return View("ShowActiveSurveyors");
        }




        public ActionResult SearchSrById(FormCollection data)
        {
            Int32 moId = Convert.ToInt32(Request.QueryString["moid"].ToString()); //Monitoring Officer Id
            Int32 srId = Convert.ToInt32(data["surveyorid"]);
            DataTable dtSurveyors = new DataTable();
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = @"SELECT S.SurveyorId, S.LoginName, S.SurveyorName, S.ContactNo, S.Description, R.RegionName, S.IsActive, 
                                                (SELECT COUNT(RetailerId) FROM Retailer WHERE SurveyorId=S.SurveyorId AND RetailerStatusId=2) AS UpdatedQuantity,
                                                (SELECT COUNT(RetailerId) FROM Retailer WHERE SurveyorId=S.SurveyorId AND RetailerStatusId=3) AS NewQuantity,
                                                (SELECT COUNT(RetailerId) FROM Retailer WHERE SurveyorId=S.SurveyorId AND IsVerifiedByRsp=1) AS VerifiedQuantity,
                                                (SELECT COUNT(DISTINCT RA.RetailerId) FROM dbo.Retailer AS R INNER JOIN dbo.RetailerAppearance AS RA ON R.RetailerId = RA.RetailerId
                                                    WHERE (R.SurveyorId = S.SurveyorId) GROUP BY R.SurveyorId) AS ViewedQuantity,

                                                (SELECT COUNT(RetailerId) FROM Retailer WHERE SurveyorId=S.SurveyorId AND RetailerStatusId=2 AND Cast(SurveyorActivityDateTime AS Date)='" + DateTime.Today.Date.ToString("yyyy-MM-dd") + @"') AS TodaysUpdatedQuantity,
                                                (SELECT COUNT(RetailerId) FROM Retailer WHERE SurveyorId=S.SurveyorId AND RetailerStatusId=3 AND Cast(SurveyorActivityDateTime AS Date)='" + DateTime.Today.Date.ToString("yyyy-MM-dd") + @"') AS TodaysNewQuantity,
                                                (SELECT COUNT(RetailerId) FROM Retailer WHERE SurveyorId=S.SurveyorId AND IsVerifiedByRsp=1 AND Cast(SurveyorActivityDateTime AS Date)='" + DateTime.Today.Date.ToString("yyyy-MM-dd") + @"') AS TodaysVerifiedQuantity

                                           FROM dbo.MonitoringOfficerRegion AS MOR INNER JOIN
                                               dbo.Region AS R ON MOR.RegionId = R.RegionId INNER JOIN
                                               dbo.Surveyors AS S ON R.RegionId = S.RegionId
                                           WHERE     (MOR.MoId = " + moId + @") AND (S.SurveyorId = "+ srId + @")
                                           ORDER BY S.LoginName";
                    command.CommandTimeout = 60;

                    using (SqlDataAdapter da = new SqlDataAdapter(command))
                    {
                        connection.Open(); da.Fill(dtSurveyors); connection.Close();
                    }

                    command.CommandText = sqlInsertMoLog;
                    command.Parameters.Clear();
                    command.Parameters.Add("@MoId", SqlDbType.Int).Value = moId;
                    command.Parameters.Add("@LogDescription", SqlDbType.VarChar).Value = "Quick Search for SR-" + srId + "";
                    connection.Open(); command.ExecuteNonQuery(); connection.Close();

                }
            }

            ViewBag.Surveyors = dtSurveyors;
            ViewBag.MoId = moId;
            ViewBag.UserName = Request.QueryString["user"].ToString(); //LoginName
            ViewBag.Title = "Show Individual Surveyor";
            ViewBag.ActiveMenuName = "Surveyor Management";
            if (Convert.ToBoolean(dtSurveyors.Rows[0]["IsActive"]))
            {
                ViewBag.ShowInactiveImage = 1;
                ViewBag.ActiveInactiveText = "Active";
            }
            else
            {
                ViewBag.ShowInactiveImage = 0;
                ViewBag.ActiveInactiveText = "Inactive";
            }
            return View("ShowActiveSurveyors");
        }


        public ActionResult ShowAll()
        {
            Int32 moId = Convert.ToInt32(Request.QueryString["moid"].ToString()); //Monitoring Officer Id
            DataTable dtSurveyors = new DataTable();
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = @"SELECT S.SurveyorId, S.LoginName, S.SurveyorName, S.ContactNo, S.Description, R.RegionName, S.IsActive, 
                                                (SELECT COUNT(RetailerId) FROM Retailer WHERE SurveyorId=S.SurveyorId AND RetailerStatusId=2) AS UpdatedQuantity,
                                                (SELECT COUNT(RetailerId) FROM Retailer WHERE SurveyorId=S.SurveyorId AND RetailerStatusId=3) AS NewQuantity,
                                                (SELECT COUNT(RetailerId) FROM Retailer WHERE SurveyorId=S.SurveyorId AND IsVerifiedByRsp=1) AS VerifiedQuantity,
                                                (SELECT COUNT(DISTINCT RA.RetailerId) FROM dbo.Retailer AS R INNER JOIN dbo.RetailerAppearance AS RA ON R.RetailerId = RA.RetailerId
                                                    WHERE (R.SurveyorId = S.SurveyorId) GROUP BY R.SurveyorId) AS ViewedQuantity,

                                                (SELECT COUNT(RetailerId) FROM Retailer WHERE SurveyorId=S.SurveyorId AND RetailerStatusId=2 AND Cast(SurveyorActivityDateTime AS Date)='" + DateTime.Today.Date.ToString("yyyy-MM-dd") + @"') AS TodaysUpdatedQuantity,
                                                (SELECT COUNT(RetailerId) FROM Retailer WHERE SurveyorId=S.SurveyorId AND RetailerStatusId=3 AND Cast(SurveyorActivityDateTime AS Date)='" + DateTime.Today.Date.ToString("yyyy-MM-dd") + @"') AS TodaysNewQuantity,
                                                (SELECT COUNT(RetailerId) FROM Retailer WHERE SurveyorId=S.SurveyorId AND IsVerifiedByRsp=1 AND Cast(SurveyorActivityDateTime AS Date)='" + DateTime.Today.Date.ToString("yyyy-MM-dd") + @"') AS TodaysVerifiedQuantity

                                           FROM dbo.MonitoringOfficerRegion AS MOR INNER JOIN
                                               dbo.Region AS R ON MOR.RegionId = R.RegionId INNER JOIN
                                               dbo.Surveyors AS S ON R.RegionId = S.RegionId
                                           WHERE     (MOR.MoId = " + moId + @") AND (S.IsActive = 0) AND (S.SurveyorId > 0)
                                           ORDER BY S.LoginName";
                    command.CommandTimeout = 60;

                    using (SqlDataAdapter da = new SqlDataAdapter(command))
                    {
                        connection.Open(); da.Fill(dtSurveyors); connection.Close();
                    }

                    command.CommandText = sqlInsertMoLog;
                    command.Parameters.Clear();
                    command.Parameters.Add("@MoId", SqlDbType.Int).Value = moId;
                    command.Parameters.Add("@LogDescription", SqlDbType.VarChar).Value = "Clicked on Show Active Surveyors";
                    connection.Open(); command.ExecuteNonQuery(); connection.Close();

                }
            }

            ViewBag.Surveyors = dtSurveyors;
            ViewBag.MoId = moId;
            ViewBag.UserName = Request.QueryString["user"].ToString(); //LoginName
            ViewBag.Title = "Show All Surveyors"; ViewBag.ActiveMenuName = "Surveyor Management";
            ViewBag.ShowInactiveImage = 0;
            ViewBag.ActiveInactiveText = "Inactive";
            return View("ShowActiveSurveyors");
        }


        public ActionResult ShowAllOld()
        {
            Int32 moId = Convert.ToInt32(Request.QueryString["moid"].ToString()); //Monitoring Officer Id
            DataTable dtSurveyors = new DataTable();
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = connection;
                    if (Request.QueryString["srid"]==null)
                    {
                        command.CommandText = @"SELECT S.SurveyorId, S.LoginName, S.SurveyorName, S.ContactNo, S.Description, R.RegionName, S.IsActive, 
                                                 (SELECT COUNT(RetailerId) FROM Retailer WHERE SurveyorId=S.SurveyorId AND RetailerStatusId=2) AS UpdatedQuantity,
                                                 (SELECT COUNT(RetailerId) FROM Retailer WHERE SurveyorId=S.SurveyorId AND RetailerStatusId=3) AS NewQuantity
                                           FROM dbo.MonitoringOfficerRegion AS MOR INNER JOIN
                                              dbo.Region AS R ON MOR.RegionId = R.RegionId INNER JOIN
                                              dbo.Surveyors AS S ON R.RegionId = S.RegionId
                                              WHERE     (MOR.MoId = " + moId + @") AND (S.SurveyorId > 0)
                                              ORDER BY S.LoginName";
                    }

                    else
                    {
                        int srId = Convert.ToInt32(Request.QueryString["srid"]);
                        command.CommandText = @"SELECT S.SurveyorId, S.LoginName, S.SurveyorName, S.ContactNo, S.Description, R.RegionName, S.IsActive, 
                                                 (SELECT COUNT(RetailerId) FROM Retailer WHERE SurveyorId=S.SurveyorId AND RetailerStatusId=2) AS UpdatedQuantity,
                                                 (SELECT COUNT(RetailerId) FROM Retailer WHERE SurveyorId=S.SurveyorId AND RetailerStatusId=3) AS NewQuantity
                                           FROM dbo.MonitoringOfficerRegion AS MOR INNER JOIN
                                              dbo.Region AS R ON MOR.RegionId = R.RegionId INNER JOIN
                                              dbo.Surveyors AS S ON R.RegionId = S.RegionId
                                              WHERE     (MOR.MoId = " + moId + @") AND (S.SurveyorId = "+ srId + @")
                                              ORDER BY S.LoginName";
                    }
                    using (SqlDataAdapter da = new SqlDataAdapter(command))
                    {
                        connection.Open(); da.Fill(dtSurveyors); connection.Close();
                    }

                    command.CommandText = sqlInsertMoLog;
                    command.Parameters.Clear();
                    command.Parameters.Add("@MoId", SqlDbType.Int).Value = moId;
                    command.Parameters.Add("@LogDescription", SqlDbType.VarChar).Value = "Clicked on Show All Surveyors";
                    connection.Open(); command.ExecuteNonQuery(); connection.Close();
                }
            }

            ViewBag.Surveyors = dtSurveyors;
            ViewBag.MoId = moId;
            ViewBag.UserName = Request.QueryString["user"].ToString(); //LoginName
            ViewBag.Title = "Show All Surveyors";
            ViewBag.ActiveMenuName = "Surveyor Management";

            return View("ShowAll");
        }

        //This is an identical method of above. Comes from quick search
        public ActionResult SearchSrByIdOld(FormCollection data)
        {
            Int32 moId = Convert.ToInt32(Request.QueryString["moid"].ToString()); //Monitoring Officer Id
            Int32 srId = Convert.ToInt32(data["surveyorid"]);
            DataTable dtSurveyors = new DataTable();
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = @"SELECT S.SurveyorId, S.LoginName, S.SurveyorName, S.ContactNo, S.Description, R.RegionName, S.IsActive, 

(SELECT COUNT(RetailerId) FROM Retailer WHERE SurveyorId=S.SurveyorId AND RetailerStatusId=2) AS UpdatedQuantity,

(SELECT COUNT(RetailerId) FROM Retailer WHERE SurveyorId=S.SurveyorId AND RetailerStatusId=3) AS NewQuantity


                                            FROM dbo.MonitoringOfficerRegion AS MOR INNER JOIN
                                           dbo.Region AS R ON MOR.RegionId = R.RegionId INNER JOIN
                                           dbo.Surveyors AS S ON R.RegionId = S.RegionId
                                           WHERE     (MOR.MoId = " + moId + @") AND (S.SurveyorId = "+ srId + @")
                                           ORDER BY S.LoginName";
                    using (SqlDataAdapter da = new SqlDataAdapter(command))
                    {
                        connection.Open(); da.Fill(dtSurveyors); connection.Close();
                    }

                    command.CommandText = sqlInsertMoLog;
                    command.Parameters.Clear();
                    command.Parameters.Add("@MoId", SqlDbType.Int).Value = moId;
                    command.Parameters.Add("@LogDescription", SqlDbType.VarChar).Value = "Quick Search for SR-"+ srId +"";
                    connection.Open(); command.ExecuteNonQuery(); connection.Close();

                }
            }

            ViewBag.Surveyors = dtSurveyors;
            ViewBag.MoId = moId;
            ViewBag.UserName = Request.QueryString["user"].ToString(); //LoginName
            ViewBag.Title = "Show All Surveyors";
            ViewBag.ActiveMenuName = "Surveyor Management";
            return View("ShowAll");
        }

        public ActionResult DeactivateSurveyor()
        {
            String userName = Request.QueryString["user"].ToString().Trim();
            string moid = Request.QueryString["moid"].ToString().Trim();
            int surveyorId = Convert.ToInt32(Request.QueryString["srid"].Trim());

            using (SqlConnection connection=new SqlConnection(ConnectionString))
            {
                using (SqlCommand command=new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = "UPDATE Surveyors SET IsActive=0,DeactivationDate='"+ DateTime.Now +"' WHERE SurveyorId="+ surveyorId +"";
                    connection.Open(); command.ExecuteNonQuery(); connection.Close();


                    command.CommandText = sqlInsertMoLog;
                    command.Parameters.Clear();
                    command.Parameters.Add("@MoId", SqlDbType.Int).Value =moid;
                    command.Parameters.Add("@LogDescription", SqlDbType.VarChar).Value = "Deactivated Surveyor-" + surveyorId.ToString();
                    connection.Open(); command.ExecuteNonQuery(); connection.Close();

                }
            }

           return RedirectToAction("ShowActiveSurveyors", new {user=userName,moid=moid });
        }
	}
}