using Microsoft.AspNetCore.Mvc;
using AAUP_LabMaster.Models;
using System.Security.Claims;

namespace AAUP_LabMaster.Controllers
{
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Dashboard()
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return RedirectToAction("Login", "Account");

            var user = _context.Users.FirstOrDefault(u => u.Email == email);
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
        public IActionResult SubmitRequest(string labName, DateTime date, TimeSpan time)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var booking = new Booking
            {
                //LabName = labName,
                //Date = date,
                //Time = time.ToString(@"hh\:mm"),
                //Description = "Booking Requested",
                //UserId = userId
            };

            _context.Bookings.Add(booking);

            var Notification = new Notification
            {
                Message = $"Your booking request for {labName} on {date.ToShortDateString()} at {time} has been submited.",
                DateCreated = DateTime.Now,
                UserId = userId
            };
            _context.Notifications.Add(Notification);
            _context.SaveChanges();

            TempData["Message"] = $"Lab '{labName}' requested for {date.ToShortDateString()} at {time}";
            return RedirectToAction("MyRequests", "User");
        }

        [HttpPost]
        public JsonResult CheckAvailability(string labName,
        DateTime date, string time)
        {
            bool isAvailable = !_context.Bookings.Any(b =>
            b.Equipment.Lab.Name == labName &&
            b.Date == date
            );

            string suggestedTime = "";
            if (!isAvailable)
            {
                var existingTimes = _context.Bookings.Where(b => b.Equipment.Lab.Name == labName && b.Date == date)
                .Select(b => b.Date.TimeOfDay).ToList();

                var allTimes = new List<string>
                {
                    "08:00", "08:30", "09:00", "09:30", "10:00", "10:30",
                    "11:00", "11:30", "12:00", "12:30", "01:00", "01:30",
                 "02:00", "02:30", "03:00", "03:30", "04:00"
                };
                //suggestedTime = allTimes.FirstOrDefault(t => !existingTimes.Contains(t)) ?? "Try another date";
            }
            return Json(new
            {
                available = isAvailable,
                suggestion = isAvailable ? "" : suggestedTime
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

            var requests = _context.Bookings.Where(b => b.ClientId == userId).ToList();
            Console.WriteLine("requests.Count: " + requests.Count);
            return View(requests);
        }


        [HttpGet]
        public IActionResult ViewAvailableEquipments()
        {
            var equipments = _context.Equipments.ToList();
            return View(equipments);
        }

        public IActionResult Notifications()
        {
            var userId =
            int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var notes = _context.Notifications
            .Where(n => n.UserId == userId).OrderByDescending(n => n.DateCreated).ToList();
            return View(notes);
        }

        [HttpPost]
        public IActionResult MarkAsRead(int id)
        {
            var notification = _context.Notifications.FirstOrDefault(n => n.Id == id);
            if (notification == null)
                return NotFound();

            notification.IsRead = true;
            _context.SaveChanges();

            return RedirectToAction("Notifications");
        }

        [HttpPost]
        public IActionResult SubmitFullForm(
            string lab,
            string equipment,
            DateTime datetime,
            string purpose,
            string notes)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return RedirectToAction("Login", "Account");
            }
            var booking = new Booking
            {
                //LabName = equipment.la,
                //Equipment = equipment,
                //Date = datetime.Date,
                //Time = datetime.ToString("HH:mm"),
                //Description = purpose,
                //UserId = userId,
                //Notes = notes
            };
            _context.Bookings.Add(booking);

            var notification = new Notification
            {
                Message = $"Booking confirmed for {lab} on {datetime.ToShortDateString()} at {datetime.ToShortTimeString()}",
                DateCreated = DateTime.Now,
                UserId = userId
            };
            _context.Notifications.Add(notification);
            _context.SaveChanges();
            TempData["Message"] = "Lab booking submited successfully!";
            return RedirectToAction("Dashboard", "User");
        }

        public IActionResult LabsPage()
        {
            var labs = _context.Labs.ToList();

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
    
    [HttpGet]
public IActionResult SeedLabs()
{
    var existingLabs = _context.Labs.ToList();

    var allLabs = new List<Lab>
    {
        new Lab { Name = "Chemistry Lab", Description = "Includes lab benches, glassware, and chemical storage." },
        new Lab { Name = "Networking Lab", Description = "Equipped with routers, switches, and topology setup." },
        new Lab { Name = "Physics Lab", Description = "Contains motion sensors, optics kits, and experimental setups." },
        new Lab { Name = "Multimedia Lab", Description = "Equipped with audio/video editing tools and green screen setup." }
    };

    foreach (var lab in allLabs)
    {
        var existing = existingLabs.FirstOrDefault(l => l.Name == lab.Name);
        if (existing != null)
        {
            
            existing.Description = lab.Description;
        }
        else
        {
            
            _context.Labs.Add(lab);
        }
    }

    _context.SaveChanges();
    return Content("Labs seeded or updated successfully. Current count: " + _context.Labs.Count());
}


    }
}
