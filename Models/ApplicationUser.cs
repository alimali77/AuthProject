using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;

namespace AuthMvcProject.Models
{
    public class ApplicationUser : IdentityUser
    {
        [PersonalData]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [PersonalData]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [PersonalData]
        public DateTime JoinDate { get; set; } = DateTime.UtcNow;

        [PersonalData]
        public string ProfilePicture { get; set; } = "default-profile.png";

        [PersonalData]
        public bool IsActive { get; set; } = true;
    }
}
