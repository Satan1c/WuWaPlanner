using Newtonsoft.Json;

namespace WuWaPlanner.Models.KuroGamesService;

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
