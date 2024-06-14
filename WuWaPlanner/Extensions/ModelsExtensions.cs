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

	public static string BannerTypeToString(this BannerTypeEnum bannerTypeEnum)
	{
		return bannerTypeEnum switch
		{
			BannerTypeEnum.EventCharacter       => nameof(BannerTypeEnum.EventCharacter),
			BannerTypeEnum.EventWeapon          => nameof(BannerTypeEnum.EventWeapon),
			BannerTypeEnum.StandardCharacter    => nameof(BannerTypeEnum.StandardCharacter),
			BannerTypeEnum.StandardWeapon       => nameof(BannerTypeEnum.StandardWeapon),
			BannerTypeEnum.Beginner             => nameof(BannerTypeEnum.Beginner),
			BannerTypeEnum.BeginnerSelector     => nameof(BannerTypeEnum.BeginnerSelector),
			BannerTypeEnum.BeginnerGiftSelector => nameof(BannerTypeEnum.BeginnerGiftSelector),
			_                                   => "null"
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
