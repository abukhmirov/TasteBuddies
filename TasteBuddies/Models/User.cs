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
<<<<<<< HEAD
       
=======
        public List<Group>? Groups { get; set; }
>>>>>>> fdedda190b44264a10fff3a2f48ca5321f005d37

        public User()
        {
            
        }

        public User(string name, string userName, string password)
        {
            Name = name;
            UserName = userName;
            Password = password;
        }

        public string GetDigestedPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("Password cannot be null or empty.");
            }

            HashAlgorithm sha = SHA256.Create();

            string PasswordInput = password;

            byte[] firstInputBytes = Encoding.ASCII.GetBytes(PasswordInput);
            byte[] firstInputDigested = sha.ComputeHash(firstInputBytes);

            StringBuilder firstInputBuilder = new StringBuilder();
            foreach (byte b in firstInputDigested)
            {
                Console.Write(b + ", ");
                firstInputBuilder.Append(b.ToString("x2"));
            }

            return firstInputBuilder.ToString();
        }


        public bool VerifyPassword(string password)
        {
            string inputHash = GetDigestedPassword(password).ToString();
            return inputHash == Password;
        }
    }
}
