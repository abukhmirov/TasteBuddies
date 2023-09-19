using Microsoft.AspNetCore.Mvc;

namespace TasteBuddies.Controllers
{
    public class UsersController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
