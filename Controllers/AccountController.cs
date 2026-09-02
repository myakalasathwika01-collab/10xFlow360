using _10xFlow360.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace _10xFlow360.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account/Login
        [AllowAnonymous]
        public ActionResult Login()
        {
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Temporary login for initial implementation.
            // We will replace this with the actual company authentication
            // mechanism/database validation later.
            if (model.UserName == "admin" && model.Password == "123")
            {
                FormsAuthentication.SetAuthCookie(
                    model.UserName,
                    model.RememberMe
                );

                return RedirectToAction("Index", "Dashboard");
            }

            ModelState.AddModelError(
                "",
                "Invalid username or password."
            );

            return View(model);
        }

        // GET: Account/Logout
        [Authorize]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();

            return RedirectToAction("Login", "Account");
        }
    }
}