using System.Collections.Frozen;
using WuWaPlanner.Models.CsvManager;
using WuWaPlanner.Models.KuroGamesService;

namespace WuWaPlanner.Extensions;

public static class ModelsExtensions
{
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

	public static string BannerTypeToString(this BannerType bannerType)
	{
		return bannerType switch
		{
			BannerType.EventCharacter       => nameof(BannerType.EventCharacter),
			BannerType.EventWeapon          => nameof(BannerType.EventWeapon),
			BannerType.StandardCharacter    => nameof(BannerType.StandardCharacter),
			BannerType.StandardWeapon       => nameof(BannerType.StandardWeapon),
			BannerType.Beginner             => nameof(BannerType.Beginner),
			BannerType.BeginnerSelector     => nameof(BannerType.BeginnerSelector),
			BannerType.BeginnerGiftSelector => nameof(BannerType.BeginnerGiftSelector),
			_                               => "null"
		};
	}

	public static string LangsToString(this Langs lang)
		=> lang switch
		{
			Langs.Ru => nameof(Langs.Ru),
			Langs.En => nameof(Langs.En),
			_        => string.Empty
		};

	public static string[] GetForLocale(this FrozenDictionary<string, LangRow> data, Langs lang)
	{
		var list = new RefList<string>(data.Count);

		foreach (var (_, v) in data)
		{
			list.Add(
					 lang switch
					 {
						 Langs.En => v.En,
						 _        => string.Empty
					 }
					);
		}

		return list.ToArray();
	}
}
