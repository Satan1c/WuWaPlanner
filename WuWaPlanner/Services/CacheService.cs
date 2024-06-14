using CacheManager.Core;
using CacheManager.Core.Internal;
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
		TypeCache.RegisterResolveType(
									  s =>
									  {
										  if (s.Contains("SaveData")) return typeof(SaveData);

										  return s == typeof(PullsDataForm).FullName ? typeof(PullsDataForm) : null;
									  }
									 );

		PullsDataFormCacheManager = CacheFactory.Build<PullsDataForm>(
																	  nameof(PullsDataForm),
																	  settings => settings.ApplyConfig(TimeSpan.FromDays(7))
																	 );

		SaveDataCacheManager = CacheFactory.Build<SaveData>(nameof(SaveData), settings => settings.ApplyConfigWithRedis(redis));
	}
}
