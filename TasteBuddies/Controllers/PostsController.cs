using Microsoft.AspNetCore.Mvc;
using TasteBuddies.DataAccess;
using TasteBuddies.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

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

            return View();
        }

        public IActionResult Feed()
        {
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


        [HttpPost]
        public IActionResult Create(Post post)
        {
            string id = Request.Cookies["CurrentUser"].ToString();
            int parseId = Int32.Parse(id);


            var user = _context.Users.Where(u => u.Id == parseId).Include(u => u.Posts).FirstOrDefault();
            post.CreatedAt = DateTime.Now.ToUniversalTime();
            _context.Posts.Add(post);
            user.Posts.Add(post);
            _context.SaveChanges();



            return Redirect("/Posts/Feed");
        }

        [Route("/Users/{userId:int}/posts/{id:int}/edit")]
        public IActionResult Edit(int userId, int postId)
        {
            var post = _context.Posts.Find(postId);
            var user = _context.Users.Find(userId);
            post.User = user;

            return View(post);
        }

        // PUT: /Users/:id
        [HttpPost]
        [Route("/posts/{id:int}")]
        public IActionResult Update(Post posts,int userId, int postId)
        {
            posts.Id = postId;
            posts.CreatedAt = DateTime.Now.ToUniversalTime();
            _context.Posts.Update(posts);
            _context.SaveChanges();

            return RedirectToAction("Feed", new { id = posts.Id });
        }

        [HttpPost]
        public IActionResult Delete(int userId,int postId)
        {
            var posts = _context.Posts.Find(postId);
            _context.Posts.Remove(posts);
            _context.SaveChanges();
            return RedirectToAction("Feed", new { id = posts.Id });
        }
        [HttpPost]
        public IActionResult Upvote(int postId)
        {
            var post = _context.Posts.FirstOrDefault(p => p.Id == postId);
            post.Upvote();
            _context.SaveChanges();
            var newUpvotes = post.Upvotes;
            return RedirectToAction("Feed", new {V = post.Upvotes = newUpvotes});
            
        }

    }
}