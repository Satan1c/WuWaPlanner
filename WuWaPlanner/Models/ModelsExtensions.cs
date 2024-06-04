namespace WuWaPlanner.Models;

public static class ModelsExtensions
{
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
}
