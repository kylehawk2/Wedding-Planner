using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wedding_Planner.Models;

namespace Wedding_Planner.Controllers
{
    public class HomeController : Controller
    {
        private WeddingPlannerContext dbContext;
        public HomeController(WeddingPlannerContext context)
        {
            dbContext = context;
        }
        [HttpGet]
        [Route("")]
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        [Route("Welcome")]
        public IActionResult Welcome()
        {
            var weddings = dbContext.Weddings
            .Include(w => w.RSVPs)
            .OrderByDescending(w => w.Date);

            ViewBag.UserId = HttpContext.Session.GetInt32("UserId");

            // wedding's user has created - if user created wedding, should be able to delete it. If not, then RSVP or UnRSVP
            var responded = weddings.Where(w => w.RSVPs.Any(r => r.UserId  == 1));



            return View("Welcome", weddings);
        }
        [HttpPost]
        [Route("Register")]
        public IActionResult Register(User user)
        {
            if(ModelState.IsValid)
            {
                if(dbContext.Users.Any(u => u.Email == user.Email))
                {
                    ModelState.AddModelError("Email", "Email is already in use!");
                }
                PasswordHasher<User> Hasher = new PasswordHasher<User>();
                user.Password = Hasher.HashPassword(user, user.Password);
                // Save your user object to the database
                User NewUser = new User
                {
                    First_Name = @user.First_Name,
                    Last_Name = @user.Last_Name,
                    Email = @user.Email,
                    Password = @user.Password,
                };
                dbContext.Add(NewUser);
                dbContext.SaveChanges();
                return RedirectToAction("Welcome");
            }
            
            return View("Index");

        }

        [HttpPost]
        [Route("Login")]
        public IActionResult Login(User userSubmission)
        {
            // if inital ModelState is valid, query for a user with provided email
            var userInDb = dbContext.Users
            .FirstOrDefault(u => u.Email == userSubmission.Email);
            // If no user exists with provided email
            if(userInDb == null)
            {
                // Add an error to ModelState and return to View!
                ModelState.AddModelError("Email", "Invalid Email/Password");
                return View("Login");
            }
            // Initialize hasher object
            var hasher = new PasswordHasher<User>();
            // verify provided password against hash store in db
            var result = hasher.VerifyHashedPassword(userSubmission, userInDb.Password, userSubmission.Password);
            // result can be compared to 0 for failure
            if(result == 0)
            {
                Console.WriteLine("Invalid Password");
                ModelState.AddModelError("Password", "Invaild Password");
                return View("Login");
            }
            HttpContext.Session.SetInt32("id", userInDb.UserId);
            return RedirectToAction("Welcome");
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
