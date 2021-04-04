using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RobiPosMapper.Areas.RSP.Controllers
{
    public class LoginController : Controller
    {
        //
        // GET: /Home/
        public ActionResult Index()
        {
            using (DataAccess.RobiPosMappingEntities db = new DataAccess.RobiPosMappingEntities())
            {
                
            }
            return View();
        }

        //Submits login info
        [HttpPost]
        public JsonResult Index(FormCollection data)
        {
            String LoginName = data["loginname"];
            String LoginPassword = data["password"];

            //data access
            using (var db = new DataAccess.RobiPosMappingEntities())
            {

                try
                {
                    int rspMsisdn = Convert.ToInt32(LoginName);
                    var user = (from c in db.RSPs where c.RspMsisdn == rspMsisdn select c).FirstOrDefault();

                    if (user == null)
                    {
                        return Json(new { result = "InvalidLoginName" }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        if (user.Password.ToString().Equals(LoginPassword, StringComparison.Ordinal))
                        {


                            Session["LoginName"] = user.RspName;
                            Session["RspId"] = user.RspId;
                            Session["AreaId"] = user.AreaId;
                            return Json(new { result = "Redirect", url = "/RSP/Home?rspid="+ user.RspId +"&user=" + LoginName + "&rsp="+ user.RspName +"" });
                        }
                        else
                        {
                            return Json(new { result = "InvalidPassword" }, JsonRequestBehavior.AllowGet);
                        }

                    }


                }
                catch (Exception ex)
                {
                    return Json("Error", JsonRequestBehavior.AllowGet);
                }



            }
            //data access
        }
    }
}