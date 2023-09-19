using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using TasteBuddies.DataAccess;
using TasteBuddies.Models;

namespace TasteBuddies.Tests
{
    [Collection("Controller Tests")]
    public class UsersControllerTests : IClassFixture<WebApplicationFactory<Program>>   
    {
        private readonly WebApplicationFactory<Program> _factory;

        public UsersControllerTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        private TasteBuddiesContext GetDbContext()
        {
            var optionsBuilder = new DbContextOptionsBuilder<TasteBuddiesContext>();
            optionsBuilder.UseInMemoryDatabase("TestDatabase");

            var context = new TasteBuddiesContext(optionsBuilder.Options);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            return context;
        }

        [Fact]
        public async Task Test1()
        {
            var client = _factory.CreateClient();
            var context = GetDbContext();
        }
    }
}