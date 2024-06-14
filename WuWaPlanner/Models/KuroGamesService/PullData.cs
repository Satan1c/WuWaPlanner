using Newtonsoft.Json;

namespace WuWaPlanner.Models.KuroGamesService;

public struct PullData
{
	[JsonProperty("qualityLevel")]
	public byte Rarity;

	[JsonProperty("resourceId")]
	public string Id;

	[JsonProperty("time")]
	public DateTime Time;

	[JsonProperty("resourceType")]
	public DropTypeEnum DropType;

	[JsonProperty("pity")]
	public byte Pity;
}
