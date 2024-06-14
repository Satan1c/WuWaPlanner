using Microsoft.IdentityModel.Tokens;

namespace WuWaPlanner.Models.CsvManager;

public readonly struct LangRow
{
	public string  Key { get; init; }
	public string  En  { get; init; }
	public string? Ru  { get; init; }

	public static Langs LangParse(string culture) => Enum.TryParse<Langs>(culture, out var lang) ? lang : Langs.En;

	public string GetForLocale(string culture) => GetForLocale(LangParse(culture));

	public string GetForLocale(Langs lang)
	{
		return lang switch
			   {
				   Langs.Ru => Ru.IsNullOrEmpty() ? En : Ru,
				   _        => En
			   }
			   ?? En;
	}
}
