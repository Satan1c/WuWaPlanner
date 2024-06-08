using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Text;
using CacheManager.Core;
using Google.Apis.Auth.AspNetCore3;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using WuWaPlanner.Extensions;
using WuWaPlanner.Models;
using File = Google.Apis.Drive.v3.Data.File;

namespace WuWaPlanner.Controllers;

[Route("pulls")]
public class PullsController(
		IGoogleAuthProvider     authProvider,
		IHttpClientFactory      httpClientFactory,
		ICacheManager<SaveData> cacheManager
) : Controller
{
	private static readonly JsonSerializerSettings s_jsonSettings = new()
	{
		Formatting       = Formatting.None,
		ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() }
	};

	public static readonly BannerType[] BannerTypes =
	[
		BannerType.EventCharacter, BannerType.EventWeapon, BannerType.StandardCharacter, BannerType.StandardWeapon,
		BannerType.Beginner, BannerType.BeginnerSelector, BannerType.BeginnerGiftSelector
	];

	private static readonly SaveData                s_emptyData         = new();
	private readonly        IGoogleAuthProvider     m_authProvider      = authProvider;
	private readonly        ICacheManager<SaveData> m_cacheManager      = cacheManager;
	private readonly        IHttpClientFactory      m_httpClientFactory = httpClientFactory;

	[Route("")]
	[ResponseCache(Duration = 3888000, Location = ResponseCacheLocation.Client)]
	public async ValueTask<IActionResult> Pulls()
	{
		var existed = HttpContext.Request.Cookies.TryGetValue("tokens", out var tokens) ? m_cacheManager.Get(tokens) : null;

		if (existed is null && (User.Identity?.IsAuthenticated ?? false))
		{
			var cred = await m_authProvider.GetCredentialAsync();

			var google = new DriveService(
										  new BaseClientService.Initializer
										  {
											  HttpClientInitializer = cred, ApplicationName = "AftertaleAU"
										  }
										 );

			var files = google.Files.List();
			files.Spaces = "appDataFolder";
			var appDataFolder = await files.ExecuteAsync();

			if (appDataFolder.Files.Count > 0)
			{
				using var data = new MemoryStream();
				await google.Files.Get(appDataFolder.Files.First().Id).DownloadAsync(data);

				var encoded = Encoding.UTF8.GetString(data.GetBuffer());
				existed = JsonConvert.DeserializeObject<SaveData>(encoded, s_jsonSettings)!;
				HttpContext.Response.Cookies.Append("tokens", existed.Tokens);
				m_cacheManager.AddOrUpdate(existed.Tokens, existed, _ => existed);
			}
		}

		return View(new PullsViewModel { Data = existed ?? s_emptyData });
	}

	[HttpGet("import")]
	[ResponseCache(Duration = 604800, Location = ResponseCacheLocation.Client)]
	public async ValueTask<IActionResult> PullsImport()
	{
		var tokens = HttpContext.Request.Cookies.TryGetValue("tokens", out var value) ? value : null;
		return View(new PullsDataForm { Tokens = User.Identity?.IsAuthenticated ?? false ? tokens ?? string.Empty : string.Empty });
	}

	[HttpPost("import")]
	[AutoValidateAntiforgeryToken]
	public async ValueTask<IActionResult> PullsImport([FromForm] PullsDataForm dataForm)
	{
		if (!ModelState.IsValid) return View();

		var data = await GrabData(dataForm.Tokens).ConfigureAwait(false);
		m_cacheManager.AddOrUpdate(dataForm.Tokens, data, _ => data);

		if (User.Identity?.IsAuthenticated ?? false)
		{
			var cred = await m_authProvider.GetCredentialAsync();

			var service = new DriveService(
										   new BaseClientService.Initializer
										   {
											   HttpClientInitializer = cred, ApplicationName = "AftertaleAU"
										   }
										  );

			var files = service.Files.List();
			files.Spaces = "appDataFolder";
			var appDataFolder = await files.ExecuteAsync();
			var existed       = appDataFolder.Files.First();

			var       file   = new File { Name = "WuWa_User.json" };
			using var stream = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data, s_jsonSettings)));
			await service.Files.Update(file, existed.Id, stream, "application/json").UploadAsync();
		}

		HttpContext.Response.Headers.CacheControl = "no-cache, no-store, must-revalidate";
		return RedirectToAction("Pulls");
	}

	private async ValueTask<SaveData> GrabData(string tokens)
	{
		var results = new ConcurrentDictionary<BannerType, BannerData>();

		await Parallel.ForEachAsync(
									BannerTypes, async (bannerType, _) =>
												 {
													 var data = JsonConvert.DeserializeObject<PullDataDto>(
														  await DoRequest(bannerType, tokens).ConfigureAwait(false), s_jsonSettings
														 );

													 results[bannerType] = new BannerData(data!.Data);
												 }
								   );

		return new SaveData { Tokens = tokens, Data = results.AsReadOnly() };
	}

	private async ValueTask<string> DoRequest(BannerType bannerType, string tokensRaw)
	{
		if (tokensRaw is null or "") return string.Empty;

		var tokens   = tokensRaw!.Split(',');
		var userId   = tokens[0];
		var serverId = tokens[1];
		var recordId = tokens[2];

		var client = m_httpClientFactory.CreateClient();
		client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

		var request = new PullsRequest
		{
			Uid        = userId,
			Server     = serverId,
			Record     = recordId,
			BannerType = (byte)bannerType
		};

		var resp = await client.PostAsync(
										  "https://gmserver-api.aki-game2.net/gacha/record/query",
										  new StringContent(
															JsonConvert.SerializeObject(request, s_jsonSettings), Encoding.ASCII,
															"application/json"
														   )
										 )
							   .ConfigureAwait(false);

		resp.EnsureSuccessStatusCode();
		return await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
	}
}

public class PullDataDto
{
	[JsonProperty("data")]
	public PullData[] Data = [];
}

public class SaveData
{
	[JsonProperty("data")]
	public IReadOnlyDictionary<BannerType, BannerData> Data = new Dictionary<BannerType, BannerData>();

	[JsonProperty("tokens")]
	public string Tokens = string.Empty;
}

public class BannerData()
{
	[JsonProperty("pity4")]
	public byte EpicPity;

	[JsonProperty("pity5")]
	public byte LegendaryPity;

	[JsonProperty("pulls")]
	public PullData[] Pulls = [];

	[JsonProperty("total")]
	public long Total;

	public BannerData(PullData[] pullsRaw) : this()
	{
		var pullsList = pullsRaw.Reverse().CalculatePity().Reverse().ToList();
		Pulls = pullsList.ToArray();
		Total = Pulls.LongLength;
		var legendaryIndex = pullsList.FindIndex(data => data.Rarity == 5);
		var epicIndex      = pullsList.FindIndex(data => data.Rarity is 4 or 5);
		LegendaryPity = (byte)Math.Max(0, legendaryIndex == -1 ? Total : legendaryIndex);
		EpicPity      = (byte)Math.Max(0, epicIndex == -1 || Total < 11 ? Total : epicIndex);
	}
}

public class PullsRequest
{
	[JsonProperty("cardPoolType")]
	public required byte BannerType;

	[JsonProperty("languageCode")]
	public string Language = "en";

	[JsonProperty("recordId")]
	public required string Record;

	[JsonProperty("serverId")]
	public required string Server;

	[JsonProperty("playerId")]
	public required string Uid;
}
