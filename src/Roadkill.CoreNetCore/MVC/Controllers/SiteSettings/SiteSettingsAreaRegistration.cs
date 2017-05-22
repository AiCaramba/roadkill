namespace Roadkill.Core.Mvc.Controllers
{
	public class SiteSettingsAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
				return "SiteSettings";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
			// "/Settings" (used by the view routing as the controller is called Settings)
			context.MapRoute(
				"SiteSettings_Default",
				"Settings",
				new { controller = "Settings", action = "Index", id = UrlParameter.Optional });

			// "/SiteSettings/{controller}/{action}/{id}"
			context.MapRoute(
				"SiteSettings_Controller",
				"SiteSettings/{controller}/{action}/{id}",
				new { controller = "Settings", action = "Index", id = UrlParameter.Optional }
			);
        }
    }
}