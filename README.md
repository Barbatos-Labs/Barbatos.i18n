# Barbatos.i18n

![Barbatos.i18n logo](https://github.com/Barbatos-Labs/Barbatos.i18n/blob/main/build/nuget.png?raw=true)

### *The Progressive Internationalization Library for .NET*

**Add cross-platform multilingual support to MAUI, WPF, WebApi, WinForms, or CLI apps efficiently. JSON, INI, CSV, YAML, and RESX resources with Dependency Injection.**

[![NuGet](https://img.shields.io/nuget/v/Barbatos.i18n.svg)](https://www.nuget.org/packages/Barbatos.i18n)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Barbatos.i18n.svg)](https://www.nuget.org/packages/Barbatos.i18n)
[![GitHub stars](https://img.shields.io/github/stars/Barbatos-Labs/Barbatos.i18n?style=social)](https://github.com/Barbatos-Labs/Barbatos.i18n/stargazers)
[![License](https://img.shields.io/github/license/Barbatos-Labs/Barbatos.i18n)](https://github.com/Barbatos-Labs/Barbatos.i18n/tree/main/build/LICENSE)

---

## 📖 Documentation Menu

* **[Getting Started](#getting-started)**
  * [Introduction](#introduction)
  * [Quick Start](#quick-start)
* **[Essentials](#essentials)**
  * [Application Setup](#application-setup)
  * [Template Syntax (XAML) & Bindings](#template-syntax-xaml--bindings)
  * [Localization in Code-Behind (C#)](#localization-in-code-behind-c)
  * [List Rendering (DataTemplates & Converters)](#list-rendering-datatemplates--converters)
  * [Reactivity (Runtime Culture Change)](#reactivity-runtime-culture-change)
* **[Advanced Features](#advanced-features)**
  * [Pluralization](#pluralization)
  * [String Formatting & Culture](#string-formatting--culture)
  * [Namespaces](#namespaces)
  * [Multiple Providers](#multiple-providers)
* **[Ecosystem](#ecosystem)**
  * [Packages](#packages)
* **[API Reference](#api-reference)**
* **[Community](#community)**

---

## Getting Started

### Introduction

#### What is Barbatos.i18n?

Barbatos.i18n is an internationalization (i18n) library for building multi-language .NET applications. It builds on top of standard .NET localization concepts and provides a declarative, component-based model that helps you efficiently develop multi-lingual interfaces of any complexity.

Traditional .NET localization with resource files requires ceremony and scattered configuration. With **Barbatos.i18n**, localizations are registered once and consumed anywhere - including directly in XAML with advanced support for Pluralization, String Formatting, Namespaces, and Multiple Providers.

> **Prerequisites**
> 
> The rest of the documentation assumes basic familiarity with C#, .NET Dependency Injection, and XAML (for WPF).

### Quick Start

To add Barbatos.i18n to your project, install the core package via NuGet:

```powershell
dotnet add package Barbatos.i18n
```

Depending on your architecture, you might want to install additional packages:

```powershell
# Microsoft.Extensions.DependencyInjection integration
dotnet add package Barbatos.i18n.DependencyInjection

# Load translations from JSON, INI, or CSV files (YAML and RESX are built-in)
dotnet add package Barbatos.i18n.Json
dotnet add package Barbatos.i18n.Ini
dotnet add package Barbatos.i18n.Csv

# WPF markup extensions
dotnet add package Barbatos.i18n.Wpf

# MAUI markup extensions and integration
dotnet add package Barbatos.i18n.Maui
```

---

## Essentials

### Application Setup

How you register localizations depends on whether you are using a Generic Host (with Dependency Injection) or a traditional WPF `App.xaml.cs`. Barbatos supports loading data from multiple sources like **JSON, CSV, INI**, as well as built-in **YAML** and standard `.resx`.

#### Supported File Formats

> [!IMPORTANT]
> **Build Action Configuration**
> 
> No matter which file format you choose, you **must** set the file's **Build Action** to **`EmbeddedResource`** in your project's properties so that the library can read it at runtime.

Barbatos.i18n is designed to read multiple structures. Below are examples of how each file format should be structured so that the library can parse them correctly into `LocalizationKey` and `string` pairs.

##### 1. JSON (`Locales.en-US.json`)
The library supports two versions of JSON formats. You must specify the `version` key at the root.

**Version 2.0 (Recommended)** supports nested objects. The nested path is combined into a single key using dots (`.`).
```json
{
  "version": "2.0",
  "MessageTitle": "Hello from Code-Behind",
  "Errors": {
    "NetworkError": "Network failed to connect."
  }
}
```

**How to use these keys:**
Nested structures are flattened. You can use either dot (`.`) or colon (`:`) separators.
- **XAML:** `Text="{i18n:StringLocalizer Text='Errors.NetworkError'}"`
- **C#:** `localizer["Errors.NetworkError"]` *(see Code-Behind section)*

**Version 1.0** uses an explicit array of key-value pairs:
```json
{
  "version": "1.0",
  "strings": [
    {
      "name": "MessageTitle",
      "value": "Hello from Code-Behind"
    }
  ]
}
```

##### 2. INI (`Locales.en-US.ini`)
Sections in INI files can be used to group keys or act as namespaces.
```ini
[common]
MessageTitle=Hello from Code-Behind
GreetingWithName=Hello {0}

[errors]
NetworkError=Network failed to connect.
```

**How to use these keys:**
INI sections are flattened as prefixes. You can access them using dot or colon separators.
- **XAML:** `Text="{i18n:StringLocalizer Text='errors.NetworkError'}"`
- **C#:** `localizer["errors.NetworkError"]` *(see Code-Behind section)*

##### 3. CSV (`Locales.csv`)
CSV files support two modes depending on the header row. The first column is always the `Key`.

**Multi-Culture Mode:** Subsequent columns are culture codes (`en-US`, `vi-VN`). This is great for managing all languages in a single file!
```csv
Key,en-US,vi-VN
MessageTitle,Hello from Code-Behind,Xin chào từ Code-Behind
NetworkError,Network failed to connect.,Mất kết nối mạng.
```

**Single-Culture Mode:** The second column must be named `Value`. You need one CSV file per culture.
```csv
Key,Value
MessageTitle,Hello from Code-Behind
NetworkError,Network failed to connect.
```

**How to use these keys:**
CSV keys are accessed exactly as they appear in the `Key` column.
- **XAML:** `Text="{i18n:StringLocalizer Text='NetworkError'}"`
- **C#:** `localizer["NetworkError"]` *(see Code-Behind section)*

##### 4. YAML (`Locales.en-US.yaml`)
YAML provides a clean, highly readable syntax. Similar to JSON, nested nodes become part of the key.
```yaml
MessageTitle: Hello from Code-Behind
GreetingWithName: Hello {0}
Errors:
  NetworkError: Network failed to connect.
```

**How to use these keys:**
Like JSON, YAML nodes are flattened. Access nested keys using dots or colons.
- **XAML:** `Text="{i18n:StringLocalizer Text='Errors.NetworkError'}"`
- **C#:** `localizer["Errors.NetworkError"]` *(see Code-Behind section)*

##### 5. RESX (`Strings.en-US.resx`)
Standard Microsoft `.resx` files are supported. These files must include the standard `<xsd:schema>` and `<resheader>` elements, along with your `<data>` pairs.
```xml
<data name="MessageTitle" xml:space="preserve">
  <value>Hello from Code-Behind</value>
</data>
<data name="MessageContent" xml:space="preserve">
  <value>Welcome to Barbatos.i18n, {0}!</value>
</data>
```

**How to use these keys:**
Keys match the `name` attribute directly.
- **XAML:** `Text="{i18n:StringLocalizer Text='MessageContent', Arg='John'}"`
- **C#:** `localizer["MessageContent", "John"]` *(see Code-Behind section)*

#### 1. Setup in WPF (`App.xaml.cs`)

```csharp
using System.Windows;
using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Barbatos.i18n.DependencyInjection;
using Barbatos.i18n.Wpf;

public partial class App : Application
{
    public IServiceProvider ServiceProvider { get; private set; }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();

        // 1. Configure Options (Optional)
        services.ConfigureLocalizationOptions(options => 
        {

            options.FormatCultureBuilder = uiCulture => 
            {
                // Example: Customize specific formatting rules globally
                uiCulture.NumberFormat.NumberDecimalSeparator = ".";
                return uiCulture;
            };
        });

        // 2. Register Providers
        services.AddStringLocalizer(builder =>
        {
            // Load from YAML (Built-in)
            builder.FromYaml("Locales.Settings-en-US.yaml", new CultureInfo("en-US"));
            builder.FromYaml("Locales.Settings-vi-VN.yaml", new CultureInfo("vi-VN"));
            
            // Load from INI files
            builder.FromIni("Locales.en-US.ini", new CultureInfo("en-US"));
            builder.FromIni("Locales.vi-VN.ini", new CultureInfo("vi-VN"));

            // Load from JSON files
            builder.FromJson("Locales.Validation-en-US.json", new CultureInfo("en-US"));
            builder.FromJson("Locales.Validation-vi-VN.json", new CultureInfo("vi-VN"));

            // Load from CSV files (e.g. for errors)
            builder.FromCsv("Locales.Errors.csv");

            // Load from strongly-typed RESX files
            builder.FromResource<Locales.Strings>(new CultureInfo("en-US"));
        });

        ServiceProvider = services.BuildServiceProvider();

        // 2. Initialize WPF Localization and set default language
        ServiceProvider.UseWpfLocalization()
                       .SetLocalizationCulture(CultureInfo.CurrentUICulture);
    }
}
```

> [!NOTE]
> **Understanding `LocalizationOptions`**
> 
> The `options =>` block configures global settings. The most notable properties are:
> 
> **`FormatCultureBuilder` (Func)**
> - By default, changing the language **automatically updates both** `CultureInfo.CurrentUICulture` (translated strings) AND `CultureInfo.CurrentCulture` (affecting numbers, dates, currencies globally) to match the new language.
> - If you provide a `FormatCultureBuilder`, you can finely tune specific properties like `NumberFormat` before the formatting culture is applied:
> > ```csharp
> > options.FormatCultureBuilder = uiCulture => 
> > {
> >     uiCulture.NumberFormat.CurrencySymbol = "đ";
> >     return uiCulture;
> > };
> > // Or to DISABLE synchronizing the formatting culture entirely:
> > // options.FormatCultureBuilder = _ => CultureInfo.CurrentCulture;
> > ```

> [!IMPORTANT]
> **Initializing XAML and Setting the Default Language**
> 
> The initialization block above does two critical things:
> 1. **`UseWpfLocalization()`**: Initializes an internal bridge that allows XAML `{i18n:...}` markup extensions to communicate with your registered `ILocalizationProvider`. Without this, your XAML bindings will silently fail.
> 2. **`SetLocalizationCulture(...)`**: Tells the application which language to load and display at startup. Passing `CultureInfo.CurrentUICulture` automatically adapts your app to the user's operating system language. You can also force a specific language, e.g., `new CultureInfo("en-US")`.

#### 2. Setup in MAUI (`MauiProgram.cs`)

MAUI setup is very similar to WPF, except you use `UseStringLocalizer` directly on the `MauiAppBuilder` and `UseMauiLocalization` on the built `MauiApp`.

```csharp
using System.Globalization;
using Barbatos.i18n.Json;
using Barbatos.i18n.Maui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>();

        // 1. Configure Options (Optional)
        builder.Services.ConfigureLocalizationOptions(options => 
        {

            options.FormatCultureBuilder = uiCulture => 
            {
                // Example: Customize specific formatting rules globally
                uiCulture.NumberFormat.NumberDecimalSeparator = ".";
                return uiCulture;
            };
        });

        // 2. Register Providers on MauiAppBuilder
        builder.UseStringLocalizer(locBuilder =>
        {
            locBuilder.FromJson("Locales.Locales-en-US.json", new CultureInfo("en-US"));
            locBuilder.FromJson("Locales.Locales-vi-VN.json", new CultureInfo("vi-VN"));
        });

        var app = builder.Build();

        // 2. Initialize MAUI Localization and set default language
        app.UseMauiLocalization()
           .SetLocalizationCulture(CultureInfo.CurrentUICulture);

        return app;
    }
}
```

> [!IMPORTANT]
> The exact same initialization rules apply here: **`UseMauiLocalization()`** is required to bridge the providers to MAUI XAML `{i18n:...}` markup extensions, and **`SetLocalizationCulture(...)`** sets the default starting language.

### Template Syntax (XAML) & Bindings

Barbatos.i18n provides powerful XAML markup extensions (`{i18n:StringLocalizer}`) that allow you to declaratively bind localized strings to your UI. It supports passing both static arguments (`Arg`) and dynamic view-model properties (`BindArg`).

Add the i18n XML namespace to your Window/Page:
```xml
xmlns:i18n="http://schemas.barbatos.co/i18n/2026/xaml"
```

#### Simple Text & Static Arguments

For text that doesn't change based on data, use `Text` and `Arg`.

```xml
<!-- Simple text -->
<TextBlock Text="{i18n:StringLocalizer Text='Title'}" />

<!-- ToolTip or any other DependencyProperty -->
<Button Content="Hover Me" ToolTip="{i18n:StringLocalizer Text='ButtonTooltip'}" />

<!-- Static Argument -->
<TextBlock Text="{i18n:StringLocalizer Text='GreetingWithName', Arg='John'}" />
```

#### Dynamic Arguments (ViewModel Binding)

Often, you need to pass variables from your C# ViewModel into the localized string (e.g. `"Hello {0}"`). Use `BindArg` to achieve this seamlessly.

**ViewModel (C#):**
```csharp
public class HomeViewModel : ObservableObject
{
    [ObservableProperty]
    private string _userName = "John Doe";

    [ObservableProperty]
    private string _firstName = "John";

    [ObservableProperty]
    private string _lastName = "Smith";
}
```

**View (XAML):**
```xml
<!-- Single Bound Argument: "Hello {0}" -->
<TextBlock Text="{i18n:StringLocalizer Text='GreetingWithName', BindArg={Binding UserName}}" />

<!-- Multiple Bound Arguments: "Welcome {0} {1}" -->
<TextBlock Text="{i18n:StringLocalizer Text='GreetingWithFullName', BindArg={Binding FirstName}, BindArg2={Binding LastName}}" />
```

#### Pluralization (`PluralStringLocalizer`)

When translating items that depend on a count (e.g. "1 apple" vs "5 apples"), use the plural extension:

```xml
<!-- Static Count -->
<TextBlock Text="{i18n:PluralStringLocalizer Text='OneApple', PluralText='ManyApples', Count=5}" />

<!-- Dynamic Bound Count -->
<TextBlock Text="{i18n:PluralStringLocalizer Text='OneApple', PluralText='ManyApples', BindCount={Binding AppleCount}}" />
```

#### Custom Formatting (`StringFormat`)

You can apply standard .NET `StringFormat` directly within the extension:

```xml
<TextBlock Text="{i18n:StringLocalizer Text='GreetingWithName', BindArg={Binding UserName}, StringFormat='[ {0} ]'}" />
```

#### Namespaces & Multiple Providers

If you segmented your translations using namespaces or multiple provider keys during setup, you can access them specifically:

```xml
<!-- Accessing a specific Namespace -->
<TextBlock Text="{i18n:StringLocalizer Text='NetworkError', Namespace='errors'}" />

<!-- Accessing a specific Provider and Namespace -->
<TextBlock Text="{i18n:StringLocalizer Text='BonusMessage', ProviderKey='SecondaryProvider', Namespace='extra'}" />
```

#### DataTemplates & ItemsControl (`LocalizeConverter`)

In scenarios like `DataTemplate` where `MarkupExtension`s cannot accept direct bindings of the template's context (e.g. `Text="{Binding}"`), you must use the `LocalizeConverter`:

```xml
<Window.Resources>
    <i18n:LocalizeConverter x:Key="LocalizeConverter" />
</Window.Resources>

<ItemsControl ItemsSource="{Binding Features}">
    <ItemsControl.ItemTemplate>
        <DataTemplate>
            <!-- Translates the bound string automatically -->
            <TextBlock Text="{Binding Converter={StaticResource LocalizeConverter}}" />
        </DataTemplate>
    </ItemsControl.ItemTemplate>
</ItemsControl>
```

### Localization in Code-Behind (C#)

Sometimes you need to access localized strings directly from your C# code (e.g., displaying a `MessageBox`, logging, or generating reports). Barbatos offers two approaches depending on your needs.

#### Recommended: `ICompositeStringLocalizer` (Unified Access)

`ICompositeStringLocalizer` searches across **all** registered localization sets (JSON, YAML, INI, CSV, RESX) to find the requested key. You don't need to know which file a key lives in.

```csharp
using Barbatos.i18n.DependencyInjection;

public class HomeViewModel
{
    private readonly ICompositeStringLocalizer _localizer;

    public HomeViewModel(ICompositeStringLocalizer localizer)
    {
        _localizer = localizer;
    }

    [RelayCommand]
    private void ShowMessage()
    {
        // Keys are resolved from ANY registered source — RESX, JSON, YAML, INI, CSV
        string title = _localizer["MessageTitle"];         // might come from RESX
        string error = _localizer["NetworkError"];         // might come from JSON
        string message = _localizer["MessageContent", UserName]; // with placeholders

        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
```

You can also scope lookups to a specific resource type. Keys found in the scoped set take priority, but missing keys automatically fall back to all other sets:

```csharp
public class SettingsViewModel
{
    private readonly ICompositeStringLocalizer<Locales.Strings> _localizer;

    public SettingsViewModel(ICompositeStringLocalizer<Locales.Strings> localizer)
    {
        _localizer = localizer;
    }

    public string Title => _localizer["Title"];       // Looks in Locales.Strings first
    public string Error => _localizer["ServerError"]; // Falls back to other sets if not found
}
```

#### Traditional: `IStringLocalizer<TResource>` (Scoped to RESX)

If you only need strings from a specific `.resx` file, you can use the standard `IStringLocalizer<TResource>`:

```csharp
using Microsoft.Extensions.Localization;

public class HomeViewModel
{
    [RelayCommand]
    private void ShowMessage()
    {
        var localizer = serviceProvider.GetRequiredService<IStringLocalizer<Locales.Strings>>();

        string title = localizer["MessageTitle"];
        string message = localizer["MessageContent", UserName];

        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
```

### List Rendering (DataTemplates & Converters)

In scenarios where you render lists (like inside a `ComboBox` or `ListBox`), you cannot directly use Markup Extensions (`{i18n:...}`) inside a `DataTemplate` property binding. For this, Barbatos provides the `LocalizeConverter`.

**ViewModel (C#):**
```csharp
public class HomeViewModel
{
    // These strings correspond to keys in your JSON/YAML/INI file!
    public ObservableCollection<string> AvailableOptions { get; } = new()
    {
        "ComboBoxItem1",
        "ComboBoxItem2",
        "ComboBoxItem3"
    };
}
```

**View (XAML):**
```xml
<Page.Resources>
    <!-- 1. Declare the Converter -->
    <i18n:LocalizeConverter x:Key="LocalizeConverter" />
</Page.Resources>

<!-- 2. Bind the list to ItemsSource -->
<ComboBox ItemsSource="{Binding AvailableOptions}">
    <ComboBox.ItemTemplate>
        <DataTemplate>
            <!-- 3. Use the Converter to localize the raw string key from the ViewModel -->
            <TextBlock Text="{Binding Converter={StaticResource LocalizeConverter}}" />
        </DataTemplate>
    </ComboBox.ItemTemplate>
</ComboBox>
```

### Reactivity (Runtime Culture Change)

Barbatos.i18n **automatically updates your XAML bindings** when the language changes, much like Vue's reactivity system. You don't need to refresh the page or reload the window!

#### How to change the language

**Approach 1: Using the global Application extension (WPF)**
You can trigger a culture change directly from code-behind or anywhere in your app:

```csharp
// Changes the culture to Vietnamese. All XAML bindings update immediately!
System.Windows.Application.Current.SetLocalizationCulture(new CultureInfo("vi-VN"));
```

**Approach 2: Using Dependency Injection (`ILocalizationCultureManager`)**
If you strictly use MVVM, inject the manager into your ViewModel:

```csharp
public class SettingsViewModel
{
    private readonly ILocalizationCultureManager _cultureManager;

    public SettingsViewModel(ILocalizationCultureManager cultureManager)
    {
        _cultureManager = cultureManager;
    }

    public void SwitchToEnglish() 
    {
        // This notifies the internal provider, which updates all bindings automatically.
        _cultureManager.SetCulture("en-US");
    }
}
```

---

## Advanced Features

### Pluralization

For handling countable nouns, use `{i18n:PluralStringLocalizer}`. It automatically decides whether to use the singular or plural translation based on a count.

**ViewModel (C#):**
```csharp
[ObservableProperty]
private int _appleCount = 1;

[RelayCommand]
private void IncrementApples() => AppleCount++;
```

**View (XAML):**
```xml
<!-- PluralText='ManyApples' will be chosen if BindCount > 1 -->
<TextBlock Text="{i18n:PluralStringLocalizer Text='OneApple', PluralText='ManyApples', BindCount={Binding AppleCount}}" />
```

### String Formatting & Culture

You can format the localized string directly inside XAML. Furthermore, variables like Date, Time, and Currencies will be **automatically formatted according to the current Localization Culture** under the hood.

**ViewModel (C#):**
```csharp
[ObservableProperty]
private decimal _price = 1500000.50m; // E.g., translates to "$1,500,000.50" or "1.500.000,50 ₫"

[ObservableProperty]
private DateTime _currentDate = DateTime.Now;
```

**View (XAML):**
```xml
<!-- Using StringFormat inline to wrap the localized output -->
<TextBlock Text="{i18n:StringLocalizer Text='Greeting', StringFormat='==> {0} <=='}" />

<!-- Culture-Aware formatting (automatically adapts to en-US vs vi-VN) -->
<TextBlock Text="{i18n:StringLocalizer Text='PriceIs', BindArg={Binding Price}}" />
<TextBlock Text="{i18n:StringLocalizer Text='FullDateTimeIs', BindArg={Binding CurrentDate}}" />
```

### Namespaces

You can separate translations into specific namespaces (e.g., `errors`, `common`, `billing`) to avoid key collisions in large projects.

```xml
<!-- Fetch key from the 'errors' namespace -->
<TextBlock Text="{i18n:StringLocalizer Text='NetworkError', Namespace='errors'}" />
```

### Multiple Providers

You can configure and consume translations from multiple independent providers within the same application. 

**Registration (C#):**
```csharp
services.AddStringLocalizer("SecondaryProvider", builder =>
{
    builder.FromJson("Locales.Extra-en-US.json", new CultureInfo("en-US"));
});
```

**Usage (XAML):**
```xml
<!-- Fetch key using the specific ProviderKey -->
<TextBlock Text="{i18n:StringLocalizer Text='BonusMessage', ProviderKey='SecondaryProvider', Namespace='extra'}" />
```

---

## Ecosystem

### Packages

Barbatos is designed to be modular. Only install what you need.

| Package | Description |
|---------|-------------|
| [`Barbatos.i18n`](https://www.nuget.org/packages/Barbatos.i18n) | Core library - localization builder, provider, and YAML support |
| [`Barbatos.i18n.DependencyInjection`](https://www.nuget.org/packages/Barbatos.i18n.DependencyInjection) | `IServiceCollection` integration with `IStringLocalizer` |
| [`Barbatos.i18n.Json`](https://www.nuget.org/packages/Barbatos.i18n.Json) | Load translations from JSON files |
| [`Barbatos.i18n.Ini`](https://www.nuget.org/packages/Barbatos.i18n.Ini) | Load translations from INI files |
| [`Barbatos.i18n.Csv`](https://www.nuget.org/packages/Barbatos.i18n.Csv) | Load translations from CSV files |
| [`Barbatos.i18n.Wpf`](https://www.nuget.org/packages/Barbatos.i18n.Wpf) | WPF markup extensions (`StringLocalizer`, `PluralStringLocalizer`, `LocalizeConverter`) |
| [`Barbatos.i18n.Maui`](https://www.nuget.org/packages/Barbatos.i18n.Maui) | .NET MAUI markup extensions and integration |

---

## API Reference

The library contains a rich set of primitives for localization management (`Barbatos.i18n`), Dependency Injection integration (`Barbatos.i18n.DependencyInjection`), and XAML Data-Binding (`Barbatos.i18n.Wpf`).

Due to the extensive nature of the library's interfaces, classes, and properties, the full API Reference has been moved to a dedicated document modeled after Microsoft's official .NET documentation format.

👉 **[Read the Full API Reference](https://github.com/Barbatos-Labs/Barbatos.i18n/blob/main/API-REFERENCE.md)** 👈

In the full reference, you will find comprehensive documentation for:
- `LocalizationBuilder`, `LocalizationSet`, `LocalizationKey`
- `ILocalizationProvider`, `ILocalizationCultureManager`
- `StringLocalizerExtension`, `PluralStringLocalizerExtension`, `LocalizeConverter`
- `WpfLocalization` (Service Locator bridge)
- DI Registration Extension Methods

---

## Community

### Maintainers

- Pham The Hung ([@StHung](https://github.com/StHung))

### Support

For support, please open a [GitHub issue](https://github.com/Barbatos-Labs/Barbatos.i18n/issues/new). We welcome bug reports, feature requests, and questions.

### License

This project is licensed under the terms of the **MIT** open source license. Please refer to the [LICENSE](https://github.com/Barbatos-Labs/Barbatos.i18n/blob/main/LICENSE.md) file for the full terms.

You can use it in private and commercial projects. Keep in mind that you must include a copy of the license in your project.
