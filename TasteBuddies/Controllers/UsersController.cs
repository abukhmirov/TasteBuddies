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





        // View the login page.

        [Route("/users/login")]
        public IActionResult Login()
        {

            return View();

        }




        // Verifies the provided username and password during login.
        // If valid, sets a cookie for the current user and redirects to the user's profile.
        // Otherwise, returns an error and asks the user to try logging in again.


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




        // Logs out the current user by removing the "CurrentUser" cookie.
        // Then, redirects the user to the root (home) page.

        [Route("/users/logout")]
        public IActionResult Logout()
        {

            // Delete the "CurrentUser" cookie to log the user out.
            Response.Cookies.Delete("CurrentUser");

            // Redirect the user to the root (home) page.
            return Redirect("/");

        }






        //View the signup page

        [Route("/Users/Signup")]
        public IActionResult Signup()
        {
            ViewData["UserCreateError"] = TempData["UserCreateError"];
            return View();
        }






        // Handles the user registration process.
        // Checks if the username is already taken.
        // If not, hashes the password, creates a new user in the database, 
        // logs the action, sets a "CurrentUser" cookie, and redirects to the user's profile.

        [HttpPost]
        [Route("/Users/")]
        public IActionResult Create(User user)
        {
            if (!ModelState.IsValid)
            {
                Log.Warning("User model not valid for create action");
                TempData["UserCreateError"] = "One or more fields were invalid, please try again";

                return View("Singup", user);
            }

            var existingUser = _context.Users.FirstOrDefault(u => u.UserName == user.UserName);

            if (existingUser != null)
            {
                ModelState.AddModelError("Username", "Username is already taken. Please choose a different one.");
                return View("SignUp", user);
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





        // View the profile page for the current logged-in user.
        // Retrieves user's details along with related posts and groups.

        [Route("/Users/Profile")]
        public IActionResult Profile(bool passwordChanged = false)
        {
            if (!Request.Cookies.ContainsKey("CurrentUser"))
            {
                return Redirect("/");
            }
            string id = Request.Cookies["CurrentUser"].ToString();

            if (int.TryParse(id, out int parsedId))
            {
                var user1 = _context.Users
              .Where(u => u.Id == parsedId)
              .Include(u => u.Posts)
              .Include(u => u.Groups)
              .FirstOrDefault();

                ViewBag.PasswordChanged = passwordChanged;

                return View(user1);
            }
            else
            {
                Response.Cookies.Delete("CurrentUser");
                return NotFound();
            }
        }





        // View a page that displays details of a specific user based on the provided ID.

        [Route("/Users/{userId:int}")]
        public IActionResult Show(int? userId)
        {
            if (userId is null)
            {
                return NotFound();
            }

            //if they navigate to their own show page, redirect to profile
            if (IsCurrentUser((int)userId))
            {
                return Redirect("/users/profile");
            }
            var user = GetUserWithPosts((int)userId);

            return View(user);
        }





        // View the edit page for the current user's details

        [Route("/Users/{id:int}/Edit")]
        public IActionResult Edit(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }

            if (!IsCurrentUser((int)id))
            {
                return BadRequest();
            }

            var user = _context.Users.Find(id);

            if (user is null)
            {
                return NotFound();
            }

            return View(user);

        }





        // Handles the process of updating the current user's details.
        // If successful, redirects to the user's updated profile.

        [HttpPost]
        [Route("/Users/update/{userId:int}")]
        public IActionResult Update(int? userId, User user)
        {
            if (userId is null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return NotFound();
            }

            if (!IsCurrentUser((int)userId))
            {
                return BadRequest();
            }

            var existingUser = _context.Users.Find(userId);

            if (existingUser != null)
            {
                ModelState.AddModelError("Username", "Username is already taken. Please choose a different one.");
                return View("Edit", user);
            }

            existingUser.Name = user.Name;

            existingUser.UserName = user.UserName;

            _context.SaveChanges();
            Log.Information("A user's information has been updated.");

            return RedirectToAction("profile", new { userId = user.Id });

        }






        // Deletes the current user's account along with related posts.
        // Afterwards, logs out the user and redirects to the root page.

        [Route("/users/delete/{userId:int}")]
        public IActionResult Delete(int? userId)
        {
            if (userId is null)
            {
                return NotFound();
            }

            if (IsCurrentUser((int)userId))
            {
                var userToDelete = GetUserWithPosts((int)userId);

                _context.Users.Remove(userToDelete);

                _context.SaveChanges();

                Response.Cookies.Delete("CurrentUser");
                Log.Information($"A user has been deleted, id: {userId}");

                return Redirect("/");
            }
            else
            {
                return BadRequest();
            }
        }


        // Goes to view to reset password
        [Route("/Users/{id:int}/ResetPassword")]
        public IActionResult ResetPassword(int? id)
        {
            if (id is null)
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






        // Handles the process of updating the user's password.
        // Encrypts the new password and saves the changes.
        // Then, redirects the user to their profile.

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

            return RedirectToAction("profile", new { userId = user.Id, passwordChanged = true });

        }





        //Encoding passwords method

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


        // checks if user cookie matches userId provided
        private bool IsCurrentUser(int userId)
        {
            if (!Request.Cookies.ContainsKey("CurrentUser"))
            {
                return false;
            }
            if (int.TryParse(Request.Cookies["CurrentUser"], out int parseId))
            {
                if (userId == parseId)
                {
                    return true;
                }
                else return false;
            }
            else return false;
        }

        private User GetUserWithPosts(int userId)
        {
            return _context.Users
                .Where(user => user.Id == userId)
                .Include(user => user.Posts)
                .First();
        }
    }
}
