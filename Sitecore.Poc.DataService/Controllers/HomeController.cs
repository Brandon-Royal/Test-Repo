using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace Sitecore.Poc.DataService.Controllers
{
    public class HomeController : Controller
    {

        public HomeController() : this(GlobalConfiguration.Configuration)
        {
        }

        public HomeController(HttpConfiguration config)
        {
            Configuration = config;
        }

        public HttpConfiguration Configuration { get; private set; }

        // GET: Home
        public ActionResult Index()
        {
            ViewBag.DocumentationProvider = Configuration.Services.GetDocumentationProvider();
            return View(Configuration.Services.GetApiExplorer().ApiDescriptions);
        }
    }
}