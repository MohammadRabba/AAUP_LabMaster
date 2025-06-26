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
            User doc;

            var check = dbcontext.Users.FirstOrDefault(x => x.Email == user.Email);
            if (check != null)
            {
                message = "Email already registered.";
                return false;  // فشل التسجيل
            }

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
                dbcontext.Admins.Add((Admin)doc);
            }
            else if (user.SelectedRoleName == "Client")
            {
                doc = new Client
                {
                    FullName = user.FullName,
                    Email = user.Email,
                    Password = user.Password,
                    PhoneNumber = user.PhoneNumber,
                    Role = user.SelectedRoleName,
                    type = (Client.Type)user.type
                };
                dbcontext.Clients.Add((Client)doc);
            }
            else 
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
                dbcontext.Supervisours.Add((Supervisour)doc);
            }

            dbcontext.Users.Add(doc);
            dbcontext.SaveChanges();
            dbcontext.SaveChanges();

            message = "User registered successfully!";
            return true;
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