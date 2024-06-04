using System.Diagnostics;
using Google.Apis.Auth.AspNetCore3;
using Microsoft.AspNetCore.Mvc;
using WuWaPlanner.Models;

namespace WuWaPlanner.Controllers;

[Route("/")]
public class HomeController(IGoogleAuthProvider authProvider) : Controller
{
	private readonly IGoogleAuthProvider m_authProvider = authProvider;

	[Route("/")]
	public IActionResult Home() => View();

	[HttpPost("/signin-oidc")]
	public IActionResult Signin() => RedirectToAction("Settings", "Settings");

	[Route("/error")]
	[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
	public IActionResult Error() => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
