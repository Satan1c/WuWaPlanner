using WuWaPlanner.Models.KuroGamesService;

namespace WuWaPlanner.Extensions;

public static class PullsExtensions
{
	public static IEnumerable<PullData> CalculatePity(this IEnumerable<PullData> pulls)
	{
		ulong      id            = 0;
		byte       legendaryPity = 1;
		byte       epicPity      = 1;
		const byte commonPity    = 1;

		foreach (var i in pulls)
		{
			var pull = i;

			pull.Id = ++id;

			switch (i.Rarity)
			{
				case 5:
					pull.Pity     = legendaryPity;
					legendaryPity = 1;
					epicPity      = 1;
					break;

				case 4:
					pull.Pity = epicPity;
					epicPity  = 1;
					legendaryPity++;
					break;

				default:
					pull.Pity = commonPity;
					legendaryPity++;
					epicPity++;
					break;
			}

			yield return pull;
		}
	}
}
