---
name: barbatos-i18n-xaml
description: Translate WPF and MAUI XAML with Barbatos.i18n markup extensions - static keys, keys that come from a binding (list items, enums), pluralization, format arguments, and language switching that repaints the UI in place without reloading the window or page. Use this whenever writing or fixing XAML in a project that references Barbatos.i18n.Wpf or Barbatos.i18n.Maui, whenever a translated string needs to sit inside a DataTemplate, ItemsSource, DataGrid, ListView, CollectionView, Picker or ToolTip, or whenever translated text is not updating after the user changes language.
---

# Barbatos.i18n in XAML

Add the namespace once per file:

```xml
xmlns:i18n="clr-namespace:Barbatos.i18n.Wpf;assembly=Barbatos.i18n.Wpf"
<!-- MAUI: clr-namespace:Barbatos.i18n.Maui;assembly=Barbatos.i18n.Maui -->
```

Everything below works the same in both frameworks unless a difference is called out.

## The common cases

```xml
<!-- A key -->
<TextBlock Text="{i18n:StringLocalizer Text='Greeting'}" />

<!-- A key in a named set (a "namespace") -->
<TextBlock Text="{i18n:StringLocalizer Text='NetworkError', Namespace='errors'}" />

<!-- Static format argument: "Hello {0}, welcome back!" -->
<TextBlock Text="{i18n:StringLocalizer Text='GreetingWithName', Arg='Hung'}" />

<!-- Argument from the view model, re-evaluated when it changes -->
<TextBlock Text="{i18n:StringLocalizer Text='GreetingWithName', BindArg={Binding UserName}}" />

<!-- Up to five: Arg..Arg5 and BindArg..BindArg5, filled in the order written -->
<TextBlock Text="{i18n:StringLocalizer Text='FullName',
                  BindArg={Binding FirstName}, BindArg2={Binding LastName}}" />

<!-- A second provider registered under a key -->
<TextBlock Text="{i18n:StringLocalizer Text='BonusMessage', ProviderKey='PluginStrings'}" />
```

Translations follow the culture automatically. A language switch repaints the text in place — no window
reload, no page navigation, no manual refresh.

## Keys that come from data

This is the feature to reach for inside `DataTemplate`, `ItemsSource`, `DataGrid`, `ListView`,
`CollectionView` and `Picker`, where the key is a property of the item rather than something you can type in
the markup. Use `BindText` instead of `Text`:

```xml
<DataGridTemplateColumn>
  <DataGridTemplateColumn.CellTemplate>
    <DataTemplate>
      <TextBlock Text="{i18n:StringLocalizer BindText={Binding Status}}" />
    </DataTemplate>
  </DataGridTemplateColumn.CellTemplate>
</DataGridTemplateColumn>
```

**An enum works directly as a key.** Bind the enum member and name your keys after the members — no converter,
no lookup table, no `switch`:

```csharp
public enum OrderStatus { Active, Pending, Archived }
public record ProductRow(string Name, OrderStatus Status, int Stock);
```

```ini
Active=Currently selling
Pending=Pending review
Archived=Archived
```

Keys are normalized to lower case, so the enum's own casing never has to match the file's.

`BindText` takes precedence over `Text`, so do not set both expecting a fallback.

## Plural forms

```xml
<!-- Static count -->
<TextBlock Text="{i18n:PluralStringLocalizer Text='OneApple', PluralText='ManyApples', Count=5}" />

<!-- Count from the view model -->
<TextBlock Text="{i18n:PluralStringLocalizer Text='OneItemInStock', PluralText='ManyItemsInStock',
                  BindCount={Binding Stock}}" />
```

One is singular, anything else is plural, and the count is passed as `{0}` to the selected string. `BindText`
and `BindPluralText` supply the two keys from bindings when they are data-driven.

**Zero follows the language.** English says "0 items", French says "0 article" — zero is the only count whose
form differs between languages, so `PluralRules` decides it from the active UI culture. You do not have to do
anything for this; it is worth knowing because it means a translation's plural string has to read correctly
with a zero in it.

If a zero deserves different wording rather than a different grammatical form — "Out of stock" instead of
"0 items left" — say so explicitly:

```xml
<TextBlock>
  <TextBlock.Style>
    <Style TargetType="TextBlock">
      <Setter Property="Text" Value="{i18n:PluralStringLocalizer Text='OneItemInStock',
                                      PluralText='ManyItemsInStock', BindCount={Binding Stock}, Live=True}" />
      <Style.Triggers>
        <DataTrigger Binding="{Binding Stock}" Value="0">
          <Setter Property="Text" Value="{i18n:StringLocalizer Text='OutOfStock', Live=True}" />
        </DataTrigger>
      </Style.Triggers>
    </Style>
  </TextBlock.Style>
</TextBlock>
```

Note the `Live=True` on both: a `Setter.Value` target reports a CLR property, so without it WPF resolves once
and the text stops following culture changes.

**Two forms is the ceiling.** Russian, Polish, Arabic and Czech need three or more plural categories, which
this model cannot express. For those, choose the key in the view model and pass it through `BindText`.

Because the count is always formatted into the string, avoid a literal `{` or `}` in a plural translation
unless it is a real placeholder — `string.Format` will reject it.

## Formatting numbers, dates and currency

Write the format in the translation, not in the markup. That lets a translator move it:

```ini
PriceIs=Price: {0:C}
CurrentDateIs=Today: {0:d}
```

```xml
<TextBlock Text="{i18n:StringLocalizer Text='PriceIs', BindArg={Binding Price}}" />
```

Arguments are formatted with the active culture, so currency symbols and date order follow the language the
user picked.

## Live updates, and when to turn them off

`Live` defaults to on: the extension emits a binding that watches for culture changes. Set `Live=False` to
resolve once at load time — worth doing only for text that provably never needs to change, since the saving is
one binding.

**WPF has a wrinkle worth knowing.** It can only return a binding when the target is a `DependencyProperty`.
For a target that reports a plain CLR property — `Setter.Value` is the one people hit — it falls back to a
one-off string that will not follow later culture changes. Set `Live=True` explicitly to force a binding when
you know the target accepts one. MAUI always returns a binding and has no such case.

## `LocalizeConverter` is legacy

`LocalizeConverter` cannot re-translate: an `IValueConverter` has no source change to re-trigger it, so its
text goes stale as soon as the user switches language while everything around it updates. It is kept for
backwards compatibility and for MAUI's `Picker.ItemDisplayBinding`, which has no item template. Everywhere
else use `BindText`.

## Keys from `x:Static` (RESX)

```xml
<TextBlock Text="{i18n:StringLocalizer {x:Static locales:Strings.Title}, Namespace='Strings'}" />
```

This works because the extension reads the *source text* `Strings.Title` and takes `Title` as the key. It
therefore requires the RESX designer property to return the **key name**, not a translation. Hand-write the
wrapper to return `nameof(...)` if you want this pattern; a generated designer returns the English text and
will not resolve. Register the set under a short name so the markup stays readable:

```csharp
builder.FromResource<Locales.Strings>(new CultureInfo("en-US"), nameof(Locales.Strings));
```

## When a namespace is omitted

Without `Namespace`, the lookup means "find this key wherever it lives": it searches every registered set in
**registration order** and takes the first that carries the key. That is usually what you want. Supply
`Namespace` when two files deliberately define the same key and you need a specific one — for example a
`Title` in both `errors` and the default set.

## Practical notes

- **`&` in markup.** XAML needs `&amp;`; the extension unescapes it. A key containing an ampersand is
  unusual — prefer plain keys and keep punctuation in the translation.
- **Check both languages.** Untranslated keys render as the key text, which reads plausibly in English and
  hides the gap. Switch language and look before calling the screen done.
- **A raw key on screen** means the lookup missed. `barbatos-i18n-troubleshooting` maps that to its causes.
