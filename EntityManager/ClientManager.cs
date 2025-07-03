using AAUP_LabMaster.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AAUP_LabMaster.EntityManager
{
    public class ClientManager
    {
        private readonly ApplicationDbContext context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ClientManager(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            this.context = context;
            _httpContextAccessor = httpContextAccessor;
        }
        public bool UpdateMyData(string fullName, string phoneNumber, string role)
        {
            var email = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email))
            {
                Console.WriteLine("Email claim not found.");
                return false;
            }

            var user = context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                Console.WriteLine("User not found.");
                return false;
            }

            user.FullName = fullName;
            user.PhoneNumber = phoneNumber;
            user.Role = role;

            context.SaveChanges();
            Console.WriteLine("User data updated successfully.");
            return true;
        }
        public User? GetMyData()
        {
            var email = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email))
            {
                Console.WriteLine("Email claim not found.");
                return null;
            }

            return context.Users.FirstOrDefault(u => u.Email == email);
        }
        public List<Equipment> GetAvailableEquipment()
        {


            return context.Equipments.Where(e => e.status == Equipment.Availability.Available)
                .ToList();


        }
        public List<Lab> GetAllLabs()
        {
            return context.Labs.ToList();
        }
        public void MarkNotes()
        {
            var userEmailString = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(userEmailString))
            {
                Console.WriteLine("User email not found.");
                return;
            }
            var user = context.Users.FirstOrDefault(u => u.Email == userEmailString);
            if (user == null)
            {
                Console.WriteLine("User not found.");
                return;
            }
            var notifications = context.Notifications.Where(n => n.UserId == user.Id && n.IsRead == false).ToList();
            foreach (var notification in notifications)
            {
                notification.IsRead = true;
            }
            context.SaveChanges();
            Console.WriteLine("Notification created successfully.");
        }
        public List<Notification> GetMyNotifications()
        {
            var idString = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(idString) || !int.TryParse(idString, out int userId))
            {
                Console.WriteLine("User ID claim not found or invalid.");
                return new List<Notification>();
            }

            return context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.DateCreated)
                .ToList();
        }
        public List<Booking> GetMyBookings()
        {
            var userIdString = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdString))
            {
                Console.WriteLine("User ID claim not found or user not authenticated.");
                return new List<Booking>();
            }

            if (int.TryParse(userIdString, out int userId))
            {
                return context.Bookings
                    .Include(b => b.Client)
                    .Where(b => b.Client != null && b.Client.Id == userId)
                    .ToList();
            }

            Console.WriteLine($"Unable to parse User ID: {userIdString}");
            return new List<Booking>();
        }
        public List<string>? checEquipment(string equipName, DateTime date)
        {
            var equipment = context.Equipments.FirstOrDefault(e => e.Name == equipName);
            if (equipment == null)
            {
                Console.WriteLine($"Equipment with Name {equipName} not found.");
                return null;
            }

            if (equipment.status != Equipment.Availability.Available)
            {
                Console.WriteLine($"Equipment with Name {equipName} is not available.");
                return new List<string> { "Not available" };
            }

            var bookings = context.Bookings
                .Where(b => b.EquipmentId == equipment.Id && b.Date.Date == date.Date)
                .Select(b => b.Date.TimeOfDay)
                .ToList();

            // Check if the requested time is already booked
            if (bookings.Contains(date.TimeOfDay))
            {
                Console.WriteLine($"Equipment with Name {equipName} is already booked at {date:t} on {date:yyyy-MM-dd}.");

                var suggestedTimes = new List<string>();
                for (int hour = 8; hour <= 17; hour++)
                {
                    var time = new TimeSpan(hour, 0, 0);
                    if (!bookings.Contains(time))
                    {
                        suggestedTimes.Add($"{date.Date.Add(time):t}");
                    }
                }

                if (suggestedTimes.Count == 0)
                    suggestedTimes.Add("No available times today.");

                return suggestedTimes;
            }

            return new List<string> { "Available" };
        }
        public void CancelBooking(int book)
        {
            var booking = context.Bookings.FirstOrDefault(x => x.Id == book);
            context.Bookings.Remove(booking);
            context.SaveChanges();
            Console.WriteLine("Booking  created successfully.");
        }
        public void MakeBooking(string equipmentName, string note, DateTime time)
        {
            var userEmailString = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Email);
            var userNameString = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Name);
            var equipment = context.Equipments.FirstOrDefault(e => e.Name == equipmentName);
            if (equipment == null)
            {
                Console.WriteLine($"Equipment '{equipmentName}' not found.");
                return;
            }
            else if (equipment.status == Equipment.Availability.Available)
            {
                
                    var book = new Booking
                    {
                        ClientId = context.Users.FirstOrDefault(u => u.Email == userEmailString)?.Id ?? 0,
                        EquipmentId = equipment.Id,
                        Date = time,
                        Equipment = equipment,
                        Notes = note,
                        Price = equipment.Price
                    };
                    context.Bookings.Add(book);
                    context.SaveChanges();
                    Console.WriteLine("Booking  created successfully.");

                    // var notificationManager = new NotificationManager();
                    // notificationManager.SendEmail(
                    //     userEmailString,
                    //     "Booking Confirmation",
                    //     $"Dear {userNameString}, your booking has been created."
                    // );
                }
             
        }
    }
}
