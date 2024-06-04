using Newtonsoft.Json;

namespace WuWaPlanner.Models;

public class PullsViewModel
{
	public IReadOnlyDictionary<BannerType, PullData[]> Data = new Dictionary<BannerType, PullData[]>();
}

public struct PullData
{
	[JsonProperty("qualityLevel")]
	public byte Rarity;

	[JsonProperty("name")]
	public string Name;

	[JsonProperty("time")]
	public DateTime Time;

	[JsonProperty("resourceType")]
	public DropTypeEnum DropType;

	[JsonProperty("pity")]
	public byte Pity;

	[JsonProperty("currentPity")]
	public byte CurrentPity;
}

public enum DropTypeEnum : byte
{
	Weapons,
	Resonators
}

public enum BannerType : byte
{
	EventCharacter       = 1,
	EventWeapon          = 2,
	StandardCharacter    = 3,
	StandardWeapon       = 4,
	Beginner             = 5,
	BeginnerSelector     = 6,
	BeginnerGiftSelector = 7
}
