

namespace TasteBuddies.Models
{
    public class Group
    {
        public int Id { get; set; }
        public string Name { get; set; }
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
