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
            return View(wrapper);
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
                return RedirectToAction("Dashboard");
            }
            Login_Register_wrapper wrapper=new Login_Register_wrapper();
            return View("Index", wrapper);

        }

        [HttpPost("login")]
        public IActionResult Login(LoginUser FromForm)
        {
            if(ModelState.IsValid)
            {
                // If inital ModelState is valid, query for a user with provided email
                var userInDb = _context.Users.FirstOrDefault(u => u.Email == FromForm.Email);
                // If no user exists with provided email
                if(userInDb == null)
                {
                    // Add an error to ModelState and return to View!
                    ModelState.AddModelError("Email", "Invalid Email/Password");
                    return RedirectToAction("Login");
                }
                
                // Initialize hasher object
                var hasher = new PasswordHasher<LoginUser>();
                
                // verify provided password against hash stored in db
                var result = hasher.VerifyHashedPassword(FromForm, userInDb.Password, FromForm.Password);
                
                // result can be compared to 0 for failure
                if(result == 0)
                {
                    // handle failure (this should be similar to how "existing email" is handled)
                    return RedirectToAction("Dashboard");
                }
                HttpContext.Session.SetInt32("active_user", userInDb.UserId);
                return RedirectToAction("Details", new {accountId = userInDb.UserId});
            }
            Login_Register_wrapper wrapper=new Login_Register_wrapper();
            return RedirectToAction("Index", wrapper);
        }

        [HttpGet("dashboard")]
        public IActionResult Dashboard()
        {
            if(HttpContext.Session.GetString("Firstname") != null){
                List<WeddingPlan> AllPlans = _context.WeddingPlans
                    .Include(w => w.Guests)
                    .ToList();
                    ViewBag.UserId =(int)HttpContext.Session.GetInt32("id");
                    return View("AllPlans");
            }
            else{
                return RedirectToAction("Index");
            }
        }

        [HttpGet("un-rsvp/{PlanId}")]
        public IActionResult Unrsvp(int PlanId)
        {
            if(HttpContext.Session.GetInt32("active_userId") != null){
                List<Resevation> resevations = _context.Resevations.Where(r => r.WeddingPlanId == PlanId)
                    .ToList();
                int resevationId = 0;
                foreach(Resevation x in resevations){
                    if(x.UserId == (int)HttpContext.Session.GetInt32("id"))
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
            if(HttpContext.Session.GetInt32("active_userId") != null){
                Resevation resevation = new Resevation();
                resevation.UserId = (int)HttpContext.Session.GetInt32("id");
                resevation.WeddingPlanId = PlanId;
                _context.Resevations.Add(resevation);
                _context.SaveChanges();
                return RedirectToAction("Dashboard");
            }
            return RedirectToAction("Index");
        }

        [HttpGet("delet/{PlanId}")]
        public IActionResult Delete(int PlanId)
        {
            if(HttpContext.Session.GetInt32("active_userId") != null) {
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
               
                WeddingPlan.UserId = (int)HttpContext.Session.GetInt32("id");
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

        [HttpGet("detail/{PlanId")]
        public IActionResult Display_Plan_Detail(int PlanId)
        {
            List<Resevation> Guest = _context.Resevations
                .Include(r => r.User)
                .Where(r => r.WeddingPlanId == PlanId)
                .ToList();

            WeddingPlan Oneplan = _context.WeddingPlans
                .FirstOrDefault(w => w.WeddingPlanId == PlanId);
            DisplayOnePlan display_One_Plan = new DisplayOnePlan
            {
                Guest = Guest,
                Oneplan = Oneplan
            };
            return View(display_One_Plan);
        }




    }

}