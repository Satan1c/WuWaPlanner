using Google.Apis.Auth.AspNetCore3;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using StackExchange.Redis;

var builder  = WebApplication.CreateBuilder(args);
var rediscfg = Environment.GetEnvironmentVariable("RedisConfig")!.Split(',');

var redis = await ConnectionMultiplexer.ConnectAsync(
													 rediscfg[0], options =>
																  {
																	  options.User     = rediscfg[1];
																	  options.Password = rediscfg[2];
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

builder.Services.AddControllersWithViews();

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
					  options.Cookie.MaxAge       = TimeSpan.FromDays(25);
					  options.Cookie.MaxAge       = TimeSpan.FromDays(25);
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
