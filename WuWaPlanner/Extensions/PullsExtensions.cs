using WuWaPlanner.Models;

namespace WuWaPlanner.Extensions;

public static class PullsExtensions
{
	public static IEnumerable<PullData> CalculatePity(this IEnumerable<PullData> pulls)
	{
		byte       legendaryPity = 1;
		byte       epicPity      = 1;
		const byte commonPity    = 1;

		foreach (var i in pulls)
		{
			var pull = i;

			switch (i.Rarity)
			{
				case 5:
					pull.Pity     = legendaryPity;
					legendaryPity = 1;
					epicPity++;
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
