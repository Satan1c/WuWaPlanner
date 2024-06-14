using CacheManager.Core;
using Microsoft.AspNetCore.Mvc;
using WuWaPlanner.Models;
using WuWaPlanner.Models.CsvManager;
using WuWaPlanner.Models.KuroGamesService;
using WuWaPlanner.Services;

namespace WuWaPlanner.Controllers;

[Route("pulls")]
public class PullsController(
		ICacheManager<SaveData> cacheManager,
		CsvManager<LangRow>     csvManager,
		KuroGamesService        kuroGamesService,
		GoogleDriveService      googleDriveService
) : Controller
{
	public static readonly BannerType[] BannerTypes =
	[
		BannerType.EventCharacter, BannerType.EventWeapon, BannerType.StandardCharacter, BannerType.StandardWeapon,
		BannerType.Beginner, BannerType.BeginnerSelector, BannerType.BeginnerGiftSelector
	];

	public static readonly SaveData                EmptyData      = new();
	private readonly       ICacheManager<SaveData> m_cacheManager = cacheManager;
	private readonly       CsvManager<LangRow>     m_csvManager   = csvManager;
	private readonly       GoogleDriveService      m_googleDrive  = googleDriveService;
	private readonly       KuroGamesService        m_kuroGames    = kuroGamesService;

	[Route("")]
	[ResponseCache(Duration = 3888000, Location = ResponseCacheLocation.Client)]
	public async ValueTask<IActionResult> Pulls()
	{
		var existed = HttpContext.Request.Cookies.TryGetValue("tokens", out var tokens) ? m_cacheManager.Get(tokens) : null;

		if (existed is not null) return View(new PullsViewModel { Data = existed, CsvManager = m_csvManager });

		existed = User.Identity?.IsAuthenticated ?? false ? await m_googleDrive.ReadDataOrDefault() :
				  tokens is not null                      ? await m_kuroGames.GrabData(tokens).ConfigureAwait(false) : null;

		if (existed is null) return View(new PullsViewModel { Data = existed ?? EmptyData, CsvManager = m_csvManager });

		m_cacheManager.AddOrUpdate(tokens, existed, _ => existed);
		HttpContext.Response.Cookies.Append("tokens", tokens!, new CookieOptions { MaxAge = TimeSpan.FromDays(45) });

		return View(new PullsViewModel { Data = existed, CsvManager = m_csvManager });
	}

	[HttpGet("import")]
	public IActionResult PullsImport()
	{
		var tokens = HttpContext.Request.Cookies.TryGetValue("tokens", out var value) ? value : null;
		return View(new PullsDataForm { Tokens = User.Identity?.IsAuthenticated ?? false ? tokens ?? string.Empty : string.Empty });
	}

	[HttpPost("import")]
	public async ValueTask<IActionResult> PullsImport([FromForm] PullsDataForm dataForm)
	{
		if (!ModelState.IsValid) return View();

		var data = await m_kuroGames.GrabData(dataForm.Tokens).ConfigureAwait(false);
		m_cacheManager.AddOrUpdate(dataForm.Tokens, data, _ => data);
		HttpContext.Response.Cookies.Append("tokens", dataForm.Tokens);

		if (User.Identity?.IsAuthenticated ?? false) await m_googleDrive.WriteData(data);

		return RedirectToAction("Pulls");
	}
}
