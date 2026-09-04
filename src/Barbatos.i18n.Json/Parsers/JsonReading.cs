// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

namespace Barbatos.i18n.Json.Parsers;

/// <summary>
/// Reads localization files with <see cref="JsonDocument"/> rather than reflection-based serialization.
/// </summary>
/// <remarks>
/// The package is built with the trimming and AOT analyzers enabled, and
/// <see cref="JsonSerializer"/>'s reflection overloads are annotated RequiresUnreferencedCode and
/// RequiresDynamicCode. These files are a handful of known properties, so reading them directly keeps the
/// package genuinely trim-safe instead of merely suppressing the warnings.
/// </remarks>
internal static class JsonReading
{
    /// <summary>
    /// The parse options applied to every localization file.
    /// </summary>
    internal static readonly JsonDocumentOptions DocumentOptions = new() { AllowTrailingCommas = true };

    /// <summary>
    /// Parses a localization file, reporting a syntax error as a <see cref="LocalizationBuilderException"/>.
    /// </summary>
    /// <param name="contents">The JSON contents.</param>
    /// <returns>The parsed document, which the caller owns and must dispose.</returns>
    /// <exception cref="LocalizationBuilderException">Thrown when the contents are not valid JSON.</exception>
    /// <remarks>
    /// Every other failure in this package is reported as a <see cref="LocalizationBuilderException"/>, so a
    /// consumer that catches it to name the offending file would otherwise still be torn down by a raw
    /// <see cref="JsonException"/> when a locale file is merely mis-edited.
    /// </remarks>
    internal static JsonDocument ParseDocument(string contents)
    {
        try
        {
            return JsonDocument.Parse(contents, DocumentOptions);
        }
        catch (JsonException exception)
        {
            throw new LocalizationBuilderException(
                $"The JSON localization file is not valid JSON: {exception.Message}",
                exception
            );
        }
    }

    /// <summary>
    /// Finds a property by name, ignoring case as the previous serializer options did.
    /// </summary>
    /// <param name="element">The object to search.</param>
    /// <param name="propertyName">The property name to look for.</param>
    /// <param name="value">The property value when found.</param>
    /// <returns>True when the property exists; otherwise false.</returns>
    internal static bool TryFindProperty(JsonElement element, string propertyName, out JsonElement value)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (JsonProperty property in element.EnumerateObject())
            {
                if (string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase))
                {
                    value = property.Value;
                    return true;
                }
            }
        }

        value = default;
        return false;
    }

    /// <summary>
    /// Reads a string property, treating a missing property and an explicit null alike.
    /// </summary>
    /// <param name="element">The object to read from.</param>
    /// <param name="propertyName">The property name to read.</param>
    /// <returns>The string value, or null when absent or null.</returns>
    /// <exception cref="LocalizationBuilderException">Thrown when the property holds something other than a string.</exception>
    internal static string? ReadString(JsonElement element, string propertyName)
    {
        if (!TryFindProperty(element, propertyName, out JsonElement value))
        {
            return null;
        }

        return value.ValueKind switch
        {
            JsonValueKind.String => value.GetString(),
            JsonValueKind.Null => null,
            _ => throw new LocalizationBuilderException(
                $"The JSON file has a \"{propertyName}\" that is {value.ValueKind} where a string was expected."
            )
        };
    }
}
