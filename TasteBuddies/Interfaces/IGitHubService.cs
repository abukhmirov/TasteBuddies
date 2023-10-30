namespace TasteBuddies.Interfaces
{
    public interface IGitHubService
    {
        Task<bool> CreateIssueAsync(string title, string body);
        
            
    }
}
