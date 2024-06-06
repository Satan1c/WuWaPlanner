using Google.Apis.Auth.AspNetCore3;
using Google.Apis.Drive.v3;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSession();
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
					  options.LoginPath           = "/settings/login";
					  options.LoginPath           = "/settings/logout";
					  options.ExpireTimeSpan      = TimeSpan.FromDays(25);
					  options.Cookie.MaxAge       = TimeSpan.FromDays(25);
					  options.Cookie.SameSite     = SameSiteMode.None;
					  options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
				  }
				 )
	   .AddGoogleOpenIdConnect(
							   options =>
							   {
								   options.ClientId     = Environment.GetEnvironmentVariable("GoogleClientId");
								   options.ClientSecret = Environment.GetEnvironmentVariable("GoogleClientSecret");

								   options.Events.OnRedirectToIdentityProvider = async context =>
																				 {
																					 context.ProtocolMessage.RedirectUri
																							 = context.ProtocolMessage.RedirectUri.Replace(
																								  "http://", "https://"
																								 );

																					 await Task.FromResult(0);
																				 };

								   options.MaxAge = TimeSpan.FromDays(1);
								   options.Scope.Add(DriveService.ScopeConstants.DriveAppdata);
								   options.Scope.Add(DriveService.ScopeConstants.DriveFile);
								   options.SaveTokens                     = true;
								   options.NonceCookie.MaxAge             = TimeSpan.FromDays(25);
								   options.NonceCookie.Expiration         = TimeSpan.FromDays(25);
								   options.NonceCookie.SecurePolicy       = CookieSecurePolicy.Always;
								   options.NonceCookie.SameSite           = SameSiteMode.None;
								   options.CorrelationCookie.MaxAge       = TimeSpan.FromDays(25);
								   options.CorrelationCookie.Expiration   = TimeSpan.FromDays(25);
								   options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
								   options.CorrelationCookie.SameSite     = SameSiteMode.None;
							   }
							  );

builder.Services.AddMvc();
builder.Services.AddHttpClient();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/error");
	app.UseHsts();
}

app.UseCookiePolicy();

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
