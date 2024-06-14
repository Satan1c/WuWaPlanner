using CacheManager.Core;
using Google.Apis.Auth.AspNetCore3;
using Google.Apis.Drive.v3;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WuWaPlanner.Extensions;
using WuWaPlanner.Models;
using WuWaPlanner.Models.View;
using WuWaPlanner.Services;

namespace WuWaPlanner.Controllers;

[Route("settings")]
public class SettingsController(GoogleDriveService googleDriveService, CacheService cacheManager) : Controller
{
	private static readonly SettingsViewModel       s_authorizedModel   = new() { IsAuthorized = true };
	private static readonly SettingsViewModel       s_unAuthorizedModel = new() { IsAuthorized = false };
	private readonly        ICacheManager<SaveData> m_cacheManager      = cacheManager.SaveDataCacheManager;
	private readonly        GoogleDriveService      m_googleDrive       = googleDriveService;

	[Route("")]
	public IActionResult Settings() => View(User.Identity?.IsAuthenticated ?? false ? s_authorizedModel : s_unAuthorizedModel);

	[Route("login")]
	[GoogleScopedAuthorize(DriveService.ScopeConstants.DriveAppdata, DriveService.ScopeConstants.DriveFile)]
	public async ValueTask<IActionResult> GoogleLogin()
	{
		var data = await m_googleDrive.ReadDataOrDefault();

		if (data is not null)
		{
			m_cacheManager.AddOrUpdate(data.Tokens, data, _ => data);
			HttpContext.SaveTokens(data.Tokens);
		}

		return RedirectToAction("Settings");
	}

	[Route("logout")]
	[Authorize]
	public async ValueTask<IActionResult> Logout()
	{
		HttpContext.Session.Clear();

		if (HttpContext.ReadTokens() is { } tokens)
		{
			m_cacheManager.Remove(tokens);
			HttpContext.Response.Cookies.Delete("tokens");
		}

		await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
		return RedirectToAction("Settings");
	}
}
