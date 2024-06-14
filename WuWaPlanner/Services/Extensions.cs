using System.Collections.Frozen;
using WuWaPlanner.Models.CsvManager;

namespace WuWaPlanner.Services;

public static class Extensions
{
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
