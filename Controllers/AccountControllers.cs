using AAUP_LabMaster.EntityDTO;
using AAUP_LabMaster.EntityManager;
using AAUP_LabMaster.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
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
        [HttpGet]
        public IActionResult ResetPassword()
        {
            if (TempData["ResetEmail"] == null)
            {
                TempData["ErrorMessage"] = "Session expired. Please try again.";
                return RedirectToAction("ForgotPassword");
            }

            var model = new ResetPasswordDTO
            {
                Email = TempData["ResetEmail"].ToString()
            };

            TempData.Keep("ResetEmail"); // keep it for post
            return View(model);
        }

        [HttpPost]
        public IActionResult ResetPassword(ResetPasswordDTO model)
        {
            if (!ModelState.IsValid)
            {
                TempData.Keep("ResetEmail");
                return View(model);
            }

            var user = accountManager.GetUserByEmail(model.Email);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("ForgotPassword");
            }

            // Update password
            user.Password = model.NewPassword; // Hash in real apps!
            accountManager.AddForgetPassword(user.Email,user.Password);

            TempData["Message"] = "Password reset successfully. Please login.";
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordDTO());
        }
        [HttpPost]
        public IActionResult ForgotPassword(ForgotPasswordDTO model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = accountManager.ForgetPassword(model.FullName, model.Email, model.PhoneNumber);

            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found. Please check your information.";
                return View(model);
            }

            // Store user ID or Email temporarily for use in reset page
            TempData["ResetEmail"] = model.Email;

            // إعادة التوجيه لصفحة إدخال كلمة المرور الجديدة
            return RedirectToAction("ResetPassword");
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

        // In your AccountController.cs (or wherever your Login POST action is)

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginDTO user)
        {
            if (!ModelState.IsValid)
            {
                TempData["LoginError"] = "Please enter both email and password.";
                return View(user);
            }

            User existingUser = null;
            try
            {
                existingUser = accountManager.Login(user);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login error for {user.Email}: {ex.Message}");
                TempData["LoginError"] = "An unexpected error occurred during login. Please try again later.";
                return View(user);
            }

            if (existingUser == null)
            {
                TempData["LoginError"] = "Invalid email or password.";
                return View(user);
            }

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, existingUser.Id.ToString()),
        new Claim(ClaimTypes.Name, existingUser.FullName ?? existingUser.Email ?? "User"),
        new Claim(ClaimTypes.Email, existingUser.Email ?? ""),
    };

            if (!string.IsNullOrEmpty(existingUser.Role))
            {
                claims.Add(new Claim(ClaimTypes.Role, existingUser.Role));
            }
            else
            {
                claims.Add(new Claim(ClaimTypes.Role, "Guest"));
                Console.WriteLine($"Warning: User {existingUser.Email} logged in without a defined role. Assigned 'Guest'.");
            }
            Console.WriteLine($"User {existingUser.Email} role: {(string.IsNullOrEmpty(existingUser.Role) ? "NULL/EMPTY" : existingUser.Role)}");
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
                new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7) });
            TempData["Message"] = $"Welcome back, {existingUser.FullName ?? existingUser.Email}!";

         
            switch (existingUser.Role) 
            {
                case "Admin":
                    return RedirectToAction("Dashboard", "Admin");
                case "Client":
                case "Guest":
                case "Supervisour":
                    return RedirectToAction("Dashboard", "User");
                default:
                    TempData["LoginError"] = "Your account role is not configured for a specific dashboard.";
                    // Log out the user immediately if the role is unrecognized to prevent unintended access
                    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    return View(user);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData["Message"] = "You have been logged out.";
            return RedirectToAction("Login", "Account"); // Redirect to the login page
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
