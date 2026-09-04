// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

namespace Barbatos.i18n.Maui;

/// <summary>
/// Provides a markup extension that localizes strings in XAML, supporting both singular and plural forms.
/// </summary>
/// <remarks>
/// The extension returns a <see cref="MultiBinding"/> that observes <see cref="LocalizationSource"/>, so the
/// translation follows later culture changes without reloading the page. Set <see cref="Live"/> to
/// <see langword="false"/> to resolve the string once instead.
/// </remarks>
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
    /// Gets or sets a binding that supplies the singular localization key at runtime.
    /// </summary>
    /// <remarks>
    /// Use this inside a <see cref="DataTemplate"/> where the keys live in the item itself. It takes precedence
    /// over <see cref="Text"/>.
    /// </remarks>
    public BindingBase? BindText { get; set; } = null;

    /// <summary>
    /// Gets or sets a binding that supplies the plural localization key at runtime.
    /// </summary>
    /// <remarks>
    /// Takes precedence over <see cref="PluralText"/>.
    /// </remarks>
    public BindingBase? BindPluralText { get; set; } = null;

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
    /// Gets or sets whether the translation re-evaluates when the culture changes.
    /// </summary>
    /// <remarks>
    /// Enabled by default. Set it to <see langword="false"/> to resolve the translation once, at load time, as
    /// versions before live localization did.
    /// </remarks>
    public bool? Live { get; set; } = null;

    /// <summary>
    /// Returns a localized string for the <see cref="Text"/> property.
    /// </summary>
    /// <param name="serviceProvider">An object that provides services for the markup extension.</param>
    /// <returns>The localized string, or the original text if no localization is found.</returns>
    public BindingBase ProvideValue(IServiceProvider serviceProvider)
    {
        bool keyFromBinding = BindText is not null;
        bool pluralKeyFromBinding = BindPluralText is not null;

        if (!keyFromBinding
            && !pluralKeyFromBinding
            && string.IsNullOrEmpty(Text)
            && string.IsNullOrEmpty(PluralText))
        {
            return new Binding { Source = string.Empty };
        }

        bool cultureSlot = Live != false;

        if (!keyFromBinding && !pluralKeyFromBinding && BindCount is null && !cultureSlot)
        {
            return new Binding { Source = Localize() };
        }

        return BuildBinding(cultureSlot, keyFromBinding, pluralKeyFromBinding);
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

    /// <summary>
    /// Assembles the multi-binding that feeds <see cref="PluralStringLocalizerConverter"/>.
    /// </summary>
    /// <param name="cultureSlot">Whether to prepend the <see cref="LocalizationSource"/> culture value.</param>
    /// <param name="keyFromBinding">Whether the singular key comes from <see cref="BindText"/>.</param>
    /// <param name="pluralKeyFromBinding">Whether the plural key comes from <see cref="BindPluralText"/>.</param>
    /// <returns>The assembled multi-binding.</returns>
    private MultiBinding BuildBinding(bool cultureSlot, bool keyFromBinding, bool pluralKeyFromBinding)
    {
        var multiBinding = new MultiBinding
        {
            Converter = new PluralStringLocalizerConverter(
                Text,
                PluralText,
                Namespace,
                ProviderKey,
                cultureSlot,
                keyFromBinding,
                pluralKeyFromBinding,
                Count),
            StringFormat = StringFormat
        };

        if (cultureSlot)
        {
            multiBinding.Bindings.Add(StringLocalizerExtension.CreateCultureBinding());
        }

        if (keyFromBinding)
        {
            multiBinding.Bindings.Add(BindText!);
        }

        if (pluralKeyFromBinding)
        {
            multiBinding.Bindings.Add(BindPluralText!);
        }

        if (BindCount is not null)
        {
            multiBinding.Bindings.Add(BindCount);
        }

        return multiBinding;
    }

    /// <summary>
    /// Resolves the translation once, for the opted-out static path.
    /// </summary>
    /// <returns>The localized string, or the original text if no localization is found.</returns>
    private string Localize()
    {
        LocalizationSet? localizationSet = LocalizationLookup.ResolveSet(ProviderKey, LocalizationLookup.NormalizeNamespace(Namespace));

        bool isPlural = Count > 1;

        // Fall back to whichever form was actually supplied when the preferred one is missing.
        string? selectedKey = isPlural ? PluralText : Text;
        if (string.IsNullOrEmpty(selectedKey))
        {
            selectedKey = isPlural ? Text : PluralText;
        }

        string localizedString =
            localizationSet?[(LocalizationKey)(selectedKey ?? string.Empty)]
            ?? StringLocalizerExtension.EscapeText(selectedKey);

        string result = string.Format(CultureInfo.CurrentCulture, localizedString, Count ?? 0);

        if (StringFormat is not null)
        {
            result = string.Format(StringFormat, result);
        }

        return result;
    }
}
