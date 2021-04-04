using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;
using RobiPosMapper.Models;

namespace RobiPosMapper.Areas.RSP.Controllers
{
    public class OutletDetailsController : Controller
    {
        string sqlInsertMoLog = "INSERT INTO MonitoringOfficerActivityLog(MoId,LogDescription) VALUES (@MoId,@LogDescription)";

        public ActionResult Index()
        {
            
            Int32 retailerId = Convert.ToInt32(Request.QueryString["retailerid"]);
            Int32 moId =  Convert.ToInt32(Request.QueryString["moid"]);
            Boolean showEditButton = false;
            if (moId>0)
            {
                showEditButton = true;
            }
            String conString = WebConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;

            DataTable dtRetailer = new DataTable(); //Consists every information about retailer except EL Numbers & Sim Pos Codes
            DataTable dtElNumbers = new DataTable();
            DataTable dtSimPosCodes = new DataTable();
            DataTable dtSrRetailerLog = new DataTable();
            Boolean isMultipleSrFound = false;
            using (SqlConnection connection = new SqlConnection(conString))
            {
                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = "Select * from viewRetailerList where RetailerId=" + retailerId + "";
                    using (SqlDataAdapter da = new SqlDataAdapter(command))
                    {
                        connection.Open();
                        da.Fill(dtRetailer);
                        command.CommandText = "SELECT ElMsisdn FROM ElMsisdn where RetailerId=" + retailerId + "";
                        da.Fill(dtElNumbers);
                        command.CommandText = "SELECT SimPosCode  FROM SimPosCode where RetailerId=" + retailerId + "";
                        da.Fill(dtSimPosCodes);
                        command.CommandText = "select Count(distinct(SrId)) as Qty from SrRetailerLog where RetailerID=" + retailerId + "  group by RetailerId  Having Count(distinct(SrId))>1";
                        object objIsMultipleSr = command.ExecuteScalar();
                        if (objIsMultipleSr != null)
                        {
                            isMultipleSrFound = true;
                            command.CommandText = @"SELECT  dbo.Surveyors.SurveyorName, dbo.Surveyors.ContactNo, dbo.Surveyors.LoginName, dbo.Surveyors.AreaName,                                        dbo.SrRetailerLog.LogDateTime,  dbo.Person.PersonName AS DsrName
                                                    FROM dbo.SrRetailerLog INNER JOIN
                                                         dbo.Surveyors ON dbo.SrRetailerLog.SrId = dbo.Surveyors.SurveyorId INNER JOIN
                                                         dbo.Retailer ON dbo.SrRetailerLog.RetailerId = dbo.Retailer.RetailerId INNER JOIN
                                                         dbo.Person ON dbo.Retailer.DsrId = dbo.Person.PersonId
                                                    WHERE (dbo.SrRetailerLog.RetailerId = " + retailerId + ")";
                            da.Fill(dtSrRetailerLog);
                        }
                        connection.Close();

                        command.CommandText = sqlInsertMoLog;
                        command.Parameters.Clear();
                        command.Parameters.Add("@MoId", SqlDbType.Int).Value = moId;
                        command.Parameters.Add("@LogDescription", SqlDbType.VarChar).Value = string.Format("Viewed Outlet {0} Details.",retailerId);
                        connection.Open(); command.ExecuteNonQuery(); connection.Close();
                    }
                }
            }

            ViewBag.PosCategoryList = PosCategoryManager.PosCategoryList();
            ViewBag.PosStructureList = PosStructureManager.PosStructureList();
            ViewBag.VisitDayList = VisitDayManager.VisitDayList();
            ViewBag.ShopSignageList = ShopSignageManager.ShopSignageList();
            ViewBag.ShopTypeList = ShopTypeManager.ShopTypeList();

            ViewBag.Title = "Retailer Details";
            ViewBag.Retailer = dtRetailer;
            ViewBag.RetailerId = retailerId;
            ViewBag.MoId = moId;
            ViewBag.ElNumbers = dtElNumbers; ViewBag.SimPosCodes = dtSimPosCodes;
            ViewBag.SrRetailerLog = dtSrRetailerLog;
            ViewBag.ShowEditButton = showEditButton;

            if (dtRetailer.Rows.Count == 0)
            {
                return View("RetailerNotFound");
            }
            else
            {
                return View("OutletDetails");
            }
        }

        //This is carbon copy of above method.
        //Search retailer by retailer id. Comes from /RSP/OuteletDetails Details page on search button click event
        public ActionResult SearchRetailerById(FormCollection data)
        {
            Int32 retailerId = Convert.ToInt32(data["retailerid"]);
            Int32 moId = Convert.ToInt32(Request.QueryString["moid"]);
            Boolean showEditButton = false;
            if (moId > 0)
            {
                showEditButton = true;
            }

            String conString = WebConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;

            DataTable dtRetailer = new DataTable(); //Consists every information about retailer except EL Numbers & Sim Pos Codes
            DataTable dtElNumbers = new DataTable();
            DataTable dtSimPosCodes = new DataTable();
            DataTable dtSrRetailerLog = new DataTable();
            Boolean isMultipleSrFound = false;
            Object isMoAlignedWithThisRetailer=null;
            using (SqlConnection connection = new SqlConnection(conString))
            {
                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = "Select * from viewRetailerList where RetailerId=" + retailerId + "";
                    using (SqlDataAdapter da = new SqlDataAdapter(command))
                    {
                        connection.Open();
                        da.Fill(dtRetailer);
                        command.CommandText = "SELECT ElMsisdn FROM ElMsisdn where RetailerId=" + retailerId + "";
                        da.Fill(dtElNumbers);
                        command.CommandText = "SELECT SimPosCode  FROM SimPosCode where RetailerId=" + retailerId + "";
                        da.Fill(dtSimPosCodes);
                        command.CommandText = "select Count(distinct(SrId)) as Qty from SrRetailerLog where RetailerID=" + retailerId + "  group by RetailerId  Having Count(distinct(SrId))>1";
                        object objIsMultipleSr = command.ExecuteScalar();
                        if (objIsMultipleSr != null)
                        {
                            isMultipleSrFound = true;
                            command.CommandText = @"SELECT  dbo.Surveyors.SurveyorName, dbo.Surveyors.ContactNo, dbo.Surveyors.LoginName, dbo.Surveyors.AreaName,                                        dbo.SrRetailerLog.LogDateTime,  dbo.Person.PersonName AS DsrName
                                                    FROM dbo.SrRetailerLog INNER JOIN
                                                         dbo.Surveyors ON dbo.SrRetailerLog.SrId = dbo.Surveyors.SurveyorId INNER JOIN
                                                         dbo.Retailer ON dbo.SrRetailerLog.RetailerId = dbo.Retailer.RetailerId INNER JOIN
                                                         dbo.Person ON dbo.Retailer.DsrId = dbo.Person.PersonId
                                                    WHERE (dbo.SrRetailerLog.RetailerId = " + retailerId + ")";
                            da.Fill(dtSrRetailerLog);
                        }

                        command.Parameters.Clear();
                        command.CommandText = @"SELECT R.RetailerId
                                                FROM dbo.MonitoringOfficerRegion AS MOR INNER JOIN
                                                   dbo.Retailer AS R ON MOR.RegionId = R.RegionId
                                                WHERE (MOR.MoId = "+ moId +") AND (R.RetailerId = "+ retailerId +")";
                        isMoAlignedWithThisRetailer = command.ExecuteScalar();
                        connection.Close();

                        command.CommandText = sqlInsertMoLog;
                        command.Parameters.Clear();
                        command.Parameters.Add("@MoId", SqlDbType.Int).Value = moId;
                        command.Parameters.Add("@LogDescription", SqlDbType.VarChar).Value = string.Format("Viewed Outlet {0} Details. Is MO Aligned {1}", retailerId,isMoAlignedWithThisRetailer);
                        connection.Open(); command.ExecuteNonQuery(); connection.Close();

                    }
                }
            }

            ViewBag.PosCategoryList = PosCategoryManager.PosCategoryList();
            ViewBag.PosStructureList = PosStructureManager.PosStructureList();
            ViewBag.VisitDayList = VisitDayManager.VisitDayList();
            ViewBag.ShopSignageList = ShopSignageManager.ShopSignageList();
            ViewBag.ShopTypeList = ShopTypeManager.ShopTypeList();

            ViewBag.Title = "Retailer Details";
            ViewBag.Retailer = dtRetailer;
            ViewBag.RetailerId = retailerId;
            ViewBag.MoId = moId;
            ViewBag.ElNumbers = dtElNumbers; ViewBag.SimPosCodes = dtSimPosCodes;
            ViewBag.SrRetailerLog = dtSrRetailerLog;
            ViewBag.ShowEditButton = showEditButton;


            if (dtRetailer.Rows.Count==0)
            {
                return View("RetailerNotFound");
            }
            else
            {
                if (isMoAlignedWithThisRetailer==null)
                {
                    return View("UnauthorizedAccess");
                }
                else
                {
                    return View("OutletDetails");
                }
            }
        }


        public ActionResult VerifyOutlet()
        {
            Int32 retailerId = Convert.ToInt32(Request.QueryString["retailerid"]);
            Int32 moId = Convert.ToInt32(Request.QueryString["moid"]);
            String conString = WebConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;
            using (SqlConnection connection=new SqlConnection(conString))
            {
                using (SqlCommand command=new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = "update Retailer Set IsVerifiedByRsp=@IsVerifiedByRsp, RspVerificationDateTime=@RspVerificationDateTime where RetailerId=@RetailerId";
                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("@IsVerifiedByRsp",true);
                    command.Parameters.AddWithValue("@RspVerificationDateTime",DateTime.Now);
                    command.Parameters.AddWithValue("@RetailerId", retailerId);
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();

                    command.CommandText = sqlInsertMoLog;
                    command.Parameters.Clear();
                    command.Parameters.Add("@MoId", SqlDbType.Int).Value = moId;
                    command.Parameters.Add("@LogDescription", SqlDbType.VarChar).Value = string.Format("Verified Retailer-{0}",retailerId);
                    connection.Open(); command.ExecuteNonQuery(); connection.Close();

                }
            }
            return RedirectToAction("Index", new { retailerid = retailerId, moid = moId});
        }

        public ActionResult UnVerifyOutlet()
        {
            Int32 retailerId = Convert.ToInt32(Request.QueryString["retailerid"]);
            Int32 moId = Convert.ToInt32(Request.QueryString["moid"]);

            String conString = WebConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;
            using (SqlConnection connection = new SqlConnection(conString))
            {
                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = "update Retailer Set IsVerifiedByRsp=@IsVerifiedByRsp, RspVerificationDateTime=@RspVerificationDateTime where RetailerId=@RetailerId";
                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("@IsVerifiedByRsp", false);
                    command.Parameters.AddWithValue("@RspVerificationDateTime", DBNull.Value);
                    command.Parameters.AddWithValue("@RetailerId", retailerId);
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();

                    command.CommandText = sqlInsertMoLog;
                    command.Parameters.Clear();
                    command.Parameters.Add("@MoId", SqlDbType.Int).Value = moId;
                    command.Parameters.Add("@LogDescription", SqlDbType.VarChar).Value = string.Format("UnVerified Retailer-{0}", retailerId);
                    connection.Open(); command.ExecuteNonQuery(); connection.Close();
                }
            }
            return RedirectToAction("Index", new { retailerid = retailerId, moid = moId });
        }


        //setting IsActive=true
        public ActionResult ActivateOutlet()
        {
            Int32 retailerId = Convert.ToInt32(Request.QueryString["retailerid"]);
            Int32 moId = Convert.ToInt32(Request.QueryString["moid"]);
            String conString = WebConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;
            using (SqlConnection connection = new SqlConnection(conString))
            {
                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = "update Retailer Set IsActive=@IsActive where RetailerId=@RetailerId";
                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("@IsActive", true);
                    command.Parameters.AddWithValue("@RetailerId", retailerId);
                    connection.Open();
                    command.ExecuteNonQuery();

                    //LogId and LogDateTime is autogerenated in database. So dont include in insert sql.
                    command.CommandText = "insert into RetailerEditByRspLog(RetailerId,UpdatedFieldName,PreviousValue,NewValue) values(@RetailerId,@UpdatedFieldName,@PreviousValue,@NewValue)";
                    command.Parameters.Clear();
                    command.Parameters.Add("@RetailerId", SqlDbType.Int).Value = retailerId;
                    command.Parameters.Add("@UpdatedFieldName", SqlDbType.VarChar).Value = "IsActive";
                    command.Parameters.Add("@PreviousValue", SqlDbType.VarChar).Value = "false";
                    command.Parameters.Add("@NewValue", SqlDbType.VarChar).Value = "true";
                    command.ExecuteNonQuery(); connection.Close();
                    connection.Close();

                    command.CommandText = sqlInsertMoLog;
                    command.Parameters.Clear();
                    command.Parameters.Add("@MoId", SqlDbType.Int).Value = moId;
                    command.Parameters.Add("@LogDescription", SqlDbType.VarChar).Value = string.Format("ReActivate Retailer-{0}", retailerId);
                    connection.Open(); command.ExecuteNonQuery(); connection.Close();
                }
            }
            return RedirectToAction("Index", new { retailerid = retailerId, moid = moId });
        }

        //setting IsActive=false
        public ActionResult InactivateOutlet()
        {
            Int32 retailerId = Convert.ToInt32(Request.QueryString["retailerid"]);
            Int32 moId = Convert.ToInt32(Request.QueryString["moid"]);

            String conString = WebConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;
            using (SqlConnection connection = new SqlConnection(conString))
            {
                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = "update Retailer Set IsActive=@IsActive,InactiveDateTime=@InactiveDateTime where RetailerId=@RetailerId";
                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("@IsActive", false);
                    command.Parameters.AddWithValue("@InactiveDateTime", DateTime.Now);
                    command.Parameters.AddWithValue("@RetailerId", retailerId);
                    connection.Open();
                    command.ExecuteNonQuery();

                    //LogId and LogDateTime is autogerenated in database. So dont include in insert sql.
                    command.CommandText = "insert into RetailerEditByRspLog(RetailerId,UpdatedFieldName,PreviousValue,NewValue) values(@RetailerId,@UpdatedFieldName,@PreviousValue,@NewValue)";
                    command.Parameters.Clear();
                    command.Parameters.Add("@RetailerId", SqlDbType.Int).Value = retailerId;
                    command.Parameters.Add("@UpdatedFieldName", SqlDbType.VarChar).Value = "IsActive";
                    command.Parameters.Add("@PreviousValue", SqlDbType.VarChar).Value = "true";
                    command.Parameters.Add("@NewValue", SqlDbType.VarChar).Value = "false";
                    command.ExecuteNonQuery(); connection.Close();
                    connection.Close();

                    command.CommandText = sqlInsertMoLog;
                    command.Parameters.Clear();
                    command.Parameters.Add("@MoId", SqlDbType.Int).Value = moId;
                    command.Parameters.Add("@LogDescription", SqlDbType.VarChar).Value = string.Format("InActivate Retailer-{0}", retailerId);
                    connection.Open(); command.ExecuteNonQuery(); connection.Close();
                }
            }
            return RedirectToAction("Index", new { retailerid = retailerId, moid = moId});
        }
    }
}