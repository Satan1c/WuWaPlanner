using CacheManager.Core;
using Microsoft.AspNetCore.Mvc;
using WuWaPlanner.Extensions;
using WuWaPlanner.Models;
using WuWaPlanner.Models.CsvManager;
using WuWaPlanner.Models.KuroGamesService;
using WuWaPlanner.Models.View;
using WuWaPlanner.Services;

namespace WuWaPlanner.Controllers;

[Route("pulls")]
public class PullsController(
		ICacheManager<SaveData>      saveDataCacheManager,
		ICacheManager<PullsDataForm> pullsDataFormCacheManager,
		CsvManager<LangRow>          csvManager,
		KuroGamesService             kuroGamesService,
		GoogleDriveService           googleDriveService
) : Controller
{
	public static readonly BannerType[] BannerTypes =
	[
		BannerType.EventCharacter, BannerType.EventWeapon, BannerType.StandardCharacter, BannerType.StandardWeapon,
		BannerType.Beginner, BannerType.BeginnerSelector, BannerType.BeginnerGiftSelector
	];

	private static readonly PullsDataForm                s_emptyPullsDataForm        = new() { Tokens = string.Empty };
	public static readonly  SaveData                     EmptyData                   = new();
	private readonly        CsvManager<LangRow>          m_csvManager                = csvManager;
	private readonly        GoogleDriveService           m_googleDrive               = googleDriveService;
	private readonly        KuroGamesService             m_kuroGames                 = kuroGamesService;
	private readonly        ICacheManager<PullsDataForm> m_pullsDataFormCacheManager = pullsDataFormCacheManager;

	private readonly ICacheManager<SaveData> m_saveDataCacheManager = saveDataCacheManager;

	[Route("")]
	[ResponseCache(Duration = 3888000, Location = ResponseCacheLocation.Client)]
	public async ValueTask<IActionResult> Pulls()
	{
		var tokens  = HttpContext.ReadTokens();
		var existed = tokens is null ? null : m_saveDataCacheManager.Get(tokens);

		if (existed is not null) return View(new PullsViewModel { Data = existed, CsvManager = m_csvManager });

		existed = User.Identity?.IsAuthenticated ?? false ? await m_googleDrive.ReadDataOrDefault() :
				  tokens is not null                      ? await m_kuroGames.GrabData(tokens).ConfigureAwait(false) : null;

		if (existed is null) return View(new PullsViewModel { Data = existed ?? EmptyData, CsvManager = m_csvManager });

		m_saveDataCacheManager.AddOrUpdate(tokens, existed, _ => existed);
		HttpContext.SaveTokens(tokens!);

		return View(new PullsViewModel { Data = existed, CsvManager = m_csvManager });
	}

	[HttpGet("import")]
	public IActionResult PullsImport()
	{
		var tokens = User.Identity?.IsAuthenticated ?? false ? HttpContext.ReadTokens() : null;

		return View(
					tokens is null
							? s_emptyPullsDataForm
							: m_pullsDataFormCacheManager.GetOrAdd(tokens, t => new PullsDataForm { Tokens = t })
				   );
	}

	[HttpPost("import")]
	public async ValueTask<IActionResult> PullsImport([FromForm] PullsDataForm dataForm)
	{
		if (!ModelState.IsValid) return View();

		var data = await m_kuroGames.GrabData(dataForm.Tokens).ConfigureAwait(false);
		m_saveDataCacheManager.AddOrUpdate(dataForm.Tokens, data, _ => data);
		HttpContext.SaveTokens(dataForm.Tokens);

		if (User.Identity?.IsAuthenticated ?? false) await m_googleDrive.WriteData(data);

		return RedirectToAction("Pulls");
	}
}
