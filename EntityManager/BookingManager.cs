using AAUP_LabMaster.EntityDTO;
using AAUP_LabMaster.Models;
using Microsoft.EntityFrameworkCore;
using System.Net;
using static AAUP_LabMaster.Models.Equipment;

namespace AAUP_LabMaster.EntityManager
{
    public class BookingManager
    {
        private readonly ApplicationDbContext context;
        private readonly NotificationManager noteManager;
        public BookingManager(ApplicationDbContext context, NotificationManager noteManager)
        {
            this.context = context;
            this.noteManager = noteManager;
        }

        public bool RemoveBooking(int id)
        {

            var booking = context.Bookings.FirstOrDefault(b => b.Id == id);
            if (booking == null)
                return false;

            var notification = new Notification
            {
                UserId = booking.ClientId,Subject= "Booking Deleted",
                Body = $"Your booking for {booking.Equipment.Lab.Name} on {booking.Date.ToShortDateString()} at {booking.Date.TimeOfDay} was deleted by admin.",
                DateCreated = DateTime.Now
            };
noteManager.SendNote(notification);
            context.Bookings.Remove(booking);
            context.SaveChanges();
            return true;
        }

        public List<Booking> getBookingById(int supervisorId)
        {
            return context.Bookings
                .Include(b => b.Client)              // Load Client data
                .Include(b => b.Equipment)           // Load Equipment
                    .ThenInclude(e => e.Lab)         // Load Lab to access SupervisorId
                .Where(b => b.Equipment.Lab.SupervisorId == supervisorId)
                .ToList();
        }
        public bool EditBooking(int id, BookingDTO booking, string cientName, String EquepmentName)
        {
            {

                var newBooking = context.Bookings.FirstOrDefault(x => x.Id == id);
                newBooking.Description = booking.Description;
                newBooking.Date = booking.Date;
                newBooking.Price = booking.Price;
                newBooking.Notes = booking.Notes;
                var newClient = context.Clients.FirstOrDefault(x => x.FullName == cientName);
                var newEquip = context.Equipments.FirstOrDefault(x => x.Name == EquepmentName);
                if (!(newEquip.status == Availability.Available))
                {
                    Console.WriteLine("Doesnt Exsist now");
                    return false;
                }
                else if (newEquip.Quantity == 1)
                {

                    newBooking.Equipment = newEquip;
                    newBooking.Client = newClient;
                    newBooking.EquipmentId = newEquip.Id;
                    newBooking.ClientId = newClient.Id;
                    newEquip.status = Availability.nonAvailable;

                }
                else
                {
                    newBooking.Equipment = newEquip;
                    newEquip.Quantity--;
                    newBooking.Client = newClient;
                    newBooking.EquipmentId = newEquip.Id;
                    newBooking.ClientId = newClient.Id;
                }
                noteManager.SendNote(new Notification
                {
                    UserId = newClient.Id,
                    Subject = "Booking Updated",
                    Body = $"Your booking for {newEquip.Name} in {newEquip.Lab.Name} on {newBooking.Date.ToShortDateString()} at {newBooking.Date.TimeOfDay} was updated.",
                    DateCreated = DateTime.Now
                });
                context.SaveChanges();
                return true;

            }
        }
        public string GetBookings()
        {
            var mostUsedLab = context.Bookings.GroupBy(b => b.Equipment.Lab.Name)
                         .OrderByDescending(g => g.Count())
                         .Select(g => g.Key)
                         .FirstOrDefault() ?? "N/A";
            return mostUsedLab;
        }

        public List<Booking> getAllBooking()
        {

            return context.Bookings.ToList();
        }
        // public void updateBookingStatus(int bookingId, Booking.BookStatus status)
        // {
        //     var booking = context.Bookings.FirstOrDefault(x => x.Id == bookingId);
        //     booking.status = status;
        //     context.SaveChanges();
        // }
        
        public void updateBookingStatus(int bookingId, Booking.BookStatus status)
        {
            var booking = context.Bookings.FirstOrDefault(x => x.Id == bookingId);
            booking.status = status; // Sets status to Approved (1), Rejected (2), etc.
            if( status == Booking.BookStatus.Approved)
            {
                noteManager.SendNote(new Notification
                {
                    UserId = booking.ClientId,
                    Subject = "Booking Updated",
                    Body = $"Your booking for {booking.Equipment.Name} in {booking.Equipment.Lab.Name} on {booking.Date.ToShortDateString()} at {booking.Date.TimeOfDay} was Approved.",
                    DateCreated = DateTime.Now
                });
            }
            else if (status == Booking.BookStatus.Rejected)
            {
                noteManager.SendNote(new Notification
                {
                    UserId = booking.ClientId,
                    Subject = "Booking Updated",
                    Body = $"Your booking for {booking.Equipment.Name} in {booking.Equipment.Lab.Name} on {booking.Date.ToShortDateString()} at {booking.Date.TimeOfDay} was Rejected.",
                    DateCreated = DateTime.Now
                });
            }
        
            context.SaveChanges();
        }

    }
}
