using System.Collections.Concurrent;
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
		CacheService        cacheService,
		CsvManager<LangRow> csvManager,
		KuroGamesService    kuroGamesService,
		GoogleDriveService  googleDriveService
) : Controller
{
	public static readonly BannerTypeEnum[] BannerTypes =
	[
		BannerTypeEnum.EventCharacter, BannerTypeEnum.EventWeapon, BannerTypeEnum.StandardCharacter, BannerTypeEnum.StandardWeapon,
		BannerTypeEnum.Beginner, BannerTypeEnum.BeginnerSelector, BannerTypeEnum.BeginnerGiftSelector
	];

	private static readonly PullsDataForm s_emptyPullsDataForm = new() { Tokens = string.Empty };
	public static readonly  SaveData      EmptyData            = new();

	private readonly CsvManager<LangRow>          m_csvManager                = csvManager;
	private readonly GoogleDriveService           m_googleDrive               = googleDriveService;
	private readonly KuroGamesService             m_kuroGames                 = kuroGamesService;
	private readonly ICacheManager<PullsDataForm> m_pullsDataFormCacheManager = cacheService.PullsDataFormCacheManager;
	private readonly ICacheManager<SaveData>      m_saveDataCacheManager      = cacheService.SaveDataCacheManager;

	[Route("")]
	[ResponseCache(Duration = 3888000, Location = ResponseCacheLocation.Client)]
	public async ValueTask<IActionResult> Pulls(CancellationToken cancellationToken)
	{
		var tokens  = HttpContext.ReadTokens();
		var existed = tokens is null ? null : m_saveDataCacheManager.Get(tokens);

		if (existed is not null) return View(new PullsViewModel { Data = existed, CsvManager = m_csvManager });

		existed = User.Identity?.IsAuthenticated ?? false ? await m_googleDrive.ReadDataOrDefault(cancellationToken) :
				  tokens is not null ? await m_kuroGames.GrabData(tokens, cancellationToken).ConfigureAwait(false) : null;

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
	public async ValueTask<IActionResult> PullsImport([FromForm] PullsDataForm dataForm, CancellationToken cancellationToken)
	{
		if (!ModelState.IsValid) return View();

		var isAuth = User.Identity?.IsAuthenticated ?? false;
		Console.WriteLine(isAuth);

		var grabData  = await m_kuroGames.GrabData(dataForm.Tokens, cancellationToken).ConfigureAwait(false);
		var savedData = m_saveDataCacheManager.Get(dataForm.Tokens);

		if (savedData is null && isAuth) savedData = await m_googleDrive.ReadDataOrDefault(cancellationToken);

		Parallel.ForEach(
						 Partitioner.Create(grabData.Data), pair =>
															{
																var result  = new RefList<PullData>();
																var grabbed = pair.Value.Pulls.AsSpan();

																if (savedData is not null)
																{
																	var saved = savedData.Data[pair.Key].Pulls.AsSpan();

																	if (saved.Length > 0)
																	{
																		var same = grabbed.LastIndexOf(saved[0]);

																		if (same == -1) result.AddRange(saved);

																		var pivot = same;

																		while (pivot >= 0)
																		{
																			if (grabbed[pivot].Equals(saved[0])) same = pivot;

																			pivot--;
																		}

																		if (same > 0) result.AddRange(grabbed[..same]);

																		result.AddRange(saved);
																	}
																	else { result.AddRange(grabbed); }
																}
																else { result.AddRange(grabbed); }

																grabData.Data[pair.Key] = new BannerData(pair.Key, result.ToArray());
															}
						);

		m_saveDataCacheManager.AddOrUpdate(dataForm.Tokens, grabData, _ => grabData);
		HttpContext.SaveTokens(dataForm.Tokens);

		if (isAuth) await m_googleDrive.WriteData(grabData, cancellationToken);

		return RedirectToAction("Pulls");
	}
}
