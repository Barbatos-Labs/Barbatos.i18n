---
name: barbatos-dev-add-file-format
description: Add support for a new translation file format to the Barbatos.i18n library (for example TOML, XLIFF, gettext PO, properties or XML) as its own NuGet package - project layout, the three builder entry points, a dictionary-backed parser, set naming, error handling, tests, solution and documentation updates. Use this whenever asked to add, support or load a new locale or translation file format in this repository, to create a new Barbatos.i18n.<Format> package, or to change how an existing loader (JSON, INI, CSV, YAML) reads files, names sets or reports errors.
---

# Adding a file format to Barbatos.i18n

Each format is a small package that turns a file into `LocalizationSet`s and hands them to the builder.
`Barbatos.i18n.Ini` is the template: copy its shape rather than inventing one, because consumers rely on every
format behaving the same way - same entry points, same naming, same errors.

The invariants are in `.claude/rules/file-formats.md` (it loads once you open a loader file); this skill is the
order of work.

## 0. Decide before writing code

- **Does it need a parser dependency?** A new NuGet dependency is a permanent cost for every consumer of the
  package and has to be approved by the repository owner first. Prefer a small hand-written parser for simple
  formats (INI and CSV have one) and ask before adding a package reference. Versions go in
  `Directory.Packages.props`, never in a `.csproj`, and the packages are trimmable and AOT-analyzed, so a
  reflection-based serializer will warn.
- **Does it belong in its own package?** Yes, unless it is as fundamental as YAML/RESX, which live in the core.

## 1. The package: `src/Barbatos.i18n.<Format>/`

Copy `src/Barbatos.i18n.Ini/Barbatos.i18n.Ini.csproj`, then change `PackageId`, `Description` and the format tag
in `CommonTags`. Keep `TargetFrameworks` as `net10.0;net9.0;net8.0` and the `ProjectReference` to the core. The
shared `Directory.Build.props` supplies versioning, packing, signing, nullable and XML docs.

`Parsers/<Format>LocalizationParser.cs` - a static `Parse(string contents)` that:

- returns `IEnumerable<KeyValuePair<LocalizationKey, string?>>` **backed by a `Dictionary`**, so lookups take
  `LocalizationSet`'s O(1) path;
- throws `LocalizationBuilderException` for malformed content and for a key duplicated within the file, naming the
  key - never let a raw parser exception escape (`JsonReading.ParseDocument` shows how to wrap one);
- flattens any nesting into dotted keys (`Section.Key`), the convention every other format uses.

`LocalizationBuilderExtensions.cs` in namespace `Barbatos.i18n.<Format>`, with exactly these three public
methods, each with XML docs:

```csharp
public static LocalizationBuilder From<Format>(this LocalizationBuilder builder, string path, CultureInfo culture)
    => builder.From<Format>(Assembly.GetCallingAssembly(), path, culture);   // keep it a one-line forwarder

public static LocalizationBuilder From<Format>(this LocalizationBuilder builder, Assembly assembly, string path, CultureInfo culture)
{
    // 1. reject a wrong extension with ArgumentException ("Parameter path in From<Format> must be path to the ... file.")
    // 2. string? contents = EmbeddedResourceReader.ReadToEnd(path, assembly);
    //    null -> LocalizationBuilderException($"Resource {path} not found in assembly {assembly.FullName}.")
    // 3. builder.AddLocalization(new LocalizationSet(LocalizationSetNaming.DeriveName(path, culture), culture, Parse(contents)));
}

public static LocalizationBuilder From<Format>String(this LocalizationBuilder builder, string name, CultureInfo culture, string? contents)
// ArgumentNullException for null contents; builder.AddLocalization(name.ToLowerInvariant(), culture, Parse(contents))
```

`GetCallingAssembly()` must stay in the public overload itself: moved into a helper, it would report the
library as the "calling" assembly. Name sets only through `LocalizationSetNaming.DeriveName` - the rule used to be
copy-pasted into three packages and drifted.

## 2. Tests: `tests/Barbatos.i18n.<Format>.UnitTests/`

Copy the Ini test project (`net8.0;net10.0`, xunit, AwesomeAssertions, `<Using Include="Xunit" />`), and embed
the sample files with `EmbeddedResource` entries, named with a **dash** before the culture
(`Translations-en-US.<ext>`) so MSBuild does not move them into a satellite assembly. Cover at least:

- a file in two cultures resolving its keys, nested keys flattened;
- the set name `DeriveName` gives (`Translations-en-US` -> `translations`; `Locales.en-US` -> `locales`);
- the `String` overload, and `ArgumentNullException` for null contents;
- wrong extension -> `ArgumentException`; missing resource -> `LocalizationBuilderException` with its message;
- malformed content and a duplicate key -> `LocalizationBuilderException`;
- the set is dictionary-backed (see `CsvDictionaryBackingTests`).

## 3. Wire it in

- `Barbatos.i18n.slnx`: add the package under `/src/` and the tests under `/tests/`, keeping alphabetical order.
- `Directory.Build.props`: add the format to `CommonTags` if it is not there.
- The publishing workflow pushes every `src/**/*.nupkg`, so the new package ships with the next release - make
  sure the repository owner knows that.

## 4. Document it

- `README.md` (Ecosystem / Packages, and wherever the other formats are listed) and `API-REFERENCE.md`.
- `.claude/skills/barbatos-i18n-resources/SKILL.md`: a section for the format with a minimal file and its
  registration call, verified by actually loading it; and `barbatos-i18n-setup`'s package table.
- The package table at the top of `CLAUDE.md`, and the `paths` of `.claude/rules/file-formats.md`.

## 5. Verify

Run the `barbatos-dev-verify` skill: the core suite, the new suite, and `--api-baseline origin/main`, which reports
the new package as not present at the baseline and every existing package unchanged.
