using AAUP_LabMaster.EntityManager;
using AAUP_LabMaster.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AAUP_LabMaster.Controllers
{
    public class UserController : Controller
    {
        private readonly ClientManager clientManager;

        public UserController(ClientManager context)
        {
            clientManager = context;
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

        [HttpGet]
        public IActionResult RequestLab()
        {
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


        [HttpGet]
        public IActionResult ViewAvailableEquipments()
        {
            var equipments = clientManager.GetAvailableEquipment();
            return View(equipments);
        }

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

        //[HttpPost]
        //public IActionResult SubmitFullForm(
        //    string lab,
        //    string equipment,
        //    DateTime datetime,
        //    string purpose,
        //    string notes)
        //{
        //    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //    if (!int.TryParse(userIdClaim, out int userId))
        //    {
        //        return RedirectToAction("Login", "Account");
        //    }
        //    var booking = new Booking
        //    {
        //        //LabName = equipment.la,
        //        //Equipment = equipment,
        //        //Date = datetime.Date,
        //        //Time = datetime.ToString("HH:mm"),
        //        //Description = purpose,
        //        //UserId = userId,
        //        //Notes = notes
        //    };
        //    _context.Bookings.Add(booking);

        //    var notification = new Notification
        //    {
        //        Message = $"Booking confirmed for {lab} on {datetime.ToShortDateString()} at {datetime.ToShortTimeString()}",
        //        DateCreated = DateTime.Now,
        //        UserId = userId
        //    };
        //    _context.Notifications.Add(notification);
        //    _context.SaveChanges();
        //    TempData["Message"] = "Lab booking submited successfully!";
        //    return RedirectToAction("Dashboard", "User");
        //}

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

    //    [HttpGet]
    //    public IActionResult SeedLabs()
    //    {
    //        var existingLabs = _context.Labs.ToList();

    //        var allLabs = new List<Lab>
    //{
    //    new Lab { Name = "Chemistry Lab", Description = "Includes lab benches, glassware, and chemical storage." },
    //    new Lab { Name = "Networking Lab", Description = "Equipped with routers, switches, and topology setup." },
    //    new Lab { Name = "Physics Lab", Description = "Contains motion sensors, optics kits, and experimental setups." },
    //    new Lab { Name = "Multimedia Lab", Description = "Equipped with audio/video editing tools and green screen setup." }
    //};

    //        foreach (var lab in allLabs)
    //        {
    //            var existing = existingLabs.FirstOrDefault(l => l.Name == lab.Name);
    //            if (existing != null)
    //            {

    //                existing.Description = lab.Description;
    //            }
    //            else
    //            {

    //                _context.Labs.Add(lab);
    //            }
    //        }

    //        _context.SaveChanges();
    //        return Content("Labs seeded or updated successfully. Current count: " + _context.Labs.Count());
    //    }


    }
}
