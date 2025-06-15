using AAUP_LabMaster.EntityDTO;
using AAUP_LabMaster.Models;
using Microsoft.EntityFrameworkCore;
using System.Numerics;
using static AAUP_LabMaster.Models.Equipment;

namespace AAUP_LabMaster.EntityManager
{
    public class AdminManager
    {
        private readonly ApplicationDbContext context;

        public AdminManager(ApplicationDbContext dbcontext)
        {
            context = dbcontext;
        }
        public User? getUserByEmail(string email)
        {
            return context.Users.FirstOrDefault(u => u.Email == email);
        } 
        public bool RemoveBooking(int id)
        {

            var booking = context.Bookings.Include(b => b.Client).FirstOrDefault(b => b.Id == id);
            if (booking == null)
                return false;

            var notification = new Notification
            {
                UserId = booking.ClientId,
                Message = $"Your booking for {booking.Equipment.Lab.Name} on {booking.Date.ToShortDateString()} at {booking.Date.TimeOfDay} was deleted by admin.",
                DateCreated = DateTime.Now
            };
            context.Notifications.Add(notification);

            context.Bookings.Remove(booking);
            context.SaveChanges();
            return true;
        }
        public bool EditBooking(int id,BookingDTO booking,string cientName,String EquepmentName) {
            {

                var newBooking = context.Bookings.FirstOrDefault(x => x.Id == id);
                newBooking.Description = booking.Description;
                newBooking.Date = booking.Date;
                newBooking.Price = booking.Price;
                newBooking.Notes = booking.Notes;
                var newClient=context.Clients.FirstOrDefault(x => x.FullName== cientName);
                var newEquip=context.Equipments.FirstOrDefault(x => x.Name== EquepmentName);
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
        public List<User> getAllUsers()
        {

            return context.Users.ToList();
        }
        public List<Lab> getAllLabs()
        {

            return context.Labs.ToList();
        }
        public List<Booking> getAllBooking()
        {

            return context.Bookings.ToList();
        }
        public void AddUser(UserDTO user)
        {
            User doc;
            try
            {
                if (user.SelectedRoleName == "Admin")
                {
                    doc = new Admin
                    {
                        FullName = user.FullName,
                        Email = user.Email,
                        Password = user.Password,
                        PhoneNumber = user.PhoneNumber,
                        Role = user.SelectedRoleName
                    };
                    context.Admins.Add((Admin)doc);
                }
                else if (user.SelectedRoleName == "Client")
                {
                    doc = new Client
                    {
                        FullName = user.FullName,
                        Email = user.Email,
                        Password = user.Password,
                        PhoneNumber = user.PhoneNumber,
                        Role = user.SelectedRoleName
                    };
                    context.Clients.Add((Client)doc);
                }
                else // Supervisour
                {
                    doc = new Supervisour
                    {
                        FullName = user.FullName,
                        Email = user.Email,
                        Password = user.Password,
                        PhoneNumber = user.PhoneNumber,
                        Role = user.SelectedRoleName,
                        Specialist = user.Specialist // Make sure UserDTO has this property
                    };
                    context.Supervisours.Add((Supervisour)doc);
                }

                context.Users.Add(doc);
                context.SaveChanges();
                Console.WriteLine("User added successfully.");

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error adding user: " + ex.Message);
            }
        }
        public void UpdateUser(UserDTO user)
        {
            var newUser = context.Users.FirstOrDefault(x => x.Email == user.Email);
            try
            {
            

                    newUser.FullName = user.FullName;
                    newUser.Email = user.Email;
                    newUser.Password = user.Password;
                    newUser.PhoneNumber = user.PhoneNumber;
                newUser.Role = user.SelectedRoleName;

                context.SaveChanges();
                Console.WriteLine("User Updated successfully.");

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error adding user: " + ex.Message);
            }
        }

        public void AddLab(LabDTO lab,string SupervisourName,List<string> equipments)
        {

            
            var supervisor=context.Supervisours.FirstOrDefault(x=>x.FullName == SupervisourName);
            var newlab = new Lab { Name = lab.Name, Description = lab.Description, Supervisour=supervisor,SupervisorId=supervisor.Id};

            foreach (var equip in equipments)
            {
                var selectedEquip = context.Equipments.FirstOrDefault(x => x.Name == equip);
                newlab.Equipment.Add(selectedEquip);
            }
            context.Labs.Add(newlab);
            context.SaveChanges();
            Console.WriteLine("Lab added successfully.");
        }
        public bool UpdateLab(int id, LabDTO lab)
        {

            {

                var newLab = context.Labs.FirstOrDefault(x => x.Id == id);
                if (newLab != null)
                {
                    newLab.Name = lab.Name;
                    newLab.Description = lab.Description;

                    context.SaveChanges();
                    return true;
                }
                return false;
            }
        }
        public void RemoveLab( string LabName)
        {
            var exsistingLab = context.Labs.FirstOrDefault(x => x.Name == LabName);
            if (exsistingLab != null) {
                context.Labs.Remove(exsistingLab);
                context.SaveChanges();
                Console.WriteLine("Lab Removed successfully.");
            }

            else
            {
                Console.WriteLine("Lab Dont Exsist.");

            }
        }
    }
}
