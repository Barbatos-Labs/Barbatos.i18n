using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Barbatos.i18n;
using Barbatos.i18n.DependencyInjection;
using Barbatos.i18n.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

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

app.UseRequestLocalization();

// Create middleware to sync ASP.NET Core request culture with Barbatos.i18n culture manager
app.Use(async (context, next) =>
{
    var cultureManager = context.RequestServices.GetRequiredService<ILocalizationCultureManager>();
    // Set Barbatos culture to match ASP.NET Core culture resolved for the current request
    cultureManager.SetCulture(CultureInfo.CurrentUICulture);

    await next();
});


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
    return Results.Ok(new { Message = localizer["Greeting"] });
})
.WithName("GetGreeting");

// 2. Formatted Date/Time (JSON)
app.MapGet("/time", ([FromServices] ICompositeStringLocalizer localizer) =>
{
    return Results.Ok(new { Message = localizer["CurrentTime", DateTime.Now] });
})
.WithName("GetTime");

// 3. Formatted Currency (JSON)
app.MapGet("/price", ([FromServices] ICompositeStringLocalizer localizer) =>
{
    decimal price = 1500.50m;
    return Results.Ok(new { Message = localizer["Price", price] });
})
.WithName("GetPrice");

// 4. Nested JSON Keys
app.MapGet("/errors/notfound", ([FromServices] ICompositeStringLocalizer localizer) =>
{
    return Results.NotFound(new { Message = localizer["Errors.NotFound"] });
})
.WithName("GetNotFoundError");

app.Run();
