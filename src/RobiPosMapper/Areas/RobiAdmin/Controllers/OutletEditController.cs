using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RobiPosMapper.Areas.RobiAdmin.Controllers
{
    public class OutletEditController : Controller
    {
       
        //This controller intends to edit retailer comes from OutletDetails view
        public JsonResult UpdateRetailerName(FormCollection data)
        {
            return Json("Success", JsonRequestBehavior.AllowGet);
        }
	}
}