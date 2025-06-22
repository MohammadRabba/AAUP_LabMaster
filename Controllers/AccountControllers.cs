using AAUP_LabMaster.EntityManager;
using AAUP_LabMaster.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;


namespace AAUP_LabMaster.Controllers
{
    public class AccountController : Controller
    {
        private readonly AccountManager accountManager;

        public AccountController(AccountManager context)
        {
            accountManager = context;
        }

        [HttpGet]
        public IActionResult Signup()
        {
            return View(new UserDTO());
        }

        [HttpPost]
        public IActionResult Signup(UserDTO user)
        {
            if (!ModelState.IsValid)
            {
                TempData["Message"] = "Please fill in all required fields correctly.";
                return View(user);
            }

            bool isRegistered = accountManager.Register(user, out string message);

            if (!isRegistered)
            {
                TempData["Message"] = message;
                return View(user);
            }

            TempData["Message"] = "User registered successfully! Please log in.";
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View(new LoginDTO());
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginDTO user)
        {
            if (!ModelState.IsValid)
            {
                TempData["LoginError"] = "Please enter both email and password.";
                return View(user);
            }

            User existingUser = accountManager.Login(user);

            if (existingUser != null)
            {
                TempData["Message"] = $"Welcome back, {existingUser.FullName}!";

                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, existingUser.Id.ToString()),
            new Claim(ClaimTypes.Name, existingUser.FullName ?? ""),
            new Claim(ClaimTypes.Email, existingUser.Email ?? ""),
            new Claim(ClaimTypes.Role, existingUser.Role ?? "")
        };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                if (existingUser.Role == "Admin")
                {
                    return RedirectToAction("Dashboard", "Admin");
                }
                else if (existingUser.Role == "Client" || existingUser.Role == "Guest" || existingUser.Role == "Supervisour")
                {
                    return RedirectToAction("Dashboard", "User");
                }
                else
                {
                    TempData["LoginError"] = "Role not recognized.";
                    return View("Login", user);
                }
            }

            TempData["LoginError"] = "Invalid email or password.";
            return View("Login", user);
        }
        [HttpGet]
        public IActionResult AdminDashboard()
        {
            return View();
        }

        [HttpGet]
        public IActionResult UserDashboard()
        {
            return View();
        }
    }
}
