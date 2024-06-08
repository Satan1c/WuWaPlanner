using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WuWaPlanner.Models;

namespace WuWaPlanner.Controllers;

[Route("/")]
public class HomeController : Controller
{
	[Route("/")]
	[ResponseCache(Duration = 3888000, Location = ResponseCacheLocation.Client)]
	public IActionResult Home() => View();

	[HttpPost("/signin-oidc")]
	public IActionResult Signin() => RedirectToAction("GoogleLogin", "Settings");

	[Route("/error")]
	[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
	public IActionResult Error() => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
