using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TasteBuddies.DataAccess;
using TasteBuddies.Models;

namespace TasteBuddies.Tests
{
	[Collection("Controller Tests")]
	public class PostsControllerTests: IClassFixture<WebApplicationFactory<Program>>
	{
		private readonly WebApplicationFactory<Program> _factory;

		public PostsControllerTests(WebApplicationFactory<Program> factory)
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
		public async Task Feed_ReturnsViewOf5MostRecentPosts()
		{
			var post1 = new Post { Title = "post1", Description = "test1", CreatedAt = DateTime.Now };
			var post2 = new Post { Title = "post2", Description = "test2", CreatedAt = DateTime.Now };
			var post3 = new Post { Title = "post3", Description = "test3", CreatedAt = DateTime.Now };
			var post4 = new Post { Title = "post4", Description = "test4", CreatedAt = DateTime.Now };
			var post5 = new Post { Title = "post5", Description = "test5", CreatedAt = DateTime.Now };
			var post6 = new Post { Title = "post6", Description = "test6", CreatedAt = DateTime.Now };

			User user1 = new User { Name = "name1" , UserName = "user1", Password = "password"};
			User user2 = new User { Name = "name2" , UserName = "user2", Password = "password"};

			user1.Posts.Add(post1);
			user1.Posts.Add(post2);
			user1.Posts.Add(post3);

			user2.Posts.Add(post4);
			user2.Posts.Add(post5);
			user2.Posts.Add(post6);

			var client = _factory.CreateClient();
			var context = GetDbContext();

			context.Users.Add(user1);
			context.Users.Add(user2);
			context.SaveChanges();

			var response = await client.GetAsync("/posts/feed");
			var html = await response.Content.ReadAsStringAsync();

			Assert.Contains("post2", html);
			Assert.Contains("test2", html);
			Assert.Contains("post6", html);
			Assert.Contains("test6", html);
			Assert.DoesNotContain("post1", html);
			Assert.DoesNotContain("test1", html);
		}
	}
}
