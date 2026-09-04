// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

namespace Barbatos.i18n.Wpf;

/// <summary>
/// A value converter that translates a dynamic string key into a localized string.
/// Useful for binding collections (e.g., ItemsSource) where the key is only known at runtime.
/// </summary>
public sealed class LocalizeConverter : IValueConverter
{
    /// <summary>
    /// The provider key for localization.
    /// </summary>
    public string ProviderKey { get; set; } = string.Empty;

    /// <summary>
    /// The namespace of the text to be localized.
    /// </summary>
    public string? Namespace { get; set; }

    /// <summary>
    /// Converts a localization key into a localized string.
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not string key || string.IsNullOrWhiteSpace(key))
        {
            return value ?? string.Empty;
        }

        LocalizationSet? localizationSet = LocalizationLookup.ResolveSet(ProviderKey, LocalizationLookup.NormalizeNamespace(Namespace));

        if (localizationSet is null)
        {
            return key;
        }

        // Fall back to the key itself so an untranslated entry is visible rather than blank.
        return localizationSet.Format(culture, key) ?? key;
    }

    /// <summary>
    /// Not supported.
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}
