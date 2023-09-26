using Microsoft.EntityFrameworkCore;
using TasteBuddies.Models;

namespace TasteBuddies.DataAccess
{
    public class TasteBuddiesContext : DbContext
    {
        public DbSet<Post> Posts { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Group> Groups { get; set; }

        public TasteBuddiesContext(DbContextOptions<TasteBuddiesContext> options)
            : base(options) { }
    }
}
