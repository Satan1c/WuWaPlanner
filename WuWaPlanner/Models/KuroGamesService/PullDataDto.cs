using Newtonsoft.Json;

namespace WuWaPlanner.Models.KuroGamesService;

public class PullDataDto
{
	[JsonProperty("data")]
	public PullData[] Data = [];
}
