// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

namespace Barbatos.i18n.Maui;

/// <summary>
/// Provides a markup extension that localizes strings in XAML, supporting both singular and plural forms.
/// </summary>
[ContentProperty(nameof(Count))]
public class PluralStringLocalizerExtension : IMarkupExtension<BindingBase>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PluralStringLocalizerExtension"/> class.
    /// </summary>
    public PluralStringLocalizerExtension() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="PluralStringLocalizerExtension"/> class with the specified count, text, and plural text.
    /// </summary>
    /// <param name="count">The count that determines whether to use the singular or plural form.</param>
    /// <param name="text">The text to be localized.</param>
    /// <param name="pluralText">The plural text to be localized.</param>
    public PluralStringLocalizerExtension(int count, string text, string pluralText)
    {
        Count = count;
        Text = text;
        PluralText = pluralText;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PluralStringLocalizerExtension"/> class with the specified count, text, plural text, and namespace.
    /// </summary>
    /// <param name="count">The count that determines whether to use the singular or plural form.</param>
    /// <param name="text">The text to be localized.</param>
    /// <param name="pluralText">The plural text to be localized.</param>
    /// <param name="namespaceName">The namespace of the text to be localized.</param>
    public PluralStringLocalizerExtension(int count, string text, string pluralText, string namespaceName)
    {
        Count = count;
        Text = text;
        PluralText = pluralText;
        Namespace = namespaceName;
    }

    /// <summary>
    /// Gets or sets the count that determines whether to use the singular or plural form of the text.
    /// </summary>
    public int? Count { get; set; }

    /// <summary>
    /// Gets or sets the text to be localized.
    /// </summary>
    public string? Text { get; set; }

    /// <summary>
    /// Gets or sets the plural text to be localized.
    /// </summary>
    public string? PluralText { get; set; }

    /// <summary>
    /// Gets or sets the namespace of the text to be localized.
    /// </summary>
    public string? Namespace { get; set; }

    /// <summary>
    /// Provider key.
    /// </summary>
    public string ProviderKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the binding for count that determines whether to use the singular or plural form of the text.
    /// </summary>
    public BindingBase? BindCount { get; set; } = null;

    /// <summary>
    /// Optional string format to apply to the final localized string.
    /// </summary>
    public string? StringFormat { get; set; } = null;

    /// <summary>
    /// Returns a localized string for the <see cref="Text"/> property.
    /// </summary>
    /// <param name="serviceProvider">An object that provides services for the markup extension.</param>
    /// <returns>The localized string, or the original text if no localization is found.</returns>
    public BindingBase ProvideValue(IServiceProvider serviceProvider)
    {
        if (string.IsNullOrEmpty(Text) && string.IsNullOrEmpty(PluralText))
        {
            return new Binding { Source = string.Empty };
        }

        if (BindCount is not null)
        {
            var multiBinding = new MultiBinding
            {
                Converter = new PluralStringLocalizerConverter(Text, PluralText, Namespace, ProviderKey),
                StringFormat = StringFormat
            };
            multiBinding.Bindings.Add(BindCount);
            return multiBinding;
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
            string fallback = Count > 1 ? StringLocalizerExtension.EscapeText(PluralText) ?? string.Empty : StringLocalizerExtension.EscapeText(Text) ?? string.Empty;
            return new Binding { Source = fallback };
        }

        string localizedString;

        if (Count > 1)
        {
            var keyStr = PluralText ?? string.Empty;
            localizedString =
                localizationSet.Strings.FirstOrDefault(s => s.Key == (LocalizationKey)keyStr).Value
                ?? StringLocalizerExtension.EscapeText(PluralText)
                ?? string.Empty;
        }
        else
        {
            var keyStr = Text ?? string.Empty;
            localizedString =
                localizationSet.Strings.FirstOrDefault(s => s.Key == (LocalizationKey)keyStr).Value
                ?? StringLocalizerExtension.EscapeText(Text)
                ?? string.Empty;
        }

        string result = string.Format(CultureInfo.CurrentCulture, localizedString, Count ?? 0);
        if (StringFormat is not null)
        {
            result = string.Format(StringFormat, result);
        }
        return new Binding { Source = result };
    }

    /// <summary>
    /// Returns a localized string for the text property.
    /// </summary>
    /// <param name="serviceProvider">An object that provides services for the markup extension.</param>
    /// <returns>The localized string, or the original text if no localization is found.</returns>
    object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider)
    {
        return ProvideValue(serviceProvider);
    }
}
