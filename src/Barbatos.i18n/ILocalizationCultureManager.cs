// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

namespace Barbatos.i18n;

/// <summary>
/// Defines methods for managing localization settings.
/// </summary>
public interface ILocalizationCultureManager
{
    /// <summary>
    /// Gets the localization options.
    /// </summary>
    LocalizationOptions Options { get; }

    /// <summary>
    /// Sets the current culture.
    /// </summary>
    /// <param name="cultureName">The culture to set.</param>
    void SetCulture(string cultureName);

    /// <summary>
    /// Sets the current culture for localization.
    /// </summary>
    /// <param name="culture">The culture to set.</param>
    void SetCulture(CultureInfo culture);

    /// <summary>
    /// Gets the current culture used for localization.
    /// </summary>
    /// <returns>The current culture.</returns>
    CultureInfo GetCulture();

    /// <summary>
    /// Gets the cultures registered across the localization provider(s).
    /// </summary>
    /// <returns>The distinct set of registered cultures, or the result of <see cref="GetOperatingSystemCultures"/> if none are registered.</returns>
    IReadOnlyCollection<CultureInfo> GetSupportedCultures();

    /// <summary>
    /// Gets the cultures installed on the current operating system. Used as a fallback by <see cref="GetSupportedCultures"/> when no localization sets are registered.
    /// </summary>
    /// <returns>The specific cultures known to the OS globalization data, or a collection containing only <see cref="CultureInfo.CurrentCulture"/> if that data is unavailable (e.g. globalization-invariant mode on some mobile platforms).</returns>
    IReadOnlyCollection<CultureInfo> GetOperatingSystemCultures()
    {
        try
        {
            CultureInfo[] installedCultures = CultureInfo.GetCultures(CultureTypes.SpecificCultures);
            return installedCultures.Length > 0 ? installedCultures : [CultureInfo.CurrentCulture];
        }
        catch
        {
            return [CultureInfo.CurrentCulture];
        }
    }
}
