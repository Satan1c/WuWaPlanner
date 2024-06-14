using Newtonsoft.Json;

namespace WuWaPlanner.Models.KuroGamesService;

public struct PullsRequest()
{
	[JsonProperty("cardPoolType")]
	public required BannerType BannerType;

	[JsonProperty("languageCode")]
	public string Language = "en";

	[JsonProperty("recordId")]
	public required string Record;

	[JsonProperty("serverId")]
	public required string Server;

	[JsonProperty("playerId")]
	public required string Uid;

	public string Serialize()
		=> $@"{{""cardPoolType"":{((byte)BannerType).ToString()},""languageCode"":""{Language}"",""recordId"":""{Record}"",""serverId"":""{Server}"",""playerId"":""{Uid}""}}";
}
