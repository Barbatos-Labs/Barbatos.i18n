---
paths:
  - "src/Barbatos.i18n.Wpf/**"
  - "src/Barbatos.i18n.Maui/**"
  - "tests/Barbatos.i18n.Wpf.UnitTests/**"
  - "tests/Barbatos.i18n.Maui.UnitTests/**"
---

# WPF and MAUI packages

## Mirroring: a fix to one almost always belongs in the other

`src/Barbatos.i18n.Wpf/` and `src/Barbatos.i18n.Maui/` hold near-identical mirrored files:
`StringLocalizerExtension`, `PluralStringLocalizerExtension`, the two converters, `LocalizeConverter`,
`LocalizationSource` and `LocalizationLookup`. Keep them diffable against each other - the same member names, the
same helpers, the same order. Genuine platform differences are limited to:

- the unset sentinel: `DependencyProperty.UnsetValue` vs `BindableProperty.UnsetValue`;
- dispatcher access: `Dispatcher.CheckAccess()` vs `IDispatcher.IsDispatchRequired`;
- per-child `StringFormat`: WPF ignores it inside a `MultiBinding`, so the WPF converter applies it per argument;
  MAUI handles it natively;
- the base type: `MarkupExtension` vs `IMarkupExtension<BindingBase>`;
- when a binding is returned (below).

Shared helpers exist in both packages under the same names - reuse them rather than writing a third copy:
`StringLocalizerConverter.ReadKey` / `IsUnset`, `StringLocalizerExtension.CreateCultureBinding`,
`PluralStringLocalizerConverter.Localize`, and `LocalizationLookup` (see `core-lookup.md` for the two registries
it hides; never hand-roll the `GetProvider(...) ?? LocalizationProviderFactory.GetInstance(...)` pair).

## Live localization: how a culture change repaints in place

A markup extension runs once at XAML load, so a translation cannot follow a later culture switch on its own:

1. a culture manager raises the static `LocalizationNotifier.CultureChanged`;
2. `LocalizationSource` (a singleton per package) hears it and re-raises `INotifyPropertyChanged`, marshalled
   onto the UI dispatcher;
3. the extensions emit a `MultiBinding` whose **first value is `LocalizationSource.Culture`** - an otherwise
   unused "culture slot" that exists only to invalidate the binding.

## The slot-order invariant

The converter reads the multi-binding's values **by position**, in exactly this order:

```
[culture slot?] [key?] [plural key?] [format args… | count?]
```

The extension's `BuildBinding` and the converter's constructor flags (`hasCultureSlot`, `keyFromBinding`,
`pluralKeyFromBinding`) must agree exactly: the flags describe which leading slots were reserved. Add or reorder a
binding in `BuildBinding` without updating the flags and every value shifts by one - wrong or blank text, no
exception.

A test that builds a converter with hand-written flags cannot catch that, because it never runs the extension
that sets them. `SlotContractTests` (MAUI) drives the real extension, takes the multi-binding it emits and feeds
the converter values in that binding's own order. Measured: one unannounced binding in `BuildBinding` fails 4 of
its 8 tests and none of the older MAUI tests. Extend it when you change a binding's shape.

## Two code paths per extension that must render the same text

- **WPF** returns a live binding only when `IProvideValueTarget.TargetProperty is DependencyProperty` (see
  `StringLocalizerExtension.IsBindableTarget`), and falls back to a plain string for targets such as
  `Setter.Value` that report a CLR `PropertyInfo`. `Live=True` forces the binding.
- **MAUI** always returns a `BindingBase`; with `Live = false` it resolves once and wraps the string in a
  `Binding { Source = ... }`.

So each extension has a binding path (`BuildBinding` + converter) and a one-off path (`Localize()`). They have
drifted before. Prefer sharing a step between the two over keeping copies in sync by hand: the plural extension's
one-off path already calls `PluralStringLocalizerConverter.Localize`, and `CollectArguments()` gathers the static
`Arg..Arg5` in the order `BuildBinding` adds them. `OneOffPathParityTests` (MAUI) and `FallbackParityTests` (WPF)
run both paths over the same inputs and compare them.

## Pluralization

Both converters call `PluralRules.IsPlural`, so WPF and MAUI cannot drift on the rule. Within each package the
selection - pick the form, fall back to the other key when one is missing, resolve, escape a missing key, fill
the count in - lives once, in `PluralStringLocalizerConverter.Localize`. It passes
`LocalizationLookup.GetProviders(ProviderKey).Culture`, **not** the ambient UI culture: the translation was
selected from the provider's culture, so its form has to be decided by the same one. An app that sets its
language on the builder and never calls the culture manager leaves the two different, and reading the thread gave
French text with English grammar. The rule used to be `count > 1` inline at six sites - the French rule applied
everywhere, which made an English UI read "0 item left". `PluralZeroTests` (WPF) and `PluralBoundaryTests` (MAUI)
pin the 0/1/2 boundary.

## Traps

- **MAUI `VisualElement` cannot be constructed in the unit-test host.** A `Picker`, `Label` or any
  `BindableObject` throws `REGDB_E_CLASSNOTREG`, because `ViewHandler`'s static constructor needs the WinUI
  runtime. MAUI tests stay at the converter/extension level; element-level behaviour is verified on the WPF side,
  where real `TextBlock`/`ItemsControl` instances work on an STA thread with no `Application`.
- **A `MauiAppBuilder` can be constructed in tests** - `MauiApp.CreateBuilder()` works, so `UseStringLocalizer`
  and what it registers are testable through `builder.Services`.
- **`Application.Current` is null in tests**, so `LocalizationSource` applies changes synchronously; do not read a
  passing test as proof the dispatcher path works.
- **MAUI bindings built from a string path are not trim safe.** The culture binding uses `Binding.Create` with an
  expression for that reason; the remaining `new Binding { Source = … }` sites still warn (IL2026) and predate it.
- **`LocalizeConverter` cannot re-translate.** An `IValueConverter` has no source change to re-trigger it on a
  culture switch, so its text goes stale while live bindings update. Prefer `BindText` on the extension; the
  converter is kept for back-compat and for MAUI's `Picker.ItemDisplayBinding`, which has no item template.
  The two copies currently differ (whitespace-only keys, `ConvertBack`) - aligning them is a behaviour change.
- **Text escaping differs between the packages.** WPF's `StringLocalizerExtension.Text` setter unescapes XML
  entities (`EscapeText`); MAUI's does not, and unescapes only when echoing a missing key. Check which one you
  are in before reasoning about how a key containing `&amp;` is looked up.
