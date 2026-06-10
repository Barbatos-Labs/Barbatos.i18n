// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

namespace Barbatos.i18n.Maui;

/// <summary>
/// Provides a markup extension that localizes strings in XAML.
/// </summary>
[ContentProperty(nameof(Count))]
public class PluralStringLocalizerExtension : IMarkupExtension<BindingBase>
{
    /// <summary>
    /// Gets or sets the count.
    /// </summary>
    public int? Count { get; set; }

    /// <summary>
    /// Gets or sets the text.
    /// </summary>
    public string? Text { get; set; }

    /// <summary>
    /// Gets or sets the plural text.
    /// </summary>
    public string? PluralText { get; set; }

    /// <summary>
    /// Gets or sets the namespace.
    /// </summary>
    public string? Namespace { get; set; }

    /// <summary>
    /// Provider key.
    /// </summary>
    public string ProviderKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the binding count.
    /// </summary>
    public BindingBase? BindCount { get; set; } = null;

    /// <summary>
    /// Gets or sets the string format.
    /// </summary>
    public string? StringFormat { get; set; } = null;

    /// <summary>
    /// Returns a localized string for the text property.
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
            string fallback = Count > 1 ? EscapeText(PluralText) ?? string.Empty : EscapeText(Text) ?? string.Empty;
            return new Binding { Source = fallback };
        }

        string localizedString;

        if (Count > 1)
        {
            var keyStr = PluralText ?? string.Empty;
            localizedString =
                localizationSet.Strings.FirstOrDefault(s => s.Key == (LocalizationKey)keyStr).Value
                ?? EscapeText(PluralText)
                ?? string.Empty;
        }
        else
        {
            var keyStr = Text ?? string.Empty;
            localizedString =
                localizationSet.Strings.FirstOrDefault(s => s.Key == (LocalizationKey)keyStr).Value
                ?? EscapeText(Text)
                ?? string.Empty;
        }

        string result = string.Format(currentCulture, localizedString, Count ?? 0);
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
