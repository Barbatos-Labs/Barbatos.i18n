// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

namespace Barbatos.i18n.Maui;

/// <summary>
/// Provides a markup extension that localizes strings in XAML.
/// </summary>
/// <remarks>
/// The extension returns a <see cref="MultiBinding"/> that observes <see cref="LocalizationSource"/>, so the
/// translation follows later culture changes without reloading the page. Set <see cref="Live"/> to
/// <see langword="false"/> to resolve the string once instead.
/// </remarks>
[ContentProperty(nameof(Text))]
public class StringLocalizerExtension : IMarkupExtension<BindingBase>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StringLocalizerExtension"/> class.
    /// </summary>
    public StringLocalizerExtension() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="StringLocalizerExtension"/> class with the specified text.
    /// </summary>
    /// <param name="text">The text to be localized.</param>
    public StringLocalizerExtension(string? text)
    {
        Text = text;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StringLocalizerExtension"/> class with the specified text and namespace.
    /// </summary>
    /// <param name="text">The text to be localized.</param>
    /// <param name="textNamespace">The namespace of the text to be localized.</param>
    public StringLocalizerExtension(string? text, string? textNamespace)
    {
        Text = text;
        Namespace = textNamespace;
    }

    /// <summary>
    /// Gets or sets the text to be localized.
    /// </summary>
    public string? Text { get; set; }

    /// <summary>
    /// Gets or sets a binding that supplies the localization key at runtime.
    /// </summary>
    /// <remarks>
    /// Use this inside a <see cref="DataTemplate"/> - a <c>CollectionView</c>, <c>ListView</c> or <c>Picker</c>
    /// item - where the key lives in the item itself: <c>{i18n:StringLocalizer BindText={Binding StatusKey}}</c>.
    /// It takes precedence over <see cref="Text"/>.
    /// </remarks>
    public BindingBase? BindText { get; set; } = null;

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
    /// Gets or sets whether the translation re-evaluates when the culture changes.
    /// </summary>
    /// <remarks>
    /// Enabled by default. Set it to <see langword="false"/> to resolve the translation once, at load time, as
    /// versions before live localization did.
    /// </remarks>
    public bool? Live { get; set; } = null;

    /// <summary>
    /// Returns a localized string for the text property.
    /// </summary>
    /// <param name="serviceProvider">An object that provides services for the markup extension.</param>
    /// <returns>The localized string, or the original text if no localization is found.</returns>
    public BindingBase ProvideValue(IServiceProvider serviceProvider)
    {
        bool keyFromBinding = BindText is not null;
        bool hasArgBindings = BindArg is not null || BindArg2 is not null || BindArg3 is not null || BindArg4 is not null || BindArg5 is not null;
        bool cultureSlot = Live != false;

        if (!keyFromBinding && !hasArgBindings && !cultureSlot)
        {
            return new Binding { Source = Localize() };
        }

        return BuildBinding(cultureSlot, keyFromBinding);
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
    /// Escapes special characters in a string.
    /// </summary>
    /// <param name="text">The text to escape.</param>
    /// <returns>The escaped text.</returns>
    public static string EscapeText(string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        if (text.IndexOf('&') < 0)
        {
            return text.Trim();
        }

        return new System.Text.StringBuilder(text)
            .Replace("&amp;", "&")
            .Replace("&lt;", "<")
            .Replace("&gt;", ">")
            .Replace("&quot;", "\"")
            .Replace("&apos;", "'")
            .ToString()
            .Trim();
    }

    /// <summary>
    /// Creates the binding that watches <see cref="LocalizationSource"/> for culture changes.
    /// </summary>
    /// <returns>A one-way binding to the active culture.</returns>
    internal static Binding CreateCultureBinding() =>
        new(nameof(LocalizationSource.Culture))
        {
            Source = LocalizationSource.Instance,
            Mode = BindingMode.OneWay
        };

    /// <summary>
    /// Assembles the multi-binding that feeds <see cref="StringLocalizerConverter"/>.
    /// </summary>
    /// <param name="cultureSlot">Whether to prepend the <see cref="LocalizationSource"/> culture value.</param>
    /// <param name="keyFromBinding">Whether the key comes from <see cref="BindText"/>.</param>
    /// <returns>The assembled multi-binding.</returns>
    private MultiBinding BuildBinding(bool cultureSlot, bool keyFromBinding)
    {
        var multiBinding = new MultiBinding
        {
            Converter = new StringLocalizerConverter(Text, Namespace, ProviderKey, cultureSlot, keyFromBinding),
            StringFormat = StringFormat
        };

        if (cultureSlot)
        {
            multiBinding.Bindings.Add(CreateCultureBinding());
        }

        if (keyFromBinding)
        {
            multiBinding.Bindings.Add(BindText!);
        }

        AddArgument(multiBinding, BindArg, Arg);
        AddArgument(multiBinding, BindArg2, Arg2);
        AddArgument(multiBinding, BindArg3, Arg3);
        AddArgument(multiBinding, BindArg4, Arg4);
        AddArgument(multiBinding, BindArg5, Arg5);

        return multiBinding;
    }

    /// <summary>
    /// Appends one format argument to the multi-binding, preferring its bound form over its static form.
    /// </summary>
    /// <param name="multiBinding">The multi-binding being assembled.</param>
    /// <param name="binding">The bound form of the argument.</param>
    /// <param name="value">The static form of the argument.</param>
    private static void AddArgument(MultiBinding multiBinding, BindingBase? binding, object? value)
    {
        if (binding is not null)
        {
            multiBinding.Bindings.Add(binding);
            return;
        }

        if (value is not null)
        {
            multiBinding.Bindings.Add(new Binding { Source = value });
        }
    }

    /// <summary>
    /// Resolves the translation once, for the opted-out static path.
    /// </summary>
    /// <returns>The localized string, or the original text if no localization is found.</returns>
    private string Localize()
    {
        LocalizationSet? localizationSet = LocalizationLookup.ResolveSet(ProviderKey, LocalizationLookup.NormalizeNamespace(Namespace));

        string result = EscapeText(Text);

        if (localizationSet is not null && Text is not null)
        {
            List<object?>? args = null;
            if (Arg is not null) { args ??= new List<object?>(); args.Add(Arg); }
            if (Arg2 is not null) { args ??= new List<object?>(); args.Add(Arg2); }
            if (Arg3 is not null) { args ??= new List<object?>(); args.Add(Arg3); }
            if (Arg4 is not null) { args ??= new List<object?>(); args.Add(Arg4); }
            if (Arg5 is not null) { args ??= new List<object?>(); args.Add(Arg5); }

            result = localizationSet.Format(CultureInfo.CurrentCulture, (LocalizationKey)Text, args?.ToArray() ?? null) ?? EscapeText(Text);
        }

        if (StringFormat is not null)
        {
            result = string.Format(StringFormat, result);
        }

        return result;
    }
}
