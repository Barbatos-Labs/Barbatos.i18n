---
paths:
  - "tests/**"
---

# Tests

- **xunit + AwesomeAssertions.** Test projects target `net8.0;net10.0`, except `Barbatos.i18n.Wpf.UnitTests`
  (`net8.0-windows;net10.0-windows`) and `Barbatos.i18n.Maui.UnitTests`
  (`net9.0-windows10.0.19041.0;net10.0-windows10.0.19041.0`), which need Windows. Pass `-f <tfm>` when iterating.
- **Process-wide state.** Anything touching `LocalizationProviderFactory`, `CultureInfo.Current*`,
  `LocalizationSource`, `WpfLocalization`/`MauiLocalization` or `LocalizationNotifier` mutates state shared by
  every test. Mark the class `[Collection("Sequential")]` and restore the original culture and provider in
  `Dispose`. The DI test assembly disables parallelization outright (`AssemblyInfo.cs`). A test that needs a
  fixed culture without touching globals can use a small `ILocalizationCultureManager` stub, as
  `LookupPrecedenceTests` does.
- **Two tests with a history:**
  - `LocalizationBuilderTests.FromResource_ShouldAllowLookupUsingTestResourcePropertyDirectly` covers RESX
    satellite loading (ko-KR). It once failed on `main` and was fixed by the culture-fallback work. A failure is
    a real regression - never "fix" it by weakening the assertion.
  - `LocalizationCultureManagerTests.SetCulture_ShouldUseCustomCulture_WhenCustomBuilderIsProvided` fails on
    Linux and passes on Windows: it expects the Windows (NLS) vi-VN short date `14/06/2026`, while ICU formats
    `14/6/2026`. Environmental, not a regression.
- **Test the contract, not the assumption.** A converter built with hand-written flags cannot catch a
  disagreement with the extension that sets them; `SlotContractTests` drives the real extension. Likewise
  `OneOffPathParityTests` and `FallbackParityTests` compare an extension's two code paths with each other.
  Prefer that shape for anything with two halves that must agree.
- **Pin behaviour before refactoring.** When restructuring code whose behaviour is only implicitly covered, add
  characterization tests first, see them pass on the unchanged code, and commit them separately - so the
  refactoring commit is checked by tests that predate it.
- **What can be tested where.** MAUI tests stay at the converter/extension level: constructing any
  `VisualElement` throws `REGDB_E_CLASSNOTREG` in the test host, though `MauiApp.CreateBuilder()` works.
  Element-level behaviour is tested on the WPF side, where `TextBlock`/`ItemsControl` work on an STA thread with
  no `Application`. `Application.Current` is null in both, so dispatcher marshalling is never exercised.
- **Existing warnings.** CS8625 in `StringLocalizerBuilderExtensionsTests` and CA1063 on the WPF converter test
  fixtures predate current work; do not count them as new, and do not add more.
- Running the suites, including the MAUI and WPF ones from Linux, is covered by the `barbatos-dev-verify` skill.
