using Newtonsoft.Json;

namespace WuWaPlanner.Models.KuroGamesService;

public struct PullDataDto()
{
	[JsonProperty("data")]
	public PullData[] Data = [];
}
