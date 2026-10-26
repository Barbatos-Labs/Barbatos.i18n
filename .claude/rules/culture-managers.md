---
paths:
  - "src/Barbatos.i18n/ILocalizationCultureManager.cs"
  - "src/Barbatos.i18n/LocalizationCultureManager.cs"
  - "src/Barbatos.i18n/LocalizationNotifier.cs"
  - "src/Barbatos.i18n/LocalizationChangedEventArgs.cs"
  - "src/Barbatos.i18n/LocalizationOptions.cs"
  - "src/Barbatos.i18n.DependencyInjection/DependencyInjectionLocalizationCultureManager.cs"
  - "src/Barbatos.i18n.Maui/MauiAppBuilderExtensions.cs"
  - "src/Barbatos.i18n.Maui/MauiAppExtensions.cs"
  - "src/Barbatos.i18n.Wpf/ApplicationExtensions.cs"
  - "src/Barbatos.i18n.*/LocalizationSource.cs"
  - "tests/**/*Culture*Tests.cs"
---

# Culture managers and how a culture change travels

## Three implementations that must stay aligned

`ILocalizationCultureManager` has three implementations:

- `Barbatos.i18n/LocalizationCultureManager.cs` - non-DI; fans out over `LocalizationProviderFactory.GetAllInstances()`.
- `Barbatos.i18n.DependencyInjection/DependencyInjectionLocalizationCultureManager.cs` - fans out over the
  resolver's providers.
- a private `DefaultLocalizationCultureManager` nested in `Barbatos.i18n.Maui/MauiAppBuilderExtensions.cs` - used
  when a MAUI app does not reference the DI package.

All three must, in this order:

1. apply `Options.FormatCultureBuilder` to a clone of the culture (falling back to the culture itself on null);
2. set `CurrentUICulture` / `CurrentCulture` and the `DefaultThreadCurrentUICulture` /
   `DefaultThreadCurrentCulture` variants;
3. push the culture into **every** provider, keyed or not;
4. **then** raise `LocalizationNotifier.CultureChanged` with both the culture and the format culture.

When you touch one, check the other two - they share no code, because sharing it would need new public API in
the core package or `InternalsVisibleTo` across strong-named assemblies. "Every provider" is not a detail: while
the non-DI manager moved only the default-keyed provider, a `ProviderKey=` string stayed in the old language
while the live bindings around it repainted. `KeyedProviderCultureTests` (WPF) pins this, and
`DefaultCultureManagerTests` (MAUI) covers the private manager through `MauiApp.CreateBuilder().Services` - a
`MauiAppBuilder` can be constructed in a test even though a `VisualElement` cannot.

## The thread-culture trap

`CultureInfo.CurrentCulture` is per thread, and `SetCulture` only sets it on the calling thread.
`DefaultThreadCurrentCulture` rescues a thread that never set its own culture, but not one that did - which a
previous on-thread `SetCulture` gives it. So:

- `LocalizationSource.Apply` (one per UI package) re-applies both cultures on the dispatcher thread, where the
  converters later read them, before raising `PropertyChanged`.
- `LocalizationChangedEventArgs` carries `FormatCulture` separately from `Culture`, so a `FormatCultureBuilder`
  result survives the hop to another thread.

Do not "simplify" either back to reading ambient state. `CrossThreadCultureTests` (WPF) pins it. Note that
`Application.Current` is null in tests, so `LocalizationSource` applies changes synchronously on the calling
thread; a passing test is not proof that the dispatcher path works.

`LocalizationNotifier.CultureChanged` is a static event that lives for the process. Subscribe only from
long-lived objects (the `LocalizationSource` singletons), or unsubscribe explicitly.

## Server-side culture

A provider's culture is process-wide state: it fits one user at a time, not a server. The WebApi sample used to
turn each request's language into a `SetCulture` call, and over 400 parallel lookups **160 were answered in
another request's language**. `LocalizationOptions.UseAmbientCulture` (off by default, so desktop apps are
unchanged) makes `DependencyInjectionLocalizationCultureManager.GetCulture()` read `CultureInfo.CurrentUICulture`,
which ASP.NET Core establishes per request and which flows with the async context - so no middleware calls
`SetCulture` at all. `AmbientCultureTests` pins it. Never add per-request `SetCulture` to the WebApi sample.

## WPF window language

`ApplicationExtensions.SetLocalizationCulture` also points every window's `FrameworkElement.Language` at the new
culture so XAML `StringFormat` follows it. `LanguageProperty.OverrideMetadata` can run only once per type, so the
metadata default freezes at the first culture; each open window is therefore assigned explicitly, a class handler
on `Window.Loaded` covers windows opened later, and a window whose XAML set its own `Language` is left alone
(tracked weakly in a `ConditionalWeakTable`). `WindowLanguageTests` pins it.
