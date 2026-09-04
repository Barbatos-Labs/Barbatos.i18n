// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

namespace Barbatos.i18n;

/// <summary>
/// Provides data for the <see cref="LocalizationNotifier.CultureChanged"/> event.
/// </summary>
/// <param name="culture">The culture that localization switched to.</param>
public sealed class LocalizationChangedEventArgs(CultureInfo culture) : EventArgs
{
    /// <summary>
    /// Gets the culture that localization switched to.
    /// </summary>
    public CultureInfo Culture { get; } = culture;
}
