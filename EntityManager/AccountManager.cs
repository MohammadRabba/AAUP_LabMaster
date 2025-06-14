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
            var check = dbcontext.Users.FirstOrDefault(x => x.Email == user.Email);
            if (check != null)
            {
                message = "Email already registered.";
                return false;  // فشل التسجيل
            }

            var newUser = new User
            {
                Password = user.Password,
                Email = user.Email,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                Role = user.SelectedRoleName
            };

            dbcontext.Users.Add(newUser);
            dbcontext.SaveChanges();

            message = "User registered successfully!";
            return true;
        }

        public string? Authenticate(LoginDTO login)
        {
            var user = dbcontext.Users
                .FirstOrDefault(u => u.Email == login.Email && u.Password == login.Password);

            if (user == null)
                return null;

            // Create claims list with the user's role from the column
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Role, user.Role ?? ""),  // or user.Role if your property is called Role
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

        public void UpdateUser(int id, UserDTO newuser)
        {
            var user = dbcontext.Users.FirstOrDefault(x => x.Id == id);
            if (user == null)
                throw new Exception("User not found");

            user.Email = newuser.Email;
            user.Password = newuser.Password;
            user.FullName = newuser.FullName;
            user.PhoneNumber = newuser.PhoneNumber;

            // Assuming newuser.SelectedRoleName is a string with the new role name
            user.Role = newuser.SelectedRoleName;

            dbcontext.SaveChanges();
        }

    }
}