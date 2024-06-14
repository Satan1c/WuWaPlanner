using Newtonsoft.Json;

namespace WuWaPlanner.Models.KuroGamesService;

public struct PullsDataDto()
{
	[JsonProperty("data")]
	public PullData[] Data = [];
}
