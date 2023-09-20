namespace TasteBuddies.Models
{
    public class Post
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        //public IFormFile? Image { get; set; } or something idk
        public DateTime CreatedAt { get; set; }
        public User? User { get; set; }
    }
}
