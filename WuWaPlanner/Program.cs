using System.Globalization;
using System.IO.Compression;
using CacheManager.Core;
using Google.Apis.Auth.AspNetCore3;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.ResponseCompression;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using StackExchange.Redis;
using WuWaPlanner.Extensions;
using WuWaPlanner.Models;
using WuWaPlanner.Models.CsvManager;
using WuWaPlanner.Models.View;
using WuWaPlanner.Services;

var builder  = WebApplication.CreateBuilder(args);
var redisCfg = Environment.GetEnvironmentVariable("RedisConfig")!.Split(',');

var redis = await ConnectionMultiplexer.ConnectAsync(
													 redisCfg[0], options =>
																  {
																	  options.User     = redisCfg[1];
																	  options.Password = redisCfg[2];
																  }
													);

builder.Services.AddSingleton<KuroGamesService>();
builder.Services.AddSingleton<GoogleDriveService>();

builder.Services.AddSingleton(
							  new JsonSerializerSettings
							  {
								  Formatting       = Formatting.None,
								  ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() }
							  }
							 );

builder.Services.AddSingleton(
							  new CsvManager<LangRow>(
													  string.Concat(
																	Path.GetFullPath("../../", AppDomain.CurrentDomain.BaseDirectory),
																	"Localizations"
																   )
													 )
							 );

builder.Services.AddResponseCaching();
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

builder.Services.Configure<RequestLocalizationOptions>(
													   options =>
													   {
														   options.DefaultRequestCulture = new RequestCulture("En");

														   var cultures = new CultureInfo[] { new("En"), new("Ru"), new("Uk") };

														   options.SupportedCultures   = cultures;
														   options.SupportedUICultures = cultures;
													   }
													  );

builder.Services.AddDataProtection()
	   .SetApplicationName("WuWaPlanner")
	   .PersistKeysToStackExchangeRedis(redis, "DataProtection-Keys")
	   .DisableAutomaticKeyGeneration();

builder.Services.AddSession(
							options =>
							{
								options.IdleTimeout        = TimeSpan.FromDays(7);
								options.Cookie.HttpOnly    = true;
								options.Cookie.IsEssential = true;
							}
						   );

builder.Services.AddSingleton(CacheFactory.Build<SaveData>(nameof(SaveData), settings => settings.ApplyConfig(redis)));

builder.Services.AddSingleton(
							  CacheFactory.Build<PullsDataForm>(
																nameof(PullsDataForm),
																settings => settings.ApplyConfig(redis, TimeSpan.FromDays(7))
															   )
							 );

builder.Services.AddResponseCompression(
										options =>
										{
											options.EnableForHttps = true;
											options.Providers.Add<BrotliCompressionProvider>();
										}
									   );

builder.Services.Configure<BrotliCompressionProviderOptions>(options => { options.Level = CompressionLevel.SmallestSize; });

builder.Services.AddControllersWithViews().AddNewtonsoftJson().AddViewLocalization();

builder.Services.AddAuthentication(
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
																							 = context.ProtocolMessage.RedirectUri.Replace(
																								  "http://", "https://"
																								 );

																					 await Task.FromResult(0);
																				 };
							   }
							  );

builder.Services.AddHttpClient();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/error");
	app.UseHsts();
}

app.UseRequestLocalization();

app.UseResponseCaching();
app.UseResponseCompression();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseSession();
app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapControllerRoute("default",     "{controller=Home}/{action=Home}/{id?}");
app.MapControllerRoute("signin-oidc", "{controller=Home}/{action=Signin}");

app.Run($"{Environment.GetEnvironmentVariable("HOST")}:{Environment.GetEnvironmentVariable("PORT")}");
