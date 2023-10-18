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



        public IActionResult Index()
        {

            var groups = _context.Groups;

            string id = Request.Cookies["CurrentUser"].ToString();


            if (string.IsNullOrEmpty(id) || !int.TryParse(id, out int userId))
            {
                // Handle the case where the cookie is missing or invalid
                return RedirectToAction("Index", "Home"); // Redirect to login page 
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




        [Route("/Groups/New")]
        public IActionResult New()
        {
            return View();
        }




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
