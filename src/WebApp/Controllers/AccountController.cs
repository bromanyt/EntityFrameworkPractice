using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebRetail.Controllers;

public class AccountController : Controller
{
    [HttpGet]
    [AllowAnonymous]
    [Route("Account/")]
    [Route("Account/Index")]
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    [Route("Account/Login/{userName}/{password}")]
    public IActionResult Login(string userName, string password)
    {
        var claims = new List<Claim> { new Claim(ClaimTypes.Name, userName) };
        ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "Cookies");

        HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    [Route("Account/LogOut")]
    public IActionResult LogOut()
    {
        HttpContext.SignOutAsync(
        CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }
}
