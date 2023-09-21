using Microsoft.AspNetCore.Mvc;
using TasteBuddies.DataAccess;
using TasteBuddies.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;

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
            var posts = _context.Posts;
            return View(posts);
        }

        public IActionResult Feed()
        {
            return View();
        }


        [HttpPost]
        public IActionResult Create(Post post)
        {
            var user = _context.Users.Where(u => u.Id == post.Id).Include(u => u.Posts).First();
            post.CreatedAt = DateTime.Now.ToUniversalTime();
            _context.Posts.Add(post);
            user.Posts.Add(post);
            _context.SaveChanges();



            return RedirectToAction("Feed","Users", new { id = post.Id });

        }
        [Route("/Users/{userId:int}/posts/{id:int}/edit")]
        public IActionResult Edit(int userId, int postId)
        {
            var post = _context.Posts.Find(postId);
            var user = _context.Users.Find(userId);\
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

    }
}