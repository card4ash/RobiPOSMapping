using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace RobiPosMapper.Controllers
{
    public class SrDetailsController : Controller
    {
        // this controller provides a quick access to the sr information.
        // its only for internal use.
        public ActionResult Index()
        {
            String sql = string.Empty;
            if (Request.QueryString["id"]==null)
            {
                sql = "Select * from Surveyors where SurveyorId>0 order by SurveyorId ASC";
            }
            else
            {
                Int32 srId = Convert.ToInt32(Request.QueryString["id"]);
                sql = "Select * from Surveyors where SurveyorId="+ srId +" order by SurveyorId ASC";
            }

            String conString = WebConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;
            String surveyor = String.Empty;
            DataTable dt = new DataTable();
            using (SqlConnection connection=new SqlConnection(conString ))
            {
                using (SqlCommand command=new SqlCommand(sql,connection))
                {
                    using (SqlDataAdapter da=new SqlDataAdapter(command))
                    {
                        connection.Open();
                        da.Fill(dt); connection.Close();
                    }

                }
            }

            foreach (DataRow row in dt.Rows)
            {
                foreach (DataColumn column in dt.Columns)
                {
                    surveyor += "<b>" + column.ColumnName.ToString() + ":</b>    " + row[column].ToString() + "<br/>";
                }

                surveyor += "<br/><br/>";
            }

            ViewBag.SurveyorDetails = surveyor;
            return View("SurveyorDetails");
        }
	}
}