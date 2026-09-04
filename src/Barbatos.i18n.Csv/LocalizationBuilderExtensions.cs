// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

using System.Globalization;
using System.Reflection;
using Barbatos.i18n.Csv.Parsers;
using Barbatos.i18n.IO;

namespace Barbatos.i18n.Csv;

/// <summary>
/// Provides extension methods for the <see cref="LocalizationBuilder"/> class to support CSV configuration files.
/// </summary>
public static class LocalizationBuilderExtensions
{
    /// <summary>
    /// Adds localized strings from a single-culture CSV file in the calling assembly.
    /// </summary>
    /// <param name="builder">The builder to add the localized strings to.</param>
    /// <param name="path">The dot-notated resource path to the CSV file.</param>
    /// <param name="culture">The culture the file provides.</param>
    /// <returns>The builder, so calls can be chained.</returns>
    public static LocalizationBuilder FromCsv(
        this LocalizationBuilder builder,
        string path,
        CultureInfo culture
    )
    {
        return builder.FromCsv(Assembly.GetCallingAssembly(), path, culture);
    }

    /// <summary>
    /// Adds localized strings from a single-culture CSV file in the specified assembly.
    /// </summary>
    /// <param name="builder">The builder to add the localized strings to.</param>
    /// <param name="assembly">The assembly that contains the CSV file.</param>
    /// <param name="path">The dot-notated resource path to the CSV file.</param>
    /// <param name="culture">The culture the file provides.</param>
    /// <returns>The builder, so calls can be chained.</returns>
    /// <exception cref="ArgumentException">Thrown when the path is not a CSV file, or the file is multi-culture.</exception>
    /// <exception cref="LocalizationBuilderException">Thrown when the file cannot be found in the assembly.</exception>
    public static LocalizationBuilder FromCsv(
        this LocalizationBuilder builder,
        Assembly assembly,
        string path,
        CultureInfo culture
    )
    {
        if (!path.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                $"Parameter {nameof(path)} in {nameof(FromCsv)} must be path to the CSV file."
            );
        }

        string? contents = EmbeddedResourceReader.ReadToEnd(path, assembly);

        if (contents is null)
        {
            throw new LocalizationBuilderException(
                $"Resource {path} not found in assembly {assembly.FullName}."
            );
        }

        var parsedResults = CsvLocalizationParser.Parse(contents);
        
        if (!parsedResults.TryGetValue("", out var localizations))
        {
            throw new ArgumentException($"The CSV file {path} is formatted as a multi-culture file. Please use the FromCsv overload without the CultureInfo parameter.");
        }

        builder.AddLocalization(new LocalizationSet(LocalizationSetNaming.DeriveName(path, culture), culture, localizations));

        return builder;
    }

    /// <summary>
    /// Adds localized strings from single-culture CSV contents.
    /// </summary>
    /// <param name="builder">The builder to add the localized strings to.</param>
    /// <param name="name">The namespace to register the set under.</param>
    /// <param name="culture">The culture the contents provide.</param>
    /// <param name="contents">The CSV contents.</param>
    /// <returns>The builder, so calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the contents are null.</exception>
    /// <exception cref="ArgumentException">Thrown when the contents are a multi-culture file.</exception>
    public static LocalizationBuilder FromCsvString(
        this LocalizationBuilder builder,
        string name,
        CultureInfo culture,
        string? contents
    )
    {
        if (contents is null)
        {
            throw new ArgumentNullException(nameof(contents));
        }

        var parsedResults = CsvLocalizationParser.Parse(contents);
        if (!parsedResults.TryGetValue("", out var localizations))
        {
            throw new ArgumentException($"The CSV contents are formatted as a multi-culture file. Please use the FromCsvString overload without the CultureInfo parameter.");
        }

        builder.AddLocalization(name.ToLowerInvariant(), culture, localizations);

        return builder;
    }

    /// <summary>
    /// Adds localized strings from a multi-culture CSV file in the calling assembly, one column per culture.
    /// </summary>
    /// <param name="builder">The builder to add the localized strings to.</param>
    /// <param name="path">The dot-notated resource path to the CSV file.</param>
    /// <returns>The builder, so calls can be chained.</returns>
    public static LocalizationBuilder FromCsv(
        this LocalizationBuilder builder,
        string path
    )
    {
        return builder.FromCsv(Assembly.GetCallingAssembly(), path);
    }

    /// <summary>
    /// Adds localized strings from a multi-culture CSV file in the specified assembly, one column per culture.
    /// </summary>
    /// <param name="builder">The builder to add the localized strings to.</param>
    /// <param name="assembly">The assembly that contains the CSV file.</param>
    /// <param name="path">The dot-notated resource path to the CSV file.</param>
    /// <returns>The builder, so calls can be chained.</returns>
    /// <exception cref="ArgumentException">Thrown when the path is not a CSV file, or the file is single-culture.</exception>
    /// <exception cref="LocalizationBuilderException">Thrown when the file cannot be found in the assembly.</exception>
    public static LocalizationBuilder FromCsv(
        this LocalizationBuilder builder,
        Assembly assembly,
        string path
    )
    {
        if (!path.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                $"Parameter {nameof(path)} in {nameof(FromCsv)} must be path to the CSV file."
            );
        }

        string? contents = EmbeddedResourceReader.ReadToEnd(path, assembly);

        if (contents is null)
        {
            throw new LocalizationBuilderException(
                $"Resource {path} not found in assembly {assembly.FullName}."
            );
        }

        var parsedResults = CsvLocalizationParser.Parse(contents);
        
        if (parsedResults.ContainsKey(""))
        {
            throw new ArgumentException($"The CSV file {path} is formatted as a single-culture file. Please use the FromCsv overload with the CultureInfo parameter.");
        }

        // One file carries every culture, so the name is derived once, against the invariant culture: there is
        // no per-culture suffix to strip here.
        string? name = LocalizationSetNaming.DeriveName(path, CultureInfo.InvariantCulture);

        foreach (var kvp in parsedResults)
        {
            builder.AddLocalization(new LocalizationSet(name, new CultureInfo(kvp.Key), kvp.Value));
        }

        return builder;
    }

    /// <summary>
    /// Adds localized strings from multi-culture CSV contents, one column per culture.
    /// </summary>
    /// <param name="builder">The builder to add the localized strings to.</param>
    /// <param name="name">The namespace to register the sets under.</param>
    /// <param name="contents">The CSV contents.</param>
    /// <returns>The builder, so calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the contents are null.</exception>
    /// <exception cref="ArgumentException">Thrown when the contents are a single-culture file.</exception>
    public static LocalizationBuilder FromCsvString(
        this LocalizationBuilder builder,
        string name,
        string? contents
    )
    {
        if (contents is null)
        {
            throw new ArgumentNullException(nameof(contents));
        }

        var parsedResults = CsvLocalizationParser.Parse(contents);
        if (parsedResults.ContainsKey(""))
        {
            throw new ArgumentException($"The CSV contents are formatted as a single-culture file. Please use the FromCsvString overload with the CultureInfo parameter.");
        }

        foreach (var kvp in parsedResults)
        {
            CultureInfo culture = new CultureInfo(kvp.Key);
            builder.AddLocalization(name.ToLowerInvariant(), culture, kvp.Value);
        }

        return builder;
    }
}
