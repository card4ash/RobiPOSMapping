using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;

namespace RobiPosMapper.Areas.RSP.Controllers
{
    public class OutletEditController : Controller
    {
        //This controller intends to edit retailer comes from OutletDetails view
        public JsonResult UpdateRetailerName(FormCollection data)
        {
            try
            {
                Int32 retailerId = Convert.ToInt32(data["RetailerId"]);
                String newRetailerName = HttpUtility.UrlDecode(data["RetailerName"].ToString()).Trim();
                String userIp = data["UserIp"].ToString();

                String conString = WebConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;

                using (SqlConnection connection = new SqlConnection(conString))
                {
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandText = "select RetailerName from Retailer where RetailerId="+ retailerId +"";
                        connection.Open();
                        string oldRetailerName = command.ExecuteScalar().ToString();
                        command.CommandText = "update Retailer set RetailerName='"+ newRetailerName +"' Where RetailerId="+ retailerId +"";
                        command.ExecuteNonQuery();

                        //LogId and LogDateTime is autogerenated in database. So dont include in insert sql.
                        command.CommandText = "insert into RetailerEditByRspLog(RetailerId,UpdatedFieldName,PreviousValue,NewValue,UserIp) values(@RetailerId,@UpdatedFieldName,@PreviousValue,@NewValue,@UserIp)";
                        command.Parameters.Clear();
                        command.Parameters.Add("@RetailerId", SqlDbType.Int).Value=retailerId;
                        command.Parameters.Add("@UpdatedFieldName", SqlDbType.VarChar).Value="RetailerName";
                        command.Parameters.Add("@PreviousValue", SqlDbType.VarChar).Value=oldRetailerName;
                        command.Parameters.Add("@NewValue", SqlDbType.VarChar).Value=newRetailerName;
                        command.Parameters.Add("@UserIp", SqlDbType.VarChar).Value = userIp;

                        command.ExecuteNonQuery(); connection.Close(); 
                    }
                }

                return Json("Success", JsonRequestBehavior.AllowGet);
            }
            catch (Exception Ex)
            {
                return Json(Ex.Message, JsonRequestBehavior.AllowGet);
            }

        }



        public JsonResult UpdateAddress(FormCollection data)
        {
            try
            {
                Int32 retailerId = Convert.ToInt32(data["RetailerId"]);
                String newRetailerName = HttpUtility.UrlDecode(data["Address"].ToString()).Trim();
                String userIp = data["UserIp"].ToString();

                String conString = WebConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;

                using (SqlConnection connection = new SqlConnection(conString))
                {
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandText = "select Address from Retailer where RetailerId=" + retailerId + "";
                        connection.Open();
                        string oldRetailerName = command.ExecuteScalar().ToString();
                        command.CommandText = "update Retailer set Address='" + newRetailerName + "' Where RetailerId=" + retailerId + "";
                        command.ExecuteNonQuery();

                        //LogId and LogDateTime is autogerenated in database. So dont include in insert sql.
                        command.CommandText = "insert into RetailerEditByRspLog(RetailerId,UpdatedFieldName,PreviousValue,NewValue,UserIp) values(@RetailerId,@UpdatedFieldName,@PreviousValue,@NewValue,@UserIp)";
                        command.Parameters.Clear();
                        command.Parameters.Add("@RetailerId", SqlDbType.Int).Value = retailerId;
                        command.Parameters.Add("@UpdatedFieldName", SqlDbType.VarChar).Value = "Address";
                        command.Parameters.Add("@PreviousValue", SqlDbType.VarChar).Value = oldRetailerName;
                        command.Parameters.Add("@NewValue", SqlDbType.VarChar).Value = newRetailerName;
                        command.Parameters.Add("@UserIp", SqlDbType.VarChar).Value = userIp;

                        command.ExecuteNonQuery(); connection.Close();
                    }
                }

                return Json("Success", JsonRequestBehavior.AllowGet);
            }
            catch (Exception Ex)
            {
                return Json(Ex.Message, JsonRequestBehavior.AllowGet);
            }

        }


        //
        public JsonResult UpdateLookupId(FormCollection data)
        {
            try
            {
                Int32 retailerId = Convert.ToInt32(data["RetailerId"]);
                String fieldName = data["FieldName"].ToString();

                Int32 newId = Convert.ToInt32(data[fieldName]);
                String userIp = data["UserIp"].ToString();
                
                String conString = WebConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;

                using (SqlConnection connection = new SqlConnection(conString))
                {
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandText = "select "+ fieldName +" from Retailer where RetailerId=" + retailerId + "";
                        connection.Open();
                        string oldRetailerName = command.ExecuteScalar().ToString();
                        command.CommandText = "update Retailer set "+ fieldName +"=" + newId + " Where RetailerId=" + retailerId + "";
                        command.ExecuteNonQuery();

                        //LogId and LogDateTime is autogerenated in database. So dont include in insert sql.
                        command.CommandText = "insert into RetailerEditByRspLog(RetailerId,UpdatedFieldName,PreviousValue,NewValue,UserIp) values(@RetailerId,@UpdatedFieldName,@PreviousValue,@NewValue,@UserIp)";
                        command.Parameters.Clear();
                        command.Parameters.Add("@RetailerId", SqlDbType.Int).Value = retailerId;
                        command.Parameters.Add("@UpdatedFieldName", SqlDbType.VarChar).Value = fieldName;
                        command.Parameters.Add("@PreviousValue", SqlDbType.VarChar).Value = oldRetailerName;
                        command.Parameters.Add("@NewValue", SqlDbType.VarChar).Value = newId.ToString();
                        command.Parameters.Add("@UserIp", SqlDbType.VarChar).Value = userIp;

                        command.ExecuteNonQuery(); connection.Close();
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