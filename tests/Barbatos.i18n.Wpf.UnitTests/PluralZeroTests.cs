// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

using System.Globalization;
using AwesomeAssertions;
using Barbatos.i18n;
using Barbatos.i18n.Wpf;

namespace Barbatos.i18n.Wpf.UnitTests;

/// <summary>
/// Plural selection used to be count > 1 everywhere, which is the French rule: it made an English UI read
/// "0 item left". Only zero differs between languages, so only zero is decided by culture.
/// </summary>
[Collection("Sequential")]
public sealed class PluralZeroTests : IDisposable
{
    private readonly CultureInfo _originalUiCulture = CultureInfo.CurrentUICulture;
    private readonly CultureInfo _originalCulture = CultureInfo.CurrentCulture;

    public PluralZeroTests()
    {
        var builder = new LocalizationBuilder();

        foreach ((string culture, string one, string many) in new[]
        {
            ("en-US", "{0} item left", "{0} items left"),
            ("fr-FR", "{0} article restant", "{0} articles restants")
        })
        {
            builder.AddLocalization(new LocalizationSet(null, new CultureInfo(culture),
                new Dictionary<LocalizationKey, string?> { { "one", one }, { "many", many } }));
        }

        builder.SetCulture(new CultureInfo("en-US"));

        LocalizationProviderFactory.SetInstance(builder.Build(), string.Empty);
        WpfLocalization.Initialize(null!);
    }

    public void Dispose()
    {
        LocalizationProviderFactory.SetInstance(null!, string.Empty);
        CultureInfo.CurrentUICulture = _originalUiCulture;
        CultureInfo.CurrentCulture = _originalCulture;
    }

    private static object? Resolve(string culture, int count)
    {
        new LocalizationCultureManager().SetCulture(new CultureInfo(culture));

        return new PluralStringLocalizerExtension { Text = "one", PluralText = "many", Count = count, Live = false }
            .ProvideValue(null!);
    }

    [Theory]
    [InlineData(0, "0 items left")]
    [InlineData(2, "2 items left")]
    [InlineData(5, "5 items left")]
    public void English_TreatsEverythingButOneAsPlural(int count, string expected)
    {
        Resolve("en-US", count).Should().Be(expected);
    }

    [Fact]
    public void English_TreatsOneAsSingular()
    {
        Resolve("en-US", 1).Should().Be("1 item left");
    }

    [Theory]
    [InlineData(0, "0 article restant")]
    [InlineData(1, "1 article restant")]
    public void French_GroupsZeroWithOne(int count, string expected)
    {
        Resolve("fr-FR", count).Should().Be(expected, "French counts zero as singular");
    }

    [Fact]
    public void French_TreatsTwoAsPlural()
    {
        Resolve("fr-FR", 2).Should().Be("2 articles restants");
    }

    [Theory]
    [InlineData("en-US", 0, true)]
    [InlineData("vi-VN", 0, true)]
    [InlineData("de-DE", 0, true)]
    [InlineData("fr-FR", 0, false)]
    [InlineData("fr-CA", 0, false)]
    [InlineData("pt-BR", 0, false)]
    [InlineData("pt-PT", 0, true)]
    [InlineData("en-US", 1, false)]
    [InlineData("fr-FR", 1, false)]
    [InlineData("en-US", 2, true)]
    [InlineData("fr-FR", 2, true)]
    [InlineData("en-US", -1, true)]
    public void TheRuleItself(string culture, int count, bool expectedPlural)
    {
        PluralRules.IsPlural(count, new CultureInfo(culture)).Should().Be(expectedPlural);
    }

    [Fact]
    public void ANullCulture_Throws()
    {
        FluentActions.Invoking(() => PluralRules.IsPlural(0, null!))
            .Should().Throw<ArgumentNullException>();
    }
}
