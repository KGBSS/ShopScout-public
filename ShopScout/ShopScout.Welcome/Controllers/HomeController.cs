using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ShopScout.Welcome.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            var cookie = Request.Cookies["isBaseUrlRedirecting"];
            
            if (cookie != null && cookie == "True")
                return Redirect("https://app.shopscout.hu");
            return View();
        }
    }
}
