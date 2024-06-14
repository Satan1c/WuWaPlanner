using System.Collections.Frozen;

namespace WuWaPlanner.Models.CsvManager;

public readonly struct Category<TRow> where TRow : struct
{
	public readonly FrozenDictionary<string, FrozenDictionary<string, TRow>> Data = null!;

	internal Category(Dictionary<string, Dictionary<string, TRow>> from)
	{
		var list = new RefList<KeyValuePair<string, FrozenDictionary<string, TRow>>>(from.Count);

		foreach (var (k, v) in from) { list.Add(new KeyValuePair<string, FrozenDictionary<string, TRow>>(k, v.ToFrozenDictionary())); }

		Data = list.ToArray().ToFrozenDictionary();
	}
}
