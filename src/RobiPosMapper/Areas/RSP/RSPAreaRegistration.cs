using System.Web.Mvc;

namespace RobiPosMapper.Areas.RSP
{
    public class RSPAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "RSP";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "RSP_default",
                "RSP/{controller}/{action}/{id}",
                new { controller = "Login", action = "Index", id = UrlParameter.Optional },
                new[] { "RobiPosMapper.Areas.RSP.Controllers" }
            );
        }
    }
}