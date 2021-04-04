using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RobiPosMapper.Areas.Dsr.Controllers
{
    public class QrSearchController : Controller
    {
        //
        // GET: /Dsr/QrSearch/
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult SearchResult()
        {
            return Json("", JsonRequestBehavior.AllowGet);
        }
	}
}