using CacheManager.Core;
using StackExchange.Redis;

namespace WuWaPlanner.Extensions;

public static class CacheConfigurationExtensions
{
	public static ConfigurationBuilderCacheHandlePart
			ApplyConfig(this ConfigurationBuilderCachePart options, IConnectionMultiplexer redis, TimeSpan? expiration = null)
		=> options.WithJsonSerializer()
				  .WithRedisConfiguration("redis", redis)
				  .WithSystemRuntimeCacheHandle("system")
				  .WithExpiration(ExpirationMode.Sliding, expiration ?? TimeSpan.FromDays(45))
				  .DisablePerformanceCounters()
				  .DisableStatistics()
				  .And.WithRedisBackplane("redis")
				  .WithRedisCacheHandle("redis")
				  .WithExpiration(ExpirationMode.Sliding, expiration ?? TimeSpan.FromDays(45))
				  .DisablePerformanceCounters()
				  .DisableStatistics();
}
