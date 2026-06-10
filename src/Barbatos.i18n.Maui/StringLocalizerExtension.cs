// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

namespace Barbatos.i18n.Maui;

/// <summary>
/// Provides a markup extension that localizes strings in XAML.
/// </summary>
[ContentProperty(nameof(Text))]
public class StringLocalizerExtension : IMarkupExtension<BindingBase>
{
    /// <summary>
    /// Gets or sets the text to be localized.
    /// </summary>
    public string? Text { get; set; }

    /// <summary>
    /// Gets or sets the namespace of the text to be localized.
    /// </summary>
    public string? Namespace { get; set; }

    /// <summary>
    /// Provider key.
    /// </summary>
    public string ProviderKey { get; set; } = string.Empty;

    /// <summary>
    /// Optional argument 1 for string formatting.
    /// </summary>
    public object? Arg { get; set; } = null;

    /// <summary>
    /// Optional argument 2 for string formatting.
    /// </summary>
    public object? Arg2 { get; set; } = null;

    /// <summary>
    /// Optional argument 3 for string formatting.
    /// </summary>
    public object? Arg3 { get; set; } = null;

    /// <summary>
    /// Optional argument 4 for string formatting.
    /// </summary>
    public object? Arg4 { get; set; } = null;

    /// <summary>
    /// Optional argument 5 for string formatting.
    /// </summary>
    public object? Arg5 { get; set; } = null;

    /// <summary>
    /// Optional dynamic argument 1.
    /// </summary>
    public BindingBase? BindArg { get; set; } = null;

    /// <summary>
    /// Optional dynamic argument 2.
    /// </summary>
    public BindingBase? BindArg2 { get; set; } = null;

    /// <summary>
    /// Optional dynamic argument 3.
    /// </summary>
    public BindingBase? BindArg3 { get; set; } = null;

    /// <summary>
    /// Optional dynamic argument 4.
    /// </summary>
    public BindingBase? BindArg4 { get; set; } = null;

    /// <summary>
    /// Optional dynamic argument 5.
    /// </summary>
    public BindingBase? BindArg5 { get; set; } = null;

    /// <summary>
    /// Optional string format to apply to the final localized string.
    /// </summary>
    public string? StringFormat { get; set; } = null;

    /// <summary>
    /// Returns a localized string for the text property.
    /// </summary>
    /// <param name="serviceProvider">An object that provides services for the markup extension.</param>
    /// <returns>The localized string, or the original text if no localization is found.</returns>
    public BindingBase ProvideValue(IServiceProvider serviceProvider)
    {
        bool useBinding = BindArg is not null || BindArg2 is not null || BindArg3 is not null || BindArg4 is not null || BindArg5 is not null;

        if (useBinding)
        {
            var multiBinding = new MultiBinding
            {
                Converter = new StringLocalizerConverter(Text, Namespace, ProviderKey),
                StringFormat = StringFormat
            };

            if (BindArg is not null) multiBinding.Bindings.Add(BindArg);
            else if (Arg is not null) multiBinding.Bindings.Add(new Binding { Source = Arg });

            if (BindArg2 is not null) multiBinding.Bindings.Add(BindArg2);
            else if (Arg2 is not null) multiBinding.Bindings.Add(new Binding { Source = Arg2 });

            if (BindArg3 is not null) multiBinding.Bindings.Add(BindArg3);
            else if (Arg3 is not null) multiBinding.Bindings.Add(new Binding { Source = Arg3 });

            if (BindArg4 is not null) multiBinding.Bindings.Add(BindArg4);
            else if (Arg4 is not null) multiBinding.Bindings.Add(new Binding { Source = Arg4 });

            if (BindArg5 is not null) multiBinding.Bindings.Add(BindArg5);
            else if (Arg5 is not null) multiBinding.Bindings.Add(new Binding { Source = Arg5 });

            return multiBinding;
        }

        CultureInfo currentCulture =
            MauiLocalization.GetProvider(ProviderKey)?.GetCulture()
            ?? LocalizationProviderFactory.GetInstance(ProviderKey)?.GetCulture()
            ?? CultureInfo.CurrentUICulture;

        string? selectedNamespace = Namespace?.ToLowerInvariant();

        LocalizationSet? localizationSet = MauiLocalization.GetProvider(ProviderKey)?.GetLocalizationSet(currentCulture, selectedNamespace)
            ?? LocalizationProviderFactory.GetInstance(ProviderKey)?.GetLocalizationSet(currentCulture, selectedNamespace);

        string result = EscapeText(Text) ?? string.Empty;
        if (localizationSet is not null && Text is not null)
        {
            List<object?>? args = null;
            if (Arg is not null) { args ??= new List<object?>(); args.Add(Arg); }
            if (Arg2 is not null) { args ??= new List<object?>(); args.Add(Arg2); }
            if (Arg3 is not null) { args ??= new List<object?>(); args.Add(Arg3); }
            if (Arg4 is not null) { args ??= new List<object?>(); args.Add(Arg4); }
            if (Arg5 is not null) { args ??= new List<object?>(); args.Add(Arg5); }

            result = localizationSet.Format(currentCulture, (LocalizationKey)Text, args?.ToArray() ?? null) ?? EscapeText(Text) ?? string.Empty;
        }

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
