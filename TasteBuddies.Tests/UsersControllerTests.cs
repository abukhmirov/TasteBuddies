using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Http.Json;
using TasteBuddies.DataAccess;
using TasteBuddies.Models;
using System.Security.Cryptography;
using System.Text;
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
        public async Task CheckPassword_ValidPassword_ReturnsRedirectToUserDetails()
        {
            // Arrange

            var context = GetDbContext();
            var client = _factory.CreateClient();

            var user1 = new User { Id = 1, Name = "John", UserName = "Doe", Password = "1234" };

            context.Users.Add(user1);
            context.SaveChanges();


            var formData = new Dictionary<string, string>
        {
            { "password", "1234" }, 
        };

            var content = new FormUrlEncodedContent(formData);
            var response = await client.PostAsync("/users/1/login", content);


            // Assert
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Equal("/users/1", response.Headers.Location?.OriginalString);
        }

        [Fact]
        public async Task CheckPassword_InvalidPassword_ReturnsRedirectToUsers()
        {
            // Arrange

            var context = GetDbContext();
            var client = _factory.CreateClient();

            var user1 = new User {Id = 1, Name = "John", UserName = "Doe", Password = "1234" };

            context.Users.Add(user1);
            context.SaveChanges();


            var formData = new Dictionary<string, string>
        {
            { "password", "incorrect_password" }, // Use the incorrect password
        };

            var content = new FormUrlEncodedContent(formData);
            var response = await client.PostAsync("/users/1/login", content);


            // Assert
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Equal("/users", response.Headers.Location?.OriginalString);
        }

        

        [Fact]
        public async Task New_ReturnsViewWithForm()
        {
            var context = GetDbContext();
            var client = _factory.CreateClient();

            var response = await client.GetAsync($"/Users/Signup");
            var html = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains($"<form method=\"post\" action=\"/Users/\">", html);
        }

        [Fact]
        public async void Create_AddsUserToDatabase()
        {
            var context = GetDbContext();
            var client = _factory.CreateClient();

            context.Users.Add(new User { Name = "Skylar", UserName = "ssandler", Password = "123" });
            context.SaveChanges();

            var formData = new Dictionary<string, string>
            {
                { "Name", "Skylar" },
                { "UserName", "ssandler" },
                { "Password", "123" }
            };

            var response = await client.PostAsync("/Users/", new FormUrlEncodedContent(formData));
            var html = await response.Content.ReadAsStringAsync();

            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("Skylar", html);

            var savedUser = context.Users.FirstOrDefault(
               u => u.Name == "Skylar"
                );
            Assert.NotNull(savedUser);
            Assert.Equal("Skylar", savedUser.Name);
        }

        private string EncodePassword(string password)
        {
            HashAlgorithm sha = SHA256.Create();

            byte[] passwordBytes = Encoding.ASCII.GetBytes(password);
            byte[] passwordDigested = sha.ComputeHash(passwordBytes);
            StringBuilder passwordBuilder = new StringBuilder();
            foreach (byte b in passwordDigested)
            {
                passwordBuilder.Append(b.ToString("x2"));
            }
            return passwordBuilder.ToString();
        }
    }
}
