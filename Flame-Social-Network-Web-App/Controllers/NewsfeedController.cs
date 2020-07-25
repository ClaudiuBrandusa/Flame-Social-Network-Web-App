using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Flame_Social_Network_Web_App.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Flame_Social_Network_Web_App.Controllers
{
    [Authorize]
    public class NewsfeedController : Controller
    {
        readonly UserManager<IdentityUser> _userManager;
        readonly SignInManager<IdentityUser> _signInManager;

        public NewsfeedController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public IActionResult Index()
        {
            return View(new UserModel() { Username = User.Identity.Name});
        }
    }
}