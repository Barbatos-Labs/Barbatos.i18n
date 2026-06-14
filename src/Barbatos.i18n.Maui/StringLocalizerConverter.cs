// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

namespace Barbatos.i18n.Maui;

/// <summary>
/// Provides a multi value converter that localizes strings in XAML.
/// </summary>
public sealed class StringLocalizerConverter : IMultiValueConverter
{
    /// <summary>
    /// Gets or sets the text to be localized.
    /// </summary>
    public string? Text { get; }

    /// <summary>
    /// Gets or sets the namespace of the text to be localized.
    /// </summary>
    public string? Namespace { get; }

    /// <summary>
    /// Provider key.
    /// </summary>
    public string ProviderKey { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="StringLocalizerConverter"/> class.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <param name="textNamespace">The namespace.</param>
    /// <param name="providerKey">The provider key.</param>
    public StringLocalizerConverter(string? text, string? textNamespace, string providerKey)
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
            MauiLocalization.GetProvider(ProviderKey)?.GetCulture()
            ?? LocalizationProviderFactory.GetInstance(ProviderKey)?.GetCulture()
            ?? CultureInfo.CurrentUICulture;

        string? selectedNamespace = Namespace?.ToLowerInvariant();

        LocalizationSet? localizationSet = MauiLocalization.GetProvider(ProviderKey)?.GetLocalizationSet(currentCulture, selectedNamespace)
            ?? LocalizationProviderFactory.GetInstance(ProviderKey)?.GetLocalizationSet(currentCulture, selectedNamespace);

        if (localizationSet is null)
        {
            return StringLocalizerExtension.EscapeText(Text);
        }

        return localizationSet.Format(CultureInfo.CurrentCulture, (LocalizationKey)Text, values) ?? StringLocalizerExtension.EscapeText(Text);
    }

    /// <summary>
    /// Not supported.
    /// </summary>
    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
