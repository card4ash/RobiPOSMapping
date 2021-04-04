using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Data;
using System.Data.OleDb;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Collections;
using RobiPosMapper.Areas.RSP.Models;
using Newtonsoft.Json;
using System.Configuration;
using sCommonLib;
using System.Globalization;

namespace RobiPosMapper.Areas.RSP.Controllers
{
    public class HomeController : Controller
    {
        CultureInfo ci = new CultureInfo("bn-BD-robi");
        public ActionResult Index()
        {
            ViewBag.RspName = Request.QueryString["rsp"].ToString();
            ViewBag.RspId = Request.QueryString["rspid"].ToString();
            ViewBag.LoginName = Request.QueryString["user"].ToString();
            return View();
        }

        //show charts, graphs etc
        public ActionResult DefaultContent()
        {

            DateTime startDateTime = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, 00, 00, 01);
            DateTime endDateTime = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, 23, 59, 59);

           // Int32 rspId = Convert.ToInt32( Session["RspId"].ToString());

            Int32 rspId = Convert.ToInt32(Request.QueryString["rspid"].ToString());


            String ConnectionString = ConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;

            DataTable dtSurveyors = new DataTable(); // only of current rsp
            DataTable dtRetailers = new DataTable(); // only of current rsp
            DataTable tblRetailersQuantityPerDate = new DataTable();

            String sqlTotalOutletsOfThisRsp = @"SELECT Count(RetailerId) AS RetailerCount FROM Retailer where RspId="+ rspId +" and RetailerStatusId<3";
            int totalOutlets = 0;

            string sqlTotalUpdatedRetailers = "SELECT Count(RetailerId) AS RetailerCount FROM Retailer where RspId="+ rspId +" and RetailerStatusId = 2 AND IsActive = 1";
            int updatedOutlets = 0;

            string sqlTotalNewRetailers = "SELECT Count(RetailerId) AS RetailerCount FROM Retailer where RspId=" + rspId + " and RetailerStatusId = 3 AND IsActive = 1";
            int newOutlets = 0;

            string sqlTotalNotFoundRetailers = "SELECT Count(RetailerId) AS RetailerCount FROM Retailer where RspId=" + rspId + " AND IsActive = 0";
            int notFoundOutlets = 0;

            //updated today
            string sqlTodayUpdatedRetailers = "SELECT Count(RetailerId) AS RetailerCount FROM Retailer where Cast(SurveyorActivityDateTime as DATE)='" + DateTime.Today.Date.ToString("yyyy-MM-dd") + "' and RspId=" + rspId + " and RetailerStatusId = 2 AND IsActive = 1";
            int todayUpdatedOutlets = 0;

            //New today
            string sqlTodayNewRetailers = "SELECT Count(RetailerId) AS RetailerCount FROM Retailer where Cast(SurveyorActivityDateTime as DATE)='" + DateTime.Today.Date.ToString("yyyy-MM-dd") + "' and RspId=" + rspId + " and RetailerStatusId = 3 AND IsActive = 1";
            int todayNewOutlets = 0;


            string sqlTodayNotFoundRetailers = "SELECT Count(RetailerId) AS RetailerCount FROM Retailer where (Cast(InactiveDateTime as DATE)='" + DateTime.Today.Date.ToString("yyyy-MM-dd") + "') AND RspId=" + rspId + " AND IsActive = 0";
            int todayNotFoundOutlets = 0;

            //protidin koto gulo retailer-e kaj hoechhe tar data
            string sqlRetailersQuantityPerDate = @"SELECT  COUNT(R.RetailerId) as RetailerQuantity, Cast(R.SurveyorActivityDateTime as Date) AS WorkDate
                                                       FROM dbo.Retailer AS R 
                                                       Where  Cast(R.SurveyorActivityDateTime as Date)>'2015-05-01' AND R.RspId=" + rspId + @"
                                                       Group By Cast(R.SurveyorActivityDateTime as Date)
                                                       Order By Cast(R.SurveyorActivityDateTime as Date) DESC";

            //--> this data is requierd for DSR-wise graph
            string sqlSelectDistinctDsr = "Select Distinct DsrId from Retailer Where RspId="+ rspId +"";
            DataTable tblDistinctDsr = new DataTable();
            DataTable tblGraphData = new DataTable();

            
            tblGraphData.Columns.Add("DsrName", typeof(System.String));
            tblGraphData.Columns.Add("TotalRetailers", typeof(System.Int32));
            tblGraphData.Columns.Add("UpdatedRetailers", typeof(System.Int32));
            tblGraphData.Columns.Add("SortOrder", typeof(System.Int32));
            tblGraphData.Columns.Add("DsrId", typeof(System.Int32));
            //<-- this data is requierd for DSR-wise graph

            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    using (SqlCommand command=new SqlCommand())
                    {
                        command.Connection = connection;
                        using (SqlDataAdapter da=new SqlDataAdapter(command))
                        {
                            string sqlSelectDistinctSr = "SELECT DISTINCT R.SurveyorId, S.SurveyorName, S.LoginName, S.AreaName FROM Surveyors AS S INNER JOIN  dbo.Retailer AS R ON S.SurveyorId = R.SurveyorId WHERE S.SurveyorId>0 AND R.RspId = "+ rspId +" ORDER BY R.SurveyorId";
                            command.CommandText = sqlSelectDistinctSr;
                            connection.Open();
                            da.Fill(dtSurveyors);
                            command.CommandText = "select RetailerId,RetailerStatusId, IsReevaluated, SurveyorId,Cast(SurveyorActivityDateTime as DATE) as SurveyorDate from Retailer where RspId=" + rspId + "";

                            da.Fill(dtRetailers);
                            command.CommandText = sqlTotalOutletsOfThisRsp; totalOutlets = Convert.ToInt32(command.ExecuteScalar());
                            command.CommandText = sqlTotalUpdatedRetailers; updatedOutlets = Convert.ToInt32(command.ExecuteScalar());
                            command.CommandText = sqlTotalNewRetailers; newOutlets = Convert.ToInt32(command.ExecuteScalar());
                            command.CommandText = sqlTotalNotFoundRetailers; notFoundOutlets = Convert.ToInt32(command.ExecuteScalar());
                            //protidin koto gulo retailer-e kaj hoechhe tar data
                            command.CommandText = sqlRetailersQuantityPerDate; da.Fill(tblRetailersQuantityPerDate);

                            command.CommandText = sqlSelectDistinctDsr; da.Fill(tblDistinctDsr);
                            int rowCount = 1;
                            foreach (DataRow rowOne in tblDistinctDsr.Rows)
                            {
                                int dsrId = Convert.ToInt32(rowOne[0]);
                                string selectDsrName = "Select PersonName,SortOrder from Person Where PersonId=" + dsrId + "";
                                command.CommandText = selectDsrName;
                                DataTable tblDsrNameAndSortOrder = new DataTable();
                                da.Fill(tblDsrNameAndSortOrder);
                                string dsrName = tblDsrNameAndSortOrder.Rows[0][0].ToString();
                                Int32 sortOrder = Convert.ToInt32(tblDsrNameAndSortOrder.Rows[0][1]);

                                command.CommandText = "SELECT COUNT(RetailerId) AS TotalRetailers FROM  dbo.Retailer WHERE (RspId="+ rspId +") AND (DsrId = "+ dsrId +") AND (RetailerStatusId < 3)";
                                Int32 totalRetailersOfDsr = Convert.ToInt32(command.ExecuteScalar());

                                command.CommandText = "SELECT COUNT(RetailerId) AS TotalRetailers FROM  dbo.Retailer WHERE (RspId=" + rspId + ") AND (DsrId = " + dsrId + ") AND (RetailerStatusId = 2) AND (IsActive=1)";
                                Int32 updatedRetailersOfDsr = Convert.ToInt32(command.ExecuteScalar());
                                command.CommandText = "SELECT COUNT(RetailerId) AS TotalRetailers FROM  dbo.Retailer WHERE (RspId=" + rspId + ") AND (DsrId = " + dsrId + ") AND (IsActive=0)";
                                Int32 notFoundRetailersOfDsr = Convert.ToInt32(command.ExecuteScalar());
                                DataRow newRow = tblGraphData.NewRow();

                                newRow["DsrId"] = dsrId;
                                newRow["DsrName"] = dsrName;
                                newRow["TotalRetailers"] = totalRetailersOfDsr;
                                newRow["UpdatedRetailers"] = updatedRetailersOfDsr;
                                newRow["SortOrder"] = sortOrder;
                                tblGraphData.Rows.Add(newRow);
                                rowCount++;
                            }

                            command.CommandText = sqlTodayUpdatedRetailers; todayUpdatedOutlets = Convert.ToInt32( command.ExecuteScalar());
                            command.CommandText = sqlTodayNewRetailers; todayNewOutlets = Convert.ToInt32(command.ExecuteScalar());
                            command.CommandText = sqlTodayNotFoundRetailers; todayNotFoundOutlets = Convert.ToInt32(command.ExecuteScalar());
                            connection.Close();
                        }
                    }
                }

                DataTable dtTodaysActivityOfSurveyors = new DataTable(); //Contains only today's data of a surveyor.
                dtTodaysActivityOfSurveyors.Columns.Add("SurveyorId", typeof(System.String));
                dtTodaysActivityOfSurveyors.Columns.Add("SurveyorName", typeof(System.String));
                dtTodaysActivityOfSurveyors.Columns.Add("LoginName", typeof(System.String));
                dtTodaysActivityOfSurveyors.Columns.Add("AreaName", typeof(System.String));
                dtTodaysActivityOfSurveyors.Columns.Add("Reevaluated", typeof(System.Int32));
                dtTodaysActivityOfSurveyors.Columns.Add("Updated", typeof(System.Int32));
                dtTodaysActivityOfSurveyors.Columns.Add("NewRetailer", typeof(System.Int32));
                dtTodaysActivityOfSurveyors.Columns.Add("Total", typeof(System.Int32));

                DataTable dtOverallActivityOfSurveyors = new DataTable(); //Contains life-time data of a surveyor.
                dtOverallActivityOfSurveyors.Columns.Add("SurveyorId", typeof(System.String));
                dtOverallActivityOfSurveyors.Columns.Add("SurveyorName", typeof(System.String));
                dtOverallActivityOfSurveyors.Columns.Add("LoginName", typeof(System.String));
                dtOverallActivityOfSurveyors.Columns.Add("AreaName", typeof(System.String));
                dtOverallActivityOfSurveyors.Columns.Add("Reevaluated", typeof(System.Int32));
                dtOverallActivityOfSurveyors.Columns.Add("Updated", typeof(System.Int32));
                dtOverallActivityOfSurveyors.Columns.Add("NewRetailer", typeof(System.Int32));
                dtOverallActivityOfSurveyors.Columns.Add("Total", typeof(System.Int32));

                DataRow row;
                String today = DateTime.Today.ToString("yyyy-MM-dd");
                //---> read every surveyor from distinct records
                foreach (DataRow surveyor in dtSurveyors.Rows)
                {
                    Int32 surveyorId = Convert.ToInt32(surveyor["SurveyorId"]);
                   
                    String surveyorName = surveyor["SurveyorName"].ToString();
                    String loginName = surveyor["LoginName"].ToString().ToUpper();
                    String areaName=surveyor["AreaName"].ToString();
                    DataRow[] isWorkedToday = dtRetailers.Select("SurveyorId="+ surveyorId +" and SurveyorDate='"+ today +"'");
                    if (isWorkedToday.Length>0)
                    { //---> calculate only todays data
                        row = AddAsTodaysRecord(dtRetailers, dtTodaysActivityOfSurveyors, today, surveyorId, surveyorName, loginName, areaName);
                        dtTodaysActivityOfSurveyors.Rows.Add(row);

                        row = AddAsOverallRecord(dtRetailers, dtOverallActivityOfSurveyors, surveyorId, surveyorName, loginName, areaName);
                        dtOverallActivityOfSurveyors.Rows.Add(row);
                    } //---> calculate only todays data
                    else
                    { //--> calculate all data
                        row = AddAsOverallRecord(dtRetailers, dtOverallActivityOfSurveyors, surveyorId, surveyorName, loginName, areaName);
                        dtOverallActivityOfSurveyors.Rows.Add(row);
                    } //<-- calculate all data
                }
                //<--- read every surveyor from distinct records

                //---> add grand total row today
                row = dtTodaysActivityOfSurveyors.NewRow();
                row["SurveyorId"] = String.Empty;
                row["SurveyorName"] = String.Empty;
                row["LoginName"] = String.Empty;
                row["AreaName"] = "Total";
                row["Reevaluated"] = dtTodaysActivityOfSurveyors.Compute("SUM(Reevaluated)", "");
                row["Updated"] = dtTodaysActivityOfSurveyors.Compute("SUM(Updated)", "");
                row["NewRetailer"] = dtTodaysActivityOfSurveyors.Compute("Sum(NewRetailer)", "");
                row["Total"] = dtTodaysActivityOfSurveyors.Compute("Sum(Total)", "");
                dtTodaysActivityOfSurveyors.Rows.Add(row);
                //---> add grand total row

                //---> add grand total row life time
                row = dtOverallActivityOfSurveyors.NewRow();
                row["SurveyorId"] = String.Empty;
                row["SurveyorName"] = String.Empty;
                row["LoginName"] = String.Empty;
                row["AreaName"] = "Total";
                row["Reevaluated"] = dtOverallActivityOfSurveyors.Compute("SUM(Reevaluated)", "");
                row["Updated"] = dtOverallActivityOfSurveyors.Compute("SUM(Updated)", "");
                row["NewRetailer"] = dtOverallActivityOfSurveyors.Compute("Sum(NewRetailer)", "");
                row["Total"] = dtOverallActivityOfSurveyors.Compute("Sum(Total)", "");
                dtOverallActivityOfSurveyors.Rows.Add(row);
                //---> add grand total row

                //Work prorgress chart
                string[] chartLabels = new string[tblRetailersQuantityPerDate.Rows.Count];
                int[] chartData = new int[tblRetailersQuantityPerDate.Rows.Count];
                String makeGraph = "false";
                if (tblRetailersQuantityPerDate.Rows.Count>0)
                {
                    for (int i = tblRetailersQuantityPerDate.Rows.Count - 1; i > 0; i--)
                    {
                        chartLabels[i] = Convert.ToDateTime(tblRetailersQuantityPerDate.Rows[i]["WorkDate"]).ToString("dd-MMM");
                    }

                    chartLabels[0] = Convert.ToDateTime(tblRetailersQuantityPerDate.Rows[0]["WorkDate"]).ToString("dd-MMM");


                    for (int i = tblRetailersQuantityPerDate.Rows.Count - 1; i > 0; i--)
                    {
                        chartData[i] = Convert.ToInt32(tblRetailersQuantityPerDate.Rows[i]["RetailerQuantity"]);
                    }

                    chartData[0] = Convert.ToInt32(tblRetailersQuantityPerDate.Rows[0]["RetailerQuantity"]);
                    makeGraph = "true";
                }
                


                ViewBag.MyCulture = ci;
                ViewBag.TodaysActivityOfSurveyors = dtTodaysActivityOfSurveyors;
                ViewBag.OverallActivityOfSurveyors = dtOverallActivityOfSurveyors;
                ViewBag.RspId = rspId;
                ViewBag.LoginName = Request.QueryString["user"].ToString();
                ViewBag.TotalOutlets = totalOutlets;
                ViewBag.UpdatedOutlets = updatedOutlets;
                ViewBag.NewOutlets = newOutlets;
                ViewBag.NotFoundOutlets = notFoundOutlets;
                ViewBag.PendingOutlets = totalOutlets - (updatedOutlets + notFoundOutlets);
                ViewBag.ChartLabel = chartLabels;
                ViewBag.ChartData = chartData;

                DataView viewAgerage = tblGraphData.DefaultView;
                viewAgerage.Sort = "SortOrder ASC, DsrName ASC";
                DataTable sortedDsrGraphData = viewAgerage.ToTable();
                ViewBag.GraphData = sortedDsrGraphData;

                ViewBag.TodayUpdatedRetailers= todayUpdatedOutlets ;
                ViewBag.TodayNewRetailers = todayNewOutlets;
                ViewBag.TodayNotFoundRetailers= todayNotFoundOutlets ;

                ViewBag.MakeGraph = makeGraph;
                return View("DefaultContent");
            }
            catch (Exception ex)
            {
                var path = HttpContext.Server.MapPath("~/App_Data");
                ExceptionLogger.CreateLog(ex, path);
                return View("ErrorContent");
            }
        }

        private static DataRow AddAsOverallRecord(DataTable dtRetailers, DataTable dtOverallActivityOfSurveyors, Int32 surveyorId, String surveyorName, String loginName, String areaName)
        {
            DataRow row;
            row = dtOverallActivityOfSurveyors.NewRow();
            row["SurveyorId"] = surveyorId.ToString();
            row["SurveyorName"] = surveyorName;
            row["LoginName"] = loginName;
            row["AreaName"] = areaName;
            Int32 reevaluated = Convert.ToInt32(dtRetailers.Compute("Count(RetailerId)", "SurveyorId=" + surveyorId + " and RetailerStatusId=2"));
            row["Reevaluated"] = reevaluated;
            Int32 updated = Convert.ToInt32(dtRetailers.Compute("Count(RetailerId)", "SurveyorId=" + surveyorId + " and RetailerStatusId=2 "));
            row["Updated"] = updated;
            Int32 newRetailer = Convert.ToInt32(dtRetailers.Compute("Count(RetailerId)", "SurveyorId=" + surveyorId + " and RetailerStatusId=3"));
            row["NewRetailer"] = newRetailer;
            Int32 total = updated + newRetailer;
            row["Total"] = total;
            return row;
        }

        private static DataRow AddAsTodaysRecord(DataTable dtRetailers, DataTable dtTodaysActivityOfSurveyors, String today, Int32 surveyorId, String surveyorName, String loginName, String areaName)
        {
            DataRow row;
            row = dtTodaysActivityOfSurveyors.NewRow();
            row["SurveyorId"] = surveyorId.ToString();
            row["SurveyorName"] = surveyorName;
            row["LoginName"] = loginName;
            row["AreaName"] = areaName;
            Int32 reevaluated = Convert.ToInt32(dtRetailers.Compute("Count(RetailerId)", "SurveyorId=" + surveyorId + " and RetailerStatusId=2  AND SurveyorDate='" + today + "'"));
            row["Reevaluated"] = reevaluated;
            Int32 updated = Convert.ToInt32(dtRetailers.Compute("Count(RetailerId)", "SurveyorId=" + surveyorId + " and RetailerStatusId=2 AND SurveyorDate='" + today + "'"));
            row["Updated"] = updated;
            Int32 newRetailer = Convert.ToInt32(dtRetailers.Compute("Count(RetailerId)", "SurveyorId=" + surveyorId + " and RetailerStatusId=3 AND SurveyorDate='" + today + "'"));
            row["NewRetailer"] = newRetailer;
            Int32 total =  updated + newRetailer;
            row["Total"] = total;
          //  dtTodaysActivityOfSurveyors.Rows.Add(row);
            return row;
        }

        public ActionResult POSDetail(int id)
        {
            ViewBag.Id = id;
            return View();
        }

        public ActionResult GetElmsisdn(int id)
        {
            using (DataAccess.RobiPosMappingEntities db = new DataAccess.RobiPosMappingEntities())
            {
                var elmsisdns = (from a in db.ElMsisdns where a.RetailerId == id select new { ID = a.ElMsisdnId, Name = a.ElMsisdn1 }).ToList();
                return Json(new { msg="success",Rdata=elmsisdns});
            }
        }
        public ActionResult GetSimpos(int id)
        {
            using (DataAccess.RobiPosMappingEntities db = new DataAccess.RobiPosMappingEntities())
            {
                var simpos = (from a in db.SimPosCodes where a.RetailerId == id select new { ID = a.SimPosCodeId, Name = a.SimPosCode1}).ToList();
                return Json(new { msg = "success", Rdata = simpos });
            }
        }
        public ActionResult UpdateElmsisdn(int id, int name)
        {
            using (DataAccess.RobiPosMappingEntities db = new DataAccess.RobiPosMappingEntities())
            {
                var elpos = (from a in db.ElMsisdns where a.ElMsisdnId == id select a).FirstOrDefault();
                elpos.ElMsisdn1 = name;
                db.SaveChanges();
                return Json(new { msg = "success" });
            }
        }
        public ActionResult DeleteElmsisdn(int id)
        {
            using (DataAccess.RobiPosMappingEntities db = new DataAccess.RobiPosMappingEntities())
            {
                var elpos = (from a in db.ElMsisdns where a.ElMsisdnId == id select a).FirstOrDefault();
                db.ElMsisdns.Remove(elpos);
                db.SaveChanges();
                return Json(new { msg = "success" });
            }
        }
        public ActionResult DeleteSimpos(int id)
        {
            using (DataAccess.RobiPosMappingEntities db = new DataAccess.RobiPosMappingEntities())
            {
                var simpos = (from a in db.SimPosCodes where a.SimPosCodeId == id select a).FirstOrDefault();
                db.SimPosCodes.Remove(simpos);
                db.SaveChanges();
                return Json(new { msg = "success" });
            }
        }
        public ActionResult UpdateSimpos(int id, string name)
        {
            using (DataAccess.RobiPosMappingEntities db = new DataAccess.RobiPosMappingEntities())
            {
                var simpos = (from a in db.SimPosCodes where a.SimPosCodeId == id select a).FirstOrDefault();
                simpos.SimPosCode1 = name;
                db.SaveChanges();
                return Json(new { msg = "success" });
            }
        }
        public ActionResult Search(int id=0,int posStatus=0,int posVerification=0)
        {
            ViewBag.DSRID = id;
            ViewBag.PosStatus = posStatus;
            ViewBag.PosVerification = posVerification;

            String rspId = Session["RspId"].ToString();
            String areaId = Session["AreaId"].ToString();
            String DbPath = Path.Combine(Server.MapPath("~/App_Data"), "JPGL_LEP_2014.mdb");
            String ConnectionString = ConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;

            DataTable dtSRList = new DataTable();
            DataTable dtThanaList = new DataTable();
            DataTable dtPosTypeList = new DataTable();
            DataTable dtShopTypeList = new DataTable();
            DataTable dtLocalityList = new DataTable();

            DataTable dtStructureList = new DataTable();
            DataTable dtSignageList = new DataTable();
            DataTable dtVisitDayList = new DataTable();

            String sqlSelectSR = "select PersonId,PersonName from Person where RspId=" + rspId + " order by PersonName";
            String sqlSelectThana = "select [ThanaId],[ThanaName] from [Thana] where [AreaId]=" + areaId + "";
            String sqlSelectPosType = "SELECT [PosTypeId],[PosTypeName] FROM [PosType]";
            String sqlSelectShopType = "SELECT  [ShopTypeId],[ShopTypeName] FROM [ShopType]";
            String sqlSelectLocality = "SELECT  [LocalityId],[LocalityName] FROM [Locality]";
            String sqlSelectPosStructure = "SELECT  [PosStructureId],[PosStructureName] FROM [PosStructure]";
            String sqlSelectShopSignage = "SELECT  [ShopSignageId] ,[ShopSignageName] FROM [ShopSignage]";
            String sqlSelectVisitDays = "SELECT  [VisitDayId],[VisitDays] FROM [VisitDay]";
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {

                using (SqlCommand command = new SqlCommand(sqlSelectSR, connection))
                {
                    using (SqlDataAdapter da = new SqlDataAdapter(command))
                    {
                        connection.Open();
                        da.Fill(dtSRList);
                        command.CommandText = sqlSelectThana; da.Fill(dtThanaList);
                        command.CommandText = sqlSelectPosType; da.Fill(dtPosTypeList);
                        command.CommandText = sqlSelectShopType; da.Fill(dtShopTypeList);
                        command.CommandText = sqlSelectLocality; da.Fill(dtLocalityList);

                        command.CommandText = sqlSelectPosStructure; da.Fill(dtStructureList);
                        command.CommandText = sqlSelectShopSignage; da.Fill(dtSignageList);
                        command.CommandText = sqlSelectVisitDays; da.Fill(dtVisitDayList);
                        connection.Close();
                    }
                }
            }

            ViewBag.SRList = dtSRList;
            ViewBag.ThanaList = dtThanaList;
            ViewBag.PosTypeList = dtPosTypeList;
            ViewBag.ShopTypeList = dtShopTypeList;
            ViewBag.LocalityList = dtLocalityList;

            ViewBag.StructureList = dtStructureList;
            ViewBag.SignageList = dtSignageList;
            ViewBag.VisitDayList = dtVisitDayList;

            return View();
        }
        public JsonResult GetWards(int thanaId)
        {
            using (DataAccess.RobiPosMappingEntities db = new DataAccess.RobiPosMappingEntities())
            {
                var wards = (from a in db.Wards where a.ThanaId == thanaId select new { wardId = a.WardId, wardName = a.WardName }).ToList();
                return Json(wards);
            }
        }
        public JsonResult GetMauzas(int wardId)
        {
            using (DataAccess.RobiPosMappingEntities db = new DataAccess.RobiPosMappingEntities())
            {
                var mauzas = (from a in db.Mauzas where a.WardId == wardId select new { mauzaId = a.MauzaId, mauzaName = a.MauzaName }).ToList();
                return Json(mauzas);
            }
        }
        public JsonResult GetVillages(int mauzaId)
        {
            using (DataAccess.RobiPosMappingEntities db = new DataAccess.RobiPosMappingEntities())
            {
                var villages = (from a in db.Villages where a.MauzaId == mauzaId select new { villageId = a.VillageId, villageName = a.VillageName }).ToList();

                return Json(villages);
            }
        }

        public ActionResult ResetPOS(int id)
        {
            var retailer = new DataAccess.Retailer() { RetailerId = id, IsActive = true,QrCodeId=0, ModifiedDateTime = null,DsrActivityDateTime=null,RetailerStatusId=1 };
            using (DataAccess.RobiPosMappingEntities db = new DataAccess.RobiPosMappingEntities())
            {
                db.Retailers.Attach(retailer);
                db.Entry(retailer).Property(x => x.IsActive).IsModified = true;
                db.Entry(retailer).Property(x => x.QrCodeId).IsModified = true;
                db.Entry(retailer).Property(x => x.ModifiedDateTime).IsModified = true;
                db.Entry(retailer).Property(x => x.DsrActivityDateTime).IsModified = true;
                db.Entry(retailer).Property(x => x.RetailerStatusId).IsModified = true;
                db.SaveChanges();
                return Json(new { msg = "success" });
            }
        }
        public ActionResult DeletePOS(int id)
        {
            var retailer = new DataAccess.Retailer() { RetailerId = id ,IsActive=false,DsrActivityDateTime=DateTime.Now,ModifiedDateTime=DateTime.Now,RetailerStatusId=2};
            using (DataAccess.RobiPosMappingEntities db = new DataAccess.RobiPosMappingEntities())
            {
                db.Retailers.Attach(retailer);
                db.Entry(retailer).Property(x => x.IsActive).IsModified = true;
                db.Entry(retailer).Property(x => x.DsrActivityDateTime).IsModified = true;
                db.Entry(retailer).Property(x => x.ModifiedDateTime).IsModified = true;
                db.Entry(retailer).Property(x => x.RetailerStatusId).IsModified = true;
                db.SaveChanges();
                return Json(new { msg = "success" });
            }
        }

        [HttpPost]
        public JsonResult SearchResult(FormCollection data)
        {
            try
            {
                Int32 SRId = (data["srname"] == string.Empty) ? 0 : Convert.ToInt32(data["srname"]);
                int posStatus = (data["posStatus"] == string.Empty) ? 0 : Convert.ToInt32(data["posStatus"]);
                String startDate = data["startdate"]; String endDate = data["enddate"];

                DateTime StartDate = DateTime.Now.Date; DateTime EndDate = DateTime.Now.Date;
                if (startDate != "" && endDate != "")
                {
                    Boolean isStartDateValid = DateTime.TryParseExact(startDate, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out StartDate);
                    if (!isStartDateValid)
                    {
                        return Json(new { status = "error", details = "Start date is invalid. It must be in dd/MM/yyyy format." }, JsonRequestBehavior.AllowGet);
                    }

                    Boolean isEndDateValid = DateTime.TryParseExact(endDate, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out EndDate);
                    if (!isEndDateValid)
                    {
                        return Json(new { status = "error", details = "End date is invalid. It must be in dd/MM/yyyy format." }, JsonRequestBehavior.AllowGet);
                    }
                }
                TimeSpan ts = new TimeSpan(23, 59, 59);
                EndDate = EndDate.Add(ts);
                int elMsisdn = (data["elMsisdn"] == string.Empty) ? 0 : Convert.ToInt32(data["elMsisdn"]);
                string simPos = data["simPos"];
                int thanaId = (data["thana"] == string.Empty) ? 0 : Convert.ToInt32(data["thana"]);
                int wardId = (data["ward"] == string.Empty) ? 0 : Convert.ToInt32(data["ward"]);
                int mauzaId = (data["mauza"] == string.Empty) ? 0 : Convert.ToInt32(data["mauza"]);
                int villageId = (data["village"] == string.Empty) ? 0 : Convert.ToInt32(data["village"]);

                int posType = (data["posType"] == string.Empty) ? 0 : Convert.ToInt32(data["posType"]);
                int shopType = (data["shopType"] == string.Empty) ? 0 : Convert.ToInt32(data["shopType"]);
                int locality = (data["locality"] == string.Empty) ? 0 : Convert.ToInt32(data["locality"]);
                int verified = (data["verified"] == string.Empty) ? 0 : Convert.ToInt32(data["verified"]);
                bool elPoschk = (data["elPoschk"] == "on") ? true : false;
                bool simPoschk = (data["simPoschk"] == "on") ? true : false;
                bool scPoschk = (data["scPoschk"] == "on") ? true : false;
                bool appartmentschk = (data["appartmentschk"] == "on") ? true : false;
                bool slumschk = (data["slumschk"] == "on") ? true : false;
                bool semiUrbarchk = (data["semiUrbarchk"] == "on") ? true : false;
                bool ruralHousingchk = (data["ruralHousingchk"] == "on") ? true : false;
                bool shoppingchk = (data["shoppingchk"] == "on") ? true : false;
                bool retailHubchk = (data["retailHubchk"] == "on") ? true : false;
                bool mobileDevicechk = (data["mobileDevicechk"] == "on") ? true : false;
                bool bazzarchk = (data["bazzarchk"] == "on") ? true : false;
                bool officeAreachk = (data["officeAreachk"] == "on") ? true : false;
                bool garmentschk = (data["garmentschk"] == "on") ? true : false;
                bool generalchk = (data["generalchk"] == "on") ? true : false;
                bool urbanTransitchk = (data["urbanTransitchk"] == "on") ? true : false;
                bool ruralTarnsitchk = (data["ruralTarnsitchk"] == "on") ? true : false;
                bool urbanYouthchk = (data["urbanYouthchk"] == "on") ? true : false;
                bool semiUrbanYouthchk = (data["semiUrbanYouthchk"] == "on") ? true : false;
                bool ruralYouthchk = (data["ruralYouthchk"] == "on") ? true : false;
                bool touristDestinations = (data["touristDestinations"] == "on") ? true : false;

                int rspId = Convert.ToInt32(Session["RspId"].ToString());
                using (DataAccess.RobiPosMappingEntities db = new DataAccess.RobiPosMappingEntities())
                {
                    var retailers = from a in db.Retailers where a.RspId == rspId  select a;
                    if (posStatus == 4)
                    {
                        retailers = from c in retailers where c.RetailerStatusId == 2 && c.IsActive == false select c;
                    }
                    else 
                    {
                        retailers = from c in retailers where c.IsActive == true select c;
                    }
                    if (elMsisdn > 0)
                    {
                        retailers = from a in retailers join b in db.ElMsisdns on a.RetailerId equals b.RetailerId where b.ElMsisdn1 == elMsisdn select a;
                    }
                    if (simPos != string.Empty)
                    {
                        retailers = from a in retailers join b in db.SimPosCodes on a.RetailerId equals b.RetailerId where b.SimPosCode1 == simPos select a;
                    }
                    if (SRId > 0)
                    {
                        retailers = from b in retailers where b.DsrId == SRId select b;
                    }
                    if (posStatus > 0 && posStatus<4)
                    {
                        retailers = from c in retailers where c.RetailerStatusId == posStatus select c;
                    }
                    if (startDate != String.Empty && endDate != String.Empty)
                    {
                        retailers = from a in retailers where a.RetailerStatusId > 1 && a.DsrActivityDateTime >= StartDate && a.DsrActivityDateTime <= EndDate select a;
                    }
                    if (thanaId > 0)
                    {
                        retailers = from a in retailers where a.ThanaId == thanaId select a;
                        if (wardId > 0)
                        {
                            retailers = from b in retailers where b.WardId == wardId select b;
                            if (mauzaId > 0)
                            {
                                retailers = from c in retailers where c.MauzaId == mauzaId select c;
                                if (villageId > 0)
                                {
                                    retailers = from d in retailers where d.VillageId == villageId select d;
                                }
                            }
                        }
                    }

                    if (shopType > 0)
                    {
                        retailers = from a in retailers where a.ShopTypeId == shopType select a;
                    }

                    if (verified == 1)
                    {
                        retailers = from a in retailers where a.IsVerifiedByDsrs == true select a;
                    }
                    else if (verified == 2)
                    {
                        retailers = from a in retailers where a.RetailerStatusId > 1 && a.IsVerifiedByDsrs == false select a;
                    }
                    if (elPoschk)
                    {
                        retailers = from a in retailers where a.IsElPos == true select a;
                    }
                    if (simPoschk)
                    {
                        retailers = from a in retailers where a.IsSimPos == true select a;
                    }
                    if (scPoschk)
                    {
                        retailers = from a in retailers where a.IsScPos == true select a;
                    }
                    if (appartmentschk)
                    {
                        retailers = from a in retailers where a.IsApartments == true select a;
                    }
                    if (slumschk)
                    {
                        retailers = from a in retailers where a.IsSlums == true select a;
                    }
                    if (semiUrbarchk)
                    {
                        retailers = from a in retailers where a.IsSemiUrbunHousing == true select a;
                    }
                    if (ruralHousingchk)
                    {
                        retailers = from a in retailers where a.IsRuralHousing == true select a;
                    }
                    if (shoppingchk)
                    {
                        retailers = from a in retailers where a.IsShoppingMall == true select a;
                    }
                    if (retailHubchk)
                    {
                        retailers = from a in retailers where a.IsRetailHub == true select a;
                    }
                    if (mobileDevicechk)
                    {
                        retailers = from a in retailers where a.IsMobileDeviceMarket == true select a;
                    }
                    if (bazzarchk)
                    {
                        retailers = from a in retailers where a.IsBazaar == true select a;
                    }
                    if (officeAreachk)
                    {
                        retailers = from a in retailers where a.IsOfficeArea == true select a;
                    }
                    if (garmentschk)
                    {
                        retailers = from a in retailers where a.IsGarmentsMajorityArea == true select a;
                    }
                    if (generalchk)
                    {
                        retailers = from a in retailers where a.IsGeneralIndustrialArea == true select a;
                    }
                    if (urbanTransitchk)
                    {
                        retailers = from a in retailers where a.IsUrbanTransitPoints == true select a;
                    }
                    if (ruralTarnsitchk)
                    {
                        retailers = from a in retailers where a.IsRuralTransitPoints == true select a;
                    }
                    if (urbanYouthchk)
                    {
                        retailers = from a in retailers where a.IsUrbanYouthHangouts == true select a;
                    }
                    if (semiUrbanYouthchk)
                    {
                        retailers = from a in retailers where a.IsSemiUrbanYouthHangouts == true select a;
                    }
                    if (ruralYouthchk)
                    {
                        retailers = from a in retailers where a.IsRuralYouthHangouts == true select a;
                    }
                    if (touristDestinations)
                    {
                        retailers = from a in retailers where a.IsTouristDestinations == true select a;
                    }
                    var retailerList = (from r in retailers
                                        select new
                                        {
                                            ID = r.RetailerId,
                                            Region = r.Region.RegionName,
                                            Area = r.Area.AreaName,
                                            Thana = r.Thana.ThanaName,
                                            Ward = r.Ward.WardName,
                                            Mauza = r.Mauza.MauzaName,
                                            Village = r.Village.VillageName,
                                            RetailerName = r.RetailerName,
                                            ThanaId = r.ThanaId,
                                            WardId = r.WardId,
                                            MauzaId = r.MauzaId,
                                            VillageId = r.VillageId,
                                            POSStatus = r.RetailerStatusId,
                                            Verified = r.IsVerifiedByDsrs,
                                            Lat = r.Latitude,
                                            Lon = r.Longitude,
                                            Address = r.Address,
                                            VisitDayId = r.VisitDayId,
                                            VisitDay = r.VisitDay.VisitDays,
                                            VisitDayCode = r.VisitDay.VisitDayCode,
                                            PosStructure = r.PosStructure.PosStructureName,
                                            PosStructureId = r.PosStructureId,
                                            PosImage = r.DefaultPhotoName,
                                            ShopSignageId = r.ShopSignageId,
                                            ShopSignage = r.ShopSignage.ShopSignageName,
                                            ShopTypeId = r.ShopTypeId,
                                            ShopType = r.ShopType.ShopTypeName,
                                            DsrId = r.DsrId,
                                            Dsr = r.Person.PersonName,
                                            DsrMsisdn = r.Person.PersonMsisdn,
                                            r.QrCodeId,
                                            r.IsElPos,
                                            r.IsSimPos,
                                            r.IsScPos,
                                            r.IsApartments,
                                            r.IsSlums,
                                            r.IsSemiUrbunHousing,
                                            r.IsRuralHousing,
                                            r.IsShoppingMall,
                                            r.IsRetailHub,
                                            r.IsMobileDeviceMarket,
                                            r.IsBazaar,
                                            r.IsOfficeArea,
                                            r.IsGarmentsMajorityArea,
                                            r.IsGeneralIndustrialArea,
                                            r.IsUrbanTransitPoints,
                                            r.IsRuralTransitPoints,
                                            r.IsUrbanYouthHangouts,
                                            r.IsSemiUrbanYouthHangouts,
                                            r.IsRuralYouthHangouts
                                        }).ToList();
                    return Json(new { status = "success", data = retailerList }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                var path = HttpContext.Server.MapPath("~/App_Data");
                ExceptionLogger.CreateLog(ex, path);
                return Json(new { status = "error"}, JsonRequestBehavior.AllowGet);
            }
            

        }


        public JsonResult UpdateRetialerInfo(RetailerUpdateModel data)
        {
            try 
            {
                using (DataAccess.RobiPosMappingEntities db = new DataAccess.RobiPosMappingEntities())
                {
                    var pos = (from a in db.Retailers where a.RetailerId == data.ID select a).FirstOrDefault();
                    pos.RetailerName = data.retailerName;
                    pos.Address = data.address;
                    pos.VisitDayId = data.visitDayId;
                    pos.PosStructureId = data.retailerStuctureId;
                    pos.ShopSignageId = data.shopSingage;
                    pos.ShopTypeId = data.shopType;
                    pos.IsElPos = data.IsElPos;
                    pos.IsSimPos = data.IsSimPos;
                    pos.IsScPos = data.IsScPos;
                    pos.IsApartments = data.IsApartments;
                    pos.IsSlums = data.IsSlums;
                    pos.IsSemiUrbunHousing = data.IsSemiUrbunHousing;
                    pos.IsRuralHousing = data.IsRuralHousing;
                    pos.IsShoppingMall = data.IsShoppingMall;
                    pos.IsRetailHub = data.IsRetailHub;
                    pos.IsMobileDeviceMarket = data.IsMobileDeviceMarket;
                    pos.IsBazaar = data.IsBazaar;
                    pos.IsOfficeArea = data.IsOfficeArea;
                    pos.IsGarmentsMajorityArea = data.IsGarmentsMajorityArea;
                    pos.IsGeneralIndustrialArea = data.IsGeneralIndustrialArea;
                    pos.IsUrbanTransitPoints = data.IsUrbanTransitPoints;
                    pos.IsRuralTransitPoints = data.IsRuralTransitPoints;
                    pos.IsUrbanYouthHangouts = data.IsUrbanYouthHangouts;
                    pos.IsSemiUrbanYouthHangouts = data.IsSemiUrbanYouthHangouts;
                    pos.IsRuralYouthHangouts = data.IsRuralYouthHangouts;
                    if (data.thanaId > 0)
                    {
                        pos.ThanaId = data.thanaId;
                        if (data.wardId > 0)
                        {
                            pos.WardId = data.wardId;
                            if (data.mauzaId > 0)
                            {
                                pos.MauzaId = data.mauzaId;
                                if (data.villageId > 0)
                                {
                                    pos.VillageId = data.villageId;
                                }
                                else
                                {
                                    pos.VillageId = 0;
                                }
                            }
                            else
                            {
                                pos.MauzaId = 0;
                                pos.VillageId = 0;
                            }
                        }
                        else
                        {
                            pos.WardId = 0;
                            pos.MauzaId = 0;
                            pos.VillageId = 0;
                        }
                    }
                    else
                    {
                        pos.ThanaId = 0;
                        pos.WardId = 0;
                        pos.MauzaId = 0;
                        pos.VillageId = 0;
                    }
                    db.SaveChanges();
                    if (data.elpos != null)
                    {
                        foreach (var x in data.elpos)
                        {
                            DataAccess.ElMsisdn el = new DataAccess.ElMsisdn()
                            {
                                ElMsisdn1 = x,
                                RetailerId = data.ID
                            };
                            db.ElMsisdns.Add(el);
                        }
                        db.SaveChanges();
                    }

                    if (data.simpos != null)
                    {
                        foreach (var y in data.simpos)
                        {
                            DataAccess.SimPosCode sim = new DataAccess.SimPosCode
                            {
                                SimPosCode1 = y,
                                RetailerId = data.ID
                            };
                            db.SimPosCodes.Add(sim);
                        }
                        db.SaveChanges();
                    }
                    var updateRetailer = (from r in db.Retailers
                                          where r.RetailerId == data.ID
                                          select new
                                          {
                                              ID = r.RetailerId,
                                              Region = r.Region.RegionName,
                                              Area = r.Area.AreaName,
                                              Thana = r.Thana.ThanaName,
                                              Ward = r.Ward.WardName,
                                              Mauza = r.Mauza.MauzaName,
                                              Village = r.Village.VillageName,
                                              RetailerName = r.RetailerName,
                                              ThanaId = r.ThanaId,
                                              WardId = r.WardId,
                                              MauzaId = r.MauzaId,
                                              VillageId = r.VillageId,
                                              POSStatus = r.RetailerStatusId,
                                              Verified = r.IsVerifiedByDsrs,
                                              Lat = r.Latitude,
                                              Lon = r.Longitude,
                                              Address = r.Address,
                                              VisitDayId = r.VisitDayId,
                                              VisitDay = r.VisitDay.VisitDays,
                                              VisitDayCode = r.VisitDay.VisitDayCode,
                                              PosStructure = r.PosStructure.PosStructureName,
                                              PosStructureId = r.PosStructureId,
                                              PosImage = r.DefaultPhotoName,
                                              ShopSignageId = r.ShopSignageId,
                                              ShopSignage = r.ShopSignage.ShopSignageName,
                                              ShopTypeId = r.ShopTypeId,
                                              ShopType = r.ShopType.ShopTypeName,
                                              DsrId = r.DsrId,
                                              Dsr = r.Person.PersonName,
                                              DsrMsisdn = r.Person.PersonMsisdn,
                                              r.QrCodeId,
                                              r.IsElPos,
                                              r.IsSimPos,
                                              r.IsScPos,
                                              r.IsApartments,
                                              r.IsSlums,
                                              r.IsSemiUrbunHousing,
                                              r.IsRuralHousing,
                                              r.IsShoppingMall,
                                              r.IsRetailHub,
                                              r.IsMobileDeviceMarket,
                                              r.IsBazaar,
                                              r.IsOfficeArea,
                                              r.IsGarmentsMajorityArea,
                                              r.IsGeneralIndustrialArea,
                                              r.IsUrbanTransitPoints,
                                              r.IsRuralTransitPoints,
                                              r.IsUrbanYouthHangouts,
                                              r.IsSemiUrbanYouthHangouts,
                                              r.IsRuralYouthHangouts
                                          }).FirstOrDefault();
                    return Json(new { msg = "success", Pdata = updateRetailer });
                }
            }
            catch (Exception ex)
            {
                var path = HttpContext.Server.MapPath("~/App_Data");
                ExceptionLogger.CreateLog(ex, path);
                return Json(new { msg = "error" });
            }
            
            
            
        }
        public class RetailerUpdateModel
        {
            public int ID { get; set; }
            public string retailerName { get; set; }
            public string address { get; set; }
            public int visitDayId { get; set; }
            public int retailerStuctureId { get; set; }
            public int shopSingage { get; set; }
            public int shopType { get; set; }
            public int thanaId { get; set; }
            public int wardId { get; set; }
            public int mauzaId { get; set; }
            public int villageId { get; set; }
            public bool IsElPos { get; set; }
            public bool IsSimPos { get; set; }
            public bool IsScPos { get; set; }
            public bool IsApartments { get; set; }
            public bool IsSlums { get; set; }
            public bool IsSemiUrbunHousing { get; set; }
            public bool IsRuralHousing { get; set; }
            public bool IsShoppingMall { get; set; }
            public bool IsRetailHub { get; set; }
            public bool IsMobileDeviceMarket { get; set; }
            public bool IsBazaar { get; set; }
            public bool IsOfficeArea { get; set; }
            public bool IsGarmentsMajorityArea { get; set; }
            public bool IsGeneralIndustrialArea { get; set; }
            public bool IsUrbanTransitPoints { get; set; }
            public bool IsRuralTransitPoints { get; set; }
            public bool IsUrbanYouthHangouts { get; set; }
            public bool IsSemiUrbanYouthHangouts { get; set; }
            public bool IsRuralYouthHangouts { get; set; }
            public List<int> elpos { get; set; }
            public List<string> simpos { get; set; }
        }
        

        public JsonResult ChangeVerificationStatus(int id, bool status)
        {

            status = !status;
            var retailer = new DataAccess.Retailer() { RetailerId = id, IsVerifiedByDsrs = status,DsrsVerificationDateTime=DateTime.Now };

            using (DataAccess.RobiPosMappingEntities db = new DataAccess.RobiPosMappingEntities())
            {
                db.Retailers.Attach(retailer);
                db.Entry(retailer).Property(x => x.IsVerifiedByDsrs).IsModified = true;
                db.Entry(retailer).Property(x => x.DsrsVerificationDateTime).IsModified = true;
                db.SaveChanges();
            }
            return Json(new { msg = "success" });
        }
    }
}