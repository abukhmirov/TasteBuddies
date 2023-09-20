using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TasteBuddies.DataAccess;
using TasteBuddies.Models;

namespace TasteBuddies.Controllers
{
    public class UsersController : Controller
    {

        private readonly TasteBuddiesContext _context;

        public UsersController(TasteBuddiesContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        [Route("/users/{id:int}/login")]
        public IActionResult Login(User user)
        {


            return View(user);
        }

        [HttpPost]
        [Route("/users/{id:int}/login")]
        public IActionResult LoginAttemp(string password, int id)
        {
            var user = _context.Users.Find(id);

            if (user.VerifyPassword(password))
            {
                var userActual = _context.Users.Where(e => e.UserName == user.UserName).Single();

                return Redirect("/users/details/" + userActual.Id);
            }

            return BadRequest("Invalid username or password.");



        }

        // GET: /signup
        [Route("/Users/Signup")]
        public IActionResult Signup()
        {
            return View();
        }

        // POST: 
        [HttpPost]
        [Route("/Users/")]
        public IActionResult Create(User user)
        {
            User userModel = new User();
            string digestedPassword = userModel.GetDigestedPassword(user.Password);
            user.Password = digestedPassword;
            _context.Add(user);
            _context.SaveChanges();

            return RedirectToAction("show", new { userId = user.Id });
        }

        [Route("/Users/{userId:int}")]
        public IActionResult Show(int userId)
        {
            var user = _context.Users
              .Where(u => u.Id == userId)
              .Include(u => u.Posts)
              .FirstOrDefault();

            return View(user);
        }
    }
}
