using System.Text;
using CacheManager.Core;
using Google.Apis.Auth.AspNetCore3;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using WuWaPlanner.Models;
using File = Google.Apis.Drive.v3.Data.File;

namespace WuWaPlanner.Controllers;

[Route("settings")]
public class SettingsController(IGoogleAuthProvider authProvider, ICacheManager<SaveData> cacheManager) : Controller
{
	private static readonly JsonSerializerSettings s_jsonSettings = new()
	{
		Formatting       = Formatting.None,
		ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() }
	};

	private readonly IGoogleAuthProvider     m_authProvider = authProvider;
	private readonly ICacheManager<SaveData> m_cacheManager = cacheManager;

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
		var google = new DriveService(
									  new BaseClientService.Initializer
									  {
										  HttpClientInitializer = await m_authProvider.GetCredentialAsync().ConfigureAwait(false),
										  ApplicationName       = "AftertaleAU"
									  }
									 );

		var files = google.Files.List();
		files.Spaces   = "appDataFolder";
		files.PageSize = 1;

		var appDataFolder = await files.ExecuteAsync().ConfigureAwait(false);
		var file          = appDataFolder.Files.FirstOrDefault();

		if (file is null)
		{
			using var stream = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new Dictionary<string, string>())));

			await google.Files.Create(new File { Parents = ["appDataFolder"], Name = "WuWa_User.json" }, stream, "application/json")
						.UploadAsync()
						.ConfigureAwait(false);

			appDataFolder = await files.ExecuteAsync().ConfigureAwait(false);
			file          = appDataFolder.Files.First();
		}

		using var download = new MemoryStream();
		await google.Files.Get(file.Id).DownloadAsync(download);
		var encoded = Encoding.UTF8.GetString(download.GetBuffer());
		var data    = JsonConvert.DeserializeObject<SaveData>(encoded, s_jsonSettings)!;
		m_cacheManager.AddOrUpdate(data.Tokens, data, _ => data);
		HttpContext.Response.Cookies.Append("tokens", data.Tokens, new CookieOptions { MaxAge = TimeSpan.FromDays(45) });
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
