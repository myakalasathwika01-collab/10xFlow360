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
        private readonly SAPRestService sapService =
            new SAPRestService();

        // =========================================================
        // GET: Account/Login
        // =========================================================
        [AllowAnonymous]
        public ActionResult Login()
        {
            return View();
        }

        // =========================================================
        // POST: Account/Login
        // =========================================================
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            bool loginSuccess =
                sapService.ValidateUserCredentials(model);

            if (!loginSuccess)
            {
                string errorMessage =
                    Session["SAP_LOGIN_ERROR"]?.ToString();

                if (string.IsNullOrWhiteSpace(errorMessage))
                {
                    errorMessage =
                        "Invalid SAP username or password.";
                }

                ModelState.AddModelError(
                    "",
                    errorMessage);

                return View(model);
            }

            // Flow360 authentication
            FormsAuthentication.SetAuthCookie(
                model.UserName,
                model.RememberMe);

            return RedirectToAction(
                "Index",
                "Dashboard");
        }

        // =========================================================
        // LOGOUT
        // =========================================================
        [Authorize]
        public ActionResult Logout()
        {
            sapService.Logout();

            FormsAuthentication.SignOut();

            return RedirectToAction(
                "Login",
                "Account");
        }
    }
}