using System.Text.Json.Serialization;

namespace WuWaPlanner.Models;

public class UserModel
{
	[JsonPropertyName("tokens")]
	public string? Tokens { get; set; } = null;

	[JsonPropertyName("data")]
	public string? Data { get; set; } = null;
}
