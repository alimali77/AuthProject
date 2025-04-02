using AuthMvcProject.Data;
using AuthMvcProject.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;

namespace AuthMvcProject.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public UserService(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<ApplicationUser> GetUserByIdAsync(string userId)
        {
            return await _userManager.FindByIdAsync(userId);
        }

        public async Task<ApplicationUser> GetUserByEmailAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }

        public async Task<bool> UpdateUserProfileAsync(ApplicationUser user, UserProfileViewModel model)
        {
            Debug.WriteLine("UpdateUserProfileAsync started");
            Debug.WriteLine($"User before update: {user.FirstName}, {user.LastName}, {user.Email}");
            Debug.WriteLine($"Model values: {model.FirstName}, {model.LastName}, {model.Email}");

            try
            {
                // Update basic properties first
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;

                // Handle email change separately
                bool emailChanged = false;
                if (user.Email != model.Email)
                {
                    // Check if email already exists
                    var existingUser = await _userManager.FindByEmailAsync(model.Email);
                    if (existingUser != null && existingUser.Id != user.Id)
                    {
                        Debug.WriteLine("Email already in use by another account");
                        return false;
                    }

                    // Set new email
                    var setEmailResult = await _userManager.SetEmailAsync(user, model.Email);
                    if (!setEmailResult.Succeeded)
                    {
                        Debug.WriteLine($"Failed to set email: {string.Join(", ", setEmailResult.Errors)}");
                        return false;
                    }

                    // Set username to match email if that's your pattern
                    var setUserNameResult = await _userManager.SetUserNameAsync(user, model.Email);
                    if (!setUserNameResult.Succeeded)
                    {
                        Debug.WriteLine($"Failed to set username: {string.Join(", ", setUserNameResult.Errors)}");
                        return false;
                    }

                    emailChanged = true;
                    Debug.WriteLine("Email and username updated successfully");
                }

                // If email wasn't changed, we still need to update the user record
                if (!emailChanged)
                {
                    var result = await _userManager.UpdateAsync(user);
                    if (!result.Succeeded)
                    {
                        Debug.WriteLine($"Failed to update user: {string.Join(", ", result.Errors)}");
                        return false;
                    }
                    Debug.WriteLine("User basic properties updated successfully");
                }

                // If we got here, the update was successful
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception in UpdateUserProfileAsync: {ex.Message}");
                Debug.WriteLine(ex.StackTrace);
                throw; // Let the controller handle the exception
            }
        }

        public async Task<bool> ChangePasswordAsync(ApplicationUser user, string currentPassword, string newPassword)
        {
            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
            return result.Succeeded;
        }

        public async Task<IEnumerable<ApplicationUser>> GetAllUsersAsync()
        {
            return await _userManager.Users.ToListAsync();
        }

        public async Task<bool> ToggleUserStatusAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            // Prevent toggling Admin's IsActive
            if (await _userManager.IsInRoleAsync(user, "Admin"))
                return false;

            user.IsActive = !user.IsActive;
            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<string> SaveProfilePictureAsync(string userId, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return null;

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return null;

            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "profiles");
            Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{userId}_{DateTime.Now.Ticks}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            // Remove old picture if not default
            if (!string.IsNullOrEmpty(user.ProfilePicture) &&
                user.ProfilePicture != "default-profile.png")
            {
                var oldFilePath = Path.Combine(uploadsFolder, user.ProfilePicture);
                if (File.Exists(oldFilePath))
                {
                    try
                    {
                        File.Delete(oldFilePath);
                    }
                    catch
                    {
                        // Log the error but continue
                    }
                }
            }

            user.ProfilePicture = uniqueFileName;
            await _userManager.UpdateAsync(user);

            return uniqueFileName;
        }

        public async Task<bool> DeleteUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            // Prevent deleting Admin
            if (await _userManager.IsInRoleAsync(user, "Admin"))
                return false;

            var result = await _userManager.DeleteAsync(user);
            return result.Succeeded;
        }

        public async Task<bool> IsInRoleAsync(ApplicationUser user, string role)
        {
            return await _userManager.IsInRoleAsync(user, role);
        }
    }
}