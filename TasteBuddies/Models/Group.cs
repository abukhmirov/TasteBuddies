

using System.ComponentModel.DataAnnotations;

namespace TasteBuddies.Models
{
    public class Group
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Description is required.")]
        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string Description { get; set; }

        public List<User>? Users { get; set; } = new List<User>();


        public Group()
        {

        }

        public Group(string name, string description)
        {
            Name = name;
            Description = description;

        }
    }
}
