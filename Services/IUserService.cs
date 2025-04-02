using AuthMvcProject.Models;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AuthMvcProject.Services
{
    public interface IUserService
    {
        Task<ApplicationUser> GetUserByIdAsync(string userId);
        Task<ApplicationUser> GetUserByEmailAsync(string email);
        Task<bool> UpdateUserProfileAsync(ApplicationUser user, UserProfileViewModel model);
        Task<bool> ChangePasswordAsync(ApplicationUser user, string currentPassword, string newPassword);
        Task<IEnumerable<ApplicationUser>> GetAllUsersAsync();
        Task<bool> ToggleUserStatusAsync(string userId);
        Task<string> SaveProfilePictureAsync(string userId, IFormFile file);
        Task<bool> DeleteUserAsync(string userId);
        Task<bool> IsInRoleAsync(ApplicationUser user, string role);
    }
}