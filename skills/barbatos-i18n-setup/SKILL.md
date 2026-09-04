---
name: barbatos-i18n-setup
description: Wire up the Barbatos.i18n localization library in a .NET app - pick the right packages, register providers, and switch language at runtime. Use this whenever a project references Barbatos.i18n (any package), or when adding translations, multi-language support, or runtime language switching to a WPF, MAUI, ASP.NET Core, or console .NET app and Barbatos.i18n is the chosen library. Start here before the other barbatos-i18n skills; it routes to them.
---

# Barbatos.i18n: setup and registration

Barbatos.i18n resolves a **key** to a **translation** for the **current culture**. Everything else is
plumbing around that sentence. This skill covers choosing packages, registering translation sources, and
switching language. It is the entry point for four sibling skills:

| Task | Skill |
|---|---|
| XAML in WPF or MAUI, live language switching, keys from bindings | `barbatos-i18n-xaml` |
| ASP.NET Core, web APIs, anything serving concurrent requests | `barbatos-i18n-aspnetcore` |
| Authoring the locale files themselves (JSON/YAML/INI/CSV/RESX) | `barbatos-i18n-resources` |
| A key renders as raw text, or the language does not change | `barbatos-i18n-troubleshooting` |

Read the relevant sibling once you know which surface you are on. If you are writing a server, read
`barbatos-i18n-aspnetcore` before writing any registration code — the default configuration is wrong for
concurrent requests and the failure is silent.

## Packages

Install `Barbatos.i18n` plus whatever matches the app. They are separate NuGet packages.

| Package | Add it when |
|---|---|
| `Barbatos.i18n` | Always. Core primitives, the builder, YAML and RESX loading |
| `Barbatos.i18n.DependencyInjection` | Using `IServiceCollection` — also gives `IStringLocalizer` |
| `Barbatos.i18n.Wpf` / `Barbatos.i18n.Maui` | XAML markup extensions and live translation |
| `Barbatos.i18n.Json` / `.Ini` / `.Csv` | Those file formats. YAML and RESX need no extra package |

## Registering translations

A `LocalizationBuilder` collects translation sources and produces an `ILocalizationProvider`. There are two
ways to register it, and which one you pick determines how culture changes propagate.

### With dependency injection (preferred)

```csharp
services.AddStringLocalizer(builder =>
{
    builder.FromJson("Locales.Strings-en-US.json", new CultureInfo("en-US"));
    builder.FromJson("Locales.Strings-vi-VN.json", new CultureInfo("vi-VN"));
    builder.SetCulture(new CultureInfo("en-US"));   // the culture the app starts in
});
```

This registers `IStringLocalizer`, `IStringLocalizerFactory`, `ICompositeStringLocalizer`,
`ILocalizationProvider` and `ILocalizationCultureManager`.

WPF and MAUI then need one line to bridge the container to the XAML markup extensions, which the XAML parser
constructs without going through DI:

```csharp
// WPF, after BuildServiceProvider()
ServiceProvider.UseWpfLocalization().SetLocalizationCulture(CultureInfo.CurrentUICulture);

// MAUI: these extend MauiApp, not MauiAppBuilder, so they come after Build()
return builder.Build()
              .UseMauiLocalization()
              .SetLocalizationCulture(CultureInfo.CurrentUICulture);
```

Forgetting that bridge is a common cause of every string rendering as its raw key — the provider exists but
the extensions cannot see it.

### Without dependency injection

```csharp
// WPF
Application.Current.UseStringLocalizer(loc => { /* same builder calls */ });

// MAUI, on MauiAppBuilder
builder.UseStringLocalizer(loc => { /* same builder calls */ });
```

## Switching language at runtime

Resolve `ILocalizationCultureManager` and call `SetCulture`. That one call applies the culture to the ambient
`CultureInfo` properties, pushes it into every registered provider, and raises the notification that repaints
live XAML bindings.

```csharp
var manager = serviceProvider.GetRequiredService<ILocalizationCultureManager>();
manager.SetCulture(new CultureInfo("vi-VN"));

// In XAML apps without DI, reach it through the platform bridge:
WpfLocalization.GetCultureManager()?.SetCulture("vi-VN");
MauiLocalization.GetCultureManager()?.SetCulture("vi-VN");
```

`GetSupportedCultures()` returns the cultures actually registered, which is what a language picker should bind
to rather than a hardcoded list.

**Do not call `SetCulture` per web request.** It mutates process-wide state. See `barbatos-i18n-aspnetcore`.

## Reading translations from C#

```csharp
public sealed class CheckoutService(ICompositeStringLocalizer localizer)
{
    public string Greet(string name) => localizer["GreetingWithName", name];
}
```

Prefer `ICompositeStringLocalizer` for application code. It searches the default set first and then every
other set, so callers do not need to know which file a key lives in — the usual reason people reach for the
wrong abstraction here. Use `IStringLocalizer<T>` when you deliberately want to scope lookups to one resource.

A missing key comes back as the key itself with `ResourceNotFound` set, so the UI degrades to something
readable rather than blank. Check that flag when auditing translation coverage.

## Two behaviours that surprise people

**Keys are normalized.** `:` becomes `.` and everything is lower-cased invariantly, so `Header:Title`,
`header.TITLE` and `Header.Title` are one key. This is also why an enum member name works as a key regardless
of its casing.

**Culture falls back through parents.** A lookup for `en-GB` tries `en-GB`, then `en`, then the invariant
culture. Registering one set for `en` therefore serves every English region. Each culture is fully considered
before moving to a less specific one, so an exact match never loses to a parent.

## Multiple providers

A second, independently-cultured set of translations — a plugin's strings, say — is registered under a key:

```csharp
services.AddStringLocalizer("PluginStrings", builder => { /* ... */ });
```

XAML then selects it with `ProviderKey='PluginStrings'`. `SetCulture` moves every provider, keyed or not, so
they never drift apart.

## Verifying it works

Localization failures are quiet: the app runs and shows the wrong text. Before declaring the wiring done,
resolve one key in each language you registered and confirm the value actually changes. If it does not, go to
`barbatos-i18n-troubleshooting` — it maps each symptom to its cause.
