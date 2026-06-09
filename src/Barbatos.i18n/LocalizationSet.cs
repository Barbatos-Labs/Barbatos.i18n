// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

namespace Barbatos.i18n;

/// <summary>
/// Represents a set of localized strings for a specific culture.
/// </summary>
/// <param name="Name">The name of the localization set. This could be the name of the resource file or another identifier.</param>
/// <param name="Culture">The culture that the localized strings are for.</param>
/// <param name="Strings">The localized strings in this set.</param>
public record LocalizationSet(
    string? Name,
    CultureInfo Culture,
    IEnumerable<KeyValuePair<LocalizationKey, string?>> Strings
)
{
    public string? this[LocalizationKey key]
    {
        get
        {
            foreach (KeyValuePair<LocalizationKey, string?> localizationString in Strings)
            {
                if (localizationString.Key == key)
                {
                    return localizationString.Value;
                }
            }

            return null;
        }
    }

    public string? this[LocalizationKey key, params object[] arguments] => Format(key, arguments);

    public string? Format(LocalizationKey key, params object?[]? args) => Format(null, key, args);

    public string? Format(IFormatProvider? formatProvider, LocalizationKey key, params object?[]? args)
    {
        string? value = this[key];

        if (value is null)
        {
            return null;
        }

        if (args is null || args.Length == 0)
        {
            return value;
        }

        return string.Format(formatProvider ?? Culture, value, args);
    }
}