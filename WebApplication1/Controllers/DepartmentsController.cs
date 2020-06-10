using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers
{
    public class DepartmentsController : Controller
    {
        public IActionResult List()
        {
            //return View();
            return Json(new { path = "departments/list" });
        }

        public IActionResult Details()
        {
            //return View();
            return Json(new { path = "departments/details" });

        }
    }
}
