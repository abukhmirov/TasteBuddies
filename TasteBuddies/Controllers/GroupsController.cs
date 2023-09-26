using Microsoft.AspNetCore.Mvc;
using TasteBuddies.DataAccess;
using TasteBuddies.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;
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

            _context.Add(group);
            _context.SaveChanges();

            Response.Cookies.Append("CurrentGroup", group.Id.ToString());

            return Redirect("/Groups");
        }

        [HttpPost]
       
        public IActionResult Join(Group group)
        {
            string id = Request.Cookies["CurrentUser"].ToString();
            int parseId = Int32.Parse(id);

            Response.Cookies.Append("CurrentGroup", group.Id.ToString());

            var currentUserCookie = Request.Cookies["CurrentUser"];
            var currentGroupCookie = Request.Cookies["CurrentGroup"];



            var dbUser = _context.Users.FirstOrDefault(u => u.Id == parseId);
            var dbGroup = _context.Groups.FirstOrDefault(g => g.Id == group.Id);

            

            if (dbUser != null && dbGroup != null)
            {
                dbGroup.Users.Add(dbUser);
                dbUser.Groups.Add(dbGroup);
        
                _context.SaveChanges();
            }

            return Redirect("/Groups/Feed");
        }



        public IActionResult Feed()
        {
            //var postList = _context.Posts.Where(p => U);

            var postList = _context.Posts
                .OrderBy(post => post.CreatedAt)
                .Include(post => post.User)
                .ToList();

            var last5Posts = new List<Post>();

            if (postList.Count > 5)
            {
                last5Posts.AddRange(postList.TakeLast(5));
            }
            else
            {
                last5Posts = postList;
            }
            return View(last5Posts);
        }
    }
}
