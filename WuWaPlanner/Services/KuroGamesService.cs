using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using WuWaPlanner.Controllers;
using WuWaPlanner.Models;
using WuWaPlanner.Models.KuroGamesService;

namespace WuWaPlanner.Services;

public class KuroGamesService(JsonSerializerSettings jsonSettings, IHttpClientFactory httpClientFactory)
{
	private readonly IHttpClientFactory     m_httpClientFactory = httpClientFactory;
	private readonly JsonSerializerSettings m_jsonSettings      = jsonSettings;

	public async ValueTask<SaveData> GrabData(string tokens, CancellationToken cancellationToken = default)
	{
		var results = new ConcurrentDictionary<BannerTypeEnum, BannerData>();

		await Parallel.ForEachAsync(
									PullsController.BannerTypes, cancellationToken, async (bannerType, token) =>
																					{
																						var data = JsonConvert
																								.DeserializeObject<PullsDataDto>(
																									 await DoRequest(
																												  bannerType, tokens, token
																												 )
																											 .ConfigureAwait(false),
																									 m_jsonSettings
																									);

																						results[bannerType]
																								= new BannerData(bannerType, data.Data);
																					}
								   );

		return new SaveData { Tokens = tokens, Data = results };
	}

	public async ValueTask<string> DoRequest(BannerTypeEnum bannerTypeEnum, string tokensRaw, CancellationToken cancellationToken = default)
	{
		if (tokensRaw is null or "") return string.Empty;

		var tokens   = tokensRaw!.Split(',');
		var userId   = tokens[0];
		var serverId = tokens[1];
		var recordId = tokens[2];

		using var client = m_httpClientFactory.CreateClient();
		client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

		var request = new PullsRequest
		{
			Uid        = userId,
			Server     = serverId,
			Record     = recordId,
			BannerType = bannerTypeEnum
		};

		var resp = await client.PostAsync(
										  "https://gmserver-api.aki-game2.net/gacha/record/query",
										  new StringContent(request.Serialize(), Encoding.ASCII, "application/json"), cancellationToken
										 )
							   .ConfigureAwait(false);

		resp.EnsureSuccessStatusCode();
		return await resp.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
	}
}
