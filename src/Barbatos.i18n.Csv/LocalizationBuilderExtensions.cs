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
    public static LocalizationBuilder FromCsv(
        this LocalizationBuilder builder,
        string path,
        CultureInfo culture
    )
    {
        return builder.FromCsv(Assembly.GetCallingAssembly(), path, culture);
    }

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

        string fileName = path;
        int lastDotIndex = fileName.LastIndexOf('.');
        if (lastDotIndex > 0)
        {
            fileName = fileName.Substring(0, lastDotIndex);
            int lastDotBeforeExtension = fileName.LastIndexOf('.');
            if (lastDotBeforeExtension >= 0)
            {
                fileName = fileName.Substring(lastDotBeforeExtension + 1);
            }
        }
        
        string name = fileName;
        int cultureIndex = name.IndexOf("-" + culture.Name, StringComparison.OrdinalIgnoreCase);
        if (cultureIndex > 0)
        {
            name = name.Substring(0, cultureIndex);
        }

        builder.AddLocalization(name.ToLowerInvariant(), culture, localizations);

        return builder;
    }

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

    public static LocalizationBuilder FromCsv(
        this LocalizationBuilder builder,
        string path
    )
    {
        return builder.FromCsv(Assembly.GetCallingAssembly(), path);
    }

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

        string fileName = path;
        int lastDotIndex = fileName.LastIndexOf('.');
        if (lastDotIndex > 0)
        {
            fileName = fileName.Substring(0, lastDotIndex);
            int lastDotBeforeExtension = fileName.LastIndexOf('.');
            if (lastDotBeforeExtension >= 0)
            {
                fileName = fileName.Substring(lastDotBeforeExtension + 1);
            }
        }
        
        string name = fileName.ToLowerInvariant();

        foreach (var kvp in parsedResults)
        {
            CultureInfo culture = new CultureInfo(kvp.Key);
            builder.AddLocalization(name, culture, kvp.Value);
        }

        return builder;
    }

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
