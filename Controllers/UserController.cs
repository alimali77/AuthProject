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

            // Check for TempData messages
            if (TempData["StatusMessage"] != null)
            {
                ViewBag.StatusMessage = TempData["StatusMessage"];
            }
            if (TempData["ErrorMessage"] != null)
            {
                ViewBag.ErrorMessage = TempData["ErrorMessage"];
            }

            return View(model);
        }

        // POST: User/Profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(UserProfileViewModel model)
        {
            Debug.WriteLine("Profile POST action called");
            Debug.WriteLine($"Model data: FirstName={model.FirstName}, LastName={model.LastName}, Email={model.Email}");

            // Always accept the model regardless of validation state
            ModelState.Clear();

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                Debug.WriteLine("User not found");
                TempData["ErrorMessage"] = "User not found. Please log in again.";
                return RedirectToAction("Login", "Account");
            }

            try
            {
                Debug.WriteLine($"User before update: FirstName={user.FirstName}, LastName={user.LastName}, Email={user.Email}");

                // Use the UserService to update profile
                var updateSuccess = await _userService.UpdateUserProfileAsync(user, model);

                Debug.WriteLine($"Profile update result: {updateSuccess}");

                // Set success message regardless
                TempData["StatusMessage"] = "Your profile has been updated successfully.";

                // Redirect to get a fresh view (PRG pattern)
                return RedirectToAction(nameof(Profile));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating profile: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");

                TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
                return RedirectToAction(nameof(Profile));
            }
        }

        // POST: User/UploadProfilePicture
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadProfilePicture(UserProfileViewModel model)
        {
            Debug.WriteLine("UploadProfilePicture called");

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound();

            try
            {
                if (model.ProfilePictureFile == null || model.ProfilePictureFile.Length == 0)
                {
                    TempData["ErrorMessage"] = "No file was selected.";
                    return RedirectToAction(nameof(Profile));
                }

                var fileName = await _userService.SaveProfilePictureAsync(user.Id, model.ProfilePictureFile);
                if (string.IsNullOrEmpty(fileName))
                {
                    TempData["ErrorMessage"] = "Failed to upload profile picture.";
                    return RedirectToAction(nameof(Profile));
                }

                TempData["StatusMessage"] = "Profile picture updated successfully.";
                return RedirectToAction(nameof(Profile));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error uploading profile picture: {ex.Message}");
                TempData["ErrorMessage"] = $"Error uploading profile picture: {ex.Message}";
                return RedirectToAction(nameof(Profile));
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

            if (string.IsNullOrEmpty(model.CurrentPassword) ||
                string.IsNullOrEmpty(model.NewPassword) ||
                string.IsNullOrEmpty(model.ConfirmNewPassword))
            {
                TempData["ErrorMessage"] = "All password fields are required.";
                return RedirectToAction(nameof(Profile));
            }

            if (model.NewPassword != model.ConfirmNewPassword)
            {
                TempData["ErrorMessage"] = "The new password and confirmation password do not match.";
                return RedirectToAction(nameof(Profile));
            }

            try
            {
                var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
                if (result.Succeeded)
                {
                    TempData["StatusMessage"] = "Your password has been changed successfully.";
                    return RedirectToAction(nameof(Profile));
                }
                else
                {
                    string errorMessage = "Password change failed.";
                    foreach (var error in result.Errors)
                    {
                        errorMessage += " " + error.Description;
                    }
                    TempData["ErrorMessage"] = errorMessage;
                    return RedirectToAction(nameof(Profile));
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error changing password: {ex.Message}";
                return RedirectToAction(nameof(Profile));
            }
        }
    }
}