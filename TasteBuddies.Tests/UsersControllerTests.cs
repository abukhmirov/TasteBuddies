using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Json;
using TasteBuddies.DataAccess;
using TasteBuddies.Models;
using Xunit;

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
        public async Task LoginLogsInUser()
        {
            var client = _factory.CreateClient();
            var context = GetDbContext();

            var user1 = new User { Name = "John", UserName = "Doe", Password = "1234" };

            context.Users.Add(user1);
            context.SaveChanges();

            var loginData = new
            {
                password = user1.Password,
                id = user1.Id
            };

            Assert.NotNull(loginData.password);
            

            var response = await client.PostAsJsonAsync("/users/1/login", loginData);
            var html = await response.Content.ReadAsStringAsync();

            response.EnsureSuccessStatusCode(); 
            
            

        }
    }
}