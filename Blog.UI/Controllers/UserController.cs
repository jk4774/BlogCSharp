﻿using System.Linq;
using Blog.API.Models;
using Blog.API.Services;
using Blog.UI.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using APIController = Blog.API.Controllers;

namespace Blog.UI.Controllers
{
    [Route("[controller]")]
    [Authorize]
    public class UserController : Controller
    {
        private readonly APIController.UserController _userController;
        private readonly APIController.ArticleController _articleController;
        private BlogContext _blogContext;
        public UserController(BlogContext blogContext, UserService userService)
        {
            _userController = new APIController.UserController(blogContext, userService);
            _articleController = new APIController.ArticleController(blogContext);
            _blogContext = blogContext;
        }

        [HttpGet("{id}", Name = "GetUser")]
        public ActionResult<User> GetById(int id)
        {
            //DEBUG
            if (_blogContext.Users.Count() == 0  && Request.Cookies["access_token"] != null)
            {
                Response.Cookies.Delete("access_token");
                return RedirectToAction("Index", "Home");
            }
            //END DEBUG
            var getArticles = _articleController.GetAll();
            var response = _userController.GetById(id);
            if (response.Value == null)
                return RedirectToAction("Index", "Home");
            return View("~/Views/User/Main.cshtml", new FullUser { User = response.Value, Articles = getArticles.Value });
        }

        [AllowAnonymous]
        [HttpPost("Register")]
        public IActionResult Register([FromForm] User user)
        {
            var response = _userController.Register(user);
            if (response.GetType() != typeof(NoContentResult))
                return NotFound();

            if (User.Identity.IsAuthenticated)
                return RedirectToAction("GetUser", new { id = User.Identity.Name });
            return RedirectToAction("Index", "Home");
        }

        [HttpPost("Logout")]
        public IActionResult Logout()
        {
            Utils.DeleteCookie(Request, Response);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet("Update")]
        public IActionResult UpdateView() 
        {
            return View("~/Views/User/Update.cshtml", new User { Id = int.Parse(User.Identity.Name) });
        }

        [HttpPut("Update/{id}")]
        public IActionResult Update(int id, [FromBody] Password password)
        {
            var response = _userController.UpdatePassword(id, password);
            if (response.GetType() != typeof(NoContentResult))
               return RedirectToAction("Update", "User");
            return RedirectToAction("GetUser", new { id = User.Identity.Name });
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            if (User.Identity.Name != id.ToString())
                return RedirectToAction("GetUser", new { id = User.Identity.Name });

            var response = _userController.Delete(id);
            if (response.GetType() != typeof(NoContentResult))
                return RedirectToAction("GetUser", new { id = User.Identity.Name });

            Utils.DeleteCookie(Request, Response);
            return RedirectToAction("Index", "Home");
        }
    }
}