# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository. It is
loaded in every session, so it holds only what applies everywhere; the detail lives in `.claude/` and loads when
it is relevant (see below).

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

## Where the rest of the knowledge lives

| Location | Loads | Holds |
|---|---|---|
| `.claude/rules/*.md` | Automatically, when you read or edit a file matching the rule's `paths` | The invariants of that part of the code |
| `.claude/skills/barbatos-dev-*` | When a task calls for one | Procedures: verify a change, add a file format, change public API and release |
| `.claude/skills/barbatos-i18n-*` | When a task calls for one | The library as its **consumers** see it. Use them for samples, the README and "how should this behave" questions — they are not instructions for changing `src/` |

| Rule | Covers | Loads for |
|---|---|---|
| `core-lookup.md` | Keys, culture fallback, set precedence, the two provider registries, `LocalizationSet` traps, `PluralRules` | `src/Barbatos.i18n/**`, both `LocalizationLookup.cs`, core tests |
| `culture-managers.md` | The three culture managers, thread cultures, ambient (per-request) culture, WPF window language | The managers, `LocalizationSource`, `LocalizationNotifier`, culture tests |
| `ui-packages.md` | WPF/MAUI mirroring, live localization, slot order, the two code paths, plural selection, test-host traps | `src/Barbatos.i18n.{Wpf,Maui}/**` and their tests |
| `di-package.md` | What `AddStringLocalizer` registers, the four localizers and their precedence | The DI package, its tests, the WebApi sample |
| `file-formats.md` | Loader shape, embedded resources, set naming, parser rules | Json / Ini / Csv packages, `Yaml/`, `IO/` |
| `tests.md` | Conventions, shared process state, tests with a history | `tests/**` |
| `user-facing-docs.md` | Keeping the README, API reference, samples and consumer skills true | Those files |

A rule loads when you open a matching file, not when you plan to. If you are about to change an area whose rule
has not loaded yet, read the rule first.

## Commands

The solution is `Barbatos.i18n.slnx` (the newer XML solution format, not `.sln`).

```bash
dotnet build src/Barbatos.i18n.Wpf/Barbatos.i18n.Wpf.csproj -f net10.0-windows
dotnet test tests/Barbatos.i18n.UnitTests/Barbatos.i18n.UnitTests.csproj -f net10.0
dotnet test tests/Barbatos.i18n.Wpf.UnitTests/Barbatos.i18n.Wpf.UnitTests.csproj -f net10.0-windows --filter "FullyQualifiedName~LiveBindingIntegrationTests"
```

- **Always pass `-f <tfm>` when iterating.** Everything multi-targets, and the MAUI library alone targets
  android/ios/maccatalyst/windows — an unqualified build is slow and can fail on non-Windows targets.
- The MAUI workload is required (`dotnet workload restore`; check with `dotnet workload list`). The WPF and MAUI
  test projects need Windows. **From Linux, use the `barbatos-dev-verify` skill**: its script compiles WPF and
  runs the MAUI tests against the plain `net10.0` MAUI assets, and checks public API with ApiCompat.
- Two tests have a history: the RESX test `FromResource_ShouldAllowLookupUsingTestResourcePropertyDirectly` must
  pass (a failure is a regression), and `SetCulture_ShouldUseCustomCulture_WhenCustomBuilderIsProvided` fails on
  Linux only, because ICU formats the vi-VN date differently from Windows.
- **No CI runs on pull requests**, so verify locally. **Publishing is automatic and irreversible**:
  `.github/workflows/barbatos-i18n-cd-nuget.yml` pushes to NuGet.org on `release: published`, and a package on
  NuGet.org can only be unlisted. Its test step currently runs no tests (the tests are removed from the solution
  first), so a local run is the only gate. Never create a release or tag; see `barbatos-dev-release`.
- Packages are produced by `GeneratePackageOnBuild=true`; a Release build with `-p:SourceLinkEnabled=true` also
  strong-name-signs with `src/barbatos.snk`.

## Rules that apply everywhere

- **Public API is permanent.** Before finishing a change to `src/`, compare the public surface with ApiCompat
  (`barbatos-dev-verify`). When a change alters behaviour a consumer can observe, update the README,
  `API-REFERENCE.md`, the samples and the `barbatos-i18n-*` skills in the same change.
- **WPF and MAUI mirror each other.** A fix to one almost always belongs in the other (`ui-packages.md`).
- **The converters read multi-binding values by position**, `[culture slot?] [key?] [plural key?] [args… |
  count?]`. An extension's `BuildBinding` and its converter's flags must agree, or text silently shifts by one.
- **There are two provider registries** (DI first, then the static `LocalizationProviderFactory`). Go through
  `LocalizationLookup`, and never register one `ProviderKey` in both (`core-lookup.md`).
- **The three culture managers must stay behaviourally aligned.** Touch one, check the other two
  (`culture-managers.md`).
- **A lookup without a `Namespace` searches every set in registration order.** Do not "optimize" it into picking
  one set.
- **Lookups are a hot path**: every live binding re-runs on each culture change. No per-lookup scans, deferred
  queries or repeated registry resolution.

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
- Tests: xunit + AwesomeAssertions; classes that touch process-wide state are `[Collection("Sequential")]` and
  restore it in `Dispose` (`tests.md`).

## Layout

```
src/                 the seven packages (one folder each)
tests/               one test project per package
samples/             WPF, MAUI and WebApi sample apps
build/               the package icon (nuget.png)
.claude/rules/       path-scoped invariants (table above)
.claude/skills/      barbatos-dev-* procedures and barbatos-i18n-* consumer skills (see its README.md)
.claude/evals/       prompts and assertions for the consumer skills
```
