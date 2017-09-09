using Microsoft.AspNetCore.Mvc;

namespace CIB.OrderManagement.WebUI.Controllers
{
    public class MarketDataController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}