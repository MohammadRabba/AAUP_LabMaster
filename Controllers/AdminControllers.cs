using AAUP_LabMaster.EntityDTO;
using AAUP_LabMaster.EntityManager;
using AAUP_LabMaster.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace AAUP_LabMaster.Controllers
{
    public class AdminController : 
    Controller
    {
        private readonly AdminManager adminManager;

        public AdminController(AdminManager context)
        {
            adminManager = context;
        }

        public IActionResult Dashboard()
        {
            var email = TempData["UserEmail"] as string;

            if (string.IsNullOrEmpty(email))
                return RedirectToAction("Login", "Account");

            var user = adminManager.getUserByEmail(email);

            if (user == null)
                return RedirectToAction("Login", "Account");

            return View(user); 
        }

        public IActionResult BookingManagement()
        {
            var bookings = adminManager.getAllBooking();
            return View(bookings);
        }

         [HttpGet]
public IActionResult EditBooking()
{


    return View();
}

        [HttpPut]
        public IActionResult EditBooking(int id, BookingDTO booking, string cientName, String EquepmentName)
        {
            var boking = adminManager.EditBooking(id, booking, cientName, EquepmentName);
            if (boking == true)
            {
                TempData["Message"] = "Booking Updated Successfully.";

            }
            else
            {
                TempData["Message"] = "Error.";

            }

            return View(booking);
        }

        public IActionResult DeleteBooking(int id)
{   
   var deleted=adminManager.RemoveBooking(id);
            if (deleted == false) { return NotFound(); }
    TempData["Message"] = "Booking deleted successfully.";
    return RedirectToAction("BookingManagement");
}

        public IActionResult UserManagement()
        {
          var users = adminManager.getAllUsers();
            return View(users);
        }

        public IActionResult LabSettings()
        {
           var labs = adminManager.getAllLabs();
            return View(labs);
        }

        [HttpPost]
        public IActionResult AddLab(string labName,string Description, string supervisourName,List<string>Equepments)
        {
           


        
            var lab = new LabDTO
            {  
                Name = labName,
                Description= Description,
                
                //Status = "Available"
                };
            adminManager.AddLab(lab, supervisourName, Equepments);

           TempData["Message"] = "Lab added successfuly.";

           return RedirectToAction("LabSettings");

        } 
        
        
        public IActionResult DeleteLab(string id)
        {
            adminManager.RemoveLab(id);

            TempData["Message"] = "Lab deleted successfuly.";
           return RedirectToAction("LabSettings");
        } 
          
          
        [HttpGet]
        public IActionResult EditLab()
        {
           
           
           return View();
        } 

        [HttpPost]
        public IActionResult EditLab(int id, string name, string desc)
        {

            var lab = new LabDTO
            {
                Name = name,
                Description = desc,

                //Status = "Available"
            };
            var labs = adminManager.UpdateLab(id, lab);
            if (labs == false)
                return NotFound();

           TempData["Message"] = "Lab updated successfuly.";
           return RedirectToAction("LabSettings");
        } 

        public IActionResult Reports()
        {  
            var totalBookings =adminManager.getAllBooking().Count();
            var mostUsed = adminManager.GetBookings();

                ViewBag.TotalBookings = totalBookings;
                ViewBag.MostUsedLab = mostUsed;
            return View();
        }
    }
}
