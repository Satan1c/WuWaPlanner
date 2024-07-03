using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace WuWaPlanner.Models.KuroGamesService;

public struct PullData : IEquatable<PullData>
{
	public ulong Id;

	[JsonProperty("qualityLevel")]
	public byte Rarity;

	[JsonProperty("resourceId")]
	public string ItemId;

	[JsonProperty("time")]
	public DateTime Time;

	[JsonProperty("resourceType")]
	[JsonConverter(typeof(StringEnumConverter))]
	public DropTypeEnum DropType;

	[JsonProperty("pity")]
	public byte Pity;

	public bool Equals(PullData other) => ItemId == other.ItemId && Time == other.Time && Pity == other.Pity;
}
