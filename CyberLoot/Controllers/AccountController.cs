using CyberLoot.Models;
using CyberLoot.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace CyberLoot.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly IConfiguration configuration;

        public AccountController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager, IConfiguration configuration) 
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.configuration = configuration;
        }

        // Only shows the register page if not signed
        public IActionResult Register()
        {
            if (signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        // Creates the user when user submits
        [HttpPost]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {
            // if user is signed in it will redirect them to the home page.
            if (signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Index", "Home");
            }

            // Checks for validation errors and returns the errors to the user
            if (!ModelState.IsValid)
            {
                return View(registerDto);
            }

            // creates a new account
            var user = new ApplicationUser()
            {
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                UserName = registerDto.Email, // Used for authentication
                Email = registerDto.Email,
                PhoneNumber = registerDto.PhoneNumber,
                JoinedOn = DateTime.Now,
            };

            // checks if account can be created
            var result = await userManager.CreateAsync(user, registerDto.Password);

            if (result.Succeeded)
            {
                // successful user registration
                await userManager.AddToRoleAsync(user, "client");

                // signs in as the new user
                await signInManager.SignInAsync(user, false);

                return RedirectToAction("Index", "Home");
            }

            // The registration failed => show registration errors
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(registerDto);
        }

        // Logs user out and brings them back to the home page
        public async Task<IActionResult> Logout()
        {
            if (signInManager.IsSignedIn(User))
            {
                await signInManager.SignOutAsync();
            }

            return RedirectToAction("Index", "Home");
        }

        // if the user isn't signed in it will display the login page
        public IActionResult Login()
        {
            if (signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        // Login process for the user
        [HttpPost]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            if (signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Index", "Home");
            }

            if (!ModelState.IsValid)
            {
                return View(loginDto);
            }

            var result = await signInManager.PasswordSignInAsync(loginDto.Email, loginDto.Password,
                loginDto.RememberMe, false);

            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ViewBag.ErrorMessage = "Invalid login attempt.";
            }

            return View(loginDto);
        }

        // Displays current user's profile
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var appUser = await userManager.GetUserAsync(User);

            if (appUser == null)
            {
                return RedirectToAction("Index", "Home");
            }

            // gets the current user's information
            var profileDto = new ProfileDto()
            {
                FirstName = appUser.FirstName,
                LastName = appUser.LastName,
                Email = appUser.Email ?? "",
                PhoneNumber = appUser.PhoneNumber,
            };

            return View(profileDto);
        }

        // if user decides to edit their profile this update it
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Profile(ProfileDto profileDto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ErrorMessage = "Please fill all the required fields with valid values";
                return View(profileDto);
            }

            var appUser = await userManager.GetUserAsync(User);
            if (appUser == null)
            {
                return RedirectToAction("Index", "Home");
            }

            // Update user profile
            appUser.FirstName = profileDto.FirstName;
            appUser.LastName = profileDto.LastName;
            appUser.UserName = profileDto.Email;
            appUser.Email = profileDto.Email;
            appUser.PhoneNumber = profileDto.PhoneNumber;

            var result = await userManager.UpdateAsync(appUser);

            if (result.Succeeded)
            {
                ViewBag.SuccessMessage = "Profile Updated successfully";
            }
            else
            {
                ViewBag.ErrorMessage = "Unable to update the profile: " + result.Errors.First().Description;
            }

            return View(profileDto);
        }

        // The display for allowing the user to change their password
        [Authorize]
        public IActionResult Password()
        {
            return View();
        }

        // if user changes their password, this function updates it
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Password(PasswordDto passwordDto)
        {
            if (!ModelState.IsValid)
            {
                return View();

            }

            // Get current user
            var appUser = await userManager.GetUserAsync(User);
            if (appUser == null)
            {
                return RedirectToAction("Index", "Home");
            }

            // update password
            var result = await userManager.ChangePasswordAsync(appUser,
                passwordDto.CurrentPassword, passwordDto.NewPassword);

            if (result.Succeeded)
            {
                ViewBag.SuccessMessage = "Password Updated successfully";
            }
            else
            {
                ViewBag.ErrorMessage = "Error: " + result.Errors.First().Description;
            }

            return View();
        }


        // redirects user to the homepage if they try and go into a restricted page.
        public IActionResult AccessDenied()
        {
            return RedirectToAction("Index", "Home");
        }

        public IActionResult ForgotPassword()
        {
            if (signInManager.IsSignedIn(User)) 
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        // When a user gives their email a password reset token is sent
        [HttpPost]
        public async Task<IActionResult> ForgotPassword([Required, EmailAddress] string email)
        {
            if (signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Email = email;

            if (!ModelState.IsValid)
            {
                ViewBag.EmailError = ModelState["email"]?.Errors.First().ErrorMessage;
                return View();
            }

            var user = await userManager.FindByEmailAsync(email);

            if (user != null)
            {
                // generate password reset token
                var token = await userManager.GeneratePasswordResetTokenAsync(user);
                string resetUrl = Url.ActionLink("ResetPassword", "Account", new { token }) ?? "URL Error";

                // send url by email
                string senderName = configuration["BrevoSettings:SenderName"] ?? "";
                string senderEmail = configuration["BrevoSettings:SenderEmail"] ?? "";
                string username = user.FirstName + " " + user.LastName;
                string subject = "Password Reset";
                string message = "Dear " + username + ", \n\n" +
                                 "You can reset your password using the following link:\n\n" +
                                 resetUrl + "\n\n" +
                                 "Best Regards";

                EmailSender.SendEmail(senderName, senderEmail, username, email, subject, message);
            }

            ViewBag.SuccessMessage = "Please check your Email inbox and click the password reset link.";

            return View();
        }

        // reset password page for when the user clicks the link in the email
        public IActionResult ResetPassword(string token)
        {
            if (signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Index", "Home");
            }

            if (token == null)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(string token, PasswordResetDto model)
        {
            if (signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Index", "Home");
            }

            if (token == null)
            {
                return RedirectToAction("Index", "Home");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ViewBag.ErrorMessage = "Token not valid!";
                return View(model);
            }

            var result = await userManager.ResetPasswordAsync(user, token, model.Password);

            if (result.Succeeded)
            {
                ViewBag.SuccessMessage = "Password reset successfully!";
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(model);
        }
    }
}
