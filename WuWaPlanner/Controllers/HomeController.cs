using System.Diagnostics;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WuWaPlanner.Models;

namespace WuWaPlanner.Controllers;

[Route("/")]
[Authorize]
public class HomeController : Controller
{
	[Route("/")]
	[AllowAnonymous]
	public async ValueTask<IActionResult> Home()
	{
		var auth = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
		Console.WriteLine(auth);
		Console.WriteLine(auth.Succeeded);
		return View();
	}

	[HttpPost("/signin-oidc")]
	[HttpPost("/signin-google")]
	[AllowAnonymous]
	public IActionResult Signin() => RedirectToAction("GoogleLogin", "Settings");

	[Route("/error")]
	[AllowAnonymous]
	[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
	public IActionResult Error() => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
