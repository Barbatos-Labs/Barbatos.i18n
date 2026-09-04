// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

namespace Barbatos.i18n.Json.Parsers;

internal sealed class Version1Parser : IJsonLocalizationParser
{
    public IEnumerable<KeyValuePair<LocalizationKey, string?>> Parse(string contents)
    {
        using JsonDocument document = JsonReading.ParseDocument(contents);

        if (!JsonReading.TryFindProperty(document.RootElement, "strings", out JsonElement strings)
            || strings.ValueKind != JsonValueKind.Array)
        {
            throw new LocalizationBuilderException(
                "The JSON file declares schema version 1 but has no \"strings\" array."
            );
        }

        Dictionary<LocalizationKey, string?> localizedStrings = new();

        foreach (JsonElement entry in strings.EnumerateArray())
        {
            string? name = JsonReading.ReadString(entry, "name");

            if (string.IsNullOrEmpty(name))
            {
                throw new LocalizationBuilderException(
                    "The contents of the JSON file contain an entry without a \"name\"."
                );
            }

            LocalizationKey key = new(name);

            if (localizedStrings.ContainsKey(key))
            {
                throw new LocalizationBuilderException(
                    $"The contents of the JSON file contains duplicate \"{name}\" keys."
                );
            }

            localizedStrings.Add(key, JsonReading.ReadString(entry, "value"));
        }

        return localizedStrings;
    }
}
