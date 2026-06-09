// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

namespace Barbatos.i18n;

/// <summary>
/// Provides functionality to retrieve localization sets for specific cultures.
/// </summary>
public interface ILocalizationProvider
{
    /// <summary>
    /// Retrieves a set of localized strings for a specific culture />.
    /// </summary>
    /// <param name="cultureName">The culture for which the localized strings are provided.</param>
    /// <returns>The set of localized strings, or null if no such set exists.</returns>
    LocalizationSet? GetLocalizationSet(string cultureName);

    /// <summary>
    /// Retrieves a set of localized strings for a specific culture and name />.
    /// </summary>
    /// <param name="cultureName">The culture for which the localized strings are provided.</param>
    /// <param name="name">The base name of the resource.</param>
    /// <returns>The set of localized strings, or null if no such set exists.</returns>
    LocalizationSet? GetLocalizationSet(string cultureName, string name);

    /// <summary>
    /// Retrieves the localization set with the specified name for the specified culture.
    /// </summary>
    /// <param name="culture">The culture to get the localization set for.</param>
    /// <param name="name">The name of the localization set to get.</param>
    /// <returns>The localization set with the specified name for the specified culture, or null if no localization set is found.</returns>
    LocalizationSet? GetLocalizationSet(CultureInfo culture, string? name);

    /// <summary>
    /// Gets the current culture.
    /// </summary>
    /// <returns>The current culture.</returns>
    CultureInfo GetCulture();

    /// <summary>
    /// Sets the current culture.
    /// </summary>
    /// <param name="cultureInfo">The culture to set.</param>
    void SetCulture(CultureInfo cultureInfo);

    /// <summary>
    /// Retrieves all localization sets managed by this provider.
    /// </summary>
    /// <returns>A collection of all localization sets.</returns>
    IEnumerable<LocalizationSet> GetLocalizationSets();

    /// <summary>
    /// Retrieves all localization sets for the specified culture.
    /// </summary>
    /// <param name="culture">The culture to filter localization sets by.</param>
    /// <returns>A collection of localization sets matching the specified culture.</returns>
    IEnumerable<LocalizationSet> GetLocalizationSets(CultureInfo culture);
}
