﻿namespace TasteBuddies.Models
{
    public class Post
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageURL { get; set; }
        public DateTime CreatedAt { get; set; }
        public User User { get; set; }
        public int Upvotes { get; set; }


        public void Upvote()
        {
            Upvotes++;
        }
    }
}
