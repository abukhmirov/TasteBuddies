﻿using Microsoft.AspNetCore.Mvc;
using TasteBuddies.DataAccess;
using TasteBuddies.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;

namespace TasteBuddies.Controllers
{
    public class PostsController : Controller
    {
        private readonly TasteBuddiesContext _context;

        public PostsController(TasteBuddiesContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var posts = _context.Posts.ToList();
            return View(posts);
        }

        public IActionResult Feed()
        {
            return View();
        }

        public IActionResult Create(Post post)
        {

            _context.Posts.Add(post);
            _context.SaveChanges();



            return RedirectToAction("Feed", new { id = post.Id });

        }

    }
}