using System.ComponentModel.DataAnnotations;

namespace WuWaPlanner.Models.View;

public class PullsDataForm
{
	[Required(AllowEmptyStrings = false, ErrorMessage = "field is required")]
	[StringLength(80, MinimumLength = 70, ErrorMessage = "invalid length")]
	[Display(Name = "Data from script")]
	public string Tokens { get; set; } = string.Empty;
}
