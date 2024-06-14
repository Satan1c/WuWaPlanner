using Newtonsoft.Json;
using WuWaPlanner.Models.KuroGamesService;

namespace WuWaPlanner.Models;

public class SaveData
{
	[JsonProperty("data")]
	public IReadOnlyDictionary<BannerType, BannerData> Data = new Dictionary<BannerType, BannerData>();

	[JsonProperty("tokens")]
	public string Tokens = string.Empty;
}
