using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace RobiPosMapper.Areas.RSP.Controllers
{
    public class OutletListController : Controller
    {
        public ActionResult EvaluatedOutletList()
        {
            String queryDate = Request.QueryString["date"].ToString();
            String sr = Request.QueryString["srid"].ToString();
            String rsp = Request.QueryString["rspid"].ToString();
            String srLoginName = Request.QueryString["sr"].ToString();
            String srFullName = Request.QueryString["name"].ToString();

            String connectionString = WebConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;
            String sqlSelectRetailers = @"Select LoginName, SurveyorName,RetailerId, RetailerName, Address, Photo from viewRetailerList ";
            String whereClause = " Where RetailerStatusId=2 and IsReevaluated=1 ";

            if (!String.IsNullOrEmpty(queryDate))
            {
                DateTime date;
                DateTime.TryParseExact(queryDate, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out date);
                DateTime startDate = new DateTime(date.Year, date.Month, date.Day, 00, 00, 01);
                DateTime endDate = new DateTime(date.Year, date.Month, date.Day, 23, 59, 59);

                whereClause += " AND SurveyorActivityDateTime Between '" + startDate + "' and '" + endDate + "'";
            }

            //check sr
            if (!String.IsNullOrEmpty(sr))
            {
                Int32 srId = Convert.ToInt32(sr);
                if (String.IsNullOrEmpty(whereClause))
                {
                    whereClause = " SurveyorId=" + srId + "";
                }
                else
                {
                    whereClause += " AND SurveyorId=" + srId + "";
                }
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

            sqlSelectRetailers += whereClause + " Order By LoginName, RetailerName";

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

           // ViewBag.UserName = Request.QueryString["user"].ToString();
            ViewBag.Retailers = dtRetailers;
            ViewBag.Title = "Evaluated Retailers List " + (String.IsNullOrEmpty(queryDate)?" in life time":" as of " + queryDate) + (!String.IsNullOrEmpty(sr)? " by "+
                srFullName +" (" + srLoginName + ")":"");
            ViewBag.RspId = rsp;
            ViewBag.SrLoginName = srLoginName;
            ViewBag.SrFullName = srFullName;


            return View("OutletList");
        }

        //This is for SR-Wise
        public ActionResult UpdatedOutletList()
        {
            String queryDate = Request.QueryString["date"].ToString();
            String sr = Request.QueryString["srid"].ToString();
            String rsp = Request.QueryString["rspid"].ToString();
            String srLoginName = Request.QueryString["sr"].ToString();
            String srFullName = Request.QueryString["name"].ToString();

            String connectionString = WebConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;
            String sqlSelectRetailers = @"Select LoginName, SurveyorName,RetailerId, RetailerName, Address, Photo from viewRetailerList ";
            String whereClause = " Where RetailerStatusId=2  ";
            if (!String.IsNullOrEmpty(queryDate))
            {
                DateTime date;
                DateTime.TryParseExact(queryDate, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out date);
                DateTime startDate = new DateTime(date.Year, date.Month, date.Day, 00, 00, 01);
                DateTime endDate = new DateTime(date.Year, date.Month, date.Day, 23, 59, 59);

                whereClause += " AND SurveyorActivityDateTime Between '" + startDate + "' and '" + endDate + "'";
            }

            //check sr
            if (!String.IsNullOrEmpty(sr))
            {
                Int32 srId = Convert.ToInt32(sr);
                if (String.IsNullOrEmpty(whereClause))
                {
                    whereClause = " SurveyorId=" + srId + "";
                }
                else
                {
                    whereClause += " AND SurveyorId=" + srId + "";
                }
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

            sqlSelectRetailers += whereClause + " Order By RetailerName";

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

          //  ViewBag.UserName = Request.QueryString["user"].ToString();
            ViewBag.RspId = rsp;
            ViewBag.Retailers = dtRetailers;
            ViewBag.Title = "Updated Retailers List " + (String.IsNullOrEmpty(queryDate) ? " in life time" : " as of " + queryDate) + (!String.IsNullOrEmpty(sr) ? " by " +
               srFullName + " (" + srLoginName + ")" : "");
            return View("OutletList");
        }

        //This is for DSR-Wise
        public ActionResult UpdatedOutletListForDsr()
        {
           
            String dsrId = Request.QueryString["dsrid"].ToString();
            String dsrFullName = Request.QueryString["dsrname"].ToString();

            String connectionString = WebConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;
            String sqlSelectRetailers = @"Select LoginName, SurveyorName,RetailerId, RetailerName, Address, Photo from viewRetailerList ";
            String whereClause = " Where RetailerStatusId=2  ";


            //Check DSR
            if (!String.IsNullOrEmpty(dsrId))
            {
                Int32 intDsrId = Convert.ToInt32(dsrId);
                if (String.IsNullOrEmpty(whereClause))
                {
                    whereClause = " DsrId=" + intDsrId + "";
                }
                else
                {
                    whereClause += " AND DsrId=" + intDsrId + "";
                }
            }


            sqlSelectRetailers += whereClause + " Order By RetailerName";

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

            //  ViewBag.UserName = Request.QueryString["user"].ToString();
            ViewBag.RspId = Request.QueryString["RspId"].ToString();
            ViewBag.Retailers = dtRetailers;
            ViewBag.Title = "Updated Retailers List of " + dsrFullName;
            return View("OutletList");
        }

        public ActionResult NewOutletList()
        {
            String queryDate = Request.QueryString["date"].ToString();
            String sr = Request.QueryString["srid"].ToString();
            String rsp = Request.QueryString["rspid"].ToString();
            String srLoginName = Request.QueryString["sr"].ToString();
            String srFullName = Request.QueryString["name"].ToString();

            String connectionString = WebConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;
            String sqlSelectRetailers = @"Select LoginName, SurveyorName,RetailerId, RetailerName, Address, Photo from viewRetailerList ";
            String whereClause = " Where RetailerStatusId=3 AND SurveyorActivityDateTime is not NULL ";
            if (!String.IsNullOrEmpty(queryDate))
            {
                DateTime date;
                DateTime.TryParseExact(queryDate, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out date);
                DateTime startDate = new DateTime(date.Year, date.Month, date.Day, 00, 00, 01);
                DateTime endDate = new DateTime(date.Year, date.Month, date.Day, 23, 59, 59);

                whereClause += " AND SurveyorActivityDateTime Between '" + startDate + "' and '" + endDate + "'";
            }

            //check sr
            if (!String.IsNullOrEmpty(sr))
            {
                Int32 srId = Convert.ToInt32(sr);
                if (String.IsNullOrEmpty(whereClause))
                {
                    whereClause = " SurveyorId=" + srId + "";
                }
                else
                {
                    whereClause += " AND SurveyorId=" + srId + "";
                }
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

            sqlSelectRetailers += whereClause + " Order By LoginName,RetailerName";

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

            //ViewBag.UserName = Request.QueryString["user"].ToString();
            ViewBag.Retailers = dtRetailers;
            ViewBag.Title = "New Retailers List " + (String.IsNullOrEmpty(queryDate) ? " in life time" : " as of " + queryDate) + (!String.IsNullOrEmpty(sr) ? " by " +
              srFullName + " (" + srLoginName + ")" : "");
            ViewBag.RspId = rsp;
            return View("OutletList");
        }

        public ActionResult TotalOutletList()
        {
            String queryDate = Request.QueryString["date"].ToString();
            String sr = Request.QueryString["srid"].ToString();
            String rsp = Request.QueryString["rspid"].ToString();
            String srLoginName = Request.QueryString["sr"].ToString();
            String srFullName = Request.QueryString["name"].ToString();

            String connectionString = WebConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;
            String sqlSelectRetailers = @"Select LoginName, SurveyorName,RetailerId, RetailerName, Address, Photo from viewRetailerList ";
            String whereClause = " Where SurveyorActivityDateTime is not NULL ";
            if (!String.IsNullOrEmpty(queryDate))
            {
                DateTime date;
                DateTime.TryParseExact(queryDate, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out date);
                DateTime startDate = new DateTime(date.Year, date.Month, date.Day, 00, 00, 01);
                DateTime endDate = new DateTime(date.Year, date.Month, date.Day, 23, 59, 59);

                whereClause += " AND SurveyorActivityDateTime Between '" + startDate + "' and '" + endDate + "'";
            }

            //check sr
            if (!String.IsNullOrEmpty(sr))
            {
                Int32 srId = Convert.ToInt32(sr);
                if (String.IsNullOrEmpty(whereClause))
                {
                    whereClause = " SurveyorId=" + srId + "";
                }
                else
                {
                    whereClause += " AND SurveyorId=" + srId + "";
                }
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

            sqlSelectRetailers += whereClause + " Order By LoginName, RetailerName";

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

           // ViewBag.UserName = Request.QueryString["user"].ToString();
            ViewBag.Retailers = dtRetailers;
            ViewBag.Title = "Total Retailers List " + (String.IsNullOrEmpty(queryDate) ? " in life time" : " as of " + queryDate) + (!String.IsNullOrEmpty(sr) ? " by " +
               srFullName + " (" + srLoginName + ")" : "");
            ViewBag.RspId = rsp;
            return View("OutletList");
        }

        //This accesssable from URL only, (for inhosue use)
        public ActionResult OverlappingOutletList()
        {


            String connectionString = WebConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;

            String sqlSelectRetailers = @"select RetailerId, Count(distinct(SrId)) as MultipleSrQuantity from SrRetailerLog  group by RetailerId  Having Count(distinct(SrId))>1 Order By RetailerId ";


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

            return View();
        }

        //This accesssable from URL only, (for inhosue use), from RETAILER table
        public ActionResult TotalOutletGreaterThanAnSpecificDateTimeList()
        {
            String queryDate = Request.QueryString["datetime"].ToString();

            String connectionString = WebConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;
            String sqlSelectRetailers = @"Select RegionName,AreaName,RspName,RetailerId, RetailerName, Address, Photo from viewRetailerList ";
            String whereClause = " Where SurveyorActivityDateTime is not NULL ";
            if (!String.IsNullOrEmpty(queryDate))
            {
                DateTime date;
                DateTime.TryParseExact(queryDate, "yyyy-MM-dd hh:mm", null, System.Globalization.DateTimeStyles.None, out date);

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



        //This accesssable from URL only, (for inhosue use), from SR Log table
        public ActionResult TotalOutletGreaterThanAnSpecificDateTimeListFromSrLog()
        {
            String queryDate = Request.QueryString["datetime"].ToString();
            DataTable dtRetailers = new DataTable();
            String connectionString = WebConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;
            String sqlSelectRetailers = @"SELECT R.RegionName, R.AreaName, R.RspName, R.RetailerId, R.RetailerName, R.Address, R.Photo, S.LogDateTime FROM dbo.viewRetailerList AS R INNER JOIN dbo.SrRetailerLog AS S ON R.RetailerId = S.RetailerId WHERE S.LogDateTime> @LogDateTime Order BY  
              S.LogDateTime";

            if (!String.IsNullOrEmpty(queryDate))
            {
                DateTime date;
                Boolean isValidDateTime = DateTime.TryParseExact(queryDate, "yyyy-MM-dd hh:mm", null, System.Globalization.DateTimeStyles.None, out date);
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
            Int32 surveyorId = Convert.ToInt32(Request.QueryString["srid"]);
            DataTable dtRetailers = new DataTable();
            String connectionString = WebConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;
            String sqlSelectRetailers = @"SELECT R.RegionName, R.AreaName, R.RspName, R.RetailerId, R.RetailerName, R.Address, R.Photo, S.LogDateTime FROM dbo.viewRetailerList AS R INNER JOIN dbo.SrRetailerLog AS S ON R.RetailerId = S.RetailerId WHERE S.SrId= @SrId Order BY  
              R.RegionName ASC, R.AreaName ASC, R.RspName ASC, S.LogDateTime DESC";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(sqlSelectRetailers, connection))
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

            ViewBag.UserName = Request.QueryString["user"].ToString();
            ViewBag.Retailers = dtRetailers;
            ViewBag.Title = "Total retailers ";
            return View("OutletList");
        }
    }
}