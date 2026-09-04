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
/// The plural form is selected on count > 1, which is the French rule: zero and one both take the singular.
/// English does not work that way - "0 items left" is plural - so an English UI showing a zero count reads
/// "0 item left" unless the application handles zero itself. This test pins the behaviour so the choice is
/// visible rather than surprising; changing it would alter output for every consumer.
/// </summary>
[Collection("Sequential")]
public sealed class PluralZeroTests : IDisposable
{
    private static readonly CultureInfo Culture = new("en-US");

    public PluralZeroTests()
    {
        var builder = new LocalizationBuilder();
        builder.AddLocalization(new LocalizationSet(null, Culture, new Dictionary<LocalizationKey, string?>
        {
            { "one", "{0} item left" },
            { "many", "{0} items left" }
        }));
        builder.SetCulture(Culture);

        LocalizationProviderFactory.SetInstance(builder.Build(), string.Empty);
        WpfLocalization.Initialize(null!);
        new LocalizationCultureManager().SetCulture(Culture);
    }

    public void Dispose() => LocalizationProviderFactory.SetInstance(null!, string.Empty);

    private static object? Resolve(int count) =>
        new PluralStringLocalizerExtension { Text = "one", PluralText = "many", Count = count, Live = false }
            .ProvideValue(null!);

    [Theory]
    [InlineData(2, "2 items left")]
    [InlineData(5, "5 items left")]
    public void MoreThanOne_TakesThePluralForm(int count, string expected)
    {
        Resolve(count).Should().Be(expected);
    }

    [Fact]
    public void One_TakesTheSingularForm()
    {
        Resolve(1).Should().Be("1 item left");
    }

    [Fact]
    public void Zero_TakesTheSingularForm_WhichEnglishDoesNotExpect()
    {
        Resolve(0).Should().Be(
            "0 item left",
            "the rule is count > 1, so an English UI needs to handle zero itself");
    }
}
