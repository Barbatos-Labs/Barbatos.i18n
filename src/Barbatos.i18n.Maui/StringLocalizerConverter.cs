// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

namespace Barbatos.i18n.Maui;

/// <summary>
/// Provides a multi value converter that localizes strings in XAML.
/// </summary>
/// <remarks>
/// The converter reads the values of a <see cref="MultiBinding"/> in a fixed order: an optional culture slot
/// produced by <see cref="LocalizationSource"/>, an optional slot carrying the localization key, and finally the
/// format arguments. <see cref="StringLocalizerExtension"/> assembles the bindings to match.
/// </remarks>
public sealed class StringLocalizerConverter : IMultiValueConverter
{
    private readonly string? _normalizedNamespace;
    private readonly bool _hasCultureSlot;
    private readonly bool _keyFromBinding;

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
    /// Initializes a new instance of the <see cref="StringLocalizerConverter"/> class whose bound values are
    /// format arguments only.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <param name="textNamespace">The namespace.</param>
    /// <param name="providerKey">The provider key.</param>
    public StringLocalizerConverter(string? text, string? textNamespace, string providerKey)
        : this(text, textNamespace, providerKey, hasCultureSlot: false, keyFromBinding: false)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StringLocalizerConverter"/> class, describing which leading
    /// values of the multi-binding are reserved.
    /// </summary>
    /// <param name="text">The localization key, or null when <paramref name="keyFromBinding"/> is true.</param>
    /// <param name="textNamespace">The namespace.</param>
    /// <param name="providerKey">The provider key.</param>
    /// <param name="hasCultureSlot">Whether the first value is the <see cref="LocalizationSource"/> culture and must be skipped.</param>
    /// <param name="keyFromBinding">Whether the localization key is carried by a bound value instead of <paramref name="text"/>.</param>
    public StringLocalizerConverter(
        string? text,
        string? textNamespace,
        string providerKey,
        bool hasCultureSlot,
        bool keyFromBinding
    )
    {
        Text = text;
        Namespace = textNamespace;
        ProviderKey = providerKey;
        _normalizedNamespace = LocalizationLookup.NormalizeNamespace(textNamespace);
        _hasCultureSlot = hasCultureSlot;
        _keyFromBinding = keyFromBinding;
    }

    /// <summary>
    /// Localizes the key and fills its placeholders with the bound format arguments.
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

        if (string.IsNullOrEmpty(key))
        {
            return string.Empty;
        }

        LocalizationSet? localizationSet = LocalizationLookup.ResolveSet(ProviderKey, _normalizedNamespace);

        if (localizationSet is null)
        {
            return StringLocalizerExtension.EscapeText(key);
        }

        int argumentCount = Math.Max(0, values.Length - index);
        object?[] arguments = argumentCount == 0 ? [] : new object?[argumentCount];

        for (int i = 0; i < argumentCount; i++)
        {
            object? value = values[index + i];
            arguments[i] = IsUnset(value) ? string.Empty : value;
        }

        return localizationSet.Format(CultureInfo.CurrentCulture, (LocalizationKey)key, arguments)
            ?? StringLocalizerExtension.EscapeText(key);
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
    internal static string? ReadKey(object? value)
    {
        if (IsUnset(value))
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

        return value!.ToString();
    }

    /// <summary>
    /// Determines whether a bound value carries no data.
    /// </summary>
    /// <param name="value">The bound value.</param>
    /// <returns>True when the value is null or unset; otherwise false.</returns>
    internal static bool IsUnset(object? value) =>
        value is null || ReferenceEquals(value, BindableProperty.UnsetValue);
}
