---
paths:
  - "src/Barbatos.i18n/**/*.cs"
  - "src/Barbatos.i18n.Wpf/LocalizationLookup.cs"
  - "src/Barbatos.i18n.Maui/LocalizationLookup.cs"
  - "tests/Barbatos.i18n.UnitTests/**/*.cs"
---

# Core lookup: keys, cultures, sets and registries

How a key becomes a translation. Read all of it before changing `LocalizationKey`, `LocalizationSet`,
`LocalizationProvider`, `LocalizationBuilder`, `CultureFallback` or either `LocalizationLookup`.

## The resolution chain

```
markup extension  →  MultiBinding + converter  →  LocalizationLookup.ResolveValue
                  →  ILocalizationProvider     →  LocalizationSet[LocalizationKey]
```

- `LocalizationBuilder` collects `LocalizationSet`s and `Build()`s an `ILocalizationProvider`.
- `LocalizationSet` is an immutable record of `(Name, Culture, Strings)`. `Name` is the "namespace" surfaced in
  XAML as `Namespace='errors'`.

## Keys are normalized

`LocalizationKey` is a `readonly struct` that normalizes every key: `:` becomes `.` and the whole key is
`ToLowerInvariant()`. That is why `Header:Title`, `header.TITLE` and `Header.Title` are one key, and why an enum
member name works as a key whatever its casing. It converts implicitly to and from `string`, so normalization is
often invisible at the call site. A `default(LocalizationKey)` reports an empty string rather than null.

## Culture fallback

Lookups walk `CultureFallback.EnumerateChain`: the exact culture first, then each parent, ending at the invariant
culture, so a set registered for `en` also serves `en-GB`. Each culture is fully considered before moving to a
less specific one, so an exact match never loses to a parent.

`GetLocalizationSets(culture)` deliberately stays **exact**; that is how `GetAllStrings(includeParentCultures:
false)` scopes itself. `GetLocalizationSet(culture, name)` walks the chain, and a null `name` prefers the
unnamed set before falling back to the first set registered for that culture.

## Set precedence: registration order

An extension written *without* a `Namespace` means "find this key wherever it lives", so the lookup resolves a
**value** by searching every registered set in **registration order**. It does not pick one set and index it.

Resolving to a single set broke the WPF sample outright: `FromIni("Locales.en-US.ini")` used to derive the name
`"en-us"`, so every file the sample registered was named, and the only unnamed set was the handful of keys a YAML
file contributes to its implicit default namespace - nearly every string rendered as its raw key. Registration
order is why `LocalizationBuilder` keeps a `List`, not a `HashSet`. `MultiSetResolutionTests` (WPF and MAUI) pins
this.

`AddLocalization` **merges** a set whose name and culture match one already registered instead of throwing. The
set registered first keeps its position and wins on a duplicated key, the same precedence a lookup applies.
`LocalizationSetMergingTests` pins it.

## The provider is on the hot path

One culture change re-evaluates every live binding on screen, and each of those walks the fallback chain.
`LocalizationProvider` therefore copies the sets up front, materializes each one's strings into a `Dictionary`
(the first entry for a key wins), and indexes the sets by culture name. `LocalizationSet`'s indexer only takes its
O(1) path for dictionary-backed strings, which is why parsers return a `Dictionary`. Do not reintroduce a
per-lookup scan or hold a deferred LINQ query as a set's strings: a YAML file's strings once arrived as a
projection and every key read re-ran it.

## Two provider registries

There are **two** registries, both keyed by `ProviderKey` (empty string = default), which is what the XAML
`ProviderKey='...'` argument selects:

1. `LocalizationProviderResolver` - the DI one, reached from XAML through the `WpfLocalization` /
   `MauiLocalization` service-locator bridges.
2. `LocalizationProviderFactory` - a static `ConcurrentDictionary`, used by the non-DI wiring
   (`Application.UseStringLocalizer`, `MauiAppBuilder.UseStringLocalizer`). Clearing a registration stores a
   null, so `GetAllInstances()` filters nulls out.

Every consumer tries them in that fixed order. `LocalizationLookup.ResolveValue` / `ResolveFormatted` (one copy
per UI package) encapsulate "DI first, static fallback" - use them, and never hand-roll the
`GetProvider(...) ?? LocalizationProviderFactory.GetInstance(...)` pair again. It used to be duplicated at ten
sites and resolved each registry twice per lookup.

When one call site needs both a value and the language it is in - the plural converters do - take
`GetProviders(providerKey)` once and pass the resulting `ProviderPair` to `ResolveValue`, reading `.Culture` from
the same pair. Asking two separate questions costs two `IServiceProvider.GetService` calls per conversion.

**Do not populate both registries for the same key with different cultures.** `ResolveValue` searches DI then
the static registry, while `ProviderPair.Culture` - which decides the plural form - returns the DI provider's
culture whenever one exists. If the DI provider lacks the key and the static one supplies it, the text comes from
one language and its grammar from the other (measured as English "0 item left", the French rule on an English
string). Fixing that would mean searching for the key twice on the hot path to learn which provider answered,
which is not worth paying on every binding for a misconfiguration. Register a given `ProviderKey` in one registry.

## Traps in this code

- **`LocalizationSet`'s string indexer reads the caller's source text.** `this[string? value,
  [CallerArgumentExpression] string expression]` falls back to the property name in `expression` when the value
  itself misses - that is what makes `{x:Static locales:Strings.Title}` resolve `Title`. The value wins when it
  resolves; the guess runs only after it fails. When you move a `set[x]` call into a helper, the expression
  becomes the helper's parameter name, so keep that name free of dots (`name`, `key`) or call the
  `LocalizationKey` indexer explicitly. The DI package's `LocalizationSetLookup.Find` relies on this.
- **`LocalizationBuilder.FromResource<TResource>(culture)` dispatches through the virtual
  `FromResource(Assembly, string, CultureInfo)`.** `DependencyInjectionLocalizationBuilder` overrides
  `FromResource<TResource>` to also register `IStringLocalizer<TResource>`. Calling the parser directly from a
  generic overload would bypass a subclass's override.
- **`LocalizationSetResourceParser` swaps the thread cultures while it reads a RESX** and restores both
  `CurrentCulture` and `CurrentUICulture` - restoring only one silently replaced the UI culture.
- `LocalizationBuilderTests.FromResource_ShouldAllowLookupUsingTestResourcePropertyDirectly` covers the RESX
  satellite path. A failure there is a real regression; never weaken its assertion.

## Pluralization lives here too

`PluralRules.IsPlural` is the single place the singular/plural choice is made: one is singular, any other
non-zero count is plural, and zero is the only count decided by culture - plural in English and most languages,
singular in French, Brazilian Portuguese, Armenian and Kabyle. It is deliberately a two-form model; languages with
three or more categories (Russian, Polish, Arabic, Czech) select the key in the view model instead. The UI-side
rules for calling it are in `ui-packages.md`. `PluralRulesTests` and `PluralZeroTests` pin both halves.
