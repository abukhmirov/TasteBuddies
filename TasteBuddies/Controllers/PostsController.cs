using Microsoft.AspNetCore.Mvc;

namespace TasteBuddies.Controllers
{
    public class PostsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
