using CacheManager.Core;
using Google.Apis.Auth.AspNetCore3;
using Google.Apis.Drive.v3;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WuWaPlanner.Models;
using WuWaPlanner.Models.KuroGamesService;
using WuWaPlanner.Services;

namespace WuWaPlanner.Controllers;

[Route("settings")]
public class SettingsController(GoogleDriveService googleDriveService, ICacheManager<SaveData> cacheManager) : Controller
{
	private readonly ICacheManager<SaveData> m_cacheManager = cacheManager;
	private readonly GoogleDriveService      m_googleDrive  = googleDriveService;

	[Route("")]
	public IActionResult Settings()
	{
		var user = new SettingsViewModel { IsAuthorized = User.Identity?.IsAuthenticated ?? false };

		return View(user);
	}

	[Route("login")]
	[GoogleScopedAuthorize(DriveService.ScopeConstants.DriveAppdata, DriveService.ScopeConstants.DriveFile)]
	public async ValueTask<IActionResult> GoogleLogin()
	{
		var data = await m_googleDrive.ReadDataOrDefault();

		if (data is not null)
		{
			m_cacheManager.AddOrUpdate(data.Tokens, data, _ => data);
			HttpContext.Response.Cookies.Append("tokens", data.Tokens, new CookieOptions { MaxAge = TimeSpan.FromDays(45) });
		}

		return RedirectToAction("Settings");
	}

	[Route("logout")]
	[Authorize]
	public async ValueTask<IActionResult> Logout()
	{
		HttpContext.Session.Clear();

		if (HttpContext.Request.Cookies.TryGetValue("tokens", out var tokens))
		{
			m_cacheManager.Remove(tokens);
			HttpContext.Response.Cookies.Delete("tokens");
		}

		await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
		return RedirectToAction("Settings");
	}
}
