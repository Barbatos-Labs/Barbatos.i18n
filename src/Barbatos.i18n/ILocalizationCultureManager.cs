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
}
