using System.Net.Http.Headers;
using System.Text;
using Google.Apis.Auth.AspNetCore3;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using WuWaPlanner.Models;
using File = Google.Apis.Drive.v3.Data.File;

namespace WuWaPlanner.Controllers;

[Route("pulls")]
public class PullsController(IHttpClientFactory httpClientFactory, IGoogleAuthProvider authProvider) : Controller
{
	public static readonly BannerType[] BannerTypes =
	[
		BannerType.EventCharacter, BannerType.EventWeapon, BannerType.StandardCharacter, BannerType.StandardWeapon,
		BannerType.Beginner, BannerType.BeginnerSelector, BannerType.BeginnerGiftSelector
	];

	private static readonly Dictionary<BannerType, PullData[]> s_emptyDictionary   = new();
	private readonly        IGoogleAuthProvider                m_authProvider      = authProvider;
	private readonly        IHttpClientFactory                 m_httpClientFactory = httpClientFactory;

	[Route("")]
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

		var result = existed is null ? s_emptyDictionary : JsonConvert.DeserializeObject<Dictionary<BannerType, PullData[]>>(existed)!;

		return View(new PullsViewModel { Data = result });
	}

	[HttpGet("import")]
	public IActionResult PullsImport() => View();

	[HttpPost("import")]
	[AutoValidateAntiforgeryToken]
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

	private async ValueTask<Dictionary<BannerType, PullData[]>> GrabData()
	{
		var tasks = new Dictionary<BannerType, Task<string>>();

		foreach (var i in BannerTypes) { tasks[i] = DoRequest(i); }

		var vals = tasks.Values.ToArray();
		await Task.WhenAll(vals);

		var select = tasks.Select(
								  x => new KeyValuePair<BannerType, PullData[]>(
																				x.Key,
																				JsonConvert.DeserializeObject<PullDataDto>(x.Value.Result)!
																						   .Data.Reverse()
																						   .ToArray()
																			   )
								 );

		return new Dictionary<BannerType, PullData[]>(select);
	}

	private async Task<string> DoRequest(BannerType bannerType)
	{
		var tokensRaw = HttpContext.Session.GetString("tokens");

		if (tokensRaw is null || tokensRaw == string.Empty) return string.Empty;

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
	[JsonIgnore]
	public DateTime CreatedAt;

	[JsonProperty("data")]
	public PullData[] Data = [];
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
