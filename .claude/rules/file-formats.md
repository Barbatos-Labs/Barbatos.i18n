---
paths:
  - "src/Barbatos.i18n.Json/**"
  - "src/Barbatos.i18n.Ini/**"
  - "src/Barbatos.i18n.Csv/**"
  - "src/Barbatos.i18n/Yaml/**"
  - "src/Barbatos.i18n/IO/**"
  - "tests/Barbatos.i18n.Json.UnitTests/**"
  - "tests/Barbatos.i18n.Ini.UnitTests/**"
  - "tests/Barbatos.i18n.Csv.UnitTests/**"
  - "tests/Barbatos.i18n.UnitTests/Yaml/**"
---

# File-format loaders

Every format package has the same shape, with `Barbatos.i18n.Ini` as the template. To add a whole new format,
follow the `barbatos-dev-add-file-format` skill; the invariants below apply to any change in this code.

- **The three entry points.** A `LocalizationBuilderExtensions` class with `From<Format>(path, culture)`,
  `From<Format>(assembly, path, culture)` and `From<Format>String(name, culture, contents)`. The calling-assembly
  overload captures `Assembly.GetCallingAssembly()` and forwards; keep it a one-line forwarder, since moving the
  call deeper changes which assembly is "calling".
- **Files are embedded resources.** Content is read by `EmbeddedResourceReader.ReadToEnd(path, assembly)`, so
  files must be `EmbeddedResource` and the path is dot-notation matching the logical resource name
  (`"Locales.Locales-en-US.json"`), not a file path. Folder separators are accepted and normalized; a leading
  assembly name is stripped only on a segment boundary.
- **The set name comes from `LocalizationSetNaming.DeriveName(path, culture)`** - call it, never re-derive it; the
  rule used to be copy-pasted into three packages. It is the file name minus extension, folders and the
  `-{culture}` suffix, lowercased, so `Translations-en-US.ini` becomes `translations`. When nothing but the culture
  is left, the **folder** names the set: `Locales.en-US.ini` becomes `locales`, so all languages share one
  namespace instead of `en-us`/`vi-vn`/`ko-kr`; with no folder either, it returns null - the default namespace.
  `LocalizationSetNamingTests` pins all of it. YAML is the exception: its namespaces come from its headers,
  root-level keys (column 0, before or after a header block) go to the default namespace, and a key under a header
  belongs to that namespace only - it used to be copied into the default namespace too, which made the C#
  localizers disagree with the XAML lookup (`YamlNamespaceTests`, `YamlNamespacePrecedenceTests`). A multi-culture
  CSV derives one name against the invariant culture.
- **Same name and culture merge** in `LocalizationBuilder.AddLocalization`; the first registered wins per key (see
  `core-lookup.md`).
- **Parsers return a `Dictionary`** typed as `IEnumerable<KeyValuePair<LocalizationKey, string?>>`, because
  `LocalizationSet`'s indexer only takes its O(1) path for dictionary-backed strings. A duplicate key inside one
  file throws. (`FromYaml` still hands over a projection; `LocalizationProvider` materializes it once at build.)
- **Failures throw `LocalizationBuilderException`**, never a raw parser exception - `JsonReading.ParseDocument`
  shows the wrapping. A wrong extension or the wrong CSV shape (single vs multi-culture) is an
  `ArgumentException`. Exception messages are user-facing and some are pinned by tests (`FromCsvErrorTests`) and
  quoted in `barbatos-i18n-troubleshooting`; change them deliberately.
- **Format specifics worth knowing.** JSON v2 nests and flattens with dots, and its top-level `version` property is
  the schema marker, not a translation (`ReadMajorVersion` accepts `"2.0"` and `2`); v1 is a flat `strings` array.
  INI sections prefix keys, and an inline comment needs whitespace before `;`/`#` so `#FF0000` survives. CSV is
  `Key,Value` (single culture, takes a culture argument) or `Key,en-US,vi-VN,...` (multi-culture, does not).
