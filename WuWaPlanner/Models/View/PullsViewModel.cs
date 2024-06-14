using WuWaPlanner.Models.CsvManager;
using WuWaPlanner.Services;

namespace WuWaPlanner.Models.View;

public class PullsViewModel
{
	public required CsvManager<LangRow> CsvManager = null!;
	public          SaveData            Data       = new();
}

