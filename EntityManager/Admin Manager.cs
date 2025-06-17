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
        public List<User> getAllUsers()
        {

            return context.Users.ToList();
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
                        Specialist = user.Specialist 
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

        public void RemoveUser(int id)
        {
            var user = context.Users.FirstOrDefault(u => u.Id == id);
            if (user != null)
            {
                context.Users.Remove(user);
                context.SaveChanges();
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

    
    }
}
