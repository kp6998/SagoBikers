
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace SagoBikers
{
    
    public class CheckSessionFilter : ActionFilterAttribute
    {

        public override void OnActionExecuting(System.Web.Mvc.ActionExecutingContext filterContext)
        {
            string ExcludeAction = "SignUp|Login|CheckUserName|RegistrationLogin";
            string ActionName = filterContext.ActionDescriptor.ActionName;
            string strUserType = HttpContext.Current.Session["userId"] != null && HttpContext.Current.Session["userId"].ToString() != "" ? HttpContext.Current.Session["userId"].ToString() : "";
            if (!ExcludeAction.Contains(ActionName) && strUserType == "")
            {
                HttpContext.Current.Session.Clear();
                filterContext.HttpContext.Response.Redirect("/", true);
            }
        }

    }
}