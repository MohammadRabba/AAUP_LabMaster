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

        public void MakeBooking(Booking appointment)
        {
            context.Bookings.Add(appointment);
            context.SaveChanges();
            Console.WriteLine("Appointment created successfully.");
        }
    }
}
