// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

namespace Barbatos.i18n.Wpf;

/// <summary>
/// Provides a multi value converter that localizes strings in XAML, supporting both singular and plural forms.
/// </summary>
/// <remarks>
/// The converter reads the values of a <see cref="MultiBinding"/> in a fixed order: an optional culture slot
/// produced by <see cref="LocalizationSource"/>, an optional singular key, an optional plural key, and finally
/// the count. <see cref="PluralStringLocalizerExtension"/> assembles the bindings to match.
/// </remarks>
public sealed class PluralStringLocalizerConverter : IMultiValueConverter
{
    private readonly string? _normalizedNamespace;
    private readonly bool _hasCultureSlot;
    private readonly bool _keyFromBinding;
    private readonly bool _pluralKeyFromBinding;
    private readonly int? _staticCount;

    /// <summary>
    /// Gets the singular key, or null when it is supplied by a binding.
    /// </summary>
    public string? Text { get; }

    /// <summary>
    /// Gets the plural key, or null when it is supplied by a binding.
    /// </summary>
    public string? PluralText { get; }

    /// <summary>
    /// Gets the namespace of the text to be localized.
    /// </summary>
    public string? Namespace { get; }

    /// <summary>
    /// Provider key.
    /// </summary>
    public string ProviderKey { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PluralStringLocalizerConverter"/> class whose single bound
    /// value is the count.
    /// </summary>
    /// <param name="text">The singular localization key.</param>
    /// <param name="pluralText">The plural localization key.</param>
    /// <param name="textNamespace">The namespace of the text to be localized.</param>
    /// <param name="providerKey">The provider key.</param>
    public PluralStringLocalizerConverter(string? text, string? pluralText, string? textNamespace, string providerKey)
        : this(text, pluralText, textNamespace, providerKey, hasCultureSlot: false, keyFromBinding: false, pluralKeyFromBinding: false, staticCount: null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PluralStringLocalizerConverter"/> class, describing which
    /// leading values of the multi-binding are reserved.
    /// </summary>
    /// <param name="text">The singular key, or null when <paramref name="keyFromBinding"/> is true.</param>
    /// <param name="pluralText">The plural key, or null when <paramref name="pluralKeyFromBinding"/> is true.</param>
    /// <param name="textNamespace">The namespace of the text to be localized.</param>
    /// <param name="providerKey">The provider key.</param>
    /// <param name="hasCultureSlot">Whether the first value is the <see cref="LocalizationSource"/> culture and must be skipped.</param>
    /// <param name="keyFromBinding">Whether the singular key is carried by a bound value.</param>
    /// <param name="pluralKeyFromBinding">Whether the plural key is carried by a bound value.</param>
    /// <param name="staticCount">The count to use when no bound value carries one.</param>
    public PluralStringLocalizerConverter(
        string? text,
        string? pluralText,
        string? textNamespace,
        string providerKey,
        bool hasCultureSlot,
        bool keyFromBinding,
        bool pluralKeyFromBinding,
        int? staticCount
    )
    {
        Text = text;
        PluralText = pluralText;
        Namespace = textNamespace;
        ProviderKey = providerKey;
        _normalizedNamespace = LocalizationLookup.NormalizeNamespace(textNamespace);
        _hasCultureSlot = hasCultureSlot;
        _keyFromBinding = keyFromBinding;
        _pluralKeyFromBinding = pluralKeyFromBinding;
        _staticCount = staticCount;
    }

    /// <summary>
    /// Selects the singular or plural translation according to the count and fills its placeholder with it.
    /// </summary>
    /// <param name="values">The values produced by the source bindings.</param>
    /// <param name="targetType">The type of the binding target property.</param>
    /// <param name="parameter">The converter parameter to use.</param>
    /// <param name="culture">The culture to use in the converter.</param>
    /// <returns>The localized string, or the key itself when no translation is found.</returns>
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        values ??= [];

        int index = _hasCultureSlot ? 1 : 0;

        string? key = Text;
        if (_keyFromBinding)
        {
            key = index < values.Length ? ReadKey(values[index]) : null;
            index++;
        }

        string? pluralKey = PluralText;
        if (_pluralKeyFromBinding)
        {
            pluralKey = index < values.Length ? ReadKey(values[index]) : null;
            index++;
        }

        if (string.IsNullOrEmpty(key) && string.IsNullOrEmpty(pluralKey))
        {
            return string.Empty;
        }

        int count = _staticCount ?? 0;
        if (index < values.Length && ReadCount(values[index]) is int boundCount)
        {
            count = boundCount;
        }

        // Fall back to whichever form was actually supplied when the preferred one is missing.
        string? selectedKey = count > 1 ? pluralKey : key;
        if (string.IsNullOrEmpty(selectedKey))
        {
            selectedKey = count > 1 ? key : pluralKey;
        }

        string localizedString =
            LocalizationLookup.ResolveValue(ProviderKey, _normalizedNamespace, selectedKey ?? string.Empty)
            ?? StringLocalizerExtension.EscapeText(selectedKey);

        return string.Format(CultureInfo.CurrentCulture, localizedString, count);
    }

    /// <summary>
    /// Not supported.
    /// </summary>
    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }

    /// <summary>
    /// Reads a localization key out of a bound value.
    /// </summary>
    /// <param name="value">The bound value.</param>
    /// <returns>The key, or null when the value carries none.</returns>
    private static string? ReadKey(object? value)
    {
        if (value is null || value == DependencyProperty.UnsetValue)
        {
            return null;
        }

        if (value is string text)
        {
            return text;
        }

        if (value is LocalizationKey localizationKey)
        {
            return localizationKey.ToString();
        }

        return value.ToString();
    }

    /// <summary>
    /// Reads a count out of a bound value.
    /// </summary>
    /// <param name="value">The bound value.</param>
    /// <returns>The count, or null when the value carries none.</returns>
    private static int? ReadCount(object? value)
    {
        if (value is null || value == DependencyProperty.UnsetValue)
        {
            return null;
        }

        if (value is int count)
        {
            return count;
        }

        return int.TryParse(value.ToString(), NumberStyles.Integer, CultureInfo.CurrentCulture, out int parsed)
            ? parsed
            : null;
    }
}
