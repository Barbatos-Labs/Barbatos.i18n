// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

using System.Runtime.CompilerServices;

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
    /// <summary>
    /// Gets the localized value for the specified string, falling back to the name of the property the value
    /// came from (e.g. <c>TestResource.Test</c> resolves the "Test" entry).
    /// </summary>
    /// <param name="value">The key to look up.</param>
    /// <param name="expression">The caller's source text for <paramref name="value"/>, supplied by the compiler.</param>
    /// <remarks>
    /// The value that was actually passed wins. Guessing from the caller's property name first meant that
    /// <c>set[row.Status]</c>, with a Status of "Active", returned the translation of the key "Status" whenever
    /// the set happened to carry one - the wrong string, with nothing to indicate it. The property-name guess
    /// exists for generated RESX designers, whose properties return a translation rather than a key, so it only
    /// has to apply once the value itself has failed to resolve.
    /// </remarks>
    public string? this[string? value, [CallerArgumentExpression(nameof(value))] string expression = ""]
    {
        get
        {
            if (value is null)
            {
                return null;
            }

            if (this[new LocalizationKey(value)] is string direct)
            {
                return direct;
            }

            if (!string.IsNullOrWhiteSpace(expression))
            {
                string trimmed = expression.Trim();
                if (trimmed.Length > 0 && trimmed[0] is not ('"' or '\'' or '$' or '@') && trimmed.Contains('.'))
                {
                    string propertyName = trimmed[(trimmed.LastIndexOf('.') + 1)..].Trim();

                    return this[new LocalizationKey(propertyName)];
                }
            }

            return null;
        }
    }

    public string? this[LocalizationKey key]
    {
        get
        {
            if (Strings is IReadOnlyDictionary<LocalizationKey, string?> readOnlyDict)
            {
                return readOnlyDict.TryGetValue(key, out string? value) ? value : null;
            }
            if (Strings is IDictionary<LocalizationKey, string?> dict)
            {
                return dict.TryGetValue(key, out string? value) ? value : null;
            }

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