using Microsoft.AspNetCore.Authorization;
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


        //Login verification

        [HttpPost]
        [Route("/users/login")]
        public IActionResult CheckPassword(string password, string username)
        {

			// Check if either the password or username is missing.

			if (password == null || username == null)
			{
				// If either is missing, add a validation error and return to the login page.
				ModelState.AddModelError("LoginFail", "Wrong password or username. Try again!");
				return View("Login");
			}

			// Query the database to find a user with the provided username and password.
			var user = _context.Users
                .Where(user => user.UserName == username 
                && user.Password == EncodePassword(password))
                .FirstOrDefault();


			//If the database query doesn't return anything, add a validation error and return to the login page.
			if (user == null)
            {
                ModelState.AddModelError("LoginFail", "Wrong password or username. Try again!");
                return View("Login");
            }

            //Otherwise add the user cookie and return the user's profile
            else
            {
                Response.Cookies.Append("CurrentUser", user.Id.ToString());
                return Redirect($"/users/profile");
            }
        }



        
        [Route("/users/logout")]
        public IActionResult Logout()
        {

			// Delete the "CurrentUser" cookie to log the user out.
			Response.Cookies.Delete("CurrentUser");
			// Redirect the user to the root (home) page.
			return Redirect($"/");
            
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
            var existingUser = _context.Users.FirstOrDefault(u => u.UserName == user.UserName);

            if (existingUser != null)
            {
                ModelState.AddModelError("Username", "Username is already taken. Please choose a different one.");
                return View("SignUp");
            }

            User userModel = new User();
            string digestedPassword = userModel.GetDigestedPassword(user.Password);
            user.Password = digestedPassword;
            _context.Add(user);
            _context.SaveChanges();

            Response.Cookies.Append("CurrentUser", user.Id.ToString());

            return RedirectToAction("profile", new { userId = user.Id });
        }

        [Route("/Users/Profile")]
        public IActionResult Profile(int userId)
        {
            string id = Request.Cookies["CurrentUser"].ToString();
            int parseId = Int32.Parse(id); 
            var user1 = _context.Users
              .Where(u => u.Id == parseId)
              .Include(u => u.Posts)
              .Include (u => u.Groups)
              .FirstOrDefault();

            return View(user1);
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

        [Route("/Users/{id:int}/Edit")]
        public IActionResult Edit(int id)
        {
            var currentUserId = Request.Cookies["CurrentUser"];

            if (currentUserId != id.ToString())
            {
                return StatusCode(403);
            }

            var user = _context.Users.Find(id);

            return View(user);
        }

        [HttpPost]
        [Route("/Users/update/{userId:int}")]
        public IActionResult Update(int userId, User user)
        {
            var currentUserId = Request.Cookies["CurrentUser"];

            if (currentUserId != userId.ToString())
            {
                return StatusCode(403);
            }

            var existingUser = _context.Users.Find(userId);

            existingUser.Name = user.Name;
            existingUser.UserName = user.UserName;
            _context.SaveChanges();

            return RedirectToAction("profile", new { userId = user.Id });
        }

        [Route("/users/delete/{userId:int}")]
        public IActionResult Delete(int userId)
        {
            if (Request.Cookies.ContainsKey("CurrentUser"))
            {
                if (userId == int.Parse(Request.Cookies["CurrentUser"]))
                {
                    var userToDelete = _context.Users
                        .Where(user => user.Id == userId)
                        .Include(user => user.Posts)
                        .First();

                    _context.Users.Remove(userToDelete);
                    _context.SaveChanges();

                    Response.Cookies.Delete("CurrentUser");

                    return Redirect("/");
                }
                else
                {
                    return StatusCode(403);
                }
            }
            else
            {
                return StatusCode(403);
            }
        }

        // Goes to view to reset password
        [Route("/Users/{id:int}/ResetPassword")]
        public IActionResult ResetPassword(int id)
        {
            var currentUserId = Request.Cookies["CurrentUser"];

            if (currentUserId != id.ToString())
            {
                return StatusCode(403);
            }

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

            return RedirectToAction("profile", new { userId = user.Id });
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
