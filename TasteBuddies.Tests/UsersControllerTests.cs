using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Http.Json;
using TasteBuddies.DataAccess;
using TasteBuddies.Models;
using System.Security.Cryptography;
using System.Text;
using Xunit;
using Microsoft.Net.Http.Headers;
using NuGet.ContentModel;

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

            var user1 = new User { Id = 1, Name = "John", UserName = "Doe", Password = "1234" };

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
        public async Task Signup_ReturnsViewWithForm()
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

        //Needs work
        [Fact]
        public async Task Profile_ReturnsViewWithLoggedinUserDetails()
        {
            var context = GetDbContext();
            context.Users.Add(new User { Name = "Skylar", UserName = "ssandler", Password = "123" });
            context.Users.Add(new User { Name = "Scott", UserName = "sganz", Password = "456" });
            context.SaveChanges();

            var client = _factory.CreateClient();
            var response = await client.GetAsync("/Users/1");
            var html = await response.Content.ReadAsStringAsync();

            response.EnsureSuccessStatusCode();
            Assert.Contains("Skylar", html);
            Assert.Contains("ssandler", html);
            Assert.Contains("Reset Skylar's Password", html);
            Assert.DoesNotContain("sganz", html);
        }

        //Needs work
        [Fact]
        public async Task Edit_ReturnsViewWithFormPrePopulated()
        {
            var context = GetDbContext();
            var client = _factory.CreateClient();

            User user = new User { Name = "Skylar", UserName = "ssandler", Password = "123" };
            context.Users.Add(user);
            context.SaveChanges();

            var response = await client.GetAsync($"/Users/{user.Id}/Edit");
            var html = await response.Content.ReadAsStringAsync();

            Assert.Contains("Edit User", html);
            Assert.Contains(user.Name, html);
            Assert.Contains(user.UserName, html);
        }

        //Needs work
        [Fact]
        public async Task Update_SavesChangesToUser()
        {
            // Arrange
            var context = GetDbContext();
            var client = _factory.CreateClient();

            User user = new User { Name = "Skylar", UserName = "ssandler", Password = "123" };
            context.Users.Add(user);
            context.SaveChanges();

            var formData = new Dictionary<string, string>
            {
                { "Name", "Skylar Sandler" }
            };

            // Act
            var response = await client.PostAsync(
                $"/User/Profile",
                new FormUrlEncodedContent(formData)
            );
            var html = await response.Content.ReadAsStringAsync();

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Contains("Skylar Sandler", html);
        }

        //Needs work
        [Fact]
        public async Task ResetPassword_ReturnsViewwithForm()
        {
            var context = GetDbContext();
            var client = _factory.CreateClient();

            User user = new User { Name = "Skylar", UserName = "ssandler", Password = "123" };
            context.Users.Add(user);
            context.SaveChanges();

            var response = await client.GetAsync($"/Users/{user.Id}/ResetPassword");
            var html = await response.Content.ReadAsStringAsync();

            Assert.Contains("Reset Password", html);
        }

       
    }
}

     

