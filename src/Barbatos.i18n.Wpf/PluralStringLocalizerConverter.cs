// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

using System.Windows.Data;

namespace Barbatos.i18n.Wpf;

public sealed class PluralStringLocalizerConverter : IMultiValueConverter
{
    public string? Text { get; }
    public string? PluralText { get; }
    public string? Namespace { get; }
    public string ProviderKey { get; }

    public PluralStringLocalizerConverter(string? text, string? pluralText, string? textNamespace, string providerKey)
    {
        Text = text;
        PluralText = pluralText;
        Namespace = textNamespace;
        ProviderKey = providerKey;
    }

    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (Text is null && PluralText is null)
        {
            return string.Empty;
        }

        int count = 0;
        if (values.Length > 0 && values[0] != DependencyProperty.UnsetValue)
        {
            if (values[0] is int i) count = i;
            else if (values[0] is string s && int.TryParse(s, out int parsed)) count = parsed;
            else if (values[0] is not null) int.TryParse(values[0].ToString(), out count);
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
            return Text ?? string.Empty;
        }

        string localizedString;
        if (count > 1)
        {
            localizedString =
                localizationSet.Strings.FirstOrDefault(s => s.Key == PluralText).Value
                ?? PluralText
                ?? string.Empty;
        }
        else
        {
            localizedString =
                localizationSet.Strings.FirstOrDefault(s => s.Key == Text).Value
                ?? Text
                ?? string.Empty;
        }

        return string.Format(culture, localizedString, count);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
