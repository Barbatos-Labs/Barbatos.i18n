// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

using Barbatos.i18n.IO;
using Barbatos.i18n.Json.Parsers;

namespace Barbatos.i18n.Json;

/// <summary>
/// Provides extension methods for the <see cref="LocalizationBuilder"/> class.
/// </summary>
public static class LocalizationBuilderExtensions
{
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

        builder.AddLocalization(
            new LocalizationSet(
                LocalizationSetNaming.DeriveName(path, culture),
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

        IJsonLocalizationParser parser = JsonLocalizationParserFactory.Create(ReadMajorVersion(ReadVersion(contents)));
        return parser.Parse(contents);
    }

    /// <summary>
    /// Reads the file's declared schema version.
    /// </summary>
    /// <param name="contents">The JSON contents.</param>
    /// <returns>The version as written in the file, or "1.0.0" when the file declares none.</returns>
    /// <exception cref="LocalizationBuilderException">Thrown when the root of the file is not an object.</exception>
    private static string ReadVersion(string contents)
    {
        using JsonDocument document = JsonReading.ParseDocument(contents);

        if (document.RootElement.ValueKind != JsonValueKind.Object)
        {
            throw new LocalizationBuilderException(
                $"The JSON localization file must have an object at its root, but it starts with {document.RootElement.ValueKind}."
            );
        }

        if (!JsonReading.TryFindProperty(document.RootElement, "version", out JsonElement version))
        {
            return "1.0.0";
        }

        // A numeric version is accepted as readily as a quoted one; ReadMajorVersion validates the spelling.
        return version.ValueKind == JsonValueKind.String
            ? version.GetString() ?? "1.0.0"
            : version.ToString();
    }

    /// <summary>
    /// Reads the major schema version from the value of the file's <c>version</c> property.
    /// </summary>
    /// <param name="version">The version as it was written in the file.</param>
    /// <returns>The major version number.</returns>
    /// <exception cref="LocalizationBuilderException">Thrown when the value is not a version number.</exception>
    /// <remarks>
    /// Both "2.0" and a bare "2" are accepted; only the major component decides which parser runs. Feeding the
    /// raw value to <see cref="Version"/> used to throw a bare ArgumentException that named neither the file
    /// nor the offending value.
    /// </remarks>
    private static int ReadMajorVersion(string version)
    {
        if (Version.TryParse(version, out Version? parsed))
        {
            return parsed.Major;
        }

        if (int.TryParse(version, NumberStyles.Integer, CultureInfo.InvariantCulture, out int major))
        {
            return major;
        }

        throw new LocalizationBuilderException(
            $"The JSON file declares an unreadable schema version \"{version}\". Expected a value such as \"1.0\" or \"2.0\"."
        );
    }
}
