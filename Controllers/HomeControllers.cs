using Microsoft.AspNetCore.Mvc;
using AAUP_LabMaster.Models;
namespace AAUP_LabMaster.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

       public IActionResult FAQ()
        {
            return View();
        }

        public IActionResult ContactUs()
        {
            return View();
        } 
         public IActionResult Labs()
        {
            return View();
        } 
         public IActionResult BookNow()
        {
            return View();
        } 
    }
}