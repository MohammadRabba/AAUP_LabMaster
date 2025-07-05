using AAUP_LabMaster.EntityDTO;
using AAUP_LabMaster.EntityManager;
using AAUP_LabMaster.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
        private readonly SupervisourManager superManager;

        public AdminController(AdminManager context, BookingManager bookingManager, LabManager labManager, SupervisourManager superManager)
        {
            adminManager = context;
            this.bookingManager = bookingManager;
            this.labManager = labManager;
            this.superManager = superManager;
        }
      

        public IActionResult DeleteUser(int id)
        {
           

            adminManager.RemoveUser(id);
            TempData["Message"] = "User deleted successfully.";
            return RedirectToAction("UserManagement");
        }

        public IActionResult AddUser()
        {
            return View(new UserDTO());
        }
        
             public IActionResult UpdateUser()
        {
            return View(new UserDTO());
        }

        [HttpGet]
        public IActionResult UpdateUser(int id)
        {
            var user = adminManager.getAllUsers().FirstOrDefault(u => u.Id == id);
            if (user == null)
                return NotFound();

            var dto = new UserDTO
            {
                FullName = user.FullName,
                Email = user.Email,
                Password = user.Password,
                ConfirmPassword = user.Password,
                PhoneNumber = user.PhoneNumber,
                SelectedRoleName = user.Role,
                Specialist = user is Supervisour sup ? sup.Specialist : null,
                type = user is Client cli ? cli.type : null
            };

            return View(dto);
        }

        [HttpPost]
        public IActionResult UpdateUser(UserDTO user)
        {
            if (!ModelState.IsValid)
            {
                return View(user);
            }

            try
            {
                adminManager.UpdateUser(user);
                TempData["SuccessMessage"] = "User updated successfully";
                return RedirectToAction("UserManagement");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View(user);
            }
        }

        [HttpPut]
        public IActionResult UpdateUser123(UserDTO user)
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
            var users = labManager.getAllLabs();
            return View(users);
        }
        [HttpGet]
        public IActionResult AddLab()
        {
            PopulateSupervisorsDropdown();
            return View(new LabDTO());
        }
        [HttpPost]
        public IActionResult AddLab(LabDTO labDto)
        {
            PopulateSupervisorsDropdown(); 

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please correct the errors and try again.";
                return View("AddLab", labDto);
            }

            try
            {
                labManager.AddLab(labDto);

                TempData["Message"] = "Lab added successfully.";
                return RedirectToAction("LabSettings");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding lab: {ex.Message}");
                TempData["ErrorMessage"] = $"Error adding lab: {ex.Message}";
                return View("AddLab", labDto); 
            }
        }

        [HttpGet]
        public IActionResult EditLab(int id)
        {
            var lab = labManager.GetLabById(id); 
            if (lab == null)
            {
                return NotFound();
            }

            return View(lab); 
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditLab(Lab lab, List<string> EquipmentNames) 
        {

            //if (EquipmentNames != null && EquipmentNames.Any(string.IsNullOrWhiteSpace))
            //{
            //    ModelState.AddModelError("EquipmentNames", "Equipment names cannot be empty.");
            //}

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please correct the errors and try again.";
                return View(lab); 
            }

            try
            {
              
                var labDto = new LabDTO
                {
                    Name = lab.Name,
                    Description = lab.Description,
                    SelectedSupervisorId = lab.Supervisour.FullName,
                    EquipmentNames = EquipmentNames ?? new List<string>()
                };

                bool updateSuccess = labManager.UpdateLab(labDto);

                if (!updateSuccess)
                {
                    TempData["ErrorMessage"] = "Failed to update lab. It might not exist or another error occurred.";
                    return View(lab);
                }

                TempData["Message"] = "Lab updated successfully.";
                return RedirectToAction("LabSettings");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating lab: {ex.Message}");
                TempData["ErrorMessage"] = $"Error updating lab: {ex.Message}";
                return View(lab);
            }
        }


        private void PopulateSupervisorsDropdown(int? selectedId = null) 
        {
            ViewBag.SupervisorsList = superManager.GetAllSupervisours()
                .Select(u => new SelectListItem
                {
                    Value = u.FullName,
                    Text = u.FullName,
                    Selected = (selectedId.HasValue && u.Id == selectedId.Value) 
                })
                .ToList();
        }

        public IActionResult DeleteLab(int id)
        {
            labManager.RemoveLab(id);

            TempData["Message"] = "Lab deleted successfuly.";
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
