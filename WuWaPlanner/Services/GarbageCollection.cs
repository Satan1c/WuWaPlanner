using NCrontab;
using NCrontab.Scheduler;

namespace WuWaPlanner.Services;

public class GarbageCollection
{
	private const string c_cron = "0 0 */4 * * *";

	public GarbageCollection(IScheduler scheduler)
	{
		scheduler.AddTask(
						  CrontabSchedule.Parse(c_cron, new CrontabSchedule.ParseOptions { IncludingSeconds = true }), _ =>
						  {
							  for (byte i = 0; i <= GC.MaxGeneration; i++) { GC.Collect(i, GCCollectionMode.Forced, false, true); }

							  return Task.CompletedTask;
						  }
						 );
	}
}
