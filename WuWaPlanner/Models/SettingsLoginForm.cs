using System.ComponentModel.DataAnnotations;

namespace WuWaPlanner.Models;

public class SettingsLoginForm
{
	[Required(AllowEmptyStrings = false, ErrorMessage = "field is required")]
	[StringLength(15, MinimumLength = 3, ErrorMessage = "required field")]
	[Display(Name = "Login")]
	public string Login { get; set; } = string.Empty;

	[Required(AllowEmptyStrings = false, ErrorMessage = "field is required")]
	[StringLength(15, MinimumLength = 4, ErrorMessage = "required field")]
	[Display(Name = "Password")]
	public string Password { get; set; } = string.Empty;
}
