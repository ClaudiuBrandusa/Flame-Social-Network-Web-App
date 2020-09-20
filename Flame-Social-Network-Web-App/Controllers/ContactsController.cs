using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

using Flame_Social_Network_Web_App.Models.Chat;
using Flame_Social_Network_Web_App.Hubs;
using Flame_Social_Network_Web_App.Data;

namespace Flame_Social_Network_Web_App.Controllers
{
    [Authorize]
    public class ContactsController : Controller
    {
        readonly SignInManager<IdentityUser> _signInManager;
        readonly AppDbContext _dbContext;

        public ContactsController(SignInManager<IdentityUser> signInManager, AppDbContext dbContext)
        {
            _signInManager = signInManager;
            _dbContext = dbContext;
        }
        [AllowAnonymous]
        public IActionResult Index()
        {
            if (_signInManager.IsSignedIn(User))
            {
                var model = ListContacts();
                return View(model);
            }
            return View();
        }

        public IActionResult Chat(string tag)
        {
            return View();
        }

        //[AllowAnonymous] // this will allow non-authenticated users to reach this action

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> SetNickname(string nickname="default")
        {
            // we will sign in only if we are signed out
            if(!_signInManager.IsSignedIn(User))
            {
                await _signInManager.SignInAsync(new IdentityUser { UserName = nickname }, false);
            }
            return RedirectToAction("Index");
        }

        // This method will redirect the user to the chat opening a chat room with the selected tag
        [Authorize]
        public async Task<IActionResult> Contact(string tag)
        {
            // validate the tag
            if(!String.IsNullOrEmpty(tag) /*there goes the FindUserByTagMethod which will be implemented in the chat signalR hub*/)
            {
                //return RedirectToAction("EnterRoomWith", new { Tag = tag});
            }
            // redirect
            return RedirectToAction("Index");
        }
        
        [Authorize]
        public string EnterRoomWith(string tag)
        {
            // validate tag
            // check if we can enter this room
            // get the last 10 messages
            return "works";
        }

        public ContactsList ListContacts()
        {
            return ChatHub.GetContactsListFor(User.Identity.Name);
        }
    }
}