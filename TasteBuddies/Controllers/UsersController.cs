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
    }
}
