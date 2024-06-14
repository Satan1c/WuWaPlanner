using Newtonsoft.Json;
using WuWaPlanner.Models.KuroGamesService;

namespace WuWaPlanner.Models;

public class SaveData
{
	[JsonProperty("data")]
	public IReadOnlyDictionary<BannerTypeEnum, BannerData> Data = new Dictionary<BannerTypeEnum, BannerData>();

	[JsonProperty("tokens")]
	public string Tokens = string.Empty;
}
