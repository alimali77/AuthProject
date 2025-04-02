using AuthMvcProject.Models;
using AuthMvcProject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace AuthMvcProject.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserService _userService;

        public UserController(UserManager<ApplicationUser> userManager, IUserService userService)
        {
            _userManager = userManager;
            _userService = userService;
        }

        // GET: User/Profile
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound();

            var model = new UserProfileViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                UserName = user.UserName,
                ProfilePicture = user.ProfilePicture
            };

            return View(model);
        }

        // POST: User/Profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(UserProfileViewModel model)
        {
            Debug.WriteLine("Profile POST action called");
            Debug.WriteLine($"Model state valid: {ModelState.IsValid}");
            Debug.WriteLine($"Model data: FirstName={model.FirstName}, LastName={model.LastName}, Email={model.Email}");

            if (!ModelState.IsValid)
            {
                foreach (var state in ModelState)
                {
                    Debug.WriteLine($"Key: {state.Key}, Errors: {state.Value.Errors.Count}");
                    foreach (var error in state.Value.Errors)
                    {
                        Debug.WriteLine($"Error: {error.ErrorMessage}");
                    }
                }
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                Debug.WriteLine("User not found");
                return NotFound();
            }

            try
            {
                // Log current user data before changes
                Debug.WriteLine($"User before update: FirstName={user.FirstName}, LastName={user.LastName}, Email={user.Email}");

                // KEY CHANGE: Use the UserService instead of direct manipulation
                var updateSuccess = await _userService.UpdateUserProfileAsync(user, model);

                if (!updateSuccess)
                {
                    Debug.WriteLine("UserService.UpdateUserProfileAsync returned false");
                    ViewBag.ErrorMessage = "Failed to update profile. Please try again.";
                    return View(model);
                }

                // Refresh user data after update
                user = await _userManager.FindByIdAsync(user.Id);
                Debug.WriteLine($"User after update: FirstName={user.FirstName}, LastName={user.LastName}, Email={user.Email}");

                // Create a new model with the updated user data
                var updatedModel = new UserProfileViewModel
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    UserName = user.UserName,
                    ProfilePicture = user.ProfilePicture
                };

                ViewBag.StatusMessage = "Your profile has been updated successfully.";
                return View(updatedModel);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating profile: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                ViewBag.ErrorMessage = $"An error occurred: {ex.Message}";
                return View(model);
            }
        }

        // POST: User/UploadProfilePicture
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadProfilePicture(UserProfileViewModel model)
        {
            Debug.WriteLine("UploadProfilePicture called");
            Debug.WriteLine($"Model: FirstName={model.FirstName}, LastName={model.LastName}, Email={model.Email}");

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound();

            // Prepare model to return to view - preserve all form fields
            var returnModel = new UserProfileViewModel
            {
                FirstName = model.FirstName ?? user.FirstName,
                LastName = model.LastName ?? user.LastName,
                Email = model.Email ?? user.Email,
                UserName = user.UserName,
                ProfilePicture = user.ProfilePicture
            };

            try
            {
                if (model.ProfilePictureFile == null || model.ProfilePictureFile.Length == 0)
                {
                    ViewBag.ErrorMessage = "No file was selected.";
                    return View("Profile", returnModel);
                }

                var fileName = await _userService.SaveProfilePictureAsync(user.Id, model.ProfilePictureFile);

                if (string.IsNullOrEmpty(fileName))
                {
                    ViewBag.ErrorMessage = "Failed to upload profile picture.";
                    return View("Profile", returnModel);
                }

                // Refresh user data
                user = await _userManager.FindByIdAsync(user.Id);

                // Update return model with new profile picture
                returnModel.ProfilePicture = user.ProfilePicture;

                ViewBag.StatusMessage = "Profile picture updated successfully.";
                return View("Profile", returnModel);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error uploading profile picture: {ex.Message}");
                ViewBag.ErrorMessage = $"Error uploading profile picture: {ex.Message}";
                return View("Profile", returnModel);
            }
        }

        // POST: User/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(UserProfileViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound();

            // Prepare model to return to view with current user data
            var returnModel = new UserProfileViewModel
            {
                FirstName = model.FirstName ?? user.FirstName,
                LastName = model.LastName ?? user.LastName,
                Email = model.Email ?? user.Email,
                UserName = user.UserName,
                ProfilePicture = user.ProfilePicture
            };

            if (string.IsNullOrEmpty(model.CurrentPassword))
            {
                ModelState.AddModelError("CurrentPassword", "Current password is required.");
                return View("Profile", returnModel);
            }

            if (string.IsNullOrEmpty(model.NewPassword))
            {
                ModelState.AddModelError("NewPassword", "New password is required.");
                return View("Profile", returnModel);
            }

            if (string.IsNullOrEmpty(model.ConfirmNewPassword))
            {
                ModelState.AddModelError("ConfirmNewPassword", "Confirm password is required.");
                return View("Profile", returnModel);
            }

            if (model.NewPassword != model.ConfirmNewPassword)
            {
                ModelState.AddModelError("", "The new password and confirmation password do not match.");
                return View("Profile", returnModel);
            }

            try
            {
                var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

                if (result.Succeeded)
                {
                    ViewBag.StatusMessage = "Your password has been changed successfully.";
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }

                return View("Profile", returnModel);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Error changing password: {ex.Message}";
                return View("Profile", returnModel);
            }
        }
    }
}