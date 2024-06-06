using Google.Apis.Auth.AspNetCore3;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

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
									   // This forces challenge results to be handled by Google OpenID Handler, so there's no
									   // need to add an AccountController that emits challenges for Login.
									   o.DefaultChallengeScheme = GoogleOpenIdConnectDefaults.AuthenticationScheme;

									   // This forces forbid results to be handled by Google OpenID Handler, which checks if
									   // extra scopes are required and does automatic incremental auth.
									   o.DefaultForbidScheme = GoogleOpenIdConnectDefaults.AuthenticationScheme;

									   // Default scheme that will handle everything else.
									   // Once a user is authenticated, the OAuth2 token info is stored in cookies.
									   o.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
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

;

/*builder.Services.ConfigureApplicationCookie(
											options =>
											{
												options.Cookie.HttpOnly     = true;
												options.ExpireTimeSpan      = TimeSpan.FromDays(25);
												options.SlidingExpiration   = true;
												options.Cookie.SameSite     = SameSiteMode.None;
												options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
											}
										   );*/

/*builder.Services.AddAuthentication(
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
					  options.Cookie.HttpOnly     = true;
					  options.SlidingExpiration   = true;
				  }
				 )
	   .AddGoogleOpenIdConnect(
							   options =>
							   {
								   options.ClientId                      = Environment.GetEnvironmentVariable("GoogleClientId");
								   options.ClientSecret                  = Environment.GetEnvironmentVariable("GoogleClientSecret");
								   options.Authority                     = "https://accounts.google.com";
								   options.CallbackPath                  = "/signin-oidc";
								   options.SaveTokens                    = true;
								   options.UseTokenLifetime              = false;
								   options.GetClaimsFromUserInfoEndpoint = true;

								   options.Events.OnRedirectToIdentityProvider = async context =>
																				 {
																					 context.ProtocolMessage.RedirectUri
																							 = context.ProtocolMessage.RedirectUri.Replace(
																								  "http://", "https://"
																								 );

																					 await Task.FromResult(0);
																				 };

								   options.Scope.Add(DriveService.ScopeConstants.DriveAppdata);
								   options.Scope.Add(DriveService.ScopeConstants.DriveFile);
							   }
							  );*/

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
