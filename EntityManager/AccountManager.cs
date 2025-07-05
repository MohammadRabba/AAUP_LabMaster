using AAUP_LabMaster.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


namespace AAUP_LabMaster.EntityManager
{
    public class AccountManager
    {


        private readonly ApplicationDbContext dbcontext;

        public AccountManager(ApplicationDbContext context)
        {
            dbcontext = context;
        }
        public bool Register(UserDTO user, out string message)
        {
            using var transaction = dbcontext.Database.BeginTransaction();

            try
            {
                if (dbcontext.Users.Any(x => x.Email == user.Email))
                {
                    message = "Email already registered.";
                    return false;
                }

                User newUser;

                if (user.SelectedRoleName == "Admin")
                {
                    newUser = new Admin
                    {
                        FullName = user.FullName,
                        Email = user.Email,
                        Password = user.Password,
                        PhoneNumber = user.PhoneNumber,
                        Role = user.SelectedRoleName
                    };
                    dbcontext.Admins.Add((Admin)newUser);
                }
                else if (user.SelectedRoleName == "Client")
                {
                    newUser = new Client
                    {
                        FullName = user.FullName,
                        Email = user.Email,
                        Password = user.Password,
                        PhoneNumber = user.PhoneNumber,
                        Role = user.SelectedRoleName,
                        type = (Client.Type)user.type
                    };
                    dbcontext.Clients.Add((Client)newUser);
                }
                else if (user.SelectedRoleName == "Supervisour")
                {
                    newUser = new Supervisour
                    {
                        FullName = user.FullName,
                        Email = user.Email,
                        Password = user.Password,
                        PhoneNumber = user.PhoneNumber,
                        Role = user.SelectedRoleName,
                        Specialist = user.Specialist
                    };
                    dbcontext.Supervisours.Add((Supervisour)newUser);
                }
                else
                {
                    message = "Invalid role specified.";
                    return false;
                }

                dbcontext.SaveChanges();

                if (user.SelectedRoleName == "Supervisour")
                {
                    var lab = new Lab
                    {
                        Name = "Empty Lab",
                        Description = "Empty Lab",
                        SupervisorId = newUser.Id,
                                                SupervisourId = newUser.Id

                    };

                    dbcontext.Labs.Add(lab);
                    dbcontext.SaveChanges();
                }

                transaction.Commit();
                message = "User registered successfully!";
                return true;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                message = $"Registration failed: {ex.Message}";
                return false;
            }
        }

        public bool ForgetPassword(string email, string phoneMumber,string name)
        {
            var user = dbcontext.Users.FirstOrDefault(u => u.Email == email&&u.PhoneNumber==phoneMumber&&u.FullName==name);
            if (user == null)
            {
                return false; // User not found
            }
      
            return true;
        }
        public bool AddForgetPassword(string email, string password)
        {
            var user = dbcontext.Users.FirstOrDefault(u => u.Email == email );
            if (user == null)
            {
                return false; // User not found
            }
            user.Password = password; // Update the password
            dbcontext.SaveChanges(); // Save changes to the database
            return true;
        }
        public string? Authenticate(LoginDTO login)
        {
            var user = dbcontext.Users
                .FirstOrDefault(u => u.Email == login.Email && u.Password == login.Password);

            if (user == null)
                return null;

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Role, user.Role ), 
        new Claim("ClientId", user.Id.ToString()),
        new Claim(ClaimTypes.Name, user.FullName ?? user.Email)
    };

            // Create security key and signing credentials
            var key = "sdfsdfkasdfajsfkLsdfsdfkasdfajsfkL"; // Move this to config or secret manager
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // Generate the token
            var token = new JwtSecurityToken(
                expires: DateTime.UtcNow.AddHours(1),
                claims: claims,
                signingCredentials: signingCredentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public User? Login(LoginDTO login)
        {
            var user = dbcontext.Users
                .FirstOrDefault(u => u.Email == login.Email && u.Password == login.Password);

            return user;  // returns null if not found or invalid login
        }
        public User? GetUserByEmail(string id)
        {
            var user = dbcontext.Users
                .FirstOrDefault(u => u.Email == id);
            return user;  // returns null if not found
        }
        public void UpdateUser(int id, UserDTO newuser)
        {
            var user = dbcontext.Users.FirstOrDefault(x => x.Id == id);
            if (user == null)
                throw new Exception("User not found");

            user.Email = newuser.Email;
            user.Password = newuser.Password;
            user.FullName = newuser.FullName;
            user.PhoneNumber = newuser.PhoneNumber;

            user.Role = newuser.SelectedRoleName;

            dbcontext.SaveChanges();
        }

    }
}