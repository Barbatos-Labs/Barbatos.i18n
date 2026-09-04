// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

namespace Barbatos.i18n;

/// <summary>
/// Broadcasts localization changes so that presentation layers can refresh already-rendered translations.
/// </summary>
/// <remarks>
/// <para>
/// The core library resolves translations on demand and holds no reference to any UI element, so a culture
/// switch alone cannot repaint a view. Every <see cref="ILocalizationCultureManager"/> implementation shipped
/// with Barbatos.i18n raises <see cref="CultureChanged"/> after it has applied the new culture, which lets the
/// WPF and MAUI packages invalidate their XAML bindings without reloading the window or the page.
/// </para>
/// <para>
/// The event is static and therefore lives for the lifetime of the process. Subscribe from long-lived objects
/// only, or unsubscribe explicitly, otherwise the handler keeps its target alive.
/// </para>
/// </remarks>
public static class LocalizationNotifier
{
    /// <summary>
    /// Occurs after the localization culture has been changed.
    /// </summary>
    /// <remarks>
    /// Handlers are invoked on the thread that performed the culture change, which is not necessarily the UI
    /// thread. Presentation layers are responsible for marshalling to their own dispatcher.
    /// </remarks>
    public static event EventHandler<LocalizationChangedEventArgs>? CultureChanged;

    /// <summary>
    /// Raises the <see cref="CultureChanged"/> event.
    /// </summary>
    /// <param name="culture">The culture that localization switched to.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="culture"/> is null.</exception>
    public static void NotifyCultureChanged(CultureInfo culture) => NotifyCultureChanged(culture, culture);

    /// <summary>
    /// Raises the <see cref="CultureChanged"/> event with a separate formatting culture.
    /// </summary>
    /// <param name="culture">The culture that localization switched to.</param>
    /// <param name="formatCulture">The culture that dates, numbers and currency are formatted with.</param>
    /// <exception cref="ArgumentNullException">Thrown when either culture is null.</exception>
    public static void NotifyCultureChanged(CultureInfo culture, CultureInfo formatCulture)
    {
        if (culture is null)
        {
            throw new ArgumentNullException(nameof(culture));
        }

        if (formatCulture is null)
        {
            throw new ArgumentNullException(nameof(formatCulture));
        }

        CultureChanged?.Invoke(null, new LocalizationChangedEventArgs(culture, formatCulture));
    }
}
