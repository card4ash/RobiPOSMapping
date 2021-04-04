using System.Web.Mvc;

namespace RobiPosMapper.Areas.Dsr
{
    public class DsrAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Dsr";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "Dsr_default",
                "Dsr/{controller}/{action}/{id}",
                new { controller = "Login", action = "Index", id = UrlParameter.Optional }

                // new { action = "Index", id = UrlParameter.Optional },
                //new[] { "AcMonitoringSystem.Areas.Admin.Controllers" }
            );
        }
    }
}