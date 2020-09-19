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
    public class ProfileController : Controller
    {
        readonly UserManager<IdentityUser> _userManager;
        readonly SignInManager<IdentityUser> _signInManager;

        public ProfileController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [Authorize]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public IActionResult MyAccount()
        {
            return View();
        }

        public IActionResult Register()
        {
            if(_signInManager.IsSignedIn(User)) // if we are already signed in then we shouldn't be allowed to register
            {
                return Redirect(Constants.DefaultConnectedPath);
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if(ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(model.Username);

                if (user == null)
                {
                    var result = await _userManager.CreateAsync(new IdentityUser() { UserName = model.Username, Email = model.Email }, model.Password);

                    if(result.Succeeded)
                    {
                        await _signInManager.PasswordSignInAsync(model.Username, model.Password, false, false);
                        return Redirect(Constants.DefaultConnectedPath); // then we are redirected to the main page
                    }

                    // here we are printing the validation error messages
                    foreach(var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }
            return View(model);
        }
        public IActionResult Login()
        {
            if (_signInManager.IsSignedIn(User)) // if we are already signed in then we shouldn't be allowed to login
            {
                return Redirect(Constants.DefaultConnectedPath);
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(model.Username);

                if(user != null) // if we have found an user
                {
                    // then we try to sign in
                    var result = await _signInManager.PasswordSignInAsync(user, model.Password, false, false);

                    if(result.Succeeded) // if we signed in successfully
                    {
                        // then we go to the main page
                        return Redirect(Constants.DefaultConnectedPath);
                    }

                    // something went wrong so we will show an error
                    ModelState.AddModelError(string.Empty, "Invalid Login attempt");
                }
            }
            return View(model);
        }

        [HttpPost] // lets us to log out by a post request in a form
        [HttpGet] // lets us to log out by browsing the url
        [Authorize] // you have to be signed in to be able to log out
        public async Task<IActionResult> LogOut()
        {
            await _signInManager.SignOutAsync();
            return Redirect(Constants.DefaultLoginPath);
        }
    }
}