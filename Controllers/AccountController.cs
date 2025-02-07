using BooksEcommerce.Models;
using BooksEcommerce.Services;
using BooksEcommerce.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BooksEcommerce.Controllers
{
    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly EmailSender emailSender;
        public AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, EmailSender emailSender)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.emailSender = emailSender;
        }

        [Authorize]
        public IActionResult Index()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }


        // Register page - GET  
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // Register page - POST
        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
            };

            var result = await userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                // Generate email confirmation token
                var token = await userManager.GenerateEmailConfirmationTokenAsync(user);

                // Generate confirmation link
                var confirmationLink = Url.Action("ConfirmEmail", "Account",
                new { userId = user.Id, token }, Request.Scheme);

                try
                {
                    // Send confirmation email
                    var emailSent = await emailSender.SendEmailAsync(user.Email, "Confirm Your Email",
                    $"<p>Click <a href='{confirmationLink}'>here</a> to confirm your email.</p>");

                    if (emailSent)
                    {
                        TempData["Message"] = "Registration successful! Please check your email to confirm.";
                        return RedirectToAction("Login", "Account");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, $"Error sending email: {ex.Message}");
                }
            }

            // Add errors to model state if registration fails
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(model);
        }


        // Login page - GET
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // Login page - POST
        [HttpPost]
        public async Task<IActionResult> LoginAsync(LoginVM model)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByNameAsync(model.Email);
                if (user == null)
                {
                    ModelState.AddModelError("", "Invalid username or password.");
                    return View(model);
                }
                if (!user.EmailConfirmed)
                {
                    ModelState.AddModelError("", "Email not confirmed. Please check your inbox for the confirmation link.");
                    return View(model);
                }
                if (user != null)
                {
                    var result = await signInManager.PasswordSignInAsync(user, model.Password, false, false);
                    if (result.Succeeded)
                    {
                        var roles = await userManager.GetRolesAsync(user);
                        if (roles.Contains("Admin"))
                        {
                            return RedirectToAction("AllBooks", "Category");
                        }
                        else if (roles.Contains("User"))
                        {
                            return RedirectToAction("UserPage", "User");
                        }
                        else
                        {
                            return RedirectToAction("AllBooks", "Category");
                        }
                    }
                    else
                    {
                        TempData["LoginError"] = "Invalid login attempt. Please try again!";
                    }
                }
                else
                {
                    TempData["LoginError"] = "User not found. Please check your email.";
                }


            }
            return View(model);
        }


        // ConfirmEmail page - GET
        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                return View("Error", "Invalid email confirmation link.");

            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
                return View("Error", $"User with ID {userId} not found.");

            var result = await userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
                return View("ConfirmEmail");

            return View("Error", "Email confirmation failed.");
        }


        // GET: Forgot Password
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // POST: Forgot Password
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                TempData["AlertMessage"] = "If the email is registered, you will receive a reset link.";
                return RedirectToAction("ForgotPassword");
            }

            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var callbackUrl = Url.Action("ResetPassword", "Account",
                new { userId = user.Id, token = token }, protocol: HttpContext.Request.Scheme);

            await emailSender.SendEmailAsync(model.Email, "Reset Password",
                $"<p>Click <a href='{callbackUrl}'>here</a> to reset your password.</p>");

            TempData["AlertMessage"] = "If the email is registered, you will receive a reset link.";
            return RedirectToAction("ForgotPassword");
        }


        // GET: Reset Password
        [HttpGet]
        public IActionResult ResetPassword(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                ModelState.AddModelError(string.Empty, "Invalid password reset token.");
                return View("Error");
            }

            var model = new ResetPasswordVM { UserId = userId, Token = token };
            return View(model);
        }

        // POST: Reset Password
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                TempData["AlertMessage"] = "Invalid user.";
                return RedirectToAction("ResetPassword", new { userId = model.UserId, token = model.Token });
            }

            var saveResult = await userManager.UpdateAsync(user);

            if (!saveResult.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Failed to remove old password.");
                return View(model);
            }
            var result = await userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
            if (result.Succeeded)
            {
                TempData["AlertMessage"] = "Password has been reset successfully.";
                return RedirectToAction("ForgotPassword", "Account");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }


        // Logout - POST
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("AllBooks", "Category");
        }


        [Authorize]
        // Display the user profile
        public async Task<IActionResult> DisplayProfile()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }
            var model = new UserProfileVM
            {
                Username = user.UserName,
                Email = user.Email,
                Name = user.Name,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address
            };
            return View(model);
        }

        // Show the edit profile form
        [HttpGet]
        public async Task<IActionResult> UpdateProfile()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }
            var model = new UserProfileVM
            {
                Name = user.Name,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address
            };
            return View(model);
        }

        // Handle user profile update
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UpdateProfile(UserProfileVM model)
        {

            if (!ModelState.IsValid)
            {
                return View("DisplayProfile", model);
            }

            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            user.Name = model.Name;
            user.PhoneNumber = model.PhoneNumber;
            user.Address = model.Address;

            var result = await userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                await signInManager.RefreshSignInAsync(user);
                TempData["SuccessMessage"] = "Userdata updated successfully!";
                TempData["RedirectUrl"] = Url.Action("DisplayProfile", "Account");
            }
            else
            {
                ViewBag.Message = "Error updating profile.";
            }

            return View("DisplayProfile", model);
        }
    
    }
}
