using Microsoft.AspNetCore.Mvc;
using TasteBuddies.DataAccess;
using TasteBuddies.Models;

namespace TasteBuddies.Controllers
{
    public class PostsController : Controller
    {
        private readonly TasteBuddiesContext _context;

        public PostsController(TasteBuddiesContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var postList = _context.Posts.OrderBy(post => post.CreatedAt).ToList();
            var last5Posts = new List<Post>();
            if(postList.Count > 5)
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
