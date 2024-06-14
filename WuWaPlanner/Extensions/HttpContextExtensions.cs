namespace WuWaPlanner.Extensions;

public static class HttpContextExtensions
{
	private static readonly CookieOptions s_cookieOptions = new() { MaxAge = TimeSpan.FromDays(45) };

	public static string? ReadTokens(this HttpContext context)
		=> context.Request.Cookies.TryGetValue("tokens", out var value) ? value : null;

	public static void SaveTokens(this HttpContext context, string tokens)
	{
		context.Response.Cookies.Append("tokens", tokens, s_cookieOptions);
	}
}
