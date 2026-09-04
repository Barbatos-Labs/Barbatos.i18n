// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

namespace Barbatos.i18n;

/// <summary>
/// Provides data for the <see cref="LocalizationNotifier.CultureChanged"/> event.
/// </summary>
public sealed class LocalizationChangedEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LocalizationChangedEventArgs"/> class where the same culture
    /// is used for lookups and for formatting.
    /// </summary>
    /// <param name="culture">The culture that localization switched to.</param>
    public LocalizationChangedEventArgs(CultureInfo culture)
        : this(culture, culture)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalizationChangedEventArgs"/> class.
    /// </summary>
    /// <param name="culture">The culture that localization switched to.</param>
    /// <param name="formatCulture">The culture that dates, numbers and currency are formatted with.</param>
    public LocalizationChangedEventArgs(CultureInfo culture, CultureInfo formatCulture)
    {
        Culture = culture;
        FormatCulture = formatCulture;
    }

    /// <summary>
    /// Gets the culture that localization switched to, used to select translations.
    /// </summary>
    public CultureInfo Culture { get; }

    /// <summary>
    /// Gets the culture that dates, numbers and currency are formatted with.
    /// </summary>
    /// <remarks>
    /// Equal to <see cref="Culture"/> unless <see cref="LocalizationOptions.FormatCultureBuilder"/> transformed
    /// it. Carried separately so a listener on another thread can apply both without reading ambient state,
    /// which is per-thread and therefore unreliable across a dispatcher hop.
    /// </remarks>
    public CultureInfo FormatCulture { get; }
}
