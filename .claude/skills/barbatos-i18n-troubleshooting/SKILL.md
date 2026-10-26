---
name: barbatos-i18n-troubleshooting
description: Diagnose Barbatos.i18n localization failures from the symptom - text showing a raw key instead of a translation, language not changing when the user switches, part of the screen changing language while the rest does not, wrong language under concurrent load, a startup exception about a missing resource or a duplicate localization set, or dates and currency formatting in the wrong locale. Use this whenever localized text misbehaves in a project referencing any Barbatos.i18n package, before rewriting registration code, because these symptoms have a small set of specific causes and guessing tends to make things worse.
---

# Diagnosing Barbatos.i18n

Localization fails quietly: the app runs and shows the wrong text. Work from the symptom.

## Text shows the raw key ("Greeting" instead of "Hello")

The lookup returned nothing and fell back to the key. In order of likelihood:

1. **The provider is invisible to XAML.** With DI, the markup extensions reach the container through a
   bridge that must be called after building the provider:
   `ServiceProvider.UseWpfLocalization()` / `builder.Build().UseMauiLocalization()`. Skipping it makes *every* string
   render as its key — a whole-screen failure rather than a single one.
2. **The file was never loaded.** Locale files must be `EmbeddedResource`, and the path is dot-notation, not
   a file path. See `barbatos-i18n-resources`.
3. **The key really is absent** for the active culture. Registration is per culture: a key added to
   `en-US.json` and forgotten in `vi-VN.json` shows as the key only in Vietnamese.
4. **A namespace that does not match.** `Namespace='Errors'` addresses the set named `errors`; matching is
   case-insensitive, but a *wrong* name silently finds nothing. Drop `Namespace` to search every set and see
   whether the key appears — that isolates the problem to naming.
5. **Nested JSON keys need their prefix.** `{"Validation": {"Required": ...}}` is `Validation.Required`.

Key casing and `:` versus `.` are *not* causes — keys are normalized.

## Language does not change when the user switches

- **Was `SetCulture` called on the culture manager?** Setting `CultureInfo.CurrentUICulture` directly does not
  notify anything. Go through `ILocalizationCultureManager.SetCulture`, or the
  `WpfLocalization`/`MauiLocalization` bridge in a non-DI app.
- **Is `Live=False` on the extension?** That resolves once at load time by design.
- **WPF only:** the target may not be a `DependencyProperty`. `Setter.Value` reports a CLR property, so WPF
  falls back to a one-off string that cannot update. Set `Live=True` to force a binding.
- **Is it a `LocalizeConverter`?** It cannot re-translate — an `IValueConverter` has no source change to
  re-trigger it, so its text goes stale while everything around it updates. Switch to `BindText`.

## Part of the screen changes language, the rest does not

Look for what is different about the stale part:

- **A different `ProviderKey`.** Culture changes reach every registered provider, so this should not happen
  on a current version — if it does, the app is likely registering that provider outside the builder.
- **A `LocalizeConverter`** in the stale region (see above).
- **A second window.** Language and formatting are applied per window; a window opened before the switch is
  updated, but one whose `Language` the app set explicitly in XAML is deliberately left alone.

## Wrong language, but only sometimes or only under load

This is a server. The default configuration keeps one culture for the whole process, so concurrent requests
overwrite each other and a response can come back in another request's language.

Fix: `options.UseAmbientCulture = true` plus `app.UseRequestLocalization()`, and delete any middleware calling
`SetCulture` per request. Full explanation in `barbatos-i18n-aspnetcore`. A sequential test will not reproduce
this — drive it concurrently.

## Dates, numbers or currency in the wrong locale

Translation and formatting are separate cultures. `CurrentUICulture` picks the translation; `CurrentCulture`
formats `{0:C}` and `{0:d}`.

- Put the format inside the translation (`Price: {0:C}`), not in the markup, so a translator can move it.
- In WPF, a window's `Language` drives XAML `StringFormat`. Windows are kept in step automatically unless the
  app set `Language` on one itself, which is respected.
- Note that `CultureInfo.CurrentCulture` is per thread. Reading it on a thread that never had it set gives the
  default, which is why culture is applied on the UI dispatcher thread rather than wherever the switch was
  requested.

## Startup exceptions

**`LocalizationBuilderException: Resource ... not found in assembly ...`** — the file is not an
`EmbeddedResource`, or the dot-notation path is wrong. Check the `.csproj` first.

**`LocalizationBuilderException: The JSON localization file is not valid JSON`** — a syntax error; the inner
exception points at the offending position.

**`LocalizationBuilderException: ... duplicate "X" keys`** — the same key twice in one file. Legitimate on
purpose across *different* files, where the first registered wins, but never within one.

**`ArgumentException: The CSV contents are formatted as a multi-culture file`** (or single-) — the wrong
`FromCsv` overload. A `Key,Value` header takes a culture argument; a `Key,en-US,vi-VN` header does not.

**`InvalidOperationException: ... is already registered in a form this method cannot extend`** — something
else registered `ILocalizationProviderResolver` or `LocalizationOptions` in a way the library cannot build on.
Register every provider through `AddStringLocalizer` / `UseStringLocalizer`.

## Two keys collide

Without `Namespace`, a lookup searches every set in **registration order** and takes the first hit. If a key
defined in two files resolves to the wrong one, either name the set you mean with `Namespace`, or change the
order the files are registered in.

Note that files landing in the same namespace merge, with the first registration winning per key — so
registration order decides that too.

## Isolating a lookup

When the cause is not obvious, take XAML out of the picture and resolve the key from C#:

```csharp
var manager = provider.GetRequiredService<ILocalizationCultureManager>();
manager.SetCulture(new CultureInfo("vi-VN"));

var localizer = provider.GetRequiredService<ICompositeStringLocalizer>();
var result = localizer["Greeting"];
Console.WriteLine($"{result.Value} (missing: {result.ResourceNotFound})");
```

`ResourceNotFound` separates the two failure modes cleanly: true means the key was never found, so the problem
is registration or the key itself; false with unchanged text means the translation is genuinely identical in
both languages, and the bug is elsewhere.

Also worth printing while diagnosing:

```csharp
foreach (var set in provider.GetRequiredService<ILocalizationProvider>()
                            .GetLocalizationSets(new CultureInfo("vi-VN")))
{
    Console.WriteLine($"{set.Name ?? "<default>"}: {set.Strings.Count()} keys");
}
```

If a set you expect is missing, the file never loaded. If it is there with a surprising name, the file name is
deciding the namespace — see `barbatos-i18n-resources`.
