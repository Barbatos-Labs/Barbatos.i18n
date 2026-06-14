# Barbatos.i18n API Reference

This document provides a comprehensive reference for the Barbatos.i18n library, modeled after the official .NET API documentation.

## Namespaces

| Namespace | Description |
|-----------|-------------|
| **[`Barbatos.i18n`](#barbatosi18n-namespace)** | Contains the core localization interfaces, primitives, and builder configuration classes. |
| **[`Barbatos.i18n.DependencyInjection`](#barbatosi18ndependencyinjection-namespace)** | Contains integration components for Microsoft.Extensions.DependencyInjection. |
| **[`Barbatos.i18n.Wpf`](#barbatosi18nwpf-namespace)** | Contains XAML markup extensions and converters for Windows Presentation Foundation (WPF). |
| **[`Barbatos.i18n.Maui`](#barbatosi18nmaui-namespace)** | Contains XAML markup extensions, converters, and initialization bridges for .NET MAUI. |
| **[`Barbatos.i18n.Json`](#barbatosi18njson-namespace)** | Contains extensions to parse and load JSON files. |
| **[`Barbatos.i18n.Ini`](#barbatosi18nini-namespace)** | Contains extensions to parse and load INI files. |
| **[`Barbatos.i18n.Csv`](#barbatosi18ncsv-namespace)** | Contains extensions to parse and load CSV files. |

---

## `Barbatos.i18n` Namespace

Contains the core primitives used to fetch, manage, and build localization sets.

### Classes

| Class | Description |
|-------|-------------|
| [`LocalizationBuilder`](#localizationbuilder-class) | Configures and builds a set of localized strings for different cultures. |
| [`LocalizationSet`](#localizationset-class) | Represents an immutable collection of localized strings mapped to a specific culture. |
| [`LocalizationKey`](#localizationkey-struct) | Represents a normalized key (e.g. `header.title`) used for localization lookups. |
| [`LocalizationCultureManager`](#localizationculturemanager-class) | The default implementation of `ILocalizationCultureManager`. |
| [`LocalizationProvider`](#localizationprovider-class) | The default implementation of `ILocalizationProvider`. |

### Interfaces

| Interface | Description |
|-----------|-------------|
| [`ILocalizationProvider`](#ilocalizationprovider-interface) | Provides functionality to retrieve localization sets for specific cultures at runtime. |
| [`ILocalizationCultureManager`](#ilocalizationculturemanager-interface) | Defines methods for managing and switching the active culture at runtime. |

---

### `LocalizationBuilder` Class

Provides functionality to build a collection of localized strings for different cultures. It acts as a central registry that aggregates translations from various file formats (JSON, YAML, CSV, INI, RESX) before compiling them into a unified `LocalizationProvider`. Internally, it prevents duplicate registrations for the same culture and namespace using a `HashSet<LocalizationSet>`.

```csharp
public class LocalizationBuilder
```

#### Methods

- **`AddLocalization(LocalizationSet localization)`**
  Manually adds a predefined `LocalizationSet` to the builder. Throws an `InvalidOperationException` if a set with the exact same `Name` and `Culture` is already registered.

- **`FromResource<TResource>(CultureInfo culture)`**
  Extracts localized strings from an embedded `.resx` resource inside the assembly containing `TResource` and registers them for the given `culture`.

- **`FromResource(Assembly assembly, string baseName, CultureInfo culture)`**
  Extracts localized strings from an embedded `.resx` resource using the specified assembly and base name.

- **`FromYaml(string filePath, CultureInfo culture)`**
  Parses a YAML file from the filesystem and extracts its key-value pairs into a `LocalizationSet`. *(Built into the core package)*.

- **`SetCulture(CultureInfo culture)`**
  Sets the default fallback culture for the provider being built.

- **`Build()`**
  Instantiates and returns the final `ILocalizationProvider` using the registered localization sets and the selected fallback culture.

*(Note: Extension methods like `FromJson`, `FromIni`, `FromCsv` are provided via their respective NuGet packages.)*

---

### `LocalizationSet` Class (record)

Represents an **immutable** set of localized strings for a specific culture. Being an immutable `record` ensures that translations cannot be altered at runtime after they are loaded, making the library thread-safe.

```csharp
public record LocalizationSet(string? Name, CultureInfo Culture, IEnumerable<KeyValuePair<LocalizationKey, string?>> Strings)
```

#### Properties

- **`Name`** (`string?`): The name or namespace of the localization set. Used to group or isolate keys (e.g., `errors`, `common`).
- **`Culture`** (`CultureInfo`): The exact culture that the localized strings in this set belong to.
- **`Strings`** (`IEnumerable<KeyValuePair<LocalizationKey, string?>>`): The raw key-value pairs stored in this set.

#### Indexers

- **`this[LocalizationKey key]`**
  Retrieves the raw string value for the given key. Returns `null` if the key is not found within this specific set.
  
- **`this[LocalizationKey key, params object[] arguments]`**
  An extremely convenient indexer that retrieves the string value and immediately formats it using the set's inherent `Culture`. If the string is `Hello {0}` and `arguments` contains `"John"`, it evaluates to `"Hello John"`.

#### Methods

- **`Format(LocalizationKey key, params object?[]? args)`**
  Retrieves the string by key and applies standard `string.Format` using the set's `Culture`. If `args` is null or empty, it returns the raw unformatted string.
  
- **`Format(IFormatProvider? formatProvider, LocalizationKey key, params object?[]? args)`**
  Retrieves the string by key and applies `string.Format` using the provided custom `IFormatProvider`.

---

### `LocalizationKey` Struct

Represents a strictly normalized key used for all localization string lookups. It is designed as a `readonly struct` to eliminate unnecessary heap allocations during dictionary/array lookups, boosting performance.

```csharp
public readonly struct LocalizationKey : IEquatable<LocalizationKey>
```

#### Remarks

The struct enforces **automatic normalization** upon initialization:
1. It automatically converts all colons (`:`) to dots (`.`).
2. It transforms the entire string to lowercase (`ToLowerInvariant()`).

This means a key requested as `"Header:Title"` or `"header.TITLE"` will internally evaluate to the exact same `"header.title"`. 

The struct also implements **implicit conversions** from and to `string`. This allows developers to pass regular strings seamlessly to any method expecting a `LocalizationKey`.

```csharp
// Implicitly converted to LocalizationKey and normalized to "general.error"
string message = localizer["General:Error"]; 
```

---

### `LocalizationCultureManager` Class

The default implementation of `ILocalizationCultureManager`. It acts as the "orchestrator" for culture switching. It is responsible not only for notifying providers but also for managing the actual .NET OS Thread cultures.

```csharp
public class LocalizationCultureManager : ILocalizationCultureManager
```

#### Remarks

When `SetCulture(CultureInfo culture)` is called, the manager performs several critical tasks:
1. Notifies the underlying static `LocalizationProviderFactory` to update the translation lookup culture.
2. Updates `CultureInfo.CurrentUICulture` and `CultureInfo.DefaultThreadCurrentUICulture` so standard .NET UI resources are aligned.
3. Updates `CultureInfo.CurrentCulture` (affecting numbers, dates, and currencies) to align with the new language. If `LocalizationOptions.FormatCultureBuilder` is provided, the culture is passed through the builder for customization before applying.

---

### `LocalizationProvider` Class

The default implementation of `ILocalizationProvider`. It acts as the live runtime container that holds all the instantiated `LocalizationSet`s.

```csharp
public class LocalizationProvider : ILocalizationProvider
```

#### Remarks

Once the `LocalizationBuilder` finishes parsing files, it generates a `LocalizationProvider`. This provider is then queried by the XAML extensions (like `{i18n:StringLocalizer}`) or the `ICompositeStringLocalizer` in C# to resolve the most appropriate `LocalizationSet` for the active culture.

---

### `ILocalizationProvider` Interface

Provides functionality to retrieve localization sets for specific cultures.

```csharp
public interface ILocalizationProvider
```

#### Methods

- **`GetCulture()`**
  Gets the current active culture. Returns `CultureInfo`.
- **`SetCulture(CultureInfo cultureInfo)`**
  Sets the current active culture.
- **`GetLocalizationSet(string cultureName)`**
  Retrieves a set of localized strings for a specific culture name. Returns `LocalizationSet?`.
- **`GetLocalizationSet(string cultureName, string name)`**
  Retrieves a set of localized strings for a specific culture name and namespace.
- **`GetLocalizationSet(CultureInfo culture, string? name)`**
  Retrieves a set of localized strings for a specific `CultureInfo` object.

---

### `ILocalizationCultureManager` Interface

Defines methods for managing localization settings and propagating culture changes globally.

```csharp
public interface ILocalizationCultureManager
```

#### Properties

- **`Options`** (`LocalizationOptions`): Gets the global localization options (such as `FormatCultureBuilder`).

#### Methods

- **`SetCulture(string cultureName)`**
  Switches the active culture for all managed providers using the culture name.
- **`SetCulture(CultureInfo culture)`**
  Switches the active culture for all managed providers using a `CultureInfo` instance.
- **`GetCulture()`**
  Gets the currently active `CultureInfo`.

---

## `Barbatos.i18n.DependencyInjection` Namespace

Provides integration with `Microsoft.Extensions.DependencyInjection`.

### Classes

| Class | Description |
|-------|-------------|
| `ServiceCollectionExtensions` | Contains `IServiceCollection` extension methods. |
| `DependencyInjectionLocalizationBuilder` | The builder implementation used during DI registration. |

### Extension Methods

- **`ConfigureLocalizationOptions(this IServiceCollection services, Action<LocalizationOptions> configureOptions)`**
  Configures the global localization options.

- **`AddStringLocalizer(this IServiceCollection services, Action<LocalizationBuilder> builderAction)`**
  Registers the default localization provider and `IStringLocalizer` implementations into the DI container.

- **`AddStringLocalizer(this IServiceCollection services, string providerKey, Action<LocalizationBuilder> builderAction)`**
  Registers a secondary, named localization provider into the DI container. Use this to configure Multiple Providers.

---

## `Barbatos.i18n.Wpf` Namespace

Provides XAML elements to bind localized strings dynamically in WPF applications.

### Classes

| Class | Description |
|-------|-------------|
| [`WpfLocalization`](#wpflocalization-class) | Service Locator bridge for initializing DI within WPF XAML. |
| [`StringLocalizerExtension`](#stringlocalizerextension-class) | The core `MarkupExtension` (`{i18n:StringLocalizer}`) used in XAML. |
| [`PluralStringLocalizerExtension`](#pluralstringlocalizerextension-class) | A `MarkupExtension` (`{i18n:PluralStringLocalizer}`) for handling pluralization. |
| [`LocalizeConverter`](#localizeconverter-class) | An `IValueConverter` for translating bound keys inside `DataTemplate`s. |

### Extension Methods

- **`UseWpfLocalization(this IServiceProvider serviceProvider)`**
  Extracts the `IServiceProvider` and initializes `WpfLocalization.Initialize(...)` so that XAML Markup Extensions can resolve localizations at runtime. Returns an `ILocalizationCultureManager` to chain `SetLocalizationCulture`.

- **`SetLocalizationCulture(this IServiceProvider serviceProvider, CultureInfo culture)`**
  A helper extension on `IServiceProvider` that resolves the `ILocalizationCultureManager` and sets the culture.

---

### `WpfLocalization` Class

Because XAML `MarkupExtension`s are instantiated by the XAML parser using default constructors, they cannot use standard constructor injection. `WpfLocalization` bridges this gap by holding a static reference to the application's `IServiceProvider`.

```csharp
public static class WpfLocalization
```

#### Properties
- **`ServiceProvider`** (`IServiceProvider?`): Gets the globally registered service provider.

#### Methods
- **`Initialize(IServiceProvider serviceProvider)`**: Configures the static service provider.
- **`GetProvider(string? key = null)`**: Retrieves a specific `ILocalizationProvider` from the DI container using its key.

---

### `StringLocalizerExtension` Class

The primary way to retrieve localized strings in XAML. Inherits from `MarkupExtension`.

```xml
<TextBlock Text="{i18n:StringLocalizer Text='Key', BindArg={Binding Value}}" />
```

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Text` | `string` | **(Required)** The localization key to resolve. |
| `Namespace` | `string?` | The specific resource namespace to look in. |
| `ProviderKey` | `string?` | The name of the specific DI provider to query. |
| `StringFormat` | `string?` | A standard `.NET` string format string (e.g. `"Price: {0}"`). |
| `Arg` | `object?` | Static argument for formatting (index 0). |
| `Arg2` - `Arg5` | `object?` | Additional static arguments (indices 1 to 4). |
| `BindArg` | `BindingBase?` | Dynamic data-binding for formatting (index 0). |
| `BindArg2` - `BindArg5`| `BindingBase?` | Additional dynamic data-bindings (indices 1 to 4). |

#### Remarks
When `BindArg` properties are provided, the extension sets up a `MultiBinding` underneath to automatically refresh the string whenever the bound view model property changes or when the global language changes.

---

### `PluralStringLocalizerExtension` Class

Handles pluralization in XAML based on a bound count. Inherits from `StringLocalizerExtension`.

```xml
<TextBlock Text="{i18n:PluralStringLocalizer Text='Singular', PluralText='Plural', BindCount={Binding Count}}" />
```

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Text` | `string` | **(Required)** The singular localization key. |
| `PluralText` | `string` | **(Required)** The plural localization key. |
| `Count` | `int?` | A static integer determining plurality. |
| `BindCount` | `BindingBase?` | A dynamic data-binding resolving to an integer to determine plurality. |

#### Remarks
The extension evaluates the integer value. If `value > 1`, it fetches the translation associated with `PluralText`; otherwise, it uses `Text`.

---

### `LocalizeConverter` Class

An `IValueConverter` designed to translate text bound from an `ItemsSource`. It takes the incoming string (from the binding) and uses it as the localization key.

```xml
<TextBlock Text="{Binding KeyProperty, Converter={StaticResource LocalizeConverter}}" />
```

#### Properties
- **`Namespace`** (`string?`): The specific namespace to look in.
- **`ProviderKey`** (`string?`): The specific DI provider to query.

#### Remarks
Use this converter inside `DataTemplate`s where `MarkupExtension`s cannot accept direct property bindings from the template context.

---

## `Barbatos.i18n.Maui` Namespace

Provides XAML elements to bind localized strings dynamically in .NET MAUI applications. Its structure and XAML extensions are functionally identical to the `Barbatos.i18n.Wpf` namespace.

### Classes

| Class | Description |
|-------|-------------|
| `MauiLocalization` | Service Locator bridge for initializing DI within MAUI XAML. |
| `StringLocalizerExtension` | The core `IMarkupExtension` (`{i18n:StringLocalizer}`) used in MAUI XAML. |
| `PluralStringLocalizerExtension` | An `IMarkupExtension` (`{i18n:PluralStringLocalizer}`) for handling pluralization. |
| `LocalizeConverter` | An `IValueConverter` for translating bound keys inside `DataTemplate`s. |

### Extension Methods

- **`UseStringLocalizer(this MauiAppBuilder builder, Action<LocalizationBuilder> builderAction)`**
  Registers the localization provider into the `MauiAppBuilder`'s service collection.

- **`UseMauiLocalization(this MauiApp app)`**
  Extracts the `IServiceProvider` from the built `MauiApp` and initializes `MauiLocalization.Initialize(...)` so that MAUI XAML Markup Extensions can resolve localizations at runtime. Returns an `ILocalizationCultureManager` to chain `SetLocalizationCulture`.

### `MauiLocalization` Class

Functions identically to `WpfLocalization` but tailored for the MAUI application lifecycle.

---

## `Barbatos.i18n.Json` Namespace

Provides extensions to load JSON files into the localization builder.

### Classes
| Class | Description |
|-------|-------------|
| `LocalizationBuilderExtensions` | Contains `LocalizationBuilder` extension methods. |

### Extension Methods

- **`FromJson(this LocalizationBuilder builder, string path, CultureInfo culture)`**
  Loads localization data from a JSON file in the calling assembly.
  
- **`FromJson(this LocalizationBuilder builder, Assembly assembly, string path, CultureInfo culture)`**
  Loads localization data from a JSON file in the specified assembly.
  
- **`FromJsonString(this LocalizationBuilder builder, string jsonString, CultureInfo culture)`**
  Parses a raw JSON string into localization pairs.

- **`FromJsonString(this LocalizationBuilder builder, string jsonString, string? baseName, CultureInfo culture)`**
  Parses a raw JSON string into localization pairs with a specific base name namespace.

---

## `Barbatos.i18n.Ini` Namespace

Provides extensions to load INI configuration files into the localization builder.

### Classes
| Class | Description |
|-------|-------------|
| `LocalizationBuilderExtensions` | Contains `LocalizationBuilder` extension methods. |

### Extension Methods

- **`FromIni(this LocalizationBuilder builder, string path, CultureInfo culture)`**
  Adds localized strings from an INI file in the calling assembly.
  
- **`FromIni(this LocalizationBuilder builder, Assembly assembly, string path, CultureInfo culture)`**
  Adds localized strings from an INI file in the specified assembly.
  
- **`FromIniString(this LocalizationBuilder builder, string name, CultureInfo culture, string? contents)`**
  Adds localized strings from a raw INI string.

---

## `Barbatos.i18n.Csv` Namespace

Provides extensions to load CSV configuration files into the localization builder. Supports both single-culture and multi-culture CSV formats.

### Classes
| Class | Description |
|-------|-------------|
| `LocalizationBuilderExtensions` | Contains `LocalizationBuilder` extension methods. |

### Extension Methods

#### Single-Culture Loaders
Used when the CSV file targets exactly one culture.
- **`FromCsv(this LocalizationBuilder builder, string path, CultureInfo culture)`**
- **`FromCsv(this LocalizationBuilder builder, Assembly assembly, string path, CultureInfo culture)`**
- **`FromCsvString(this LocalizationBuilder builder, string name, CultureInfo culture, string? contents)`**

#### Multi-Culture Loaders
Used when the CSV file contains multiple languages (e.g. columns for `en-US`, `vi-VN`).
- **`FromCsv(this LocalizationBuilder builder, string path)`**
- **`FromCsv(this LocalizationBuilder builder, Assembly assembly, string path)`**
- **`FromCsvString(this LocalizationBuilder builder, string name, string? contents)`**
