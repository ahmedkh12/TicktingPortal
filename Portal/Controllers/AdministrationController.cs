using Microsoft.AspNetCore.Authorization;
using Portal.Services;
using System.Text.Encodings.Web;

namespace Portal.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdministrationController : Controller
    {

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ISenderEmail _emailSender;
        private readonly RoleManager<ApplicationRole> _roleManager;



        // generic methods that will help us to use UserManager and SignInManager classes

        public AdministrationController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ISenderEmail emailSender, RoleManager<ApplicationRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _roleManager = roleManager;
        }



        [HttpGet]
        public IActionResult ListUsers(string searchString)
        {


            var users = from m in _userManager.Users
                        select m;

            if (!String.IsNullOrEmpty(searchString))
            {
                users = users.Where(

                s => s.UserName.Trim().Contains(searchString.Trim())
                || s.Company.Trim().Contains(searchString.Trim())
                || s.FirstName.Trim().Contains(searchString.Trim())
                || s.LastName.Trim().Contains(searchString.Trim())

                );
            }
            return View(users);
        }

        [HttpGet]
        public IActionResult AddUser()
        {

            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]


        public async Task<IActionResult> AddUser(AddUserModelView model)
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

                if (result.Succeeded)
                {
                    await SendConfirmationEmail(model.Email, user);
                    TempData["create"] = "User Created sucssefully";

                }
                // If there are any errors, add them to the ModelState object
                // which will be displayed by the validation summary tag helper
                foreach (var error in result.Errors)
                {

                    ModelState.AddModelError(string.Empty, error.Description);
                }

            }
            return RedirectToAction("ListUsers");
        }


        private async Task SendConfirmationEmail(string? email, ApplicationUser? user)
        {
            //Generate the Token
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            //Build the Email Confirmation Link which must include the Callback URL
            var ConfirmationLink = Url.Action("ConfirmEmail", "Account",
            new { UserId = user.Id, Token = token }, protocol: HttpContext.Request.Scheme);

            //Send the Confirmation Email to the User Email Id
            await _emailSender.SendEmailAsync(email, $"Your account {user.Email} have login to Customer Portal Please  Confirm Your Email Address", $"Please confirm your account by <a c" +
                $" href='{HtmlEncoder.Default.Encode(ConfirmationLink)}'>clicking here</a>.", true);


        }

        [HttpGet]
        public async Task<IActionResult> EditUser(string UserId)
        {
            //First Fetch the User Details by UserId
            var user = await _userManager.FindByIdAsync(UserId);

            //Check if User Exists in the Database
            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id = {UserId} cannot be found";
                return View("NotFound");
            }

            // GetClaimsAsync retunrs the list of user Claims
            var userClaims = await _userManager.GetClaimsAsync(user);

            // GetRolesAsync returns the list of user Roles
            var userRoles = await _userManager.GetRolesAsync(user);

            //Store all the information in the EditUserViewModel instance
            var model = new EditUserViewModel
            {
                Id = user.Id,
                Email = user.Email,
                UserName = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Company = user.Company,
                Mobile = user.Mobile
            };

            //Pass the Model to the View
            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> EditUser(EditUserViewModel model)
        {
            //First Fetch the User by Id from the database
            var user = await _userManager.FindByIdAsync(model.Id);

            //Check if the User Exists in the database
            if (user == null)
            {
                //If the User does not exists in the database, then return Not Found Error View
                ViewBag.ErrorMessage = $"User with Id = {model.Id} cannot be found";
                return View("NotFound");
            }
            else
            {
                //If the User Exists, then proceed and update the data
                //Populate the user instance with the data from EditUserViewModel
                user.Email = model.Email;
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.Company = model.Company;
                user.Mobile = model.Mobile;

                //UpdateAsync Method will update the user data in the AspNetUsers Identity table
                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    TempData["update"] = "User Updated sucssefully";
                    //Once user data updated redirect to the ListUsers view
                    return RedirectToAction("ListUsers");
                }
                else
                {
                    //In case any error, stay in the same view and show the model validation error
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }

                return View(model);
            }
        }



        [HttpPost]
        public async Task<IActionResult> DeleteUser(string UserId)
        {
            //First Fetch the User you want to Delete
            var user = await _userManager.FindByIdAsync(UserId);

            if (user == null)
            {
                // Handle the case where the user wasn't found
                ViewBag.ErrorMessage = $"User with Id = {UserId} cannot be found";
                return View("NotFound");
            }
            else
            {
                //Delete the User Using DeleteAsync Method of UserManager Service
                var result = await _userManager.DeleteAsync(user);

                if (result.Succeeded)
                {
                    // Handle a successful delete
                    return RedirectToAction("ListUsers");
                }
                else
                {
                    // Handle failure
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }

                return View("ListUsers");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ResetUserPasswordFromAdmin(string userId, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                // User not found, return an error
                return BadRequest("User not found");
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
            if (result.Succeeded)
            {
                // Password reset successful
                return Ok("Password reset successful");
            }
            else
            {
                // Password reset failed
                return BadRequest("Password reset failed");
            }
        }


        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }


        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string Token, string Email)
        {
            // If password reset token or email is null, most likely the
            // user tried to tamper the password reset link
            if (Token == null || Email == null)
            {
                ViewBag.ErrorTitle = "Invalid Password Reset Token";
                ViewBag.ErrorMessage = "The Link is Expired or Invalid";
                return View("Error");
            }
            else
            {
                ResetPasswordViewModel model = new ResetPasswordViewModel();
                model.Token = Token;
                model.Email = Email;
                return View(model);
            }
        }



        [HttpGet]
        public IActionResult CreateRole()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole(CreateRoleViewModel roleModel)
        {
            if (ModelState.IsValid)
            {
                // Check if the role already exists
                bool roleExists = await _roleManager.RoleExistsAsync(roleModel?.RoleName);
                if (roleExists)
                {
                    ModelState.AddModelError("", "Role Already Exists");
                }
                else
                {
                    // Create the role
                    // We just need to specify a unique role name to create a new role
                    ApplicationRole identityRole = new ApplicationRole
                    {
                        Name = roleModel?.RoleName,
                        Description = roleModel?.Description
                    };

                    // Saves the role in the underlying AspNetRoles table
                    IdentityResult result = await _roleManager.CreateAsync(identityRole);

                    if (result.Succeeded)
                    {
                        TempData["create"] = "Role Created Successfully";
                        return RedirectToAction("ListRoles", "Administration");
                    }

                    foreach (IdentityError error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }

            return View(roleModel);
        }


        [HttpGet]
        public async Task<IActionResult> ListRoles()
        {
            List<ApplicationRole> roles = await _roleManager.Roles.ToListAsync();
            return View(roles);
        }


        [HttpGet] // get view to edit role 
        public async Task<IActionResult> EditRole(string roleId)
        {
            //First Get the role information from the database
            ApplicationRole role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                // Handle the scenario when the role is not found
                return View("Error");
            }

            //Populate the EditRoleViewModel from the data retrived from the database
            var model = new EditRoleViewModel
            {
                Id = role.Id,
                RoleName = role.Name,
                Description = role.Description,
                Users = new List<string>()
                // You can add other properties here if needed
            };
            foreach (var user in _userManager.Users.ToList())
            {
                // If the user is in this role, add the username to
                // Users property of EditRoleViewModel. 
                // This model object is then passed to the view for display
                if (await _userManager.IsInRoleAsync(user, role.Name))
                {
                    model.Users.Add(user.UserName);
                }
            }

            return View(model);
        }


        [HttpGet]
        public async Task<IActionResult> EditUsersInRole(string roleId)
        {
            ViewBag.roleId = roleId;

            var role = await _roleManager.FindByIdAsync(roleId);

            if (role == null)
            {
                ViewBag.ErrorMessage = $"Role with Id = {roleId} cannot be found";
                return View("NotFound");
            }

            ViewBag.RollName = role.Name;
            var model = new List<UserRoleViewModel>();

            foreach (var user in _userManager.Users.ToList())
            {
                var userRoleViewModel = new UserRoleViewModel
                {
                    UserId = user.Id,
                    UserName = user.UserName
                };

                if (await _userManager.IsInRoleAsync(user, role.Name))
                {
                    userRoleViewModel.IsSelected = true;
                }
                else
                {
                    userRoleViewModel.IsSelected = false;
                }

                model.Add(userRoleViewModel);
            }

            return View(model);
        }




        [HttpPost]
        public async Task<IActionResult> EditRole(EditRoleViewModel model)
        {
            if (ModelState.IsValid)
            {
                var role = await _roleManager.FindByIdAsync(model.Id);
                if (role == null)
                {
                    // Handle the scenario when the role is not found
                    ViewBag.ErrorMessage = $"Role with Id = {model.Id} cannot be found";
                    return View("NotFound");
                }
                else
                {
                    role.Name = model.RoleName;
                    role.Description = model.Description;
                    // Update other properties if needed

                    var result = await _roleManager.UpdateAsync(role);
                    if (result.Succeeded)
                    {
                        TempData["update"] = "Role Updated Successfully";
                        return RedirectToAction("ListRoles"); // Redirect to the roles list
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }

                    return View(model);
                }
            }

            return View(model);
        }


        //-----------------------------------------


        [HttpPost]
        public async Task<IActionResult> DeleteRole(string roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                // Role not found, handle accordingly
                ViewBag.ErrorMessage = $"Role with Id = {roleId} cannot be found";
                return View("NotFound");
            }

            var result = await _roleManager.DeleteAsync(role);
            if (result.Succeeded)
            {
                // Role deletion successful
                return RedirectToAction("ListRoles"); // Redirect to the roles list page
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            // If we reach here, something went wrong, return to the view
            return View("ListRoles", await _roleManager.Roles.ToListAsync());
        }



        //-------------------------------------------------------------------


        [HttpPost]
        public async Task<IActionResult> EditUsersInRole(List<UserRoleViewModel> model, string roleId)
        {
            //First check whether the Role Exists or not
            var role = await _roleManager.FindByIdAsync(roleId);

            if (role == null)
            {
                ViewBag.ErrorMessage = $"Role with Id = {roleId} cannot be found";
                return View("NotFound");
            }

            for (int i = 0; i < model.Count; i++)
            {
                var user = await _userManager.FindByIdAsync(model[i].UserId);

                IdentityResult? result;

                if (model[i].IsSelected && !(await _userManager.IsInRoleAsync(user, role.Name)))
                {
                    //If IsSelected is true and User is not already in this role, then add the user
                    result = await _userManager.AddToRoleAsync(user, role.Name);
                }
                else if (!model[i].IsSelected && await _userManager.IsInRoleAsync(user, role.Name))
                {
                    //If IsSelected is false and User is already in this role, then remove the user
                    result = await _userManager.RemoveFromRoleAsync(user, role.Name);
                }
                else
                {
                    //Don't do anything simply continue the loop
                    continue;
                }

                //If you add or remove any user, please check the Succeeded of the IdentityResult
                if (result.Succeeded)
                {
                    if (i < (model.Count - 1))
                        continue;
                    else
                        return RedirectToAction("EditRole", new { roleId = roleId });
                }
            }

            return RedirectToAction("EditRole", new { roleId = roleId });
        }















    }
}
