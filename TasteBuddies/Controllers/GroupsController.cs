using Microsoft.AspNetCore.Mvc;
using TasteBuddies.DataAccess;
using TasteBuddies.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;
using Serilog;
namespace TasteBuddies.Controllers
{
    public class GroupsController : Controller
    {


        private readonly TasteBuddiesContext _context;



        public GroupsController(TasteBuddiesContext context)
        {
            _context = context;
        }




        // This action returns a list of all groups.
        // If the user has a valid "CurrentUser" cookie, it retrieves the user from the database and sets it to the ViewBag.
        // If the user does not have a valid "CurrentUser" cookie, it redirects them to the Home controller's Index action.
        public IActionResult Index()
        {

            var groups = _context.Groups;

            string id = Request.Cookies["CurrentUser"].ToString();


            if (string.IsNullOrEmpty(id) || !int.TryParse(id, out int userId))
            {
                // Handle the case where the cookie is missing or invalid
                return RedirectToAction("Index", "Home"); // Redirect to Index page 
            }



            int parseId = Int32.Parse(id);

            var groupsWithUsers = _context.Groups.Include(g => g.Users);

            User user = new User();

            foreach (var group in groupsWithUsers)
            {

                foreach (var user1 in group.Users)
                {

                    if (user1.Id == parseId)
                    {
                        user = user1;
                        break;
                    }

                }
            }

            ViewBag.user = user;

            return View(groups);
        }



        // This action returns the view for creating a new group.
        [Route("/Groups/New")]
        public IActionResult New()
        {
            return View();
        }



        // This action handles the creation of a new group.
        // It first checks for a valid "CurrentUser" cookie.
        // If the cookie is valid, it associates the current user with the group being created and then saves it.
        // It also logs the creation of the group.
        // After creation, it sets a "CurrentGroup" cookie and redirects the user back to the list of groups.
        [HttpPost]
        [Route("/Groups")]
        public IActionResult Create(Group group)
        {

            string id = Request.Cookies["CurrentUser"].ToString();

            if (string.IsNullOrEmpty(id) || !int.TryParse(id, out int userId))
            {
                // Handle the case where the cookie is missing or invalid
                return RedirectToAction("Index", "Home"); // Redirect to login page 
            }


            int parseId = Int32.Parse(id);


            var dbUser = _context.Users.FirstOrDefault(u => u.Id == parseId);
           

            group.Users.Add(dbUser);


            _context.Add(group);
            _context.SaveChanges();
            Log.Information($"A new [{group.Id}]group has been created by [{id}]user.");

            Response.Cookies.Append("CurrentGroup", group.Id.ToString());

            return Redirect("/Groups");
        }




        // This action handles a user joining a group.
        // It first checks for a valid "CurrentUser" cookie.
        // If the cookie is valid, it associates the current user with the group they want to join and then saves the changes.
        // It also logs the action.
        // After joining, it redirects the user to their profile page.
        [HttpPost]
        public IActionResult Join()
        {

            string id = Request.Cookies["CurrentUser"].ToString();

            if (string.IsNullOrEmpty(id) || !int.TryParse(id, out int userId))
            {
                // Handle the case where the cookie is missing or invalid
                return RedirectToAction("Index", "Home"); // Redirect to login page 
            }


            int parseId = Int32.Parse(id);

            int groupId = Int32.Parse(Request.Form["GroupId"]);



            var dbUser = _context.Users.FirstOrDefault(u => u.Id == parseId);
            var dbGroup = _context.Groups.FirstOrDefault(g => g.Id == groupId);
       
            dbGroup.Users.Add(dbUser);
        

            _context.SaveChanges();
            Log.Information($"A [{dbUser.Id}]user has joined a new [{dbGroup.Id}]group.");

            return RedirectToAction("Profile", "Users");
        }

    }
}
