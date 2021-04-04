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
    public class DuplicatePhotosController : Controller
    {
        public ActionResult Index()
        {

            int moId = Convert.ToInt32(Request.QueryString["moid"]);

            //Source: viewDuplicatePhotos
//            string sqlDuplicateImages = @"SELECT Photo.GroupId, Photo.RetailerId, Photo.PhotoDateTime, R.SurveyorActivityDateTime, R.RetailerName, R.Address, R.DefaultPhotoName, dbo.RSP.RspName, S.LoginName AS SR, S.SurveyorName, R.DsrId, P.PersonName, Photo.IsRemovedLaterOn, Photo.ThisIsNotDuplicate, dbo.Region.RegionName, dbo.Area.AreaName, S.ContactNo
//                      FROM dbo.Area INNER JOIN
//                      dbo.DuplicatePhotos AS Photo INNER JOIN
//                      dbo.Retailer AS R ON Photo.RetailerId = R.RetailerId INNER JOIN
//                      dbo.RSP ON R.RspId = dbo.RSP.RspId INNER JOIN
//                      dbo.Person AS P ON R.DsrId = P.PersonId ON dbo.Area.AreaId = R.AreaId LEFT OUTER JOIN
//                      dbo.Surveyors AS S INNER JOIN
//                      dbo.Region ON S.RegionId = dbo.Region.RegionId ON R.SurveyorId = S.SurveyorId
//               WHERE (Photo.ThisIsNotDuplicate = 0) AND (S.SurveyorId IS NOT NULL) AND (S.RegionId IN
//                          (SELECT     RegionId
//                            FROM          dbo.MonitoringOfficerRegion
//                            WHERE      (MoId = " + moId + "))) ORDER BY Photo.GroupId ASC";


            //Cross-checked with DupliTwo table.
            string sqlDuplicateImages = @"SELECT     TOP (100) PERCENT Photo.GroupId, Photo.RetailerId, Photo.PhotoDateTime, R.SurveyorActivityDateTime, R.RetailerName, R.Address, R.DefaultPhotoName, dbo.RSP.RspName, S.LoginName AS SR,
                       S.SurveyorName, R.DsrId, P.PersonName, Photo.IsRemovedLaterOn, Photo.ThisIsNotDuplicate, dbo.Region.RegionName, dbo.Area.AreaName, S.ContactNo, 
                      CASE WHEN dbo.DupliTwo.RetailerId IS NULL THEN 'No' ELSE 'Yes' END AS IsRepeated
FROM         dbo.Area INNER JOIN
                      dbo.DuplicatePhotos AS Photo INNER JOIN
                      dbo.Retailer AS R ON Photo.RetailerId = R.RetailerId INNER JOIN
                      dbo.RSP ON R.RspId = dbo.RSP.RspId INNER JOIN
                      dbo.Person AS P ON R.DsrId = P.PersonId ON dbo.Area.AreaId = R.AreaId LEFT OUTER JOIN
                      dbo.DupliTwo ON Photo.RetailerId = dbo.DupliTwo.RetailerId LEFT OUTER JOIN
                      dbo.Surveyors AS S INNER JOIN
                      dbo.Region ON S.RegionId = dbo.Region.RegionId ON R.SurveyorId = S.SurveyorId
WHERE     (Photo.ThisIsNotDuplicate = 0) AND (S.SurveyorId IS NOT NULL) AND (S.RegionId IN
                          (SELECT     RegionId
                            FROM          dbo.MonitoringOfficerRegion
                            WHERE      (MoId = "+ moId +"))) ORDER BY Photo.GroupId";


            String conString = WebConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;
            DataTable tblDuplicateImages = new DataTable(); 
            using (SqlConnection connection = new SqlConnection(conString))
            {
                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = sqlDuplicateImages;
                    using (SqlDataAdapter da = new SqlDataAdapter(command))
                    {
                        connection.Open();
                        da.Fill(tblDuplicateImages);
                        connection.Close();
                    }
                }
            }

            ViewBag.Retailers = tblDuplicateImages;
            ViewBag.UserName = Request.QueryString["user"].ToString();
            ViewBag.MoId = moId;
            return View("DuplicatePhotos");
        }

        public ActionResult DuplicatePhotosForUs()
        {

            string sqlCustomWhereClause = string.Empty;
            if (Request.QueryString["groupid"]!=null)
            {
                int groupId = Convert.ToInt32(Request.QueryString["groupid"]);
                if (String.IsNullOrEmpty(sqlCustomWhereClause))
                {
                    sqlCustomWhereClause = " GroupId=" + groupId;
                }
                else
                {
                    sqlCustomWhereClause += " AND GroupId=" + groupId;
                }
            }

            //Source: viewDuplicatePhotos
            string sqlDuplicateImages = @"SELECT  GroupId, RetailerId, RetailerName, WorkedBySurveyor, DefaultPhotoName, RemovedBySurveyor, UploadDateTime, ResolvedBy, ResolvedDateTime, Address, ShopSignageName, PosStructureName
                                          FROM dbo.DuplicateRemovingIntegrityStatus ";


            if (!String.IsNullOrEmpty(sqlCustomWhereClause))
            {
                sqlDuplicateImages = sqlDuplicateImages + " Where " + sqlCustomWhereClause + " " + "  ORDER BY GroupId, RetailerId";
            }


            String conString = WebConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;
            DataTable tblDuplicateImages = new DataTable();
            using (SqlConnection connection = new SqlConnection(conString))
            {
                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = sqlDuplicateImages;
                    using (SqlDataAdapter da = new SqlDataAdapter(command))
                    {
                        connection.Open();
                        da.Fill(tblDuplicateImages);
                        connection.Close();
                    }
                }
            }

            ViewBag.Retailers = tblDuplicateImages;
            ViewBag.UserName = "skpaul";
            ViewBag.MoId = 0;
            return View("DuplicatePhotosForUs");
        }

        public ActionResult ViewByGroupId()
        {

          Int32 groupId = Convert.ToInt32(Request.QueryString["id"]); //group id
          string  sqlSelect = "Select * from viewDuplicatePhotos where GroupId="+ groupId +" order by GroupId, RetailerId";

            String conString = WebConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;
            DataTable dtRetailer = new DataTable(); //Consists every information about retailer except EL Numbers & Sim Pos Codes
            using (SqlConnection connection = new SqlConnection(conString))
            {
                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = sqlSelect;
                    using (SqlDataAdapter da = new SqlDataAdapter(command))
                    {
                        connection.Open();
                        da.Fill(dtRetailer);
                        connection.Close();
                    }
                }
            }

            ViewBag.Retailers = dtRetailer;
            return View("DuplicatePhotos");
        }


        public JsonResult RemoveDuplicate(FormCollection data)
        {
            try
            {
                Int32 retailerId = Convert.ToInt32(data["RetailerId"]);
                Int32 groupId = Convert.ToInt32(data["GroupId"]);
                Int32 moId = Convert.ToInt32(data["MoId"]);

                String conString = WebConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;

                using (SqlConnection connection = new SqlConnection(conString))
                {
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandText = "update DuplicatePhotos set ThisIsNotDuplicate=1,ResolvedDateTime='" + DateTime.Now + "', ResolvedBy=" + moId + " Where RetailerId=" + retailerId + " And GroupId=" + groupId + "";
                        connection.Open();
                        command.ExecuteNonQuery();
                        connection.Close();
                    }
                }

                return Json("Success", JsonRequestBehavior.AllowGet);
            }
            catch (Exception Ex)
            {
                return Json(Ex.Message, JsonRequestBehavior.AllowGet);
            }
        }
	}
}