using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RobiPosMapper.Controllers
{
    public class ErrorLogsController : Controller
    {
        //
        // GET: /ErrorLogs/
        public ActionResult Index()
        {
            int counter = 0;
            string line;
            string strLogs=String.Empty;
            // Read the file and display it line by line.
            System.IO.StreamReader file = new System.IO.StreamReader(Server.MapPath(@"~/App_Data/ErrorLogs/ErrorLogs.txt"));


            strLogs = file.ReadToEnd().Replace("\n", "<br />");
            //while ((line = file.ReadLine()) != null)
            //{
            //   // Console.WriteLine(line);
            //    strLogs += line + Environment.NewLine;
            //    counter++;
            //}

            file.Close();
            //ViewBag.ErrorLogs = System.IO.File.ReadAllText(Server.MapPath(@"~/App_Data/ErrorLogs/ErrorLogs.txt"));

            ViewBag.ErrorLogs = strLogs;
            return View();
        }
	}
}