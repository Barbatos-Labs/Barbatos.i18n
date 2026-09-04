using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Barbatos.i18n;
using Barbatos.i18n.DependencyInjection;
using Barbatos.i18n.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// A server answers many requests at once, so localization must follow the culture of the request being handled
// rather than a process-wide "current" one. UseRequestLocalization below establishes CurrentUICulture per
// request, and this makes lookups read it.
builder.Services.ConfigureLocalizationOptions(options => options.UseAmbientCulture = true);

// Register Localization using Barbatos.i18n
builder.Services.AddStringLocalizer(i18nBuilder =>
{
    var assembly = typeof(Program).Assembly;

    // JSON files. We use Barbatos.i18n.Sample.WebApi.Locales.Locales.en-US.json because MSBuild EmbeddedResource creates it this way.
    i18nBuilder.FromJson(assembly, "Barbatos.i18n.Sample.WebApi.Locales.Locales.en-US.json", new CultureInfo("en-US"));
    i18nBuilder.FromJson(assembly, "Barbatos.i18n.Sample.WebApi.Locales.Locales.vi-VN.json", new CultureInfo("vi-VN"));

    // Set default fallback culture
    i18nBuilder.SetCulture(new CultureInfo("en-US"));
});

// Configure localization request culture for ASP.NET Core
var supportedCultures = new[] { "en-US", "vi-VN" };
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.SetDefaultCulture(supportedCultures[0])
           .AddSupportedCultures(supportedCultures)
           .AddSupportedUICultures(supportedCultures);
});

var app = builder.Build();

// This is the only wiring needed. There used to be middleware here calling ILocalizationCultureManager.SetCulture
// with the request's culture, which mutates process-wide state: under concurrent requests they overwrote each
// other and a request could be answered in another request's language. UseAmbientCulture reads the culture that
// this call already establishes for the request, so no middleware is required.
app.UseRequestLocalization();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Barbatos.i18n Sample WebApi v1");
        options.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();

// Use cases:

// 1. Basic Greeting (JSON)
// https://127.0.0.1:7168/greeting?culture=vi-VN
app.MapGet("/greeting", ([FromServices] ICompositeStringLocalizer localizer) =>
{
    return Results.Ok(new { Message = localizer["Greeting"].Value });
})
.WithName("GetGreeting");

// 2. Formatted Date/Time (JSON)
app.MapGet("/time", ([FromServices] ICompositeStringLocalizer localizer) =>
{
    return Results.Ok(new { Message = localizer["CurrentTime", DateTime.Now].Value });
})
.WithName("GetTime");

// 3. Formatted Currency (JSON)
app.MapGet("/price", ([FromServices] ICompositeStringLocalizer localizer) =>
{
    decimal price = 1500.50m;
    return Results.Ok(new { Message = localizer["Price", price].Value });
})
.WithName("GetPrice");

// 4. Nested JSON Keys
app.MapGet("/errors/notfound", ([FromServices] ICompositeStringLocalizer localizer) =>
{
    return Results.NotFound(new { Message = localizer["Errors.NotFound"].Value });
})
.WithName("GetNotFoundError");

app.Run();
