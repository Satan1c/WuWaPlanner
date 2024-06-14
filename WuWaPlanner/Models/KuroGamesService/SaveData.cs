using Newtonsoft.Json;

namespace WuWaPlanner.Models.KuroGamesService;

public class SaveData
{
	[JsonProperty("data")]
	public IReadOnlyDictionary<BannerType, BannerData> Data = new Dictionary<BannerType, BannerData>();

	[JsonProperty("tokens")]
	public string Tokens = string.Empty;
}
