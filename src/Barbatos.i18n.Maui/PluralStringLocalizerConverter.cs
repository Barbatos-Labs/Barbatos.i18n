// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

namespace Barbatos.i18n.Maui;

/// <summary>
/// Provides a multi value converter that localizes strings in XAML.
/// </summary>
public sealed class PluralStringLocalizerConverter : IMultiValueConverter
{
    /// <summary>
    /// Gets the text.
    /// </summary>
    public string? Text { get; }

    /// <summary>
    /// Gets the plural text.
    /// </summary>
    public string? PluralText { get; }

    /// <summary>
    /// Gets the namespace.
    /// </summary>
    public string? Namespace { get; }

    /// <summary>
    /// Gets the provider key.
    /// </summary>
    public string ProviderKey { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PluralStringLocalizerConverter"/> class.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <param name="pluralText">The plural text.</param>
    /// <param name="textNamespace">The namespace.</param>
    /// <param name="providerKey">The provider key.</param>
    public PluralStringLocalizerConverter(string? text, string? pluralText, string? textNamespace, string providerKey)
    {
        Text = text;
        PluralText = pluralText;
        Namespace = textNamespace;
        ProviderKey = providerKey;
    }

    /// <summary>
    /// Converts a value.
    /// </summary>
    /// <param name="values">The array of values that the source bindings in the <see cref="MultiBinding"/> produces.</param>
    /// <param name="targetType">The type of the binding target property.</param>
    /// <param name="parameter">The converter parameter to use.</param>
    /// <param name="culture">The culture to use in the converter.</param>
    /// <returns>A converted value. If the method returns null, the valid null value is used.</returns>
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (string.IsNullOrEmpty(Text) && string.IsNullOrEmpty(PluralText))
        {
            return string.Empty;
        }

        int count = 0;
        if (values.Length > 0 && values[0] is int countValue)
        {
            count = countValue;
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
            return count > 1 ? EscapeText(PluralText) ?? string.Empty : EscapeText(Text) ?? string.Empty;
        }

        string localizedString;

        if (count > 1)
        {
            var keyStr = PluralText ?? string.Empty;
            localizedString =
                localizationSet.Strings.FirstOrDefault(s => s.Key.ToString() == keyStr).Value
                ?? EscapeText(PluralText)
                ?? string.Empty;
        }
        else
        {
            var keyStr = Text ?? string.Empty;
            localizedString =
                localizationSet.Strings.FirstOrDefault(s => s.Key.ToString() == keyStr).Value
                ?? EscapeText(Text)
                ?? string.Empty;
        }

        return string.Format(currentCulture, localizedString, count);
    }

    /// <summary>
    /// Converts a value.
    /// </summary>
    /// <param name="value">The value that the binding target produces.</param>
    /// <param name="targetTypes">The array of types to convert to.</param>
    /// <param name="parameter">The converter parameter to use.</param>
    /// <param name="culture">The culture to use in the converter.</param>
    /// <returns>An array of values that have been converted from the target value back to the source values.</returns>
    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }

    private static string EscapeText(string? text)
    {
        if (text is null)
        {
            return string.Empty;
        }

        return text.Replace("&amp;", "&")
            .Replace("&lt;", "<")
            .Replace("&gt;", ">")
            .Replace("&quot;", "\"")
            .Replace("&apos;", "'")
            .Trim();
    }
}
