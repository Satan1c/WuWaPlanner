using System.Text;
using Google.Apis.Auth.AspNetCore3;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using WuWaPlanner.Models;
using File = Google.Apis.Drive.v3.Data.File;

namespace WuWaPlanner.Controllers;

[Route("settings")]
public class SettingsController(IGoogleAuthProvider authProvider) : Controller
{
	private readonly IGoogleAuthProvider m_authProvider = authProvider;

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

		using var data = new MemoryStream();
		await google.Files.Get(file.Id).DownloadAsync(data);
		HttpContext.Session.SetString(nameof(PullDataDto), Encoding.UTF8.GetString(data.GetBuffer()));
		return RedirectToAction("Settings");
	}

	[Route("logout")]
	[Authorize]
	public async ValueTask<IActionResult> Logout()
	{
		HttpContext.Session.Clear();
		await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

		return RedirectToAction("Settings");
	}
}
