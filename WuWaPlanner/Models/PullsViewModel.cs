using Newtonsoft.Json;
using WuWaPlanner.Models.CsvManager;
using WuWaPlanner.Models.KuroGamesService;
using WuWaPlanner.Services;

namespace WuWaPlanner.Models;

public class PullsViewModel
{
	public required CsvManager<LangRow> CsvManager = null!;
	public          SaveData            Data       = new();
}

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
