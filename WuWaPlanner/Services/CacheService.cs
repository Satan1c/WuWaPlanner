using CacheManager.Core;
using StackExchange.Redis;
using WuWaPlanner.Extensions;
using WuWaPlanner.Models;
using WuWaPlanner.Models.View;

namespace WuWaPlanner.Services;

public class CacheService
{
	public readonly ICacheManager<PullsDataForm> PullsDataFormCacheManager;
	public readonly ICacheManager<SaveData>      SaveDataCacheManager;

	public CacheService(IConnectionMultiplexer redis)
	{
		PullsDataFormCacheManager = CacheFactory.Build<PullsDataForm>(
																	  nameof(PullsDataForm),
																	  settings => settings.ApplyConfig(TimeSpan.FromDays(7))
																	 );

		SaveDataCacheManager = CacheFactory.Build<SaveData>(nameof(SaveData), settings => settings.ApplyConfigWithRedis(redis));
	}
}
