using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Flame_Social_Network_Web_App.Models
{
    public class LoginModel
    {
        [StringLength(maximumLength: Constants.MaximumUsernameLength, MinimumLength = Constants.MinimumUsernameLength, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.")]
        [Required]
        public string Username { get; set; }
        [StringLength(maximumLength: Constants.MaximumPasswordLength, MinimumLength = Constants.MinimumPasswordLength, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.")]
        [DataType(DataType.Password)]
        [Required]
        public string Password { get; set; }
    }
}
