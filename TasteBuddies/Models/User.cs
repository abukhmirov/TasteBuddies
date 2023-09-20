using System.Security.Cryptography;
using System.Text;

namespace TasteBuddies.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public List<Post> Posts { get; set; } = new List<Post>();

        public User()
        {
            
        }

        public User(string name, string userName, string password)
        {
            Name = name;
            UserName = userName;
            Password = password;
        }

     
    }
}
