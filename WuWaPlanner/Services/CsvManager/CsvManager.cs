using System.Collections.Frozen;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using CsvHelper;
using CsvHelper.Configuration;
using WuWaPlanner.Services.CsvManager.Models;

namespace WuWaPlanner.Services.CsvManager;

public class CsvManager<TRow> where TRow : struct
{
	public readonly FrozenDictionary<string, Category<TRow>> Categories;

	public CsvManager(string filesPath)
	{
		var categories = new Dictionary<string, Category<TRow>>();

		var files = Directory.GetFiles(filesPath, "*.csv");

		if (files.Length > 0) { Load(ref files, ref categories); }
		else
		{
			var directories = Directory.GetDirectories(filesPath);

			ref var directory = ref MemoryMarshal.GetArrayDataReference(directories);
			ref var end       = ref Unsafe.Add(ref directory, directories.Length);

			while (Unsafe.IsAddressLessThan(ref directory, ref end))
			{
				files = Directory.GetFiles(directory, "*.csv");
				Load(ref files, ref categories);

				directory = ref Unsafe.Add(ref directory, 1);
			}
		}

		Categories = categories.ToFrozenDictionary();
	}

	private static void Load(ref string[] filesPaths, ref Dictionary<string, Category<TRow>> categories)
	{
		var     filesPathSpan = filesPaths.AsSpan();
		ref var filesPath     = ref MemoryMarshal.GetReference(filesPathSpan);
		ref var end           = ref Unsafe.Add(ref filesPath, filesPathSpan.Length);

		var container = new Dictionary<string, Dictionary<string, Dictionary<string, TRow>>>();

		while (Unsafe.IsAddressLessThan(ref filesPath, ref end))
		{
			var path     = filesPath.Replace('\\', '/').AsSpan();
			var fileName = path[(path.LastIndexOf('/') + 1)..];

			var category = new string(fileName[..fileName.IndexOf('.')]);
			var name     = new string(fileName[(fileName.IndexOf('.') + 1)..fileName.LastIndexOf('.')]);

			using var file = File.OpenRead(new string(path));

			using var reader = new CsvReader(
											 new StreamReader(file),
											 new CsvConfiguration(CultureInfo.InvariantCulture) { MissingFieldFound = null }
											);

			ref var data = ref CollectionsMarshal.GetValueRefOrAddDefault(container, category, out _);
			data ??= new Dictionary<string, Dictionary<string, TRow>>();

			ref var value = ref CollectionsMarshal.GetValueRefOrAddDefault(data, name, out _);
			value ??= new Dictionary<string, TRow>();

			foreach (var r in reader.GetRecords<TRow>().ToArray().AsSpan())
			{
				if (r.GetType().GetProperty("Key") is { } prop && prop.GetValue(r) is { } val && val.ToString() is { } str) value[str] = r;
			}

			filesPath = ref Unsafe.Add(ref filesPath, 1);
		}

		foreach (var (k, v) in container) { categories[k] = new Category<TRow>(v); }
	}
}
