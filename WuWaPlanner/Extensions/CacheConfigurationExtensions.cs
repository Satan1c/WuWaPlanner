using CacheManager.Core;
using StackExchange.Redis;

namespace WuWaPlanner.Extensions;

public static class CacheConfigurationExtensions
{
	public static void ApplyConfigWithRedis(
			this ConfigurationBuilderCachePart options,
			IConnectionMultiplexer             redis,
			TimeSpan?                          expiration = null
	)
		=> options.WithJsonSerializer().WithRedisConfiguration("redis", redis).AddSystem(expiration).And.AddRedisBackplane();

	public static void ApplyConfig(this ConfigurationBuilderCachePart options, TimeSpan? expiration = null)
		=> options.WithJsonSerializer().AddSystem(expiration);

	private static ConfigurationBuilderCacheHandlePart AddSystem(this ConfigurationBuilderCachePart options, TimeSpan? expiration = null)
		=> options.WithSystemRuntimeCacheHandle()
				  .WithExpiration(ExpirationMode.Sliding, expiration ?? TimeSpan.FromDays(45))
				  .DisablePerformanceCounters()
				  .DisableStatistics();

	private static ConfigurationBuilderCacheHandlePart AddRedisBackplane(
			this ConfigurationBuilderCachePart options,
			TimeSpan?                          expiration = null
	)
		=> options.WithRedisBackplane("redis")
				  .WithRedisCacheHandle("redis")
				  .WithExpiration(ExpirationMode.Sliding, expiration ?? TimeSpan.FromDays(45))
				  .DisablePerformanceCounters()
				  .DisableStatistics();
}
