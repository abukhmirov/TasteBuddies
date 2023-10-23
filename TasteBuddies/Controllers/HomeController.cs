using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using TasteBuddies.Models;

namespace TasteBuddies.Controllers
{
    public class HomeController : Controller
    {

        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }




        // This action returns the home page view.
        // If a user has a "CurrentUser" cookie, it redirects them to their user profile.
        // If no such cookie exists, it returns the default home view.
        public IActionResult Index()
        {

            if (Request.Cookies.ContainsKey("CurrentUser"))
            {
                return Redirect("/users/profile");
            }

            else
            {
				return View();
			}

        }



        // This action returns the privacy policy page view.
        public IActionResult Privacy()
        {
            return View();
        }



        // This action returns a not-found page view to use for 404 error. 
        
        public IActionResult NotFound()
        {
            return View();
        }




        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


    }
}