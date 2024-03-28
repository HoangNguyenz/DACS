using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HocAspMVC4_Test.Areas.AdminCP.Controllers
{
    [Area("AdminCP")]
    [Authorize(Roles = "Admin")]
    public class AdminCPController : Controller
    {

        [Route("/admincp/")]
        public IActionResult Index()
        {
            return View();
        }
    }
}