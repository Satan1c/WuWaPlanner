using WuWaPlanner.Models;

namespace WuWaPlanner.Extensions;

public static class PullsExtensions
{
	public static string Title(this string source)
	{
		if (string.IsNullOrEmpty(source.Trim())) return source;

		var span = source.ToCharArray().AsSpan();

		if (char.IsLower(span[0])) span[0] = char.ToUpper(span[0]);

		return new string(span);
	}

	public static string DropTypeToString(this DropTypeEnum dropType)
	{
		return dropType switch
		{
			DropTypeEnum.Weapons    => nameof(DropTypeEnum.Weapons),
			DropTypeEnum.Resonators => nameof(DropTypeEnum.Resonators),
			_                       => string.Empty
		};
	}

	public static string RarityToString(this byte rarity)
	{
		return rarity switch
		{
			5 => "legendary",
			4 => "epic",
			_ => "common"
		};
	}

	public static string WeaponIdToType(this string id)
	{
		return id.AsSpan()[3..4] switch
		{
			"1" => "broadblade",
			"2" => "sword",
			"3" => "pistols",
			"4" => "gauntlets",
			"5" => "rectifier",
			_   => string.Empty
		};
	}

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
