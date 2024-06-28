using Newtonsoft.Json;

namespace WuWaPlanner.Models.KuroGamesService;

public struct PullData
{
	public ulong Id;

	[JsonProperty("qualityLevel")]
	public byte Rarity;

	[JsonProperty("resourceId")]
	public string ItemId;

	[JsonProperty("time")]
	public DateTime Time;

	[JsonProperty("resourceType")]
	public DropTypeEnum DropType;

	[JsonProperty("pity")]
	public byte Pity;
}
