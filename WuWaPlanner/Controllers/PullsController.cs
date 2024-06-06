using System.Net.Http.Headers;
using System.Text;
using Google.Apis.Auth.AspNetCore3;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using WuWaPlanner.Extensions;
using WuWaPlanner.Models;
using File = Google.Apis.Drive.v3.Data.File;

namespace WuWaPlanner.Controllers;

[Route("pulls")]
[Authorize]
public class PullsController(IGoogleAuthProvider authProvider, IHttpClientFactory httpClientFactory) : Controller
{
	public static readonly BannerType[] BannerTypes =
	[
		BannerType.EventCharacter, BannerType.EventWeapon, BannerType.StandardCharacter, BannerType.StandardWeapon,
		BannerType.Beginner, BannerType.BeginnerSelector, BannerType.BeginnerGiftSelector
	];

	private static readonly Dictionary<BannerType, BannerData> s_emptyDictionary   = new();
	private readonly        IGoogleAuthProvider                m_authProvider      = authProvider;
	private readonly        IHttpClientFactory                 m_httpClientFactory = httpClientFactory;

	[Route("")]
	[AllowAnonymous]
	public async ValueTask<IActionResult> Pulls()
	{
		var existed = HttpContext.Session.GetString(nameof(PullDataDto));

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

				existed = Encoding.UTF8.GetString(data.GetBuffer());
				HttpContext.Session.SetString(nameof(PullDataDto), existed);
			}
		}

		var result = existed is null ? s_emptyDictionary : JsonConvert.DeserializeObject<Dictionary<BannerType, BannerData>>(existed)!;

		return View(new PullsViewModel { Data = result });
	}

	[HttpGet("import")]
	[AllowAnonymous]
	public IActionResult PullsImport() => View();

	[HttpPost("import")]
	[AutoValidateAntiforgeryToken]
	[AllowAnonymous]
	public async ValueTask<IActionResult> PullsImport([FromForm] PullsDataForm dataForm)
	{
		if (!ModelState.IsValid) return View();

		HttpContext.Session.SetString("tokens", dataForm.Tokens);
		var data = JsonConvert.SerializeObject(await GrabData().ConfigureAwait(false));
		HttpContext.Session.SetString(nameof(PullDataDto), data);

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
			using var stream = new MemoryStream(Encoding.UTF8.GetBytes(data));
			await service.Files.Update(file, existed.Id, stream, "application/json").UploadAsync();
		}

		return RedirectToAction("Pulls");
	}

	private async ValueTask<Dictionary<BannerType, BannerData>> GrabData()
	{
		var tasks = new Dictionary<BannerType, Task<string>>();

		foreach (var i in BannerTypes) { tasks[i] = DoRequest(i); }

		var vals = tasks.Values.ToArray();
		await Task.WhenAll(vals);

		var pairs = tasks.Select(
								 x =>
								 {
									 var pullsRaw = JsonConvert.DeserializeObject<PullDataDto>(x.Value.Result)!.Data;
									 var result   = new BannerData(pullsRaw);
									 return new KeyValuePair<BannerType, BannerData>(x.Key, result);
								 }
								);

		return new Dictionary<BannerType, BannerData>(pairs);
	}

	private async Task<string> DoRequest(BannerType bannerType)
	{
		var tokensRaw = HttpContext.Session.GetString("tokens");

		if (tokensRaw is null or "") return string.Empty;

		var tokens   = tokensRaw.Split(',');
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
										  new StringContent(JsonConvert.SerializeObject(request), Encoding.ASCII, "application/json")
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
