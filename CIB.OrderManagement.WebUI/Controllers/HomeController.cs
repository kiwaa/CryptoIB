using System.Threading.Tasks;
using CIB.Exchange.Model;
using CIB.OrderManagement.WebUI.Dto;
using CIB.OrderManagement.WebUI.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace CIB.OrderManagement.WebUI.Controllers
{
    public class HomeController : Controller
    {

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }

        //public IActionResult Cancel(long orderId)
        //{
        //    var order = _storage.Get(orderId);
        //    if (order == null)
        //        return NotFound();
        //    order.Cancel();
        //    return RedirectToAction("Index");
        //}

    }
}
