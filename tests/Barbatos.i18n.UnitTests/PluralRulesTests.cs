// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

namespace Barbatos.i18n.UnitTests;

/// <summary>
/// One is singular and any other non-zero count is plural in every language this two-form model serves. Only
/// zero differs, so only zero is decided by culture.
/// </summary>
public sealed class PluralRulesTests
{
    [Theory]
    [InlineData("en-US")]
    [InlineData("vi-VN")]
    [InlineData("de-DE")]
    [InlineData("fr-FR")]
    [InlineData("pt-BR")]
    public void OneIsAlwaysSingular(string culture)
    {
        PluralRules.IsPlural(1, new CultureInfo(culture)).Should().BeFalse();
    }

    [Theory]
    [InlineData("en-US")]
    [InlineData("vi-VN")]
    [InlineData("fr-FR")]
    [InlineData("pt-BR")]
    public void TwoOrMoreIsAlwaysPlural(string culture)
    {
        PluralRules.IsPlural(2, new CultureInfo(culture)).Should().BeTrue();
        PluralRules.IsPlural(100, new CultureInfo(culture)).Should().BeTrue();
    }

    [Theory]
    [InlineData("en-US")]
    [InlineData("en-GB")]
    [InlineData("vi-VN")]
    [InlineData("de-DE")]
    [InlineData("es-ES")]
    [InlineData("ko-KR")]
    [InlineData("pt-PT")]
    public void ZeroIsPluralInMostLanguages(string culture)
    {
        PluralRules.IsPlural(0, new CultureInfo(culture)).Should().BeTrue();
    }

    [Theory]
    [InlineData("fr-FR")]
    [InlineData("fr-CA")]
    [InlineData("fr")]
    [InlineData("pt-BR")]
    [InlineData("hy-AM")]
    public void ZeroIsSingularWhereTheLanguageGroupsItWithOne(string culture)
    {
        PluralRules.IsPlural(0, new CultureInfo(culture)).Should().BeFalse();
    }

    [Fact]
    public void ANegativeCountIsPlural()
    {
        // "-1 items" reads better than "-1 item", and a negative count is a data problem either way.
        PluralRules.IsPlural(-1, new CultureInfo("en-US")).Should().BeTrue();
    }

    [Fact]
    public void TheAmbientOverloadFollowsTheUiCulture()
    {
        CultureInfo original = CultureInfo.CurrentUICulture;

        try
        {
            CultureInfo.CurrentUICulture = new CultureInfo("fr-FR");
            PluralRules.IsPlural(0).Should().BeFalse();

            CultureInfo.CurrentUICulture = new CultureInfo("en-US");
            PluralRules.IsPlural(0).Should().BeTrue();
        }
        finally
        {
            CultureInfo.CurrentUICulture = original;
        }
    }

    [Fact]
    public void ANullCultureThrows()
    {
        FluentActions.Invoking(() => PluralRules.IsPlural(0, null!))
            .Should().Throw<ArgumentNullException>();
    }
}
