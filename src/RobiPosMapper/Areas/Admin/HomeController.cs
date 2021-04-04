using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RobiPosMapper.Areas.Admin
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/
        public ActionResult Index()
        {
            return View();
        }

        //Submits login info
        [HttpPost]
        public JsonResult Index(FormCollection data)
        {
            String LoginName = data["loginname"];
            String LoginPassword = data["password"];

            //data access
            //using (var db = new Entities.OneBankEntities())
            //{
            //    Entities.User user = null;
            //    try
            //    {
            //        user = (from c in db.Users where c.LoginName == LoginName select c).FirstOrDefault();
            //    }
            //    catch (Exception)
            //    {
            //        return Json("Error", JsonRequestBehavior.AllowGet);
            //    }

            //    if (user == null)
            //    {
            //        return Json(new { result = "InvalidLoginName" }, JsonRequestBehavior.AllowGet);
            //    }
            //    else
            //    {
            //        if (user.LoginPassword.ToString().Equals(LoginPassword, StringComparison.Ordinal))
            //        {

            //            var employee = (from e in db.tblEmployees where e.EmployeeId == user.EmployeeId select new { FullName = e.EmployeeName }).FirstOrDefault();

            //            Session["UserId"] = user.UserId.ToString();
            //            Session["LoginName"] = LoginName;
            //            Session["UserTypeId"] = user.UserTypeId.ToString();
            //            Session["FullName"] = employee.FullName;

            //            db.LoginRecords.Add(new LoginRecord() { UserId = user.UserId }); db.SaveChanges();

            //            return Json(new { result = "Redirect", url = "/Dashboard?user=" + LoginName + "" });
            //        }
            //        else
            //        {
            //            return Json(new { result = "InvalidPassword" }, JsonRequestBehavior.AllowGet);
            //        }

            //    }

            //}
            //data access

            return Json("");
        }
    }
}