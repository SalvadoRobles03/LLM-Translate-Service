using LLMTranslateService.Api.Services;
using Microsoft.AspNetCore.Authentication;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc.Authorization;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: false, reloadOnChange: true);

builder.Services.AddHttpClient<ITranslationService, TranslationService>();
builder.Services.AddScoped<ITranslationService, TranslationService>();

builder.Services.AddAuthentication("BasicAuthentication").AddScheme<AuthenticationSchemeOptions, BasicAuthHandler>("BasicAuthentication", null);
builder.Services.AddControllers(options =>
{
    options.Filters.Add(new AuthorizeFilter());
});

var app = builder.Build();

app.UseAuthentication();

app.MapControllers();

app.Run();


public class BasicAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IConfiguration _configuration;

    public BasicAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        System.Text.Encodings.Web.UrlEncoder encoder,
        IConfiguration configuration)
        : base(options, logger, encoder)
    {
        _configuration = configuration;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.ContainsKey("Authorization"))
            return Task.FromResult(AuthenticateResult.Fail("Missing Authorization Header"));

        try
        {
            var authHeaderValue = Request.Headers["Authorization"].FirstOrDefault();
            if (string.IsNullOrEmpty(authHeaderValue))
                return Task.FromResult(AuthenticateResult.Fail("Missing Authorization Header"));

            var authHeader = AuthenticationHeaderValue.Parse(authHeaderValue);
            if (authHeader.Scheme != "Basic")
                return Task.FromResult(AuthenticateResult.Fail("Invalid Scheme"));

            if (string.IsNullOrEmpty(authHeader.Parameter))
                return Task.FromResult(AuthenticateResult.Fail("Missing Credentials"));

            var credentialBytes = Convert.FromBase64String(authHeader.Parameter);
            var credentials = Encoding.UTF8.GetString(credentialBytes).Split(':', 2);
            var username = credentials[0];
            var password = credentials[1];

            var validUsername = _configuration["BasicAuth:Username"];
            var validPassword = _configuration["BasicAuth:Password"];

            if (username != validUsername || password != validPassword)
                return Task.FromResult(AuthenticateResult.Fail("Invalid Username or Password"));

            var claims = new[] { new Claim(ClaimTypes.Name, username) };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
        catch
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid Authorization Header"));
        }
    }
}