using AAUP_LabMaster.EntityManager;
using AAUP_LabMaster.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AAUP_LabMaster.Controllers
{
    public class UserController : Controller
    {
        private readonly ClientManager clientManager;
        private readonly SupervisourManager supervisourManager;
        private readonly EquipmentManager equipmentManager;
        private readonly LabManager labManager;
        public UserController(ClientManager context, SupervisourManager supervisourManager,EquipmentManager equipmentManager,LabManager labManager)
        {
            clientManager = context;
            this.supervisourManager = supervisourManager;
            this.equipmentManager = equipmentManager;
            this.labManager = labManager;
        }
     
        public IActionResult Dashboard()
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return RedirectToAction("Login", "Account");

            var user = clientManager.GetMyData();
            if (user == null)
                return RedirectToAction("Login", "Account");

            return View(user);
        }

        [Authorize] // Ensure only authenticated users can access
        [HttpGet]
        public IActionResult ViewAllEquipments(string searchString, bool isPartial = false) // isPartial default to false
        {
            // Debugging output for role (keep this for now, it's helpful)
            var roleFromClaim = User.FindFirst(ClaimTypes.Role)?.Value;
            var roleFromIsInRole = User.IsInRole("Supervisour") ? "Supervisour" : null;
            Console.WriteLine($"Role from Claim: {roleFromClaim}");
            Console.WriteLine($"IsInRole('Supervisour'): {User.IsInRole("Supervisour")}");
            ViewBag.UserRole = roleFromClaim ?? roleFromIsInRole;

            IEnumerable<Equipment> equipments;

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                equipments = equipmentManager.GetAllEquipments()
                                              .Where(e => e.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                                                          e.Description.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                                              .ToList();
                ViewData["CurrentFilter"] = searchString; // To keep the search term in the input
            }
            else
            {
                equipments = equipmentManager.GetAllEquipments();
            }

            // Determine if this is an AJAX request or if the isPartial flag is set
            // The fetch request from JS will set isPartial=true
            if (isPartial || Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_EquipmentTable", equipments.ToList()); // Return ONLY the table HTML
            }

            // This path is for the initial full page load (e.g., direct navigation to /User/ViewAllEquipments)
            // You would typically still pass the data to the main view, but the main view's JS
            // will handle rendering the table initially.
            return View(equipments.ToList());
        
}

        // [HttpGet]
        // public IActionResult RequestLab()
        // {
        //     return View();
        // }

        [HttpGet]
        public IActionResult RequestLab(int? equipmentId)
        {
            if (equipmentId.HasValue)
            {
                var equipment = equipmentManager.GetEquipmentById(equipmentId.Value);
                ViewBag.SelectedEquipment = equipment;
            }

            return View();
        }

        [HttpPost]
        public IActionResult MakeBook(string Equipment, DateTime date,string note)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return RedirectToAction("Login", "Account");
            }

           

                clientManager.MakeBooking(Equipment, note, date);


            TempData["Message"] = $"Equipment '{Equipment}' requested for {date.ToShortDateString()} at {date.TimeOfDay}";
            return RedirectToAction("MyRequests", "User");
        }

        [HttpPost]
        public JsonResult CheckAvailability(string labName, DateTime date)
        {
            var result = clientManager.checEquipment(labName, date);

            bool isAvailable = result != null && result.Count == 1 && result[0] == "Available";
            string suggestion = "";

            if (!isAvailable)
            {
                if (result == null)
                {
                    suggestion = "Equipment not found";
                }
                else if (result.Count > 0)
                {
                    suggestion = "Suggested times: " + string.Join(", ", result);
                }
                else
                {
                    suggestion = "No available times today.";
                }
            }

            return Json(new
            {
                available = isAvailable,
                suggestion = isAvailable ? "" : suggestion
            });
        }

        [HttpGet]
        public IActionResult MyRequests()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var requests = clientManager.GetMyBookings();
            Console.WriteLine("requests.Count: " + requests.Count);
            return View(requests);
        }




        // [Authorize(Roles = "Supervisour")]
        // public IActionResult ViewAvailableEquipments()
        // {
        //     var equipments = equipmentManager.GetAllEquipments()
        //                                     .Where(e => e.status == Equipment.Availability.Available)
        //                                     .ToList();
        //     ViewBag.UserRole = User.FindFirst(ClaimTypes.Role)?.Value;
        //     return View(equipments);
        // }

       [Authorize(Roles = "Client,Supervisour")]
        public IActionResult ViewAvailableEquipments()
        {
            var equipments = equipmentManager.GetAllEquipments();
            ViewBag.UserRole = User.FindFirst(ClaimTypes.Role)?.Value;
            return View(equipments);
        }


        // [Authorize(Roles = "Supervisour")]
        // public IActionResult ViewAvailableEquipments()
        // {
        //     // Get equipment list from your service
        //     var equipments = equipmentManager.GetAllEquipments();

        //     // Set user role in ViewBag if needed
        //     ViewBag.UserRole = User.FindFirst(ClaimTypes.Role)?.Value;

        //     // Return the correct model type
        //     return View(equipments);
        // }


        public IActionResult Notifications()
        {
            var notifications = clientManager.GetMyNotifications();
            return View(notifications);
        }
       
        
        [HttpPost]
        public IActionResult MarkAsRead()
        {
           clientManager.MarkNotes();

            return RedirectToAction("Notifications");
        }


        public IActionResult LabsPage()
        {
            var labs =clientManager.GetAllLabs();

            Console.WriteLine($"Labs count: {labs.Count}");
            foreach (var lab in labs)
            {
                Console.WriteLine($"- {lab.Name}: {lab.Description}");
            }
            return View(labs);
        }

        public IActionResult FormPage()
        {
            return View();
        }

  

    }
}
