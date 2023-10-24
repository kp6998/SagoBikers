using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace SagoBikers
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            //config.Formatters.JsonFormatter.SerializerSettings.MaxDepth = 2147483647;
            routes.MapMvcAttributeRoutes();
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Login", id = UrlParameter.Optional }
            );
           
        }
    }
}
