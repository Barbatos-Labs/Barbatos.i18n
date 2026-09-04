// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

namespace Barbatos.i18n.Wpf;

/// <summary>
/// Provides a markup extension that localizes strings in XAML.
/// </summary>
/// <remarks>
/// <para>
/// This class extends <see cref="MarkupExtension"/> and overrides the <see cref="ProvideValue"/> method to return localized strings.
/// </para>
/// <para>
/// When the target is a <see cref="DependencyProperty"/>, the extension returns a <see cref="MultiBinding"/> that
/// observes <see cref="LocalizationSource"/>, so the translation follows later culture changes without reloading
/// the view. Anywhere a binding cannot be used - a <see cref="System.Windows.Setter"/> value, a plain CLR
/// property - it falls back to resolving the string once, as it always did. Use <see cref="Live"/> to override
/// that choice.
/// </para>
/// </remarks>
[ContentProperty(nameof(Text))]
[MarkupExtensionReturnType(typeof(string))]
public class StringLocalizerExtension : MarkupExtension
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
        Text = EscapeText(text);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StringLocalizerExtension"/> class with the specified text and namespace.
    /// </summary>
    /// <param name="text">The text to be localized.</param>
    /// <param name="textNamespace">The namespace of the text to be localized.</param>
    public StringLocalizerExtension(string? text, string? textNamespace)
    {
        Text = EscapeText(text);
        Namespace = textNamespace;
    }

    /// <summary>
    /// Gets or sets the text to be localized.
    /// </summary>
    public string? Text
    {
        get;
        set => field = EscapeText(value);
    }

    /// <summary>
    /// Gets or sets a binding that supplies the localization key at runtime.
    /// </summary>
    /// <remarks>
    /// Use this inside a <see cref="DataTemplate"/> - an <c>ItemsControl</c>, <c>ListView</c> or <c>DataGrid</c>
    /// row - where the key lives in the item itself: <c>{i18n:StringLocalizer BindText={Binding StatusKey}}</c>.
    /// It takes precedence over <see cref="Text"/>, and unlike <see cref="Text"/> the resolved key is used as-is,
    /// without XML entity unescaping.
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
    /// Left unset, the extension decides for itself: it produces a live binding when the target is a
    /// <see cref="DependencyProperty"/> and a one-off string otherwise. Set it to <see langword="true"/> to force
    /// a binding, or to <see langword="false"/> to opt out of the culture watch entirely.
    /// </remarks>
    public bool? Live { get; set; } = null;

    /// <summary>
    /// Returns a localized string for the <see cref="Text"/> property.
    /// </summary>
    /// <param name="serviceProvider">An object that provides services for the markup extension.</param>
    /// <returns>The localized string, or the original text if no localization is found.</returns>
    public override object? ProvideValue(IServiceProvider serviceProvider)
    {
        bool keyFromBinding = BindText is not null;

        if (!keyFromBinding && string.IsNullOrEmpty(Text))
        {
            return string.Empty;
        }

        bool hasArgBindings = BindArg is not null || BindArg2 is not null || BindArg3 is not null || BindArg4 is not null || BindArg5 is not null;
        bool cultureSlot = Live != false;

        if (keyFromBinding || hasArgBindings || (cultureSlot && IsBindableTarget(serviceProvider, Live)))
        {
            return BuildBinding(cultureSlot, keyFromBinding).ProvideValue(serviceProvider);
        }

        return Localize();
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
    /// Determines whether the XAML target can accept a binding.
    /// </summary>
    /// <param name="serviceProvider">An object that provides services for the markup extension.</param>
    /// <param name="live">The value of the extension's <see cref="Live"/> property.</param>
    /// <returns>True when a binding may be produced; otherwise false.</returns>
    internal static bool IsBindableTarget(IServiceProvider? serviceProvider, bool? live)
    {
        if (live == true)
        {
            return true;
        }

        return serviceProvider?.GetService(typeof(IProvideValueTarget)) is IProvideValueTarget target
            && target.TargetProperty is DependencyProperty;
    }

    /// <summary>
    /// Assembles the multi-binding that feeds <see cref="StringLocalizerConverter"/>.
    /// </summary>
    /// <param name="cultureSlot">Whether to prepend the <see cref="LocalizationSource"/> culture value.</param>
    /// <param name="keyFromBinding">Whether the key comes from <see cref="BindText"/>.</param>
    /// <returns>The assembled multi-binding.</returns>
    private MultiBinding BuildBinding(bool cultureSlot, bool keyFromBinding)
    {
        var multiBinding = new MultiBinding { StringFormat = StringFormat };

        if (cultureSlot)
        {
            multiBinding.Bindings.Add(
                new Binding(nameof(LocalizationSource.Culture))
                {
                    Source = LocalizationSource.Instance,
                    Mode = BindingMode.OneWay
                });
        }

        if (keyFromBinding)
        {
            multiBinding.Bindings.Add(BindText!);
        }

        // WPF ignores the StringFormat of a MultiBinding child, so it is captured here and applied per argument
        // by the converter. The list is built alongside the bindings to stay aligned with the arguments actually
        // contributed - arguments are skipped when both their static and bound forms are unset.
        var stringFormats = new List<string?>(5);

        AddArgument(multiBinding, stringFormats, BindArg, Arg);
        AddArgument(multiBinding, stringFormats, BindArg2, Arg2);
        AddArgument(multiBinding, stringFormats, BindArg3, Arg3);
        AddArgument(multiBinding, stringFormats, BindArg4, Arg4);
        AddArgument(multiBinding, stringFormats, BindArg5, Arg5);

        multiBinding.Converter = new StringLocalizerConverter(
            Text,
            Namespace,
            ProviderKey,
            stringFormats.ToArray(),
            cultureSlot,
            keyFromBinding);

        return multiBinding;
    }

    /// <summary>
    /// Appends one format argument to the multi-binding, preferring its bound form over its static form.
    /// </summary>
    /// <param name="multiBinding">The multi-binding being assembled.</param>
    /// <param name="stringFormats">The per-argument formats collected so far.</param>
    /// <param name="binding">The bound form of the argument.</param>
    /// <param name="value">The static form of the argument.</param>
    private static void AddArgument(MultiBinding multiBinding, List<string?> stringFormats, BindingBase? binding, object? value)
    {
        if (binding is not null)
        {
            multiBinding.Bindings.Add(binding);
            stringFormats.Add((binding as Binding)?.StringFormat);
            return;
        }

        if (value is not null)
        {
            multiBinding.Bindings.Add(new Binding { Source = value });
            stringFormats.Add(null);
        }
    }

    /// <summary>
    /// Resolves the translation once, for targets that cannot hold a binding.
    /// </summary>
    /// <returns>The localized string, or the original text if no localization is found.</returns>
    private object Localize()
    {
        List<object?>? args = null;

        if (Arg is not null)
        {
            args ??= new List<object?>();
            args.Add(Arg);
        }

        if (Arg2 is not null)
        {
            args ??= new List<object?>();
            args.Add(Arg2);
        }

        if (Arg3 is not null)
        {
            args ??= new List<object?>();
            args.Add(Arg3);
        }

        if (Arg4 is not null)
        {
            args ??= new List<object?>();
            args.Add(Arg4);
        }

        if (Arg5 is not null)
        {
            args ??= new List<object?>();
            args.Add(Arg5);
        }

        string result = LocalizationLookup.ResolveFormatted(
            ProviderKey,
            LocalizationLookup.NormalizeNamespace(Namespace),
            Text!,
            CultureInfo.CurrentCulture,
            args?.ToArray()) ?? Text ?? string.Empty;

        if (StringFormat is not null)
        {
            return string.Format(StringFormat, result);
        }

        return result;
    }
}
