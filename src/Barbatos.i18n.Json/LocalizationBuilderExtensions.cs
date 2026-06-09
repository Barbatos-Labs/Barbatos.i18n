// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

using Barbatos.i18n.IO;
using Barbatos.i18n.Json.Converters;
using Barbatos.i18n.Json.Models;
using Barbatos.i18n.Json.Parsers;

namespace Barbatos.i18n.Json;

/// <summary>
/// Provides extension methods for the <see cref="LocalizationBuilder"/> class.
/// </summary>
public static class LocalizationBuilderExtensions
{
    internal static readonly JsonSerializerOptions DefaultJsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true,
        Converters = { new TranslationsContainerConverter() },
    };

    public static LocalizationBuilder FromJsonString(
        this LocalizationBuilder builder,
        string jsonString,
        CultureInfo culture
    )
    {
        return builder.FromJsonString(jsonString, default, culture);
    }

    public static LocalizationBuilder FromJsonString(
        this LocalizationBuilder builder,
        string jsonString,
        string? baseName,
        CultureInfo culture
    )
    {
        builder.AddLocalization(
            new LocalizationSet(baseName, culture, ComputeLocalizationPairs(jsonString))
        );

        return builder;
    }

    /// <summary>
    /// Loads localization data from a JSON file in the calling assembly.
    /// </summary>
    /// <param name="builder">The <see cref="LocalizationBuilder"/> to add the localization data to.</param>
    /// <param name="path">The path to the JSON file.</param>
    /// <param name="culture">The culture of the localization data.</param>
    /// <returns>The updated <see cref="LocalizationBuilder"/>.</returns>
    public static LocalizationBuilder FromJson(
        this LocalizationBuilder builder,
        string path,
        CultureInfo culture
    )
    {
        return builder.FromJson(Assembly.GetCallingAssembly(), path, culture);
    }

    /// <summary>
    /// Loads localization data from a JSON file in the specified assembly.
    /// </summary>
    /// <param name="builder">The <see cref="LocalizationBuilder"/> to add the localization data to.</param>
    /// <param name="assembly">The assembly that contains the JSON file.</param>
    /// <param name="path">The path to the JSON file.</param>
    /// <param name="culture">The culture of the localization data.</param>
    /// <returns>The updated <see cref="LocalizationBuilder"/>.</returns>
    public static LocalizationBuilder FromJson(
        this LocalizationBuilder builder,
        Assembly assembly,
        string path,
        CultureInfo culture
    )
    {
        if (!path.EndsWith(".json"))
        {
            throw new ArgumentException(
                $"Parameter {nameof(path)} in {nameof(FromJson)} must be path to the JSON file."
            );
        }

        string? contents = EmbeddedResourceReader.ReadToEnd(path, assembly);
        if (contents is null)
        {
            throw new LocalizationBuilderException($"Could not find the JSON localization resource: {path}");
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

        builder.AddLocalization(
            new LocalizationSet(
                name.ToLowerInvariant(),
                culture,
                ComputeLocalizationPairs(contents)
            )
        );

        return builder;
    }

    private static IEnumerable<KeyValuePair<LocalizationKey, string?>> ComputeLocalizationPairs(
        string? contents
    )
    {
        if (contents is null)
        {
            throw new ArgumentNullException(nameof(contents));
        }

        Version schemaVersion = new(
            JsonSerializer
                .Deserialize<ITranslationsContainer>(contents, DefaultJsonSerializerOptions)
                ?.Version
                ?? "1.0.0"
        );

        IJsonLocalizationParser parser = JsonLocalizationParserFactory.Create(schemaVersion.Major);
        return parser.Parse(contents);
    }
}
