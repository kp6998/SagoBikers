using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SagoBikers.Controllers
{
    public class RedirectController : Controller
    {
        // GET: Redirect
        public ActionResult SessionExp()
        {
            return View();
        }
        public ActionResult NotAuthorize()
        {
            return View();
        }
    }
}