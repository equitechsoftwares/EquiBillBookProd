using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EquiBillBook.Controllers.Admin
{
    public class AdminReportsController : Controller
    {
        // GET: AdminReports
        public ActionResult SalesSummary()
        {
            return View();
        }
    }
}