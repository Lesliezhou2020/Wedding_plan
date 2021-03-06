using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WeddingPlanner.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace WeddingPlanner.Controllers
{
    public class HomeController : Controller
    {
        private MyContext _context;
        
        public HomeController(MyContext context)
        {
            _context = context;
        }

        [HttpGet("")]
        public IActionResult Index()
        {
            Login_Register_wrapper wrapper = new Login_Register_wrapper();
            return View("Index", wrapper);
        }

        [HttpPost("process_register")]
        public IActionResult Process_Register(User FromForm)
        {
            if(ModelState.IsValid)
            {
                PasswordHasher<User> Hasher = new PasswordHasher<User>();
                FromForm.Password = Hasher.HashPassword(FromForm, FromForm.Password);
                _context.Add(FromForm);
                _context.SaveChanges();
                var user = _context.Users.FirstOrDefault(u => u.Email == FromForm.Email);
                HttpContext.Session.SetInt32("active_user", user.UserId);
                return RedirectToAction("Dashboard");
            }
            Login_Register_wrapper wrapper=new Login_Register_wrapper();
            return View("Index", wrapper);

        }

        [HttpPost("login")]
        public IActionResult Login(LoginUser FromForm)
        {
            Login_Register_wrapper wrapper=new Login_Register_wrapper();
            if(ModelState.IsValid)
            {
                // If inital ModelState is valid, query for a user with provided email
                var userInDb = _context.Users.FirstOrDefault(u => u.Email == FromForm.Email);
                // If no user exists with provided email
                if(userInDb == null)
                {
                    // Add an error to ModelState and return to View!
                    ModelState.AddModelError("Email", "Invalid Email");
                    return View("Index", wrapper);
                }
                
                // Initialize hasher object
                var hasher = new PasswordHasher<LoginUser>();
                
                // verify provided password against hash stored in db
                var result = hasher.VerifyHashedPassword(FromForm, userInDb.Password, FromForm.Password);
                
                // result can be compared to 0 for failure
                if(result == 0)
                {
                    // handle failure (this should be similar to how "existing email" is handled)
                    ModelState.AddModelError("Password", "Invalid Password");
                    return View("Index", wrapper);
                }
                HttpContext.Session.SetInt32("active_user", userInDb.UserId);
                return RedirectToAction("Dashboard");
            }
            return RedirectToAction("Index", wrapper);
        }

        [HttpGet("dashboard")]
        public IActionResult Dashboard()
        {
            int? active_user= HttpContext.Session.GetInt32("active_user");
            if(active_user.HasValue){
                List<WeddingPlan> AllPlans = _context.WeddingPlans
                    .Include(w => w.Guests)
                    .ToList();
                    ViewBag.UserId = active_user;
                    return View("Dashboard", AllPlans);
            }
            else{
                return RedirectToAction("Index");
            }
        }

        [HttpGet("un-rsvp/{PlanId}")]
        public IActionResult Unrsvp(int PlanId)
        {
            int? active_userId = HttpContext.Session.GetInt32("active_user");
            if(active_userId.HasValue){
                List<Resevation> resevations = _context.Resevations.Where(r => r.WeddingPlanId == PlanId)
                    .ToList();
                int resevationId = 0;
                foreach(Resevation x in resevations){
                    if(x.UserId == active_userId.Value)
                    {
                        resevationId = x.ResevationId;
                    }
                }
                Resevation res = _context.Resevations.FirstOrDefault(r => r.ResevationId == resevationId);
                _context.Resevations.Remove(res);
                _context.SaveChanges();
                return RedirectToAction("Dashboard");
            
            }
            else{
                return RedirectToAction("Index");
            }
        }

        [HttpGet("rsvp/{PlanId}")]
        public IActionResult Rsvp(int PlanId)
        {
            int? active_user = HttpContext.Session.GetInt32("active_user");
            if(active_user.HasValue){
                Resevation resevation = new Resevation();
                resevation.UserId = active_user.Value;
                resevation.WeddingPlanId = PlanId;
                _context.Resevations.Add(resevation);
                _context.SaveChanges();
                return RedirectToAction("Dashboard");
            }
            return RedirectToAction("Index");
        }

        [HttpGet("delete/{PlanId}")]
        public IActionResult Delete(int PlanId)
        {
            int? active_user = HttpContext.Session.GetInt32("active_user");
            if(active_user.HasValue) {
                WeddingPlan plan = _context.WeddingPlans.FirstOrDefault(p => p.WeddingPlanId == PlanId);
                _context.WeddingPlans.Remove(plan);
                _context.SaveChanges();
                return RedirectToAction("Dashboard");
            }
            return RedirectToAction("Index");
        }

        [HttpPost("process_newplan")]
        public IActionResult Process_newPlan(WeddingPlan WeddingPlan)
        {
            if(ModelState.IsValid){
               
                WeddingPlan.UserId = HttpContext.Session.GetInt32("active_user").Value;
                _context.Add(WeddingPlan);
                _context.SaveChanges();
                return RedirectToAction("Dashboard");
            }
            return View("AddWeddingPlan");
        }

        [HttpGet("new")]
        public IActionResult AddWeddingPlan()
        {
            return View();
        }

        [HttpGet("detail/{PlanId}")]
        public IActionResult DisplayOnePlan(int PlanId)
        {
            List<Resevation> guest = _context.Resevations
                .Include(r => r.User)
                .Where(r => r.WeddingPlanId == PlanId)
                .ToList();

            WeddingPlan oneplan = _context.WeddingPlans
                .FirstOrDefault(w => w.WeddingPlanId == PlanId);
            DisplayOnePlan DisplayOnePlan = new DisplayOnePlan
            {
                Guest = guest,
                Oneplan = oneplan
            };
            return View("DisplayOnePlan", DisplayOnePlan);
        }




    }

}