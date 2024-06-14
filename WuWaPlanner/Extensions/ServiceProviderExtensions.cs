using System.Globalization;
using System.IO.Compression;
using CacheManager.Core;
using Google.Apis.Auth.AspNetCore3;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.ResponseCompression;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using StackExchange.Redis;
using WuWaPlanner.Models;
using WuWaPlanner.Models.CsvManager;
using WuWaPlanner.Models.View;
using WuWaPlanner.Services;

namespace WuWaPlanner.Extensions;

public static class ServiceProviderExtensions
{
	public static IServiceCollection AddServices(this IServiceCollection services)
		=> services.AddSingleton<KuroGamesService>()
				   .AddSingleton<GoogleDriveService>()
				   .AddResponseCompression(
										   options =>
										   {
											   options.EnableForHttps = true;
											   options.Providers.Add<BrotliCompressionProvider>();
										   }
										  )
				   .Configure<BrotliCompressionProviderOptions>(options => { options.Level = CompressionLevel.SmallestSize; })
				   .AddSingleton(
								 new JsonSerializerSettings
								 {
									 Formatting       = Formatting.None,
									 ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() }
								 }
								)
				   .AddSingleton(
								 new CsvManager<LangRow>(
														 string.Concat(
																	   Path.GetFullPath("../../", AppDomain.CurrentDomain.BaseDirectory),
																	   "Localizations"
																	  )
														)
								);

	public static IServiceCollection AddLocalizations(this IServiceCollection services)
	{
		return services.AddLocalization(options => options.ResourcesPath = "Resources")
					   .Configure<RequestLocalizationOptions>(
															  options =>
															  {
																  options.DefaultRequestCulture = new RequestCulture("En");

																  var cultures = new CultureInfo[] { new("En"), new("Ru"), new("Uk") };

																  options.SupportedCultures   = cultures;
																  options.SupportedUICultures = cultures;
															  }
															 );
	}

	public static IServiceCollection AddCaches(this IServiceCollection services, IConnectionMultiplexer redis)
	{
		return services.AddResponseCaching()
					   .AddSingleton(
									 CacheFactory.Build<PullsDataForm>(
																	   nameof(PullsDataForm),
																	   settings => settings.ApplyConfig(TimeSpan.FromDays(7))
																	  )
									)
					   .AddSingleton(CacheFactory.Build<SaveData>(nameof(SaveData), settings => settings.ApplyConfigWithRedis(redis)));
	}

	public static IServiceCollection AddGoogleAuthenticate(this IServiceCollection services)
	{
		services.AddSession(
							options =>
							{
								options.IdleTimeout        = TimeSpan.FromDays(7);
								options.Cookie.HttpOnly    = true;
								options.Cookie.IsEssential = true;
							}
						   )
				.AddAuthentication(
								   o =>
								   {
									   o.DefaultChallengeScheme = GoogleOpenIdConnectDefaults.AuthenticationScheme;
									   o.DefaultForbidScheme    = GoogleOpenIdConnectDefaults.AuthenticationScheme;
									   o.DefaultScheme          = CookieAuthenticationDefaults.AuthenticationScheme;
								   }
								  )
				.AddCookie(
						   options =>
						   {
							   options.Cookie.MaxAge       = TimeSpan.FromDays(45);
							   options.Cookie.MaxAge       = TimeSpan.FromDays(45);
							   options.Cookie.HttpOnly     = true;
							   options.Cookie.IsEssential  = true;
							   options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
							   options.Cookie.SameSite     = SameSiteMode.None;
							   options.SlidingExpiration   = true;
						   }
						  )
				.AddGoogleOpenIdConnect(
										options =>
										{
											options.ClientId     = Environment.GetEnvironmentVariable("GoogleClientId");
											options.ClientSecret = Environment.GetEnvironmentVariable("GoogleClientSecret");
											options.SaveTokens   = true;

											options.Events.OnRedirectToIdentityProvider = async context =>
																						  {
																							  context.ProtocolMessage.RedirectUri
																									  = context.ProtocolMessage.RedirectUri
																											  .Replace(
																												   "http://", "https://"
																												  );

																							  await Task.FromResult(0);
																						  };
										}
									   );

		return services;
	}
}
