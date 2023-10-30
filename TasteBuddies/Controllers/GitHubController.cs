using Microsoft.AspNetCore.Mvc;
using TasteBuddies.Interfaces;
using TasteBuddies.Models;

namespace TasteBuddies.Controllers
{
    public class GitHubController : Controller
    {
        private readonly IGitHubService _gitHubServices;

        public GitHubController(IGitHubService gitHubServices)
        {
            _gitHubServices = gitHubServices;
        }


        public IActionResult Index()
        {
            return View();
        }

        public IActionResult CreateIssue()
        {
            return View();
        }

        [HttpPost("createissue")]
        public async Task<IActionResult> CreateIssueAsync(GitHubIssue issue)
        {
            return Ok("You done good!");
        }

    }
}
