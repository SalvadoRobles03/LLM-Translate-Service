using LLMTranslateService.Api.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient<ITranslationService, TranslationService>();

builder.Services.AddScoped<ITranslationService, TranslationService>();

builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();

app.Run();