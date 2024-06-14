using Newtonsoft.Json;
using WuWaPlanner.Extensions;

namespace WuWaPlanner.Models.KuroGamesService;

public struct BannerData()
{
	[JsonProperty("pity4")]
	public byte EpicPity;

	[JsonProperty("pity5")]
	public byte LegendaryPity;

	[JsonProperty("pulls")]
	public PullData[] Pulls = [];

	[JsonProperty("total")]
	public long Total;

	public BannerData(PullData[] pullsRaw) : this()
	{
		var pullsList = pullsRaw.Reverse().CalculatePity().Reverse().ToList();
		Pulls = pullsList.ToArray();
		Total = Pulls.LongLength;
		var legendaryIndex = pullsList.FindIndex(data => data.Rarity == 5);
		var epicIndex      = pullsList.FindIndex(data => data.Rarity is 4 or 5);
		LegendaryPity = (byte)Math.Max(0, legendaryIndex == -1 ? Total : legendaryIndex);
		EpicPity      = (byte)Math.Max(0, epicIndex == -1 || Total < 11 ? Total : epicIndex);
	}
}
