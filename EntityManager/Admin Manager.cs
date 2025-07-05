using AAUP_LabMaster.EntityDTO;
using AAUP_LabMaster.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Numerics;
using static AAUP_LabMaster.Models.Equipment;

namespace AAUP_LabMaster.EntityManager
{
    public class AdminManager
    {

        private readonly ApplicationDbContext context;
        private readonly LabManager labManager;

        public AdminManager(ApplicationDbContext dbcontext, LabManager labManager)
        {
            context = dbcontext;
            this.labManager = labManager;
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
                    context.Supervisours.Add((Supervisour)doc); var lab = new Lab
                    {
                        Name = "Empty Lab",
                        Description = "Empty Lab",
                        SupervisorId = doc.Id,
                        SupervisourId = doc.Id

                    };
                    context.Labs.Add(lab);
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
                if (user is Admin)
                {
                    context.Admins.Remove((Admin)user);
                }
                else if (user is Client)
                {
                    context.Clients.Remove((Client)user);
                }
                else if (user is Supervisour)
                {
                    var Labs = context.Labs.FirstOrDefault(l => l.Supervisour.Email == user.Email);
                    foreach (var lab in context.Labs)
                    {

                        labManager.RemoveLab(lab.Id);
                    }

                }
                context.Supervisours.Remove((Supervisour)user);
            }
            context.Users.Remove(user);
            context.SaveChanges();
        
}

        public async Task UpdateUserAsync(UserDTO user)
        {
            // Find existing user with tracking
            var existingUser = await context.Users
                .Include(u => u is Supervisour ? (u as Supervisour).Labs : null)
                .FirstOrDefaultAsync(x => x.Email == user.Email);

            if (existingUser == null)
            {
                throw new ArgumentException("User not found");
            }

            using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                // Handle role change from supervisor to another role
                if (existingUser.Role == "Supervisour" && user.SelectedRoleName != "Supervisour")
                {
                    // Remove associated labs
                    var labs = await context.Labs
                        .Where(l => l.SupervisourId == existingUser.Id&& l.SupervisorId == existingUser.Id)
                        .ToListAsync();

                    foreach (var lab in labs)
                    {
                        await labManager.RemoveLabAsync(lab.Id);
                    }
                }

                // Update basic user properties
                existingUser.FullName = user.FullName;
                existingUser.Password = user.Password; // Should be hashed in production
                existingUser.PhoneNumber = user.PhoneNumber;

                // Handle role change
                if (existingUser.Role != user.SelectedRoleName)
                {
                    existingUser.Role = user.SelectedRoleName;

                    // If changing to supervisor, create a lab if none exists
                    if (user.SelectedRoleName == "Supervisour")
                    {
                        var hasLab = await context.Labs
                            .AnyAsync(l => l.SupervisourId == existingUser.Id&& l.SupervisorId == existingUser.Id);

                        if (!hasLab)
                        {
                            var lab = new Lab
                            {
                                Name = "Empty Lab",
                                Description = "Empty Lab",
                                SupervisourId = existingUser.Id,
                                                                SupervisorId = existingUser.Id

                            };
                            await context.Labs.AddAsync(lab);
                        }
                    }
                }

                // Handle supervisor-specific updates
                if (existingUser is Supervisour supervisor && user.SelectedRoleName == "Supervisour")
                {
                    supervisor.Specialist = user.Specialist;
                }

                // Handle client-specific updates
                if (existingUser is Client client && user.SelectedRoleName == "Client")
                {
                    client.type = (Client.Type)user.type;
                }

                await context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"Error updating user: {ex.Message}");
                throw;
            }
        }

        public void UpdateUser(UserDTO user)
        {
            // Find existing user with tracking
            var existingUser = context.Users.FirstOrDefault(x => x.Email == user.Email);
            if (existingUser == null)
            {
                throw new ArgumentException("User not found");
            }

            using var transaction = context.Database.BeginTransaction();

            try
            {
                // Handle role change - this requires special handling for TPH inheritance
                if (existingUser.Role != user.SelectedRoleName)
                {
                    // First remove from current role-specific table
                    if (existingUser is Admin)
                    {
                        context.Admins.Remove((Admin)existingUser);
                    }
                    else if (existingUser is Client)
                    {
                        context.Clients.Remove((Client)existingUser);
                    }
                    else if (existingUser is Supervisour)
                    {
                        // Remove associated labs first
                        var labs = context.Labs.Where(l => l.SupervisourId == existingUser.Id).ToList();
                        foreach (var lab in labs)
                        {
                            labManager.RemoveLab(lab.Id);
                        }
                        context.Supervisours.Remove((Supervisour)existingUser);
                    }

                    // Create new instance of the correct type
                    User newUserInstance = user.SelectedRoleName switch
                    {
                        "Admin" => new Admin(),
                        "Client" => new Client(),
                        "Supervisour" => new Supervisour(),
                        _ => throw new ArgumentException("Invalid role specified")
                    };

                    // Copy properties
                    newUserInstance.Id = existingUser.Id;
                    newUserInstance.FullName = user.FullName;
                    newUserInstance.Email = user.Email;
                    newUserInstance.Password = user.Password;
                    newUserInstance.PhoneNumber = user.PhoneNumber;
                    newUserInstance.Role = user.SelectedRoleName;

                    // Handle role-specific properties
                    if (newUserInstance is Supervisour supervisor && user.SelectedRoleName == "Supervisour")
                    {
                        supervisor.Specialist = user.Specialist;

                        // Create lab if changing to supervisor
                        var lab = new Lab
                        {
                            Name = "Empty Lab",
                            Description = "Empty Lab",
                            SupervisourId = newUserInstance.Id,SupervisorId = newUserInstance.Id
                        };
                        context.Labs.Add(lab);
                    }
                    else if (newUserInstance is Client client && user.SelectedRoleName == "Client")
                    {
                        client.type = (Client.Type)user.type;
                    }

                    // Add to the appropriate table
                    context.Users.Remove(existingUser);
                    context.Users.Add(newUserInstance);

                    if (newUserInstance is Admin admin)
                    {
                        context.Admins.Add(admin);
                    }
                    else if (newUserInstance is Client client)
                    {
                        context.Clients.Add(client);
                    }
                    else if (newUserInstance is Supervisour supervisour)
                    {
                        context.Supervisours.Add(supervisour);
                    }
                }
                else
                {
                    // No role change, just update properties
                    existingUser.FullName = user.FullName;
                    existingUser.Password = user.Password;
                    existingUser.PhoneNumber = user.PhoneNumber;

                    if (existingUser is Supervisour supervisor && user.SelectedRoleName == "Supervisour")
                    {
                        supervisor.Specialist = user.Specialist;
                    }
                    else if (existingUser is Client client && user.SelectedRoleName == "Client")
                    {
                        client.type = (Client.Type)user.type;
                    }
                }

                context.SaveChanges();
                transaction.Commit();
                Console.WriteLine("User updated successfully.");
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Console.WriteLine($"Error updating user: {ex.Message}");
                throw;
            }
        }
    }
}
