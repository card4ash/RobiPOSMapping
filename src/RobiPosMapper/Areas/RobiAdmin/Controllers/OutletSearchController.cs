using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Globalization;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace RobiPosMapper.Areas.RobiAdmin.Controllers
{
    public class OutletSearchController : Controller
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

        public ActionResult Index()
        {

            //-->Query String Values
            string userName = Request.QueryString["user"].ToString();
            int moId = Convert.ToInt32(Request.QueryString["moid"]);
            //<--Query String Values

            //--> Variables
            DataTable tblRegions = new DataTable();
            DataTable tblSurveyors = new DataTable();
            //<-- Variables

            String sqlSelectRegions = @"SELECT Region.RegionId, dbo.Region.RegionName
                                   FROM dbo.Region INNER JOIN
                                         dbo.MonitoringOfficerRegion ON dbo.Region.RegionId = dbo.MonitoringOfficerRegion.RegionId
                                   WHERE (dbo.MonitoringOfficerRegion.MoId = " + moId + ") ORDER BY dbo.Region.RegionName";

            string sqlSelectSurveyors = @"SELECT S.SurveyorId, S.LoginName, S.SurveyorName
                                        FROM dbo.Surveyors AS S INNER JOIN
                                             dbo.MonitoringOfficerRegion AS MOR ON S.RegionId = MOR.RegionId
                                        WHERE (MOR.MoId = "+ moId + @")
                                        ORDER BY S.LoginName";

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = connection;
                    using (SqlDataAdapter da = new SqlDataAdapter(command))
                    {

                        connection.Open();

                        //fill Region table.
                        command.CommandText = sqlSelectRegions; da.Fill(tblRegions);
                        //fill Surveyors table
                        command.CommandText = sqlSelectSurveyors; da.Fill(tblSurveyors);
                        
                        connection.Close();
                    }

                    command.CommandText = sqlInsertMoLog;
                    command.Parameters.Clear();
                    command.Parameters.Add("@MoId", SqlDbType.Int).Value = Convert.ToInt32(moId);
                    command.Parameters.Add("@LogDescription", SqlDbType.VarChar).Value = "Clicked on Search menu item";
                    connection.Open(); command.ExecuteNonQuery(); connection.Close();

                }
            }


            //-->ViewBag
            ViewBag.Title = "Search";
            ViewBag.ActiveMenuName = "Search";
            ViewBag.UserName = userName;
            ViewBag.MoId = moId;
            ViewBag.Regions = tblRegions;
            ViewBag.Surveyors = tblSurveyors;
            //<--ViewBag

            return View();
        }


        //First search. Lazy load will come later.
        public ActionResult SearchResult(FormCollection formData)
        {

            Int32 MoId = Convert.ToInt32(formData["MoId"]); // it must be in form data
            string userName = formData["UserName"].ToString();
            //--> Variables
            int startNumber = 0; //start from 0
            int endNumber = 10;  //end at 5 numbers of rows

            String sqlSelectRetailers = String.Empty; 
            String whereClause = " Where MOR.MoId = " + MoId + " ";
            string moLogText = String.Empty;
            //<-- Variables

            //Checking Source Button/Link/etc.
            if (formData.AllKeys.Contains("source"))
            {
                moLogText += "Source-" + formData["source"].ToString();
            }

            //checking RegionId
            if (formData.AllKeys.Contains("RegionId"))
            {
                int regionId = Convert.ToInt32(formData["RegionId"]);
                if (regionId>-1)
                {
                    if (string.IsNullOrEmpty(whereClause))
                    {
                        whereClause = " Region.RegionId=" + regionId + " ";
                    }
                    else
                    {
                        whereClause += " AND  Region.RegionId=" + regionId + " ";
                    }
                    moLogText += "Reg-" + regionId;
                }
            }

            //checking AreaId
            if (formData.AllKeys.Contains("AreaId"))
            {
                int AreaId = Convert.ToInt32(formData["AreaId"]);
                if (AreaId > -1)  //-1 mean dont include it in search
                {
                    if (string.IsNullOrEmpty(whereClause))
                    {
                        whereClause = "A.AreaId=" + AreaId + " ";
                    }
                    else
                    {
                        whereClause += " AND  A.AreaId=" + AreaId + " ";
                    }
                    moLogText += ",Area-" + AreaId;
                }
            }


            //checking RspId
            if (formData.AllKeys.Contains("RspId"))
            {
                int RspId = Convert.ToInt32(formData["RspId"]);
                if (RspId > -1)
                {
                    if (string.IsNullOrEmpty(whereClause))
                    {
                        whereClause = "RSP.RspId=" + RspId + " ";
                    }
                    else
                    {
                        whereClause += " AND  RSP.RspId=" + RspId + " ";
                    }

                    moLogText += ",RSP-" + RspId;
                }
            }


            //checking RetailerStatusId
            if (formData.AllKeys.Contains("RetailerStatusId"))
            {
                int RetailerStatusId = Convert.ToInt32(formData["RetailerStatusId"]);
                if (RetailerStatusId> -1)
                {
                    if (RetailerStatusId != 4)
                    {
                        if (string.IsNullOrEmpty(whereClause))
                        {
                            whereClause = "Retailer.RetailerStatusId=" + RetailerStatusId + " ";
                        }
                        else
                        {
                            whereClause += " AND  Retailer.RetailerStatusId=" + RetailerStatusId + " ";
                        }
                    }

                    moLogText += ",RetStsId-" + RetailerStatusId;
                }
            }

            //checking ActiveStatusId
            if (formData.AllKeys.Contains("ActiveStatusId"))
            {
               
                string ActiveStatus = formData["ActiveStatusId"].ToString();

                switch (ActiveStatus)
                {
                    case "active":
                        if (string.IsNullOrEmpty(whereClause))
                        {
                            whereClause = "Retailer.IsActive=1 ";
                        }
                        else
                        {
                            whereClause += " AND  Retailer.IsActive=1 ";
                        }
                        moLogText += ",Actv";
                        break;
                    case "inactive":
                        if (string.IsNullOrEmpty(whereClause))
                        {
                            whereClause = "Retailer.IsActive=0 ";
                        }
                        else
                        {
                            whereClause += " AND  Retailer.IsActive=0 ";
                        }
                        moLogText += ",InActv";
                        break;
                    default:
                        break;
                }
            }


            //checking SurveyorId
            if (formData.AllKeys.Contains("SurveyorId"))
            {
                int SurveyorId = Convert.ToInt32(formData["SurveyorId"]);
                if (SurveyorId > -1)
                {
                    if (string.IsNullOrEmpty(whereClause))
                    {
                        whereClause = "S.SurveyorId=" + SurveyorId + " ";
                    }
                    else
                    {
                        whereClause += " AND S.SurveyorId=" + SurveyorId + " ";
                    }

                    moLogText += ",SrId-" + SurveyorId;
                }
            }

            //checking ViewCount (actually this is "SearchQuantity" in sql statement)
            if (formData.AllKeys.Contains("ViewCount"))
            {
                int ViewCount = Convert.ToInt32(formData["ViewCount"]);
                moLogText += ",ViewCount-" + ViewCount;
                switch (ViewCount)
                {
                    case 0:
                    case 1:
                    case 2:
                    case 3:
                        if (string.IsNullOrEmpty(whereClause))
                        {
                            whereClause = "(SELECT COUNT(AppearId) AS Expr1 FROM dbo.RetailerAppearance WHERE (RetailerId = Retailer.RetailerId) AND (MoId=" + MoId + @"))=" + ViewCount + " ";
                        }
                        else
                        {
                            whereClause += " AND (SELECT COUNT(AppearId) AS Expr1 FROM dbo.RetailerAppearance WHERE (RetailerId = Retailer.RetailerId) AND (MoId=" + MoId + @"))=" + ViewCount + " ";
                        }
                        break;
                    case 4:
                        if (string.IsNullOrEmpty(whereClause))
                        {
                            whereClause = "(SELECT COUNT(AppearId) AS Expr1 FROM dbo.RetailerAppearance WHERE (RetailerId = Retailer.RetailerId) AND (MoId=" + MoId + @")) > 3 ";
                        }
                        else
                        {
                            whereClause += " AND (SELECT COUNT(AppearId) AS Expr1 FROM dbo.RetailerAppearance WHERE (RetailerId = Retailer.RetailerId) AND (MoId=" + MoId + @")) > 3 ";
                        }
                        break;
                    default:
                        break;
                }
            }

            //checking MultipleEl
            if (formData.AllKeys.Contains("MultipleEl"))
            {
                string MultipleEl = formData["MultipleEl"].ToString();
                switch (MultipleEl)
                {
                    case "single":
                        if (string.IsNullOrEmpty(whereClause))
                        {
                            whereClause = " (SELECT COUNT(ElMsisdnId) AS Expr1 FROM dbo.ElMsisdn WHERE (RetailerId = Retailer.RetailerId))=1 ";
                        }
                        else
                        {
                            whereClause += " AND  (SELECT COUNT(ElMsisdnId) AS Expr1 FROM dbo.ElMsisdn WHERE (RetailerId = Retailer.RetailerId))=1 ";
                        }
                        moLogText += ",SingleEl";
                        break;
                    case "multiple":
                        if (string.IsNullOrEmpty(whereClause))
                        {
                            whereClause = " (SELECT COUNT(ElMsisdnId) AS Expr1 FROM dbo.ElMsisdn WHERE (RetailerId = Retailer.RetailerId)) > 1 ";
                        }
                        else
                        {
                            whereClause += " AND  (SELECT COUNT(ElMsisdnId) AS Expr1 FROM dbo.ElMsisdn WHERE (RetailerId = Retailer.RetailerId))>1 ";
                        }
                        moLogText += ",MultiEl";
                        break;
                    default:
                        break;
                }
            }

            //checking MultipleScCode
            if (formData.AllKeys.Contains("MultipleScCode"))
            {
                string MultipleScCode = formData["MultipleScCode"].ToString();
                switch (MultipleScCode)
                {
                    case "single":
                        if (string.IsNullOrEmpty(whereClause))
                        {
                            whereClause = "  (SELECT COUNT(SimPosCodeId) AS Expr1 FROM dbo.SimPosCode WHERE (RetailerId = Retailer.RetailerId))=1 ";
                        }
                        else
                        {
                            whereClause += " AND (SELECT COUNT(SimPosCodeId) AS Expr1 FROM dbo.SimPosCode WHERE (RetailerId = Retailer.RetailerId))=1 ";
                        }
                        break;
                    case "multiple":
                        if (string.IsNullOrEmpty(whereClause))
                        {
                            whereClause = "  (SELECT COUNT(SimPosCodeId) AS Expr1 FROM dbo.SimPosCode WHERE (RetailerId = Retailer.RetailerId)) > 1 ";
                        }
                        else
                        {
                            whereClause += " AND   (SELECT COUNT(SimPosCodeId) AS Expr1 FROM dbo.SimPosCode WHERE (RetailerId = Retailer.RetailerId))>1 ";
                        }
                        break;
                    default:
                        break;
                }
            }

            //checking MultipleSurveyor
            if (formData.AllKeys.Contains("MultipleSurveyor"))
            {
               
                string MultipleSurveyor = formData["MultipleSurveyor"].ToString();
                switch (MultipleSurveyor)
                {
                    case "single":
                        if (string.IsNullOrEmpty(whereClause))
                        {
                            whereClause = "(SELECT COUNT(DISTINCT SrId) AS Count FROM dbo.SrRetailerLog WHERE (RetailerId = Retailer.RetailerId)) = 1 ";
                        }
                        else
                        {
                            whereClause += " AND (SELECT COUNT(DISTINCT SrId) AS Count FROM dbo.SrRetailerLog WHERE (RetailerId = Retailer.RetailerId)) = 1 ";
                        }
                        moLogText += ",SingleSr";
                        break;
                    case "multiple":
                        if (string.IsNullOrEmpty(whereClause))
                        {
                            whereClause = "(SELECT COUNT(DISTINCT SrId) AS Count FROM dbo.SrRetailerLog WHERE (RetailerId = Retailer.RetailerId)) > 1 ";
                        }
                        else
                        {
                            whereClause += " AND (SELECT COUNT(DISTINCT SrId) AS Count FROM dbo.SrRetailerLog WHERE (RetailerId = Retailer.RetailerId)) > 1 ";
                        }
                        moLogText += ",MultiSr";
                        break;
                    default:
                        break;
                }
            }

            //Checking VerifyStatus
            if (formData.AllKeys.Contains("VerifyStatus"))
            {
                string VerifyStatus = formData["VerifyStatus"].ToString();
                switch (VerifyStatus)
                {
                    case "verified":
                        if (string.IsNullOrEmpty(whereClause))
                        {
                            whereClause = "Retailer.IsVerifiedByRsp=1 ";
                        }
                        else
                        {
                            whereClause += " AND Retailer.IsVerifiedByRsp=1 ";
                        }
                        moLogText += ",Verifd";
                        break;
                    case "notverified":
                        if (string.IsNullOrEmpty(whereClause))
                        {
                            whereClause = "Retailer.IsVerifiedByRsp = 0 ";
                        }
                        else
                        {
                            whereClause += " AND Retailer.IsVerifiedByRsp = 0 ";
                        }
                        moLogText += ",UnVerifd";
                        break;
                    default:
                        break;
                }
            }


            //checking IsAllDate
            if (formData.AllKeys.Contains("IsAllDate"))
            {
               Boolean IsAllDate = Convert.ToBoolean(formData["IsAllDate"]);
               if (!IsAllDate)
               {
                   String fromDate = formData["fromdate"].ToString();
                   String toDate = formData["todate"].ToString();

                   DateTime startDate;
                   DateTime.TryParseExact(fromDate, InputDateFormatsAllowed, myCulture, DateTimeStyles.None, out startDate);
                   DateTime endDate;
                   DateTime.TryParseExact(toDate, InputDateFormatsAllowed, myCulture, DateTimeStyles.None, out endDate);
                
                   if (string.IsNullOrEmpty(whereClause))
                   {
                       whereClause = " CAST(Retailer.SurveyorActivityDateTime AS DATE) Between '" + startDate.ToString("yyyy-MM-dd") + "' and '" + endDate.ToString("yyyy-MM-dd") + "'";
                   }
                   else
                   {
                       whereClause += " AND CAST(Retailer.SurveyorActivityDateTime AS DATE) Between '" + startDate.ToString("yyyy-MM-dd") + "' and '" + endDate.ToString("yyyy-MM-dd") + "'";
                   }

                   moLogText += "," + startDate.ToString("dd-MM-yyyy") + " " + endDate.ToString("dd-MM-yyyy");
               }
               else
               {
                   moLogText += ",AllDate";
               }
            }

            //This same SQL is used in next lazy load method.
            string sqlString = @"SELECT RegionId, RegionName, AreaId, AreaName, RetailerId, RetailerName, Address, PhotoName, Latitude, Longitude, LastActivityDateTime, IsVerified, 
                                      RetailerStatusId,SearchQuantity ,VisitedSrQuantity,ElQuantity,SimPosCodeQuantity,RspId,RspName,SurveyorId,LoginName,IsActive  FROM
                                (
                                    SELECT  ROW_NUMBER() OVER (ORDER BY Retailer.SurveyorActivityDateTime DESC) AS row, Region.RegionId, Region.RegionName, A.AreaId, A.AreaName, Retailer.RetailerId, Retailer.RetailerName, Retailer.Address, Retailer.DefaultPhotoName AS PhotoName, Retailer.Latitude, Retailer.Longitude, 
                                     Retailer.SurveyorActivityDateTime AS LastActivityDateTime, 
                                      Retailer.IsVerifiedByRsp AS IsVerified, Retailer.RetailerStatusId,
                                      (SELECT COUNT(AppearId) AS Expr1 FROM dbo.RetailerAppearance WHERE (RetailerId = Retailer.RetailerId) AND (MoId=" + MoId + @")) AS SearchQuantity,
                                      (SELECT COUNT(DISTINCT SrId) AS Count FROM dbo.SrRetailerLog WHERE (RetailerId = Retailer.RetailerId)) AS VisitedSrQuantity,
                                      (SELECT COUNT(ElMsisdnId) AS Expr1 FROM dbo.ElMsisdn WHERE (RetailerId = Retailer.RetailerId)) AS ElQuantity,
                                      (SELECT COUNT(SimPosCodeId) AS Expr1 FROM dbo.SimPosCode WHERE (RetailerId = Retailer.RetailerId)) AS SimPosCodeQuantity,
                                      RSP.RspId, RSP.RspName, S.SurveyorId, S.LoginName, Retailer.IsActive
                                      FROM dbo.Area AS A INNER JOIN
                                         dbo.RSP AS RSP INNER JOIN
                                         dbo.Retailer AS Retailer ON RSP.RspId = Retailer.RspId INNER JOIN
                                         dbo.Region AS Region ON Retailer.RegionId = Region.RegionId ON A.AreaId = Retailer.AreaId INNER JOIN
                                         dbo.Surveyors AS S ON Retailer.SurveyorId = S.SurveyorId INNER JOIN
                                         dbo.MonitoringOfficerRegion AS MOR ON Region.RegionId = MOR.RegionId " + whereClause + @"
                                 ) AS data " ;

            //This is for paging. It will be redefine in next call
            string sqlOuterWhereClause = " WHERE row BETWEEN "+ startNumber +" AND "+ endNumber +"";

            string sqlFinalQuery = sqlString + sqlOuterWhereClause + " order by LastActivityDateTime desc ";

            string sqlCount = @" SELECT  Count(Retailer.RetailerId)
                                      FROM dbo.Area AS A INNER JOIN
                                         dbo.RSP AS RSP INNER JOIN
                                         dbo.Retailer AS Retailer ON RSP.RspId = Retailer.RspId INNER JOIN
                                         dbo.Region AS Region ON Retailer.RegionId = Region.RegionId ON A.AreaId = Retailer.AreaId INNER JOIN
                                         dbo.Surveyors AS S ON Retailer.SurveyorId = S.SurveyorId INNER JOIN
                                         dbo.MonitoringOfficerRegion AS MOR ON Region.RegionId = MOR.RegionId " + whereClause + "";

            DataTable tblResult = new DataTable(); int resultCount = 0;
            using (SqlConnection connection=new SqlConnection(ConnectionString))
            {
                using (SqlCommand command=new SqlCommand(sqlFinalQuery,connection))
                {
                    connection.Open();
                    using (SqlDataAdapter da=new SqlDataAdapter(command))
                    {
                        da.Fill(tblResult);
                    }

                    command.CommandText = sqlCount;
                    resultCount = Convert.ToInt32(command.ExecuteScalar());
                    connection.Close();

                    command.CommandText = sqlInsertMoLog;
                    command.Parameters.Clear();
                    command.Parameters.Add("@MoId", SqlDbType.Int).Value = MoId;
                    command.Parameters.Add("@LogDescription", SqlDbType.VarChar).Value = "Search from " + moLogText + ". Result:" + resultCount + " outlets.";
                    connection.Open(); command.ExecuteNonQuery(); connection.Close();
                }
            }

            Boolean hasData = false;
            if (tblResult.Rows.Count>0)
            {
                hasData = true;
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = connection; connection.Open();
                        foreach (DataRow row in tblResult.Rows)
                        {
                            int retailerId = Convert.ToInt32(row["RetailerId"]);
                            command.CommandText = "Insert Into RetailerAppearance(RetailerId,MoId) Values("+ retailerId +","+ MoId +")";
                            command.ExecuteNonQuery();
                        }
                        connection.Close();
                    }
                }
            }

            //--> View Bag
            ViewBag.MoId = MoId.ToString();
            ViewBag.UserName = userName; //LoginName
            ViewBag.Retailers = tblResult;
            //<-- View Bag

            string partialData = RazorViewToStringClass. RenderRazorViewToString(this, "SearchResult");

            return Json(new {hasdata=hasData, data = partialData,sql=sqlString, resultcount=resultCount }, JsonRequestBehavior.AllowGet);
        }

        //This method depends on above method for SQL Statement.
        public ActionResult LazySearchResult(FormCollection formData)
        {
            Int32 MoId = Convert.ToInt32(formData["MoId"]); // it must be in form data
            string userName = formData["UserName"].ToString();
            int pageCount = Convert.ToInt32(formData["PageCount"]);
            string lazySql = formData["SQL"].ToString();

            int startNumber = (pageCount * 10) + 1; 
            int endNumber = (pageCount * 10) + 10; 

            String sqlSelectRetailers = String.Empty;
            String sqlSearchCountLog = String.Empty;
            String whereClause = " Where MOR.MoId = " + MoId + " ";
            String whereClauseForSearchCount = string.Empty; // " Where RetailerStatusId=2 ";

            string moLogText = String.Empty;
            //Checking Source Button/Link/etc.
            if (formData.AllKeys.Contains("source"))
            {
                moLogText += "Source-" + formData["source"].ToString();
            }

            //<-- Variables

            //This SQL is used in next lazy load method.
            string sqlString = lazySql;

            //This is for paging. 
            string sqlOuterWhereClause = " WHERE row BETWEEN " + startNumber + " AND " + endNumber + "";


            string sqlFinalQuery = sqlString + sqlOuterWhereClause;

            DataTable tblResult = new DataTable();
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                using (SqlCommand command = new SqlCommand(sqlFinalQuery, connection))
                {
                    using (SqlDataAdapter da = new SqlDataAdapter(command))
                    {
                        da.Fill(tblResult);
                    }

                    command.CommandText = sqlInsertMoLog;
                    command.Parameters.Clear();
                    command.Parameters.Add("@MoId", SqlDbType.Int).Value = MoId;
                    command.Parameters.Add("@LogDescription", SqlDbType.VarChar).Value = "Lazy View from " + moLogText + " " + startNumber.ToString() +" to "+ endNumber.ToString() +".";
                    connection.Open(); command.ExecuteNonQuery(); connection.Close();
                }
            }

            Boolean hasData = false;
            if (tblResult.Rows.Count > 0)
            {
                hasData = true;
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = connection; connection.Open();
                        foreach (DataRow row in tblResult.Rows)
                        {
                            int retailerId = Convert.ToInt32(row["RetailerId"]);
                            command.CommandText = "Insert Into RetailerAppearance(RetailerId,MoId) Values(" + retailerId + "," + MoId + ")";
                            command.ExecuteNonQuery();
                        }
                        connection.Close();
                    }
                }
            }

            //--> View Bag
            ViewBag.MoId = MoId.ToString();
            ViewBag.UserName = userName; //LoginName
            ViewBag.Retailers = tblResult;
            // ViewBag.MyCulture = myCulture;
            //<-- View Bag

            string partialData = RazorViewToStringClass.RenderRazorViewToString(this, "SearchResult");

            return Json(new { hasdata = hasData, data = partialData }, JsonRequestBehavior.AllowGet);
        }
	}
}