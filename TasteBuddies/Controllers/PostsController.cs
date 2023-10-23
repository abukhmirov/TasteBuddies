using Microsoft.AspNetCore.Mvc;
using TasteBuddies.DataAccess;
using TasteBuddies.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace TasteBuddies.Controllers
{
    public class PostsController : Controller
    {

        private readonly TasteBuddiesContext _context;

        public PostsController(TasteBuddiesContext context)
        {
            _context = context;
        }




        //This action returns the Feed view

        public IActionResult Index()
        {

            return View();
        }





        // This action handles the display of the feed to the current user.
        // It first checks for a valid "CurrentUser" cookie to determine the user's identity.
        // If the cookie is valid, it retrieves the associated user from the database and assigns it to the view.
        // It then fetches all posts from the database, orders them by creation date in descending order, and includes related user details.
        // To display only the latest content, the action takes the last 5 posts (or all if there are fewer than 5).
        // Finally, it returns the posts to be displayed on the feed view.
        [HttpGet]

        public IActionResult Feed(bool postUpdated = false, bool postDeleted = false)
        {
            if (!Request.Cookies.ContainsKey("CurrentUser"))
            {
                return Redirect("/users/login");
            }

            //Checking the current user
            string id = Request.Cookies["CurrentUser"].ToString();

            if (string.IsNullOrEmpty(id) || !int.TryParse(id, out int userId))
            {
                // Handle the case where the cookie is missing or invalid
                return RedirectToAction("Index", "Home"); // Redirect to login page 
            }

            int parseId = Int32.Parse(id);

            User user = _context.Users.Where(u => u.Id == parseId).FirstOrDefault();

            if(user is null)
            {
                return NotFound();
            }

            ViewBag.user = user;
            ViewBag.PostUpdated = postUpdated;
            ViewBag.PostDeleted = postDeleted;


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






        // This action handles the creation of a new post by the current user.
        // It first retrieves the "CurrentUser" cookie to identify the user.
        // If the cookie is valid, it fetches the associated user from the database along with their existing posts.
        // The action then sets the creation time of the post to the current UTC time.
        // It adds the post to both the global list of posts and the specific user's list of posts.
        // Once saved, a log entry is generated indicating the post creation and the responsible user.
        // After the post creation, the user is redirected to the feed showing the latest posts.

        [HttpPost]
        public IActionResult Create(Post post)
        {
            if(ModelState.IsValid)
            {
                if (!Request.Cookies.ContainsKey("CurrentUser"))
                {
                    return Redirect("/users/login");
                }
                string id = Request.Cookies["CurrentUser"].ToString();

                if (int.TryParse(id, out int parseId))
                {
                    var user = GetUserWithPosts(parseId);

                   post.CreatedAt = DateTime.Now.ToUniversalTime();

                   _context.Posts.Add(post);
                   user.Posts.Add(post);

                   _context.SaveChanges();
                   Log.Information($"A post has been created by user: [{user.Id}]{user.UserName}");

                   return Redirect("/Posts/Feed");
                }
                else
                {
                    Response.Cookies.Delete("CurrentUser");
                    return Redirect("/");
                }
            }
            else
            { 
                Log.Warning("Image URL is invalid.");

                return View("Index", post);
            }
        }






        // This action handles the process of editing a specific post by a user.
        // The route specifies two parameters: the user's ID and the post's ID.
        // It first retrieves the specific post and user based on their respective IDs.
        // After fetching the post, it returns it to the edit view, where changes can be made.

        [Route("/Users/{userId:int}/posts/{id:int}/edit")]
        public IActionResult Edit(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }
            var post = _context.Posts.Include(p => p.User).FirstOrDefault(p => p.Id == id);
            if(post is null)
            {
                return NotFound();
            }

            

            return View(post);
        }






        // This action handles the process of updating an existing post for a specific user.
        // The route specifies two parameters: the user's ID and the post's ID.
        // It retrieves the post and user from the database based on these IDs.
        // If the post doesn't exist, it redirects the user to the feed.
        // Otherwise, it updates the post details and saves them.
        // A log is generated indicating the post update and the responsible user.
        // Finally, the user is redirected to the feed with the updated post's ID.

        [HttpPost]
        [Route("/Users/{userId:int}/posts/{id:int}/update")]
        public IActionResult Update(Post post, int? id, int? userId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            if (id is null || userId is null)
            {
                return NotFound();
            }
            if (!IsCurrentUser((int)userId))
            {
                return BadRequest();
            }

            var user = _context.Users.Find(userId);
            var existingPost = _context.Posts.Find(id);

            if (user is null)
            {
                return NotFound();
            }
            if (existingPost is null) 
            {
                return NotFound();
            }

            existingPost.Title = post.Title;

            existingPost.Description = post.Description;

            existingPost.ImageURL = post.ImageURL;



            existingPost.CreatedAt = DateTime.Now.ToUniversalTime();

            _context.Posts.Update(existingPost);
            _context.SaveChanges();
            
            Log.Information($"A [{existingPost.Id}]post has been updated by user: [{user.Id}]{user.UserName}");

            return RedirectToAction("Feed", new { id = existingPost.Id, postUpdated = true });
        }





        // This action manages the deletion of a specific post by a user.
        // It first fetches the post and user based on their respective IDs.
        // The post is then removed from the database, and a log is generated indicating its deletion and the responsible user.
        // After deletion, the user is redirected to the feed.

        [HttpPost]
        [Route("/Users/{userId:int}/posts/{id:int}/delete")]
        public IActionResult Delete(int? userId,int? id)
        {
            if (userId is null || id is null)
            {
                return NotFound();
            }
            var user = _context.Users.Find(userId);
            var post = _context.Posts.Find(id);
            if (user is null || post is null)
            {
                return NotFound();
            }
            if (IsCurrentUser((int)userId))
            {
                _context.Posts.Remove(post);
                _context.SaveChanges();

                Log.Information($"A [{post.Id}]post has been deleted by user: [{user.Id}]{user.UserName}");

                return RedirectToAction("Feed", new { id = post.Id, postDeleted = true });
            }
            else return BadRequest();       
        }

        // This action facilitates the upvoting of a post.
        // It first finds the specific post based on the post ID provided.
        // The post's upvote count is then incremented, and the changes are saved to the database.
        // A log entry is generated to record the upvote action.
        // The user is then redirected back to the feed, displaying the new upvote count for the post.
        [HttpPost]
        public IActionResult Upvote(int? postId)
        {
            if (postId is null)
            {
                return NotFound();
            }
            var post = _context.Posts.FirstOrDefault(p => p.Id == postId);

            if (post is null)
            {
                return NotFound();
            }

            // if(post.Upvotes > 1)
            //{
            //    return RedirectToAction("Feed");
            //}
            if (!Request.Cookies.ContainsKey("CurrentUser"))
            {
                return Redirect("/users/login");
            }
            string id = Request.Cookies["CurrentUser"].ToString();

            if (int.TryParse(id, out int parseId))
            {
                var user = GetUserWithPosts(parseId);
                post.Upvote(user);
            }
                

            _context.SaveChanges();

            Log.Information($"A [{post.Id}]post has been upvoted");

            var newUpvotes = post.Upvotes;

            return RedirectToAction("Feed", new {V = post.Upvotes = newUpvotes});
            
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