﻿using Microsoft.AspNetCore.Mvc;

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
    }
}
