using AAUP_LabMaster.EntityDTO;
using AAUP_LabMaster.EntityManager;
using AAUP_LabMaster.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;

namespace AAUP_LabMaster.Controllers
{
    [Authorize(Roles = "Admin")]

    public class AdminController : 
    Controller
    {
        private readonly AdminManager adminManager;
        private readonly BookingManager bookingManager;
        private readonly LabManager labManager;

        public AdminController(AdminManager context, BookingManager bookingManager, LabManager labManager)
        {
            adminManager = context;
            this.bookingManager = bookingManager;
            this.labManager = labManager;
        }
        public IActionResult AddUser()
        {
            return View(new UserDTO());
        }
        
             public IActionResult UpdateUser()
        {
            return View(new UserDTO());
        }
        [HttpPut]
        public IActionResult UpdateUser(UserDTO user)
        {
            if (!ModelState.IsValid)
            {
                return View(user);
            }

            adminManager.UpdateUser(user);
            TempData["Message"] = "User Updated successfully.";
            return RedirectToAction("UserManagement");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddUser(UserDTO user)
        {
            if (!ModelState.IsValid)
            {
                return View(user);
            }

            adminManager.AddUser(user);
            TempData["Message"] = "User added successfully.";
            return RedirectToAction("UserManagement");
        }
        public IActionResult Dashboard()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);

            if (string.IsNullOrEmpty(email))
                return RedirectToAction("Login", "Account");

            var user = adminManager.getUserByEmail(email);

            if (user == null)
                return RedirectToAction("Login", "Account");

            return View(user); 
        }

        public IActionResult BookingManagement()
        {
            var bookings = bookingManager.getAllBooking();
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
            var boking = bookingManager.EditBooking(id, booking, cientName, EquepmentName);
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
   var deleted= bookingManager.RemoveBooking(id);
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
           var labs = labManager.getAllLabs();
            return View(labs);
        }

        [HttpPost]
        public IActionResult AddLab(string labName, string Description, string supervisourName, string Equepments)
        {
            var equipmentList = string.IsNullOrWhiteSpace(Equepments)
                ? new List<string>()
                : Equepments.Split(',').Select(e => e.Trim()).Where(e => !string.IsNullOrEmpty(e)).ToList();

            var lab = new LabDTO
            {
                Name = labName,
                Description = Description,
            };
            labManager.AddLab(lab, supervisourName, equipmentList);

            TempData["Message"] = "Lab added successfully.";

            return RedirectToAction("LabSettings");
        }

        public IActionResult DeleteLab(string id)
        {
            labManager.RemoveLab(id);

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
            var labs = labManager.UpdateLab(id, lab);
            if (labs == false)
                return NotFound();

           TempData["Message"] = "Lab updated successfuly.";
           return RedirectToAction("LabSettings");
        } 

        public IActionResult Reports()
        {  
            var totalBookings = bookingManager.getAllBooking().Count();
            var mostUsed = bookingManager.GetBookings();

                ViewBag.TotalBookings = totalBookings;
                ViewBag.MostUsedLab = mostUsed;
            return View();
        }
    }
}
