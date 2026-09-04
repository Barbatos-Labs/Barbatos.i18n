---
name: barbatos-i18n-resources
description: Author and register the translation files Barbatos.i18n loads - JSON, YAML, INI, CSV and RESX - including the EmbeddedResource requirement, the dot-notation resource paths, how a file's name decides its namespace, and how nested keys flatten. Use this whenever creating, editing, renaming or moving locale files in a project that uses Barbatos.i18n, whenever adding a new language, and whenever a locale file loads but its keys cannot be found or a "resource not found" error appears at startup.
---

# Barbatos.i18n locale files

Two rules cause most of the trouble, so they come first.

## 1. Locale files must be embedded resources

The library reads translations out of the assembly, never off disk. A file that is merely "Content" or
"Copy to output" will not be found.

```xml
<ItemGroup>
  <EmbeddedResource Include="Locales\Strings-en-US.json" />
  <EmbeddedResource Include="Locales\Strings-vi-VN.json" />
</ItemGroup>
```

### Separate the culture with a dash, not a dot

This one is worth getting right up front because the failure is baffling: the build succeeds, the file is
embedded, and loading it still throws "resource not found".

MSBuild infers a culture from a file named `<something>.<culture>.<ext>` and moves it into a **satellite
assembly**. The library reads the main assembly, so the file is simply not there.

| File name | MSBuild sees | Result |
|---|---|---|
| `Strings-en-US.json` | no culture — a dash is just a character | Stays in the main assembly |
| `en-US.json` | no culture — nothing before the culture to split on | Stays in the main assembly |
| `Strings.en-US.json` | culture `en-US` | **Moved to a satellite assembly, not found** |

Naming with a dash avoids the whole problem. If a dotted name is fixed for other reasons, opt out explicitly:

```xml
<EmbeddedResource Include="Locales\Strings.en-US.json">
  <LogicalName>MyApp.Locales.Strings.en-US.json</LogicalName>
  <Type>Non-Resx</Type>
  <WithCulture>false</WithCulture>
</EmbeddedResource>
```

RESX is the deliberate exception: `Strings.vi-VN.resx` *should* become a satellite, and `FromResource` reads
it through `ResourceManager`, which is built for exactly that.

## 2. The path is a resource name, not a file path

Paths use **dots**, matching the logical resource name MSBuild generates — folder separators become dots:

```csharp
builder.FromJson("Locales.Strings-en-US.json", new CultureInfo("en-US"));   // Locales/Strings-en-US.json
```

The assembly-name prefix is optional; `"MyApp.Locales.Strings-en-US.json"` works too. To load from another
assembly, pass it explicitly:

```csharp
builder.FromJson(typeof(SomeTypeInThatAssembly).Assembly, "Locales.Strings-en-US.json", culture);
```

If startup throws "Resource ... not found", check the `EmbeddedResource` entry first and the dots second.

## How a file's name becomes its namespace

Every set gets a name, used in XAML as `Namespace='...'`. It is derived from the file name: extension off,
folders off, and a trailing `-{culture}` removed.

| File | Namespace |
|---|---|
| `Locales/Translations-en-US.ini` | `translations` |
| `Locales/Errors-vi-VN.json` | `errors` |
| `Locales/en-US.ini` | `locales` — the folder names it, since nothing but the culture is left |
| `en-US.ini` (no folder) | the default namespace |

The folder fallback matters: naming files after nothing but their culture is a natural layout, and taking the
culture as the namespace would put each language in a *differently named* set, leaving no name that spans
them. With the fallback, `Namespace='locales'` addresses all three languages.

Two files that land in the same namespace **merge** rather than collide. When both define a key, the file
registered **first** wins — the same precedence a lookup applies. So registration order is meaningful; keep
the most specific file first.

## The formats

### JSON — nested, version 2

```json
{
  "version": "2.0",
  "Validation": {
    "Required": "This field is required.",
    "Email": "Invalid email format."
  }
}
```

Nesting flattens with dots: the keys above are `validation.required` and `validation.email`. In XAML that is
`Text='Validation.Required'`. The `version` property is the schema marker and is not a translation, so a
top-level key literally named `Version` cannot be used — nest it or rename it.

Version 1 is a flat array and still loads:

```json
{ "version": "1.0", "strings": [ { "name": "Greeting", "value": "Hello" } ] }
```

A malformed file raises `LocalizationBuilderException` naming the problem — catch that one type around
registration to report which locale file is broken.

### YAML — namespaces from headers

```yaml
Settings:
  Title: Application Settings
  Theme: Theme
```

Unlike every other format, a YAML file's namespaces come from its **headers**, not its file name: this
produces a `settings` set. Keys written at the root level, before any header, go to the default namespace, so
one file can contribute to several sets at once.

A line whose value ends in a colon is fine (`EnterName: Enter Name:`), but a key with an empty value reads as
a namespace header — give it a value, even an empty quoted string.

### INI — sections prefix keys

```ini
Greeting=Hello

[errors]
NetworkError=Network failed
```

That is `greeting` and `errors.networkerror`. An inline comment needs whitespace before its `;` or `#`, so
`Color=#FF0000` keeps its value; quote a value to protect one that starts with a comment character.

### CSV — one file, many languages

```csv
Key,en-US,vi-VN,ko-KR
NetworkError,Network connection failed.,Lỗi kết nối mạng.,네트워크 연결에 실패했습니다.
```

Register it without a culture — the columns supply them:

```csharp
builder.FromCsv("Locales.Errors.csv");
```

A single-culture file uses a literal `Value` header and takes a culture argument instead:

```csv
Key,Value
Greeting,Hello
```

```csharp
builder.FromCsv("Locales.Strings-en-US.csv", new CultureInfo("en-US"));
```

The two shapes are not interchangeable; each overload rejects the other's file with a message saying which one
to use. Values containing commas or quotes follow normal CSV quoting.

### RESX

```csharp
builder.FromResource<Locales.Strings>(new CultureInfo("en-US"), nameof(Locales.Strings));
```

The second argument sets a short namespace; without it the set is named after the full type name, which makes
for unpleasant XAML. Satellite assemblies resolve as usual, so `Strings.vi-VN.resx` is picked up for `vi-VN`.

If you want the `{x:Static}` pattern in XAML, hand-write the wrapper so each property returns `nameof(...)` —
a generated designer returns the translated text, which is not a key. See `barbatos-i18n-xaml`.

## Keys

Keys are normalized: `:` becomes `.`, and everything is lower-cased invariantly. `Header:Title`,
`header.TITLE` and `Header.Title` are one key. Pick a convention and let normalization absorb the rest.

Name keys after meaning, not appearance — `CheckoutButton` survives a redesign that `BlueButtonTopRight` does
not. Keys are also the fallback text shown when a translation is missing, so a readable key degrades better
than `msg_042`.

## Adding a language

1. Copy an existing file, rename the culture suffix, translate the values.
2. Add an `EmbeddedResource` entry.
3. Register it with the new `CultureInfo`.
4. Confirm `GetSupportedCultures()` lists it — that is what a language picker binds to.

Only the exact culture registered is matched, but lookups fall back through parents: a set registered for `en`
serves `en-GB` and `en-US` too. Registering the neutral culture is a good way to cover regions you have no
specific translations for.
