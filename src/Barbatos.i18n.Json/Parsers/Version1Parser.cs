// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

using Barbatos.i18n.Json.Models.v1;
using System.Text.Json;

namespace Barbatos.i18n.Json.Parsers;

internal sealed class Version1Parser : IJsonLocalizationParser
{
    public IEnumerable<KeyValuePair<LocalizationKey, string?>> Parse(string contents)
    {
        TranslationFile? translationFile = JsonSerializer.Deserialize<TranslationFile>(
            contents,
            LocalizationBuilderExtensions.DefaultJsonSerializerOptions
        );

        if (translationFile is null)
        {
            throw new LocalizationBuilderException("Unable to extract data from json file.");
        }

        if (translationFile.Strings is null)
        {
            throw new LocalizationBuilderException(
                "The JSON file declares schema version 1 but has no \"strings\" array."
            );
        }

        Dictionary<LocalizationKey, string> localizedStrings = new();

        foreach (TranslationEntity localizedString in translationFile.Strings)
        {
            if (localizedString.Name is null)
            {
                throw new LocalizationBuilderException(
                    "The contents of the JSON file contain an entry without a \"name\"."
                );
            }

            LocalizationKey key = new(localizedString.Name);
            if (localizedStrings.ContainsKey(key))
            {
                throw new LocalizationBuilderException(
                    $"The contents of the JSON file contains duplicate \"{localizedString.Name}\" keys."
                );
            }

            localizedStrings.Add(key, localizedString.Value);
        }

        return localizedStrings!;
    }
}
