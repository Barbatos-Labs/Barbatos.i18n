// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

using System.Globalization;
using System.Windows.Data;

namespace Barbatos.i18n.Wpf;

public sealed class StringLocalizerConverter : IMultiValueConverter
{
    public string Text { get; }
    public string? Namespace { get; }
    public string ProviderKey { get; }

    public StringLocalizerConverter(string text, string? textNamespace, string providerKey)
    {
        Text = text;
        Namespace = textNamespace;
        ProviderKey = providerKey;
    }

    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (Text is null)
        {
            return string.Empty;
        }

        CultureInfo currentCulture =
            WpfLocalization.GetProvider(ProviderKey)?.GetCulture()
            ?? LocalizationProviderFactory.GetInstance(ProviderKey)?.GetCulture()
            ?? CultureInfo.CurrentUICulture;

        string? selectedNamespace = Namespace?.ToLowerInvariant();

        LocalizationSet? localizationSet = WpfLocalization.GetProvider(ProviderKey)?.GetLocalizationSet(currentCulture, selectedNamespace)
            ?? LocalizationProviderFactory.GetInstance(ProviderKey)?.GetLocalizationSet(currentCulture, selectedNamespace);

        if (localizationSet is null)
        {
            return Text;
        }

        // Ignore UnsetValue to prevent formatting errors if bindings are not yet resolved
        var formatValues = values.Select(v => v == DependencyProperty.UnsetValue ? string.Empty : v).ToArray();

        return localizationSet.Format(culture, Text, formatValues);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
