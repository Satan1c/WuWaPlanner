using System.Diagnostics;
using Google.Apis.Auth.AspNetCore3;
using Microsoft.AspNetCore.Mvc;
using WuWaPlanner.Models;

namespace WuWaPlanner.Controllers;

public class HomeController(IGoogleAuthProvider authProvider) : Controller
{
	private readonly IGoogleAuthProvider m_authProvider = authProvider;

	[Route("")]
	public IActionResult Home() => View();

	public IActionResult Pulls() => RedirectToAction("Pulls", "Pulls");

	[Route("/signin-oidc")]
	[HttpGet("/signin-oidc")]
	[HttpPost("/signin-oidc")]
	public IActionResult Login() => RedirectToAction("Settings", "Settings");

	public IActionResult Settings() => RedirectToAction("Settings", "Settings");

	[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
	public IActionResult Error() => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
