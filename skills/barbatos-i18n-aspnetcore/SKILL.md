---
name: barbatos-i18n-aspnetcore
description: Use Barbatos.i18n correctly in ASP.NET Core and any other server that handles concurrent requests - per-request language via UseAmbientCulture, request culture negotiation, and localizing responses. Read this before writing localization code in a web API, MVC app, Blazor Server app, gRPC service, or background worker that references Barbatos.i18n, because the default configuration is built for a single-user desktop app and silently answers one request in another request's language. Also use it whenever a server returns the wrong language intermittently or only under load.
---

# Barbatos.i18n on the server

The library's default model is one process, one user, one current language — right for a desktop app, wrong
for a server. On a server the language belongs to the **request being handled**, not to the process. Getting
this wrong produces a bug that passes every manual test and only appears under real traffic.

## The correct setup

Two things: turn on ambient culture, and let ASP.NET Core negotiate the request's culture.

```csharp
// 1. Lookups follow the culture of the current request rather than a shared, process-wide one.
builder.Services.ConfigureLocalizationOptions(options => options.UseAmbientCulture = true);

// 2. Register translations as usual.
builder.Services.AddStringLocalizer(i18n =>
{
    var assembly = typeof(Program).Assembly;
    i18n.FromJson(assembly, "MyApi.Locales.Strings-en-US.json", new CultureInfo("en-US"));
    i18n.FromJson(assembly, "MyApi.Locales.Strings-vi-VN.json", new CultureInfo("vi-VN"));
    i18n.SetCulture(new CultureInfo("en-US"));      // fallback when a request names no language
});

var supportedCultures = new[] { "en-US", "vi-VN" };
builder.Services.Configure<RequestLocalizationOptions>(options =>
    options.SetDefaultCulture(supportedCultures[0])
           .AddSupportedCultures(supportedCultures)
           .AddSupportedUICultures(supportedCultures));

var app = builder.Build();

// 3. This is the only wiring needed. It establishes the request's culture; the library reads it.
app.UseRequestLocalization();
```

Then inject a localizer and use it normally:

```csharp
app.MapGet("/greeting", (ICompositeStringLocalizer localizer) =>
    Results.Ok(new { Message = localizer["Greeting"] }));
```

`UseRequestLocalization` resolves the culture per request — from `?culture=vi-VN`, the `Accept-Language`
header or a cookie, in that order by default — and it flows with the async context, so it is still correct
after an `await`.

## Never call SetCulture per request

This is the mistake to watch for, because it looks reasonable and it is what a naive middleware does:

```csharp
// WRONG. Do not write this.
app.Use(async (context, next) =>
{
    var manager = context.RequestServices.GetRequiredService<ILocalizationCultureManager>();
    manager.SetCulture(CultureInfo.CurrentUICulture);
    await next();
});
```

`SetCulture` is a *global* operation by design: it assigns the ambient culture, the process-wide
`DefaultThreadCurrent*` defaults, and the culture of every registered provider. Requests in flight then race
between one request's write and another's read.

This is measurable, not theoretical. Against exactly this shape, 400 concurrent lookups alternating between two
languages returned **160 responses in the other request's language** — a 40% error rate that no single-user
test would ever reveal. With `UseAmbientCulture` and no middleware, the same 400 lookups are all correct.

If you find such middleware in a codebase, deleting it and enabling `UseAmbientCulture` is the fix.

## Why an option rather than the default

`UseAmbientCulture` is off by default so desktop apps, where the provider's culture *is* the app's language,
keep working unchanged. Turn it on for anything that serves more than one user at a time. It changes exactly
one thing: `GetCulture()` reads `CultureInfo.CurrentUICulture` instead of the shared provider's culture.

## Choosing a localizer

| Inject | When |
|---|---|
| `ICompositeStringLocalizer` | Default. Searches the default set, then every other set — callers need not know which file a key lives in |
| `ICompositeStringLocalizer<T>` | Same, but prefers the set registered for `T` |
| `IStringLocalizer<T>` | You deliberately want lookups confined to one resource |

All of them are safe to inject into singleton, scoped and transient services once `UseAmbientCulture` is on,
because none of them hold a culture — they read it per call.

A missing key returns the key with `ResourceNotFound` set rather than throwing, so an endpoint degrades to a
readable identifier instead of a 500.

## Background work

`CultureInfo.CurrentUICulture` does not flow into a `Task.Run` or a hosted service started outside a request.
Capture the culture while you still have the request context and apply it on the worker:

```csharp
var culture = CultureInfo.CurrentUICulture;
_ = Task.Run(() =>
{
    CultureInfo.CurrentUICulture = culture;
    var subject = localizer["OrderConfirmedSubject"];
});
```

For a queue consumer, store the culture name alongside the work item and set it when the item is picked up.
Language is part of the message, not something to infer from the machine.

## Verifying it

A per-request localization bug will not show up in a sequential test. Prove it with concurrency:

```csharp
Parallel.For(0, 400, i =>
{
    bool english = i % 2 == 0;
    CultureInfo.CurrentUICulture = new CultureInfo(english ? "en-US" : "vi-VN");
    var value = localizer["Greeting"].Value;
    // assert value matches the language this iteration asked for
});
```

Or drive the running API: fire concurrent requests alternating `?culture=en-US` and `?culture=vi-VN` and check
every response matches its own query string.
