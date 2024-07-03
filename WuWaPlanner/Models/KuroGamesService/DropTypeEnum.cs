using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace WuWaPlanner.Models.KuroGamesService;

[JsonConverter(typeof(StringEnumConverter))]
public enum DropTypeEnum : byte
{
	Weapon,
	Resonator
}
