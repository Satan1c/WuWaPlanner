using System.Diagnostics;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using WuWaPlanner.Models.View;

namespace WuWaPlanner.Controllers;

[Route("/")]
public class HomeController : Controller
{
	[Route("/")]
	public IActionResult Home() => View();

	[HttpPost("/signin-oidc")]
	public IActionResult Signin() => RedirectToAction("GoogleLogin", "Settings");

	[HttpPost]
	public IActionResult ChangeLanguage(string culture, string returnUrl)
	{
		Response.Cookies.Append(
								CookieRequestCultureProvider.DefaultCookieName,
								CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
								new CookieOptions { MaxAge = TimeSpan.FromDays(365) }
							   );

		return LocalRedirect(returnUrl);
	}

	[Route("/error")]
	[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
	public IActionResult Error() => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
