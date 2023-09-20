using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
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

        [Route("/users/{userId:int}/login")]
        public IActionResult Login(int userId)
        {
            var user = _context.Users.Find(userId);
            return View(user);
        }

        [HttpPost]
        [Route("/users/{userId:int}/login")]
        public IActionResult CheckPassword(string password, int userId)
        {
            var user = _context.Users.Find(userId);
            if (user.Password == EncodePassword(password))
            {
                return Redirect($"/users/{user.Id}");
            }
            else
            {
                return Redirect("/users");
            }
        }

        private string EncodePassword(string password)
        {
            HashAlgorithm sha = SHA256.Create();

            byte[] passwordBytes = Encoding.ASCII.GetBytes(password);
            byte[] passwordDigested = sha.ComputeHash(passwordBytes);
            StringBuilder passwordBuilder = new StringBuilder();
            foreach (byte b in passwordDigested)
            {
                passwordBuilder.Append(b.ToString("x2"));
            }
            return passwordBuilder.ToString();
        }
    }
}
