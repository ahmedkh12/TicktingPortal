using Microsoft.AspNetCore.Authorization;
using Portal.Services;
using System.Text.Encodings.Web;


namespace Portal.Controllers
{

    public class AccountController : Controller
    {

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ISenderEmail _emailSender;

        // generic methods that will help us to use UserManager and SignInManager classes

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ISenderEmail emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
        }


        [HttpGet]

        //to get register view
        public IActionResult Register()

        {
            RegisterViewModel registerViewModel = new RegisterViewModel();
            return View(registerViewModel);

        }





        [HttpPost]
        [ValidateAntiForgeryToken]

        //register user data then login 
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Copy data from RegisterViewModel to IdentityUser
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Mobile = model.Mobile,
                    Company = model.Company,
                };
                // Store user data in AspNetUsers database table
                var result = await _userManager.CreateAsync(user, model.Password);
                // If user is successfully created, sign-in the user using
                // SignInManager and redirect to index action of HomeController
                if (result.Succeeded)
                {

                    await SendConfirmationEmail(model.Email, user);
                    //  await SendConfirmationEmail(model.Email, user);


                    //If it is not Admin user, then redirect the user to RegistrationSuccessful View
                    return View("RegistrationSuccessful");
                    //await _signInManager.SignInAsync(user, isPersistent: false);

                }
                // If there are any errors, add them to the ModelState object
                // which will be displayed by the validation summary tag helper
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }

        private async Task SendConfirmationEmail(string? email, ApplicationUser? user)
        {
            //Generate the Token
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            //Build the Email Confirmation Link which must include the Callback URL
            var ConfirmationLink = Url.Action("ConfirmEmail", "Account",
            new { UserId = user.Id, Token = token }, protocol: HttpContext.Request.Scheme);

            //Send the Confirmation Email to the User Email Id
            await _emailSender.SendEmailAsync(email, "Confirm Your Email Address", $"Please confirm your account by <a c" +
                $" href='{HtmlEncoder.Default.Encode(ConfirmationLink)}'>clicking here</a>.", true);


        }



        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string UserId, string Token)
        {
            if (UserId == null || Token == null)
            {
                ViewBag.Message = "The link is Invalid or Expired";
            }

            //Find the User By Id
            var user = await _userManager.FindByIdAsync(UserId);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"The User ID {UserId} is Invalid";
                return View("NotFound");
            }

            //Call the ConfirmEmailAsync Method which will mark the Email as Confirmed
            var result = await _userManager.ConfirmEmailAsync(user, Token);
            if (result.Succeeded)
            {
                ViewBag.Message = "Thank you for confirming your email";
                return View();
            }

            ViewBag.Message = "Email cannot be confirmed";
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResendConfirmationEmail(bool IsResend = true)
        {
            if (IsResend)
            {
                ViewBag.Message = "Resend Confirmation Email";
            }
            else
            {
                ViewBag.Message = "Send Confirmation Email";
            }
            return View();
        }


        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendConfirmationEmail(string Email)
        {
            var user = await _userManager.FindByEmailAsync(Email);
            if (user == null || await _userManager.IsEmailConfirmedAsync(user))
            {
                // Handle the situation when the user does not exist or Email already confirmed.
                // For security, don't reveal that the user does not exist or Email is already confirmed
                return View("ConfirmationEmailSent");
            }
            //Then send the Confirmation Email to the User
            await SendConfirmationEmail(Email, user);
            return View("ConfirmationEmailSent");
        }

        public IActionResult Login(string? ReturnUrl = null)
        {
            ViewData["ReturnUrl"] = ReturnUrl;
            return View();
        }





        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                //First Fetch the User Details by Email Id
                var user = await _userManager.FindByEmailAsync(model.Email);
                //Then Check if User Exists, EmailConfirmed and Password Is Valid
                //CheckPasswordAsync: Returns a flag indicating whether the given password is valid for the specified user.
                //if (user != null &&
                //            (await _userManager.CheckPasswordAsync(user, model.Password)))
                //{

                //    return View(model);
                //}

                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {

                    // here if the user requested url without auth then after login done he will back to the same url 


                    bool isInRole = await _userManager.IsInRoleAsync(user, "Admin");



                    if (isInRole)
                    {
                        return RedirectToAction("Dashboard", "Ticket");
                    }

                    else
                    {
                        return RedirectToAction("MyTickets", "Ticket");
                    }

                }



                else
                {
                    // Handle failure
                    ModelState.AddModelError(string.Empty, "Invalid Usernameor Password.");
                    return View(model);
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }



        [AllowAnonymous]
        [HttpPost]
        [HttpGet]
        public async Task<IActionResult> IsEmailAvailable(string Email)
        {
            //Check If the Email Id is Already in the Database
            var user = await _userManager.FindByEmailAsync(Email);

            if (user == null)
            {
                return Json(true);
            }
            else
            {
                return Json($"Email {Email} is already token.");
            }
        }







        // send forget password
        private async Task SendForgotPasswordEmail(string? email, ApplicationUser? user)
        {
            // Generate the reset password token
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);


            //save the token into the AspNetUserTokens database table
            await _userManager.SetAuthenticationTokenAsync(user, "ResetPassword", "ResetPasswordToken", token);


            // Build the password reset link which must include the Callback URL
            // Build the password reset link
            var passwordResetLink = Url.Action("ResetPassword", "Administration",
                    new { Email = email, Token = token }, protocol: HttpContext.Request.Scheme);

            //Send the Confirmation Email to the User Email Id
            await _emailSender.SendEmailAsync(email, "Reset Your Password", $"Please reset your password by <a href='{HtmlEncoder.Default.Encode(passwordResetLink)}'>clicking here</a>.", true);
        }



        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Find the user by email
                var user = await _userManager.FindByEmailAsync(model.Email);

                // If the user is found AND Email is confirmed
                if (user != null && await _userManager.IsEmailConfirmedAsync(user))
                {
                    await SendForgotPasswordEmail(user.Email, user);

                    // Send the user to Forgot Password Confirmation view
                    return RedirectToAction("ForgotPasswordConfirmation", "Administration");
                }

                // To avoid account enumeration and brute force attacks, don't
                // reveal that the user does not exist or is not confirmed
                return RedirectToAction("ForgotPasswordConfirmation", "Administration");
            }

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Find the user by email
                var user = await _userManager.FindByEmailAsync(model.Email);

                if (user != null)
                {
                    // reset the user password
                    var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);

                    if (result.Succeeded)
                    {
                        //Once the Password is Reset, remove the token from the database
                        await _userManager.RemoveAuthenticationTokenAsync(user, "ResetPassword", "ResetPasswordToken");
                        return RedirectToAction("ResetPasswordConfirmation", "Account");
                    }

                    // Display validation errors. For example, password reset token already
                    // used to change the password or password complexity rules not met
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return View(model);
                }

                // To avoid account enumeration and brute force attacks, don't
                // reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            // Display validation errors if model state is not valid
            return View(model);
        }


        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }



        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                //fetch the User Details
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    //If User does not exists, redirect to the Login Page
                    return RedirectToAction("Login", "Account");
                }

                // ChangePasswordAsync Method changes the user password
                var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);

                // The new password did not meet the complexity rules or the current password is incorrect.
                // Add these errors to the ModelState and rerender ChangePassword view
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View();
                }

                // Upon successfully changing the password refresh sign-in cookie
                await _signInManager.RefreshSignInAsync(user);


                await _signInManager.SignOutAsync();  // sign user out

                //Then redirect the user to the ChangePasswordConfirmation view

                return RedirectToAction("ChangePasswordConfirmation", "Account");






            }

            return View(model);
        }

    }
}
