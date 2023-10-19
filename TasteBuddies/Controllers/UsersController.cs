using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using TasteBuddies.DataAccess;
using TasteBuddies.Models;
using Serilog;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Composition;

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
			return Redirect("/");
            
        }





        // GET: /signup
        [Route("/Users/Signup")]
        public IActionResult Signup()
        {
            ViewData["UserCreateError"] = TempData["UserCreateError"];
            return View();
        }





        // POST: 
        [HttpPost]
        [Route("/Users/")]
        public IActionResult Create(User user)
        {
            if (!ModelState.IsValid)
            {
                Log.Warning("User model not valid for create action");
                TempData["UserCreateError"] = "One or more fields were invalid, please try again";

                return Redirect("/users/signup");
            }

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
            Log.Information("A user has been created");

            Response.Cookies.Append("CurrentUser", user.Id.ToString());
            return RedirectToAction("profile", new { userId = user.Id });
        }





        [Route("/Users/Profile")]
        public IActionResult Profile()
        {
            if (!Request.Cookies.ContainsKey("CurrentUser"))
            {
                return Redirect("/");
            }
            string id = Request.Cookies["CurrentUser"].ToString();

            if(int.TryParse(id, out int parsedId))
            {
                var user1 = _context.Users
              .Where(u => u.Id == parsedId)
              .Include(u => u.Posts)
              .Include(u => u.Groups)
              .FirstOrDefault();

                return View(user1);
            }
            else
            {
                Response.Cookies.Delete("CurrentUser");
                return NotFound();
            }
        }





        [Route("/Users/{userId:int}")]
        public IActionResult Show(int? userId)
        {
            if(userId is null)
            {
                return NotFound();
            }

            //if they navigate to their own show page, redirect to profile
            if (Request.Cookies.ContainsKey("CurrentUser"))
            {
                string cookieId = Request.Cookies["CurrentUser"].ToString();

                if (int.TryParse(cookieId, out int parsedId))
                {
                    if(parsedId == userId)
                    {
                        return Redirect("/users/profile");
                    }
                }
            }
            var user = _context.Users
                .Where(u => u.Id == userId)
                .Include(u => u.Posts)
                .FirstOrDefault();

            return View(user);
        }


        [Route("/Users/{id:int}/Edit")]
        public IActionResult Edit(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }

            var currentUserId = Request.Cookies["CurrentUser"];

            // if not logged in
            if(currentUserId is null)
            {
                return NotFound();
            }

            // trying to edit someone else's stuff
            if (currentUserId != id.ToString())
            {
                return StatusCode(403);
            }

            var user = _context.Users.Find(id);

            if(user is null)
            {
                return NotFound();
            }

            return View(user);

        }


        [HttpPost]
        [Route("/Users/update/{userId:int}")]
        public IActionResult Update(int? userId, User user)
        {
            if(userId is null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return NotFound();
            }

            var currentUserId = Request.Cookies["CurrentUser"];

            if (currentUserId != userId.ToString())
            {
                return StatusCode(403);
            }

            var existingUser = _context.Users.Find(userId);

            if(existingUser is null)
            {
                return NotFound();
            }

            existingUser.Name = user.Name;

            existingUser.UserName = user.UserName;

            _context.SaveChanges();
            Log.Information("A user's information has been updated.");

            return RedirectToAction("profile", new { userId = user.Id });

        }






        [Route("/users/delete/{userId:int}")]
        public IActionResult Delete(int? userId)
        {
            if(userId is null)
            {
                return NotFound();
            }
            if (Request.Cookies.ContainsKey("CurrentUser"))
            {
                if (int.TryParse(Request.Cookies["CurrentUser"], out int currentUserId))
                {
                    if (currentUserId == userId)
                    {
                        var userToDelete = _context.Users
                                        .Where(user => user.Id == userId)
                                        .Include(user => user.Posts)
                                        .First();

                        _context.Users.Remove(userToDelete);

                        _context.SaveChanges();

                        Response.Cookies.Delete("CurrentUser");
                        Log.Information($"A user has been deleted, id: {userId}");

                        return Redirect("/");
                    }
                    else
                    {
                        return StatusCode(403);
                    }
                }

                else
                {
                    Response.Cookies.Delete("CurrentUser");
                    return NotFound();
                }
            }

            else
            {
                return StatusCode(403);
            }
        }      


        // Goes to view to reset password
        [Route("/Users/{id:int}/ResetPassword")]
        public IActionResult ResetPassword(int? id)
        {
            if(id is null)
            {
                return NotFound();
            }
            if (Request.Cookies.ContainsKey("CurrentUser"))
            {
                var currentUserId = Request.Cookies["CurrentUser"];

                if (currentUserId != id.ToString())
                {
                    return StatusCode(403);
                }

                var user = _context.Users.Find(id);

                return View(user);
            }
            else
            {
                return Redirect("/users/login");
            }
        }





        // Updates password
        [Route("/Users/updatepassword/{id}")]
        public IActionResult UpdatePassword(int? id, string newPassword)
        {
            if (id is null)
            {
                return NotFound();
            }
            var user = _context.Users.Find(id);
            if (user is null)
            {
                return NotFound();
            }

            string digestedPassword = EncodePassword(newPassword);

            user.Password = digestedPassword;

            _context.SaveChanges();
            Log.Information("A user's password has been changed.");

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
