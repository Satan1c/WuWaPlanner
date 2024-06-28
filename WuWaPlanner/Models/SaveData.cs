using Newtonsoft.Json;
using WuWaPlanner.Models.KuroGamesService;

namespace WuWaPlanner.Models;

public class SaveData(Dictionary<BannerTypeEnum, BannerData>? data = null)
{
	[JsonProperty("data")]
	public IDictionary<BannerTypeEnum, BannerData> Data = data ?? new Dictionary<BannerTypeEnum, BannerData>();

	[JsonProperty("tokens")]
	public string Tokens = string.Empty;
}
