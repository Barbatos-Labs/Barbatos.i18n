// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

namespace Barbatos.i18n.Json.Parsers;

internal sealed class Version2Parser : IJsonLocalizationParser
{
    public IEnumerable<KeyValuePair<LocalizationKey, string?>> Parse(string contents)
    {
        using JsonDocument document = JsonDocument.Parse(contents);
        Dictionary<LocalizationKey, string> localizedStrings = new();

        foreach (JsonProperty property in document.RootElement.EnumerateObject())
        {
            if (string.Equals(property.Name, "Version", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            FlattenJsonElement(property.Value, property.Name, localizedStrings);
        }

        return localizedStrings!;
    }

    private static void FlattenJsonElement(
        JsonElement element,
        string currentPath,
        Dictionary<LocalizationKey, string> localizedStrings)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (JsonProperty property in element.EnumerateObject())
            {
                string nextPath = $"{currentPath}.{property.Name}";
                FlattenJsonElement(property.Value, nextPath, localizedStrings);
            }
        }
        else if (element.ValueKind == JsonValueKind.String)
        {
            string? value = element.GetString();

            if (value is not null)
            {
                LocalizationKey key = new(currentPath);
                if (localizedStrings.ContainsKey(key))
                {
                    throw new LocalizationBuilderException(
                        $"The contents of the JSON file contains duplicate \"{currentPath}\" keys."
                    );
                }

                localizedStrings.Add(key, value);
            }
        }
    }
}
