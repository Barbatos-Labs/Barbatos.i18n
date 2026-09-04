// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

namespace Barbatos.i18n;

/// <summary>
/// Decides whether a count should be rendered with the plural form of a translation.
/// </summary>
/// <remarks>
/// <para>
/// This is a two-form model: a translation supplies one singular and one plural string. That covers English,
/// Vietnamese, German, Spanish, French and most of Western Europe. It cannot express languages with three or
/// more forms - Russian, Polish, Arabic, Czech - which need a full CLDR plural-rule table; for those, select
/// the key in the view model and pass it through <c>BindText</c> instead.
/// </para>
/// <para>
/// Only zero is genuinely contentious between languages, which is why it is the only case decided by culture.
/// </para>
/// </remarks>
public static class PluralRules
{
    /// <summary>
    /// The languages whose CLDR "one" category includes zero, so that zero reads as singular.
    /// </summary>
    /// <remarks>
    /// French is the well-known case: "0 jour" is singular, where English says "0 days". Armenian and Kabyle
    /// behave the same way. Portuguese is split - Brazilian Portuguese groups zero with one, European
    /// Portuguese does not - so it is matched on the full culture name rather than the language.
    /// </remarks>
    private static readonly string[] ZeroIsSingularLanguages = ["fr", "hy", "kab"];

    /// <summary>
    /// Determines whether the plural form applies to a count, for the current UI language.
    /// </summary>
    /// <param name="count">The count being rendered.</param>
    /// <returns><see langword="true"/> when the plural form should be used.</returns>
    public static bool IsPlural(int count) => IsPlural(count, CultureInfo.CurrentUICulture);

    /// <summary>
    /// Determines whether the plural form applies to a count in a given language.
    /// </summary>
    /// <param name="count">The count being rendered.</param>
    /// <param name="culture">The language the translation is written in.</param>
    /// <returns><see langword="true"/> when the plural form should be used.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="culture"/> is null.</exception>
    /// <remarks>
    /// One is singular everywhere this model applies, and any other non-zero count is plural. Zero follows the
    /// language: plural in English and most others, singular in French and its few companions. Selecting on
    /// <c>count &gt; 1</c> instead - the French rule applied to everything - is what made an English UI read
    /// "0 item left".
    /// </remarks>
    public static bool IsPlural(int count, CultureInfo culture)
    {
        if (culture is null)
        {
            throw new ArgumentNullException(nameof(culture));
        }

        if (count == 1)
        {
            return false;
        }

        if (count != 0)
        {
            return true;
        }

        return !ZeroIsSingular(culture);
    }

    /// <summary>
    /// Determines whether a language groups zero with one.
    /// </summary>
    /// <param name="culture">The language to test.</param>
    /// <returns><see langword="true"/> when zero reads as singular.</returns>
    private static bool ZeroIsSingular(CultureInfo culture)
    {
        if (string.Equals(culture.Name, "pt-BR", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        string language = culture.TwoLetterISOLanguageName;

        foreach (string candidate in ZeroIsSingularLanguages)
        {
            if (string.Equals(language, candidate, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
