using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace WuWaPlanner.Models.CsvManager;

[JsonConverter(typeof(StringEnumConverter))]
public enum Langs : byte
{
	[EnumMember(Value = "ru")]
	Ru,

	[EnumMember(Value = "en")]
	En
}
