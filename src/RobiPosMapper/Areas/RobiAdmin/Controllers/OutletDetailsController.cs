using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;



namespace RobiPosMapper.Areas.RobiAdmin.Controllers
{
    public class OutletDetailsController : Controller
    {
        //
        // GET: /RobiAdmin/OutletDetails/
        public ActionResult Index()
        {
            Int32 retailerId = Convert.ToInt32(Request.QueryString["id"]);

            String conString = WebConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;

            DataTable dtRetailer = new DataTable(); //Consists every information about retailer except EL Numbers & Sim Pos Codes
            DataTable dtElNumbers = new DataTable();
            DataTable dtSimPosCodes = new DataTable();
            DataTable dtSrRetailerLog = new DataTable();
            DataTable dtRetailerWorkLogs = new DataTable(); // Contains all records from SrRetailerLog for this Retailer
            Boolean isMultipleSrFound = false;

            using (SqlConnection connection=new SqlConnection(conString))
            {
                using (SqlCommand command=new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = "Select * from viewRetailerList where RetailerId="+ retailerId  +"";
                    using (SqlDataAdapter da=new SqlDataAdapter(command))
                    {
                        connection.Open();
                        da.Fill(dtRetailer);
                        command.CommandText = "SELECT ElMsisdn FROM ElMsisdn where RetailerId="+ retailerId +"";
                        da.Fill(dtElNumbers);
                        command.CommandText= "SELECT SimPosCode  FROM SimPosCode where RetailerId="+ retailerId +"";
                        da.Fill(dtSimPosCodes);
                        command.CommandText = "select LogDateTime, SrId from SrRetailerLog Where RetailerId="+ retailerId +" order by LogDateTime ASC";
                        da.Fill(dtRetailerWorkLogs);
                        command.CommandText = "select Count(distinct(SrId)) as Qty from SrRetailerLog where RetailerID="+ retailerId +"  group by RetailerId  Having Count(distinct(SrId))>1";
                        object objIsMultipleSr = command.ExecuteScalar();
                        if (objIsMultipleSr!=null)
                        {
                            isMultipleSrFound = true;
                            command.CommandText = @"SELECT  dbo.Surveyors.SurveyorName, dbo.Surveyors.ContactNo, dbo.Surveyors.LoginName, dbo.Surveyors.AreaName, dbo.SrRetailerLog.LogDateTime,  dbo.Person.PersonName AS DsrName
                              FROM         dbo.SrRetailerLog INNER JOIN
                                dbo.Surveyors ON dbo.SrRetailerLog.SrId = dbo.Surveyors.SurveyorId INNER JOIN
                                dbo.Retailer ON dbo.SrRetailerLog.RetailerId = dbo.Retailer.RetailerId INNER JOIN
                                dbo.Person ON dbo.Retailer.DsrId = dbo.Person.PersonId
                              WHERE     (dbo.SrRetailerLog.RetailerId = " + retailerId +")";
                            da.Fill(dtSrRetailerLog);
                        }
                        connection.Close();
                    }
                }
            }


            ViewBag.UserName = Request.QueryString["user"].ToString();
            ViewBag.Title = dtRetailer.Rows[0]["RetailerName"].ToString();
            ViewBag.Retailer = dtRetailer;
            ViewBag.ElNumbers = dtElNumbers; ViewBag.SimPosCodes = dtSimPosCodes;
            ViewBag.SrRetailerLog = dtSrRetailerLog;
            ViewBag.RetailerWorkLogs = dtRetailerWorkLogs;
            return View("OutletDetails");
        }
	}
}