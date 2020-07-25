using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Flame_Social_Network_Web_App.Models
{
    public class RegisterModel
    {
        [StringLength(maximumLength: Constants.MaximumUsernameLength, MinimumLength = Constants.MinimumUsernameLength, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.")]
        [Required]
        public string Username { get; set; }
        [EmailAddress]
        [Required]
        public string Email { get; set; }

        [DataType(DataType.Password)]
        [StringLength(maximumLength: Constants.MaximumPasswordLength, MinimumLength = Constants.MinimumPasswordLength, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.")]
        [Required]
        public string Password { get; set; }
        [Compare("Password", ErrorMessage = "Password and confirm password does not match")]
        [Required]
        public string ConfirmPassword { get; set; }
    }
}
