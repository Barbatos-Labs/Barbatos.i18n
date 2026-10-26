---
paths:
  - "src/Barbatos.i18n.DependencyInjection/**"
  - "tests/Barbatos.i18n.DependencyInjection.UnitTests/**"
  - "samples/Barbatos.i18n.Sample.WebApi/**"
---

# The DependencyInjection package

## What `AddStringLocalizer` registers

The first call creates one `LocalizationProviderResolver` instance (singleton) and registers, all transient:
`IStringLocalizerFactory`, `ILocalizationCultureManager` (`DependencyInjectionLocalizationCultureManager`),
`ILocalizationProvider` (the default provider, read from the resolver), `IStringLocalizer` (a
`ProviderBasedStringLocalizer` with no `baseName`), `ICompositeStringLocalizer` and `ICompositeStringLocalizer<>`.
Later calls - typically with a `providerKey` - only add their provider to that resolver (`GetOrAddResolver`).
`DependencyInjectionLocalizationBuilder.FromResource<T>` additionally registers `IStringLocalizer<T>`.

`ConfigureLocalizationOptions` and `AddStringLocalizer` may be called in either order: each finds and reuses the
instance the other registered. Both throw a descriptive `InvalidOperationException` when the existing
registration is not an instance they can extend - never let that become a `NullReferenceException` or
`InvalidCastException` again.

## The four localizers

All four read a set through the internal `LocalizationSetLookup`; they differ only in which sets they ask for and
in what order. The differences are deliberate:

| Localizer | Searches |
|---|---|
| `LocalizationSetStringLocalizer` | the one set it was given |
| `ProviderBasedStringLocalizer` | the set named `baseName`, else the **default set** |
| `ProviderBasedStringLocalizer<T>` | only the set named after `typeof(T).FullName`; **no** default-set fallback |
| `CompositeStringLocalizer` | the default set, then every set for the culture in registration order |
| `CompositeStringLocalizer<T>` | the set for `T`, then exactly what `CompositeStringLocalizer` searches |

`LookupPrecedenceTests` pins the table. `ICompositeStringLocalizer` is the one to recommend to consumers who do
not want to know which file a key lives in.

A missing key comes back as the key itself with `ResourceNotFound = true`; tooling uses that flag to audit
coverage, so keep it accurate. Arguments are applied only when given, so a translation containing a literal
`{0}` or `%` is returned as written when the caller passes none. `GetAllStrings(includeParentCultures: false)`
drops a set the lookup found under a parent culture.

## Culture

`DependencyInjectionLocalizationCultureManager` is one of three culture managers that must stay aligned - see
`culture-managers.md`. With `LocalizationOptions.UseAmbientCulture` it reads `CultureInfo.CurrentUICulture`, which
is how a server answers each request in its own language; the WebApi sample relies on that and must never call
`SetCulture` per request.
