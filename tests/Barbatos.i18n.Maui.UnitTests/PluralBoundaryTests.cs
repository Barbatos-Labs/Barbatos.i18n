// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

using System.Globalization;
using Barbatos.i18n;
using Barbatos.i18n.Maui;
using AwesomeAssertions;

namespace Barbatos.i18n.Maui.UnitTests;

/// <summary>
/// WPF and MAUI must select the same plural form for the same count, and the existing MAUI tests only exercised
/// a large count - never the 0/1/2 boundary the rule actually turns on. The rule lives in PluralRules and the
/// language comes from the provider, so these mirror PluralZeroTests on the WPF side at the converter level,
/// which is as deep as MAUI can be tested without the WinUI runtime.
/// </summary>
[Collection("Sequential")]
public sealed class PluralBoundaryTests : IDisposable
{
    private readonly CultureInfo _originalUiCulture = CultureInfo.CurrentUICulture;

    public void Dispose()
    {
        LocalizationProviderFactory.SetInstance(null!, string.Empty);
        CultureInfo.CurrentUICulture = _originalUiCulture;
    }

    private static string Convert(string culture, string one, string many, int count)
    {
        var builder = new LocalizationBuilder();
        builder.AddLocalization(new LocalizationSet(null, new CultureInfo(culture),
            new Dictionary<LocalizationKey, string?> { { "one", one }, { "many", many } }));
        builder.SetCulture(new CultureInfo(culture));

        LocalizationProviderFactory.SetInstance(builder.Build(), string.Empty);
        MauiLocalization.Initialize(null!);

        var converter = new PluralStringLocalizerConverter(
            "one", "many", null, string.Empty,
            hasCultureSlot: false, keyFromBinding: false, pluralKeyFromBinding: false, staticCount: count);

        return (string)converter.Convert([], typeof(string), null!, new CultureInfo(culture));
    }

    [Theory]
    [InlineData(0, "0 items left")]
    [InlineData(1, "1 item left")]
    [InlineData(2, "2 items left")]
    public void English_UsesThePluralFormForEverythingButOne(int count, string expected)
    {
        Convert("en-US", "{0} item left", "{0} items left", count).Should().Be(expected);
    }

    [Theory]
    [InlineData(0, "0 article restant")]
    [InlineData(1, "1 article restant")]
    [InlineData(2, "2 articles restants")]
    public void French_GroupsZeroWithOne(int count, string expected)
    {
        Convert("fr-FR", "{0} article restant", "{0} articles restants", count).Should().Be(expected);
    }

    [Fact]
    public void TheFormFollowsTheProviderNotTheThread()
    {
        // The provider serves French while the thread reports English; the French rule has to win, because the
        // translation came from the French set.
        CultureInfo.CurrentUICulture = new CultureInfo("en-US");

        Convert("fr-FR", "{0} article restant", "{0} articles restants", 0)
            .Should().Be("0 article restant");
    }
}
