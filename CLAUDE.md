# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

Barbatos.i18n is a published NuGet localization (i18n) library for .NET, independently evolved from lepoco.i18n.
It is split into one core package plus optional integration and file-format packages:

| Package | Role |
|---|---|
| `Barbatos.i18n` | Core primitives, builder, providers, YAML + RESX loading |
| `Barbatos.i18n.DependencyInjection` | `Microsoft.Extensions.DependencyInjection` + `IStringLocalizer` bridge |
| `Barbatos.i18n.Wpf` / `Barbatos.i18n.Maui` | XAML markup extensions, converters, live translation |
| `Barbatos.i18n.Json` / `.Ini` / `.Csv` | Extra file-format loaders |

Because it ships to NuGet, **every public member is a permanent API commitment** — prefer adding an optional
property over changing an existing signature.

## Commands

The solution is `Barbatos.i18n.slnx` (the newer XML solution format, not `.sln`).

```bash
dotnet build src/Barbatos.i18n.Wpf/Barbatos.i18n.Wpf.csproj -f net10.0-windows
```

```bash
dotnet test tests/Barbatos.i18n.UnitTests/Barbatos.i18n.UnitTests.csproj -f net10.0
```

Run a single test:

```bash
dotnet test tests/Barbatos.i18n.Wpf.UnitTests/Barbatos.i18n.Wpf.UnitTests.csproj -f net10.0-windows --filter "FullyQualifiedName~LiveBindingIntegrationTests"
```

Notes that will save time:

- **Always pass `-f <tfm>` when iterating.** Everything multi-targets, and the MAUI library alone targets
  android/ios/maccatalyst/windows — an unqualified build is slow and can fail on non-Windows targets.
- The MAUI workload is required (`dotnet workload restore`). Verify with `dotnet workload list`.
- `Barbatos.i18n.Wpf.UnitTests` → `net8.0-windows;net10.0-windows`;
  `Barbatos.i18n.Maui.UnitTests` → `net9.0-windows10.0.19041.0;net10.0-windows10.0.19041.0`. Both need Windows.
- **CI does not run tests.** `.github/workflows/barbatos-i18n-cd-nuget.yml` is `workflow_dispatch` only and just
  packs/pushes to NuGet. The local test run is the only gate — run it before declaring anything done.
- Packages are produced by `GeneratePackageOnBuild=true`; a Release build with `-p:SourceLinkEnabled=true` also
  strong-name-signs with `src/barbatos.snk`.

### A test that used to fail

`Barbatos.i18n.UnitTests.LocalizationBuilderTests.FromResource_ShouldAllowLookupUsingTestResourcePropertyDirectly`
fails on `main` (ko-KR RESX lookup returns null) and **passes on this branch** — the culture-fallback work fixed
it. Treat a failure there as a real regression in the RESX satellite loading path, not as pre-existing noise,
and never "fix" it by weakening the assertion.

## Architecture

### Resolution chain

A XAML `{i18n:StringLocalizer Text='Key'}` resolves through this chain — you usually need to read all of it:

```
markup extension  →  MultiBinding + converter  →  LocalizationLookup.ResolveValue
                  →  ILocalizationProvider     →  LocalizationSet[LocalizationKey]
```

- `LocalizationBuilder` collects `LocalizationSet`s and `Build()`s an `ILocalizationProvider`.
- `LocalizationSet` is an immutable record of `(Name, Culture, Strings)`. `Name` is the "namespace" surfaced in
  XAML as `Namespace='errors'`.
- **Culture fallback.** Lookups walk `CultureFallback.EnumerateChain`: the exact culture first, then each
  parent, ending at the invariant culture — so a set registered for `en` also serves `en-GB`. Each culture is
  fully considered before moving to a less specific one, so an exact match never loses to a parent.
  `GetLocalizationSets(culture)` deliberately stays **exact**, which is how
  `GetAllStrings(includeParentCultures: false)` scopes itself.
- **Set precedence — read this before touching `LocalizationLookup`.** An extension written *without* a
  `Namespace` means "find this key wherever it lives", so the lookup resolves a **value** by searching every
  registered set in **registration order**, not by picking one set and indexing it. Resolving to a single set
  broke the WPF sample outright: `FromIni("Locales.en-US.ini")` derives the name `"en-us"`, so *every* file the
  sample registers is named, and the only unnamed set is the handful of keys a YAML file contributes to its
  implicit default namespace — nearly every string rendered as its raw key. Registration order is why
  `LocalizationBuilder` keeps a `List`, not a `HashSet`. `MultiSetResolutionTests` pins this.
- `LocalizationKey` is a `readonly struct` that **normalizes every key**: `:` → `.` and `ToLowerInvariant()`.
  This is why `Header:Title`, `header.TITLE` and `Header.Title` are the same key, and why enum member names work
  as keys regardless of casing. It implicitly converts to/from `string`, so normalization is often invisible at
  call sites.

### Two parallel provider registries

This trips people up constantly. There are **two** registries and every consumer tries them in a fixed order:

1. `LocalizationProviderResolver` — the DI one, reached via `WpfLocalization`/`MauiLocalization` service-locator bridges.
2. `LocalizationProviderFactory` — a static `ConcurrentDictionary`, used by the non-DI wiring
   (`Application.UseStringLocalizer`, `MauiAppBuilder.UseStringLocalizer`).

`LocalizationLookup.ResolveValue`/`ResolveFormatted` (one copy per UI package) encapsulate "DI first,
static fallback". **Use them** —
do not hand-roll the `GetProvider(...) ?? LocalizationProviderFactory.GetInstance(...)` pair again; it used to be
duplicated at ten sites (five per UI package) and resolved each registry twice per lookup.

Both registries are keyed by `ProviderKey` (empty string = default), which is what the XAML `ProviderKey='...'`
argument selects.

### Three culture managers

`ILocalizationCultureManager` has three implementations that must stay behaviourally aligned:

- `Barbatos.i18n/LocalizationCultureManager.cs` — non-DI; fans out over `LocalizationProviderFactory.GetAllInstances()`.
- `Barbatos.i18n.DependencyInjection/DependencyInjectionLocalizationCultureManager.cs` — fans out to all providers.
- a private `DefaultLocalizationCultureManager` nested in `Barbatos.i18n.Maui/MauiAppBuilderExtensions.cs` — used
  when a MAUI app does not reference the DI package.

All three must: apply `FormatCultureBuilder`, set `CurrentUICulture`/`CurrentCulture` (+ the `DefaultThread*`
variants), push the culture into **every** provider, and **then** raise `LocalizationNotifier.CultureChanged`.
When you touch one, check the other two. "Every provider" is not a detail: while the non-DI one moved only the
default-keyed provider, a `ProviderKey=` string stayed in the old language while the live bindings around it
repainted. `KeyedProviderCultureTests` pins this.

### Live localization (culture changes repaint in place)

A `MarkupExtension` runs once at XAML load, so translations cannot follow a later culture switch on their own.
The mechanism that makes them reactive:

1. A culture manager raises the static `LocalizationNotifier.CultureChanged`.
2. `LocalizationSource` (a singleton per UI package) hears it and re-raises `INotifyPropertyChanged`, marshalled
   onto the UI dispatcher.
3. The extensions emit a `MultiBinding` whose **first value is `LocalizationSource.Culture`** — an otherwise
   unused "culture slot" that exists only to invalidate the binding.

**The critical invariant:** the converter reads the multi-binding values by position, in this exact order:

```
[culture slot?] [key?] [plural key?] [format args… | count?]
```

The extension's `BuildBinding` and the converter's constructor flags (`hasCultureSlot`, `keyFromBinding`,
`pluralKeyFromBinding`) must agree exactly — the flags describe which leading slots were reserved. If you add or
reorder a binding in `BuildBinding` without updating the converter flags, values silently shift by one and you
get wrong or blank text rather than an exception.

WPF and MAUI differ deliberately here: MAUI always returns a `BindingBase`, while WPF returns a live binding only
when `IProvideValueTarget.TargetProperty is DependencyProperty` (see `StringLocalizerExtension.IsBindableTarget`),
falling back to a plain string for targets like `Setter.Value` that report a CLR `PropertyInfo`. That gives WPF
**two code paths that must stay behaviourally identical** — `BuildBinding` and `Localize()`. They have drifted before.

### Wpf / Maui mirroring

`src/Barbatos.i18n.Wpf/` and `src/Barbatos.i18n.Maui/` hold near-identical mirrored files
(`StringLocalizerExtension`, `PluralStringLocalizerExtension`, the two converters, `LocalizeConverter`,
`LocalizationSource`, `LocalizationLookup`). **A fix to one almost always belongs in the other.** Genuine platform
differences are limited to: the unset sentinel (`DependencyProperty.UnsetValue` vs `BindableProperty.UnsetValue`),
dispatcher access (`Dispatcher.CheckAccess()` vs `IDispatcher.IsDispatchRequired`), per-child `StringFormat`
(WPF ignores it inside a `MultiBinding`, so the WPF converter applies it manually; MAUI handles it natively), and
the `MarkupExtension` vs `IMarkupExtension<BindingBase>` base type.

### Adding a file format

Each format package follows the same shape — copy `Barbatos.i18n.Ini` as the template:

- A `LocalizationBuilderExtensions` with `From<Format>(path, culture)`, `From<Format>(assembly, path, culture)`
  and `From<Format>String(name, culture, contents)`.
- Content is read by `EmbeddedResourceReader.ReadToEnd(path, assembly)`, so **files must be `EmbeddedResource`**
  and the path is dot-notation matching the logical resource name (`"Locales.Locales-en-US.json"`), not a file path.
- The set `Name` comes from `LocalizationSetNaming.DeriveName(path, culture)` — **call it, do not re-derive**;
  the rule used to be copy-pasted into all three packages. It is the file name minus extension, folders and the
  `-{culture}` suffix, lowercased, so `Translations-en-US.ini` becomes `translations`. When nothing but the
  culture is left, the **folder** names the set: `Locales.en-US.ini` becomes `locales`, so all three languages
  share one namespace instead of becoming `en-us`/`vi-vn`/`ko-kr`; with no folder either it returns null, the
  default namespace. `LocalizationSetNamingTests` pins all of it.
- **`AddLocalization` merges** a set whose name and culture match one already registered, rather than throwing:
  a YAML file's root-level keys and an INI file named after nothing but its culture both belong to the default
  namespace, and refusing that aborted startup. The set registered **first** keeps its position and wins on a
  duplicated key, matching the precedence a lookup applies. `LocalizationSetMergingTests` pins it.
- The parser returns `IEnumerable<KeyValuePair<LocalizationKey, string?>>` — make it a **`Dictionary`**, since
  `LocalizationSet`'s indexer only takes its O(1) path for dictionary-backed strings. Failures should throw
  `LocalizationBuilderException`, not a raw parser exception (`JsonReading.ParseDocument` shows the wrapping).

### DI layer

`services.AddStringLocalizer(...)` registers `IStringLocalizerFactory`, `ILocalizationCultureManager`,
`ILocalizationProvider`, `IStringLocalizer`, and `ICompositeStringLocalizer(<T>)`. The distinction worth knowing:
`ProviderBasedStringLocalizer` is scoped to one set (`baseName`), while `ICompositeStringLocalizer` searches the
default set first and then every set for the current culture — that is the one to recommend to consumers who do
not want to know which file a key lives in.

## Conventions

- **Comments and XML docs in English only**, including sample projects. The repository owner is Vietnamese-speaking
  and conversation happens in Vietnamese, but code stays English.
- Every `.cs` file starts with the 4-line MIT header from `.editorconfig`'s `file_header_template`.
- `GenerateDocumentationFile` is on for shipping projects — public members need XML docs.
- `IsTrimmable`, `EnableTrimAnalyzer`, `EnableAotAnalyzer`, `EnableSingleFileAnalyzer` are all on. Avoid adding
  reflection; the RESX path (`ResourceManager`) is the existing exception.
- Central package management: versions live in `Directory.Packages.props`, never in a `.csproj`.
- `.gitattributes` enforces `* text eol=lf` — write LF, even on Windows.
- `.editorconfig` sets `csharp_style_var_elsewhere = false:warning`: use explicit types unless the type is
  apparent from a `new` on the right-hand side.
- Tests: xunit + AwesomeAssertions. Anything touching `LocalizationProviderFactory`, `CultureInfo.Current*`, or
  `LocalizationSource` mutates process-wide state — mark those classes `[Collection("Sequential")]` and restore
  the original culture/provider in `Dispose`.

## Traps

- **MAUI `VisualElement` cannot be constructed in the unit-test host.** Instantiating a `Picker`, `Label`, or any
  `BindableObject` throws `REGDB_E_CLASSNOTREG` because `ViewHandler`'s static constructor needs the WinUI runtime.
  MAUI tests must stay at the converter/extension level; element-level behaviour is verified on the WPF side,
  where real `TextBlock`/`ItemsControl` instances work on an STA thread with no `Application`.
- **`Application.Current` is null in tests**, so `LocalizationSource` applies changes synchronously on the calling
  thread instead of marshalling. Convenient for tests, but do not read that as proof the dispatcher path works.
- **`CultureInfo.CurrentCulture` is per-thread**, and `SetCulture` only sets it on the calling thread.
  `DefaultThreadCurrentCulture` rescues a thread that never set its own culture, but *not* one that did — which
  a previous on-thread `SetCulture` gives it. `LocalizationSource.Apply` therefore re-applies both cultures on
  the dispatcher thread, and `LocalizationChangedEventArgs` carries `FormatCulture` separately from `Culture` so
  a `FormatCultureBuilder` result survives the hop. Don't "simplify" that back to reading ambient state.
- **MAUI bindings built from a string path are not trim safe.** The culture binding uses `Binding.Create` with an
  expression for this reason; the remaining `new Binding { Source = … }` sites still warn (IL2026) and predate
  this work.
- **`LocalizeConverter` cannot re-translate.** An `IValueConverter` has no source change to re-trigger it on a
  culture switch, so its text goes stale while live bindings update. Prefer `BindText` on the markup extension;
  the converter is kept for back-compat and for MAUI's `Picker.ItemDisplayBinding`, which has no item template.
