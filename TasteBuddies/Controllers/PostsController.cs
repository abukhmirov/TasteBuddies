using Microsoft.AspNetCore.Mvc;
using TasteBuddies.DataAccess;
using TasteBuddies.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Hosting;

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
            //Checking the current user
            string id = Request.Cookies["CurrentUser"].ToString();

            if (string.IsNullOrEmpty(id) || !int.TryParse(id, out int userId))
            {
                // Handle the case where the cookie is missing or invalid
                return RedirectToAction("Index", "Home"); // Redirect to login page 
            }


            int parseId = Int32.Parse(id);

            User user = _context.Users.Where(u => u.Id == parseId).FirstOrDefault();

            ViewBag.user = user;

            //Checking the current user


            var postList = _context.Posts
                .OrderByDescending(post => post.CreatedAt)
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
        public IActionResult Edit(int userId, int id)
        {
            var post = _context.Posts.Find(id);
            var user = _context.Users.Find(userId);

            

            return View(post);
        }

        // PUT: /Users/:id
        [HttpPost]
        [Route("/Users/{userId:int}/posts/{id:int}/update")]
        public IActionResult Update(Post posts, int id)
        {

            var existingPost = _context.Posts.Find(id);
            if (existingPost != null) 
            {
                return RedirectToAction("Feed");
            }

            posts.Upvotes = existingPost.Upvotes;

            posts.Id = id;
            posts.CreatedAt = DateTime.Now.ToUniversalTime();

            _context.Posts.Update(posts);
            _context.SaveChanges();

            var newUpvotes = posts.Upvotes;
            return RedirectToAction("Feed", new { id = posts.Id });
        }

        [HttpPost]
        [Route("/Users/{userId:int}/posts/{id:int}/delete")]
        public IActionResult Delete(int userId,int id)
        {
            var posts = _context.Posts.Find(id);
            _context.Posts.Remove(posts);
            _context.SaveChanges();
            return RedirectToAction("Feed", new { id = posts.Id });
        }
        [HttpPost]
        public IActionResult Upvote(int postId)
        {
            var post = _context.Posts.FirstOrDefault(p => p.Id == postId);
             if(post.Upvotes > 1)
            {
                return BadRequest("Stop clicking the button");
            }
        
            post.Upvote();
            _context.SaveChanges();
            var newUpvotes = post.Upvotes;
            return RedirectToAction("Feed", new {V = post.Upvotes = newUpvotes});
            
        }

    }
}