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
                var userEnity = dbContext.Add(NewUser).Entity;
                dbContext.SaveChanges();
                HttpContext.Session.SetInt32("UserId", NewUser.UserId);
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
                return View("Index");
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
                return View("Index");
            }
            HttpContext.Session.SetInt32("UserId", userInDb.UserId);
            return RedirectToAction("Welcome");
        }
        [HttpGet]
        [Route("Logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }
        [HttpGet]
        [Route("NewWedding")]
        public IActionResult NewWedding()
        {
            return View("NewWedding");
        }
        [HttpPost]
        [Route("CreateWedding")]
        public IActionResult CreateWedding(Wedding new_wedding)
        {
            if(ModelState.IsValid)
            {
                if(new_wedding.Date < DateTime.Today)
                {
                    ModelState.AddModelError("Date", "Date must be in the future!");
                    return View("NewWedding");
                }
                else
                {
                    Wedding this_wedding = new Wedding
                    {
                        Address = new_wedding.Address,
                        Date = new_wedding.Date,
                        WedderOne = new_wedding.WedderOne,
                        WedderTwo = new_wedding.WedderTwo,
                        UserId = (int) HttpContext.Session.GetInt32("UserId")
                    };
                    dbContext.Add(this_wedding);
                    dbContext.SaveChanges();
                    return RedirectToAction("Welcome");
                }
            }
            else
            {
                if(new_wedding.Date < DateTime.Today)
                {
                    ModelState.AddModelError("Date", "Date must be in the future!");
                }
                return View("NewWedding");
            }
        }
        [HttpGet]
        [Route("ViewWedding/{weddingId}")]
        public IActionResult ViewWedding(int weddingId)
        {
            Wedding wedding = dbContext.Weddings
            .Include(r => r.RSVPs)
            .ThenInclude(u => u.User)
            .Where(w => w.WeddingId == weddingId)
            .SingleOrDefault();

            ViewBag.Wedding = wedding;
            ViewBag.Address = wedding.Address;
            return View("ViewWedding");
        }
        
        [Route("RSVP")]
        public IActionResult RSVP(int weddingId)
        {
            RSVP new_rsvp = new RSVP
            {
                UserId = (int) HttpContext.Session.GetInt32("UserId"),
                WeddingId = weddingId
            };

            dbContext.Add(new_rsvp);
            dbContext.SaveChanges();

            return RedirectToAction("Welcome");
        }
        
        [Route("UnRSVP")]
        public IActionResult UnRSVP(int weddingId)
        {
            RSVP this_attender = dbContext.RSVP
            .SingleOrDefault(u => u.UserId == HttpContext.Session
            .GetInt32("UserId") && u.WeddingId == weddingId);

            dbContext.RSVP.Remove(this_attender);
            dbContext.SaveChanges();

            return RedirectToAction("Welcome");
        }
        
        [Route("Delete")]
        public IActionResult Delete(int weddingId)
        {
            Wedding this_wedding = dbContext.Weddings
            .SingleOrDefault(w => w.WeddingId == weddingId);

            List<RSVP> rsvps = dbContext.RSVP
            .Where(a => a.WeddingId == weddingId)
            .ToList();

            foreach(var attender in rsvps)
            {
                dbContext.RSVP.Remove(attender);
            }

            dbContext.Weddings.Remove(this_wedding);
            dbContext.SaveChanges();

            return RedirectToAction("Welcome");
        }
    }
}
