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

        [Route("/users/login")]
        public IActionResult Login()
        {  
            return View();
        }

        [HttpPost]
        [Route("/users/login")]
        public IActionResult CheckPassword(string password, string username)
        {
            var user = _context.Users
                .Where(user => user.UserName == username 
                && user.Password == EncodePassword(password))
                .First();

            if(user is null)
            {
                return Redirect("/users/login");
            }
            else
            {
                Response.Cookies.Append("CurrentUser", user.Id.ToString());
                return Redirect($"/users/{user.Id}");
            }
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

            Response.Cookies.Append("CurrentUser", user.Id.ToString());

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
        
        [Route("/Users/{id:int}/edit")]
        public IActionResult Edit(int id)
        {
            var user = _context.Users.Find(id);

            return View(user);
        }

        [HttpPost]
        [Route("/Users/update/{userId:int}")]
        public IActionResult Update(int userId, User user)
        {
            user.Id = userId;
            var existingUser = _context.Users.Find(userId);

            existingUser.Name = user.Name;
            existingUser.UserName = user.UserName;
            _context.SaveChanges();

            return RedirectToAction("show", new { userId = user.Id });
        }

        // Goes to view to reset password
        [Route("/Users/{id:int}/resetpassword")]
        public IActionResult ResetPassword(int id)
        {
            var user = _context.Users.Find(id);

            return View(user);
        }

        // Updates password
        [Route("/Users/updatepassword/{id}")]
        public IActionResult UpdatePassword(int id, string newPassword)
        {
            var user = _context.Users.Find(id);
            string digestedPassword = EncodePassword(newPassword);
            user.Password = digestedPassword;
            _context.SaveChanges();

            return RedirectToAction("show", new { userId = user.Id });
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
