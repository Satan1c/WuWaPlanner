using System.Runtime;
using Microsoft.AspNetCore.DataProtection;
using NCrontab.Scheduler;
using StackExchange.Redis;
using WuWaPlanner.Extensions;
using WuWaPlanner.Services;

GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;

var builder  = WebApplication.CreateBuilder(args);
var redisCfg = Environment.GetEnvironmentVariable("RedisConfig")!.Split(',');

var redis = await ConnectionMultiplexer.ConnectAsync(
													 redisCfg[0], options =>
																  {
																	  options.User     = redisCfg[1];
																	  options.Password = redisCfg[2];
																  }
													);

builder.Services.AddHttpClient().AddLocalizations().AddGoogleAuthenticate().AddCaches(redis).AddServices().AddScheduler();

builder.Services.AddControllersWithViews().AddNewtonsoftJson().AddViewLocalization();

builder.Services.AddDataProtection().SetApplicationName("WuWaPlanner").PersistKeysToStackExchangeRedis(redis, "DataProtection-Keys");

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

app.Services.GetRequiredService<IScheduler>().Start();
app.Services.GetRequiredService<GarbageCollection>();

app.Run($"{Environment.GetEnvironmentVariable("HOST")}:{Environment.GetEnvironmentVariable("PORT")}");
