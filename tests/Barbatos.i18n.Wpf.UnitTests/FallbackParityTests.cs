// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

using System.Globalization;
using System.Windows.Data;
using AwesomeAssertions;
using Barbatos.i18n;
using Barbatos.i18n.Wpf;

namespace Barbatos.i18n.Wpf.UnitTests;

/// <summary>
/// WPF resolves a translation through two paths - a live MultiBinding for a DependencyProperty target, and a
/// one-off string for targets such as Setter.Value - and they must render an untranslated key identically. The
/// binding path escaped and trimmed the key while the one-off path returned it raw.
/// </summary>
[Collection("Sequential")]
public sealed class FallbackParityTests : IDisposable
{
    private static readonly CultureInfo Culture = new("en-US");

    public FallbackParityTests()
    {
        var builder = new LocalizationBuilder();
        builder.AddLocalization(new LocalizationSet(null, Culture,
            new Dictionary<LocalizationKey, string?> { { "known", "Known" } }));
        builder.SetCulture(Culture);

        LocalizationProviderFactory.SetInstance(builder.Build(), string.Empty);
        WpfLocalization.Initialize(null!);
        new LocalizationCultureManager().SetCulture(Culture);
    }

    public void Dispose() => LocalizationProviderFactory.SetInstance(null!, string.Empty);

    private static string ThroughConverter(string key)
    {
        var converter = new StringLocalizerConverter(null, null, string.Empty, null, hasCultureSlot: false, keyFromBinding: true);

        return (string)converter.Convert([key], typeof(string), null!, Culture);
    }

    private static string ThroughLocalize(string key) =>
        (string)new StringLocalizerExtension { Text = key, Live = false }.ProvideValue(null!)!;

    [Theory]
    [InlineData("Missing &amp;Key")]
    [InlineData("  Padded  ")]
    [InlineData("PlainMissing")]
    public void BothPathsRenderAnUntranslatedKeyIdentically(string key)
    {
        ThroughLocalize(key).Should().Be(
            ThroughConverter(key),
            "the target property must not decide how an untranslated key looks");
    }

    [Fact]
    public void BothPathsAgreeOnAKnownKey()
    {
        ThroughLocalize("known").Should().Be("Known");
        ThroughConverter("known").Should().Be("Known");
    }
}
