// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

namespace Barbatos.i18n.Maui;

/// <summary>
/// A value converter that translates a dynamic string key into a localized string.
/// Useful for binding collections (e.g., ItemsSource) where the key is only known at runtime.
/// </summary>
public class LocalizeConverter : IValueConverter
{
    /// <summary>
    /// The namespace of the text to be localized.
    /// </summary>
    public string? Namespace { get; set; }

    /// <summary>
    /// The provider key for localization.
    /// </summary>
    public string ProviderKey { get; set; } = string.Empty;

    /// <summary>
    /// Converts a localization key into a localized string.
    /// </summary>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string stringValue || string.IsNullOrEmpty(stringValue))
        {
            return value;
        }

        // Fall back to the key itself so an untranslated entry is visible rather than blank.
        return LocalizationLookup.ResolveValue(ProviderKey, LocalizationLookup.NormalizeNamespace(Namespace), stringValue) ?? stringValue;
    }

    /// <summary>
    /// Not supported.
    /// </summary>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
