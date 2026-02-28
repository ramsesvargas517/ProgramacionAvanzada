using ProgramacionAvanzadaG8.EntityFramework;
using System.Web.Mvc;

namespace ProgramacionAvanzadaG8.Controllers
{
    public class HomeController : Controller
    {
        // GET: /Home/Index  o  /
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }
    }
}
