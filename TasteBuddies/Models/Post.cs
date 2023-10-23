using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace TasteBuddies.Models
{
    public class Post
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        [StringLength(50, ErrorMessage = "Title cannot exceed 50 characters")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Description is required.")]
        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Image URL is required.")]
        [Url(ErrorMessage = "Image URL is not valid.")]
        public string ImageURL { get; set; }
        public DateTime CreatedAt { get; set; }

        [ValidateNever]
        public User User { get; set; }
        public int Upvotes { get; set; }


        public void Upvote()
        {
            Upvotes++;
        }
    }
}
