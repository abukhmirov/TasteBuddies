using TasteBuddies.Interfaces;

namespace TasteBuddies.Services
{
    public class GitHubService : IGitHubService
    {
        private readonly HttpClient _httpClient;
        public GitHubService(HttpClient httpClient) 
        {
            
        }
        public async Task<bool> CreateIssueAsync(string title, string body)
        {
            return true;
        }
    }
}
