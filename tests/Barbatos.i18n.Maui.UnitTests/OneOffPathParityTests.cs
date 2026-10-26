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
/// An extension resolves its text along one of two paths: the live multi-binding, whose converter runs on every
/// culture change, and the one-off path taken with <c>Live = false</c>. Both have to produce the same text for
/// the same input, and the existing tests only checked the one-off path for two plural fallbacks. These pin
/// what each path renders and compare the two against each other, so the paths cannot drift apart unnoticed.
/// </summary>
[Collection("Sequential")]
public sealed class OneOffPathParityTests : IDisposable
{
    public void Dispose() => LocalizationProviderFactory.SetInstance(null!, string.Empty);

    private static void UseProvider(string culture)
    {
        var builder = new LocalizationBuilder();
        builder.AddLocalization(new LocalizationSet(null, new CultureInfo("en-US"), new Dictionary<LocalizationKey, string?>
        {
            { "one", "{0} item left" },
            { "many", "{0} items left" },
            { "fullname", "Hello {0} {1}" },
            { "three", "{0}-{1}-{2}" }
        }));
        builder.AddLocalization(new LocalizationSet("extra", new CultureInfo("en-US"), new Dictionary<LocalizationKey, string?>
        {
            { "one", "{0} extra item" },
            { "many", "{0} extra items" }
        }));
        builder.AddLocalization(new LocalizationSet(null, new CultureInfo("fr-FR"), new Dictionary<LocalizationKey, string?>
        {
            { "one", "{0} article restant" },
            { "many", "{0} articles restants" }
        }));
        builder.SetCulture(new CultureInfo(culture));

        LocalizationProviderFactory.SetInstance(builder.Build(), string.Empty);
        MauiLocalization.Initialize(null!);
    }

    private static string OneOff(PluralStringLocalizerExtension extension)
    {
        extension.Live = false;

        return (string)extension.ProvideValue(null!).Should().BeOfType<Binding>().Subject.Source;
    }

    private static string Live(PluralStringLocalizerExtension extension)
    {
        MultiBinding multi = extension.ProvideValue(null!).Should().BeOfType<MultiBinding>().Subject;

        return (string)multi.Converter.Convert([CultureInfo.CurrentUICulture], typeof(string), null!, CultureInfo.CurrentCulture);
    }

    [Theory]
    [InlineData("en-US", 0, "0 items left")]
    [InlineData("en-US", 1, "1 item left")]
    [InlineData("en-US", 2, "2 items left")]
    [InlineData("fr-FR", 0, "0 article restant")]
    [InlineData("fr-FR", 1, "1 article restant")]
    [InlineData("fr-FR", 2, "2 articles restants")]
    public void Plural_BothPathsSelectTheSameForm(string culture, int count, string expected)
    {
        UseProvider(culture);

        OneOff(new PluralStringLocalizerExtension { Text = "one", PluralText = "many", Count = count })
            .Should().Be(expected);
        Live(new PluralStringLocalizerExtension { Text = "one", PluralText = "many", Count = count })
            .Should().Be(expected);
    }

    [Theory]
    [InlineData(null, "many", 1, "1 items left")]
    [InlineData("one", null, 3, "3 item left")]
    [InlineData("", "many", 1, "1 items left")]
    public void Plural_BothPathsFallBackToTheFormThatWasSupplied(string? text, string? pluralText, int count, string expected)
    {
        UseProvider("en-US");

        OneOff(new PluralStringLocalizerExtension { Text = text, PluralText = pluralText, Count = count })
            .Should().Be(expected);
        Live(new PluralStringLocalizerExtension { Text = text, PluralText = pluralText, Count = count })
            .Should().Be(expected);
    }

    [Fact]
    public void Plural_BothPathsHonourTheNamespace()
    {
        UseProvider("en-US");

        OneOff(new PluralStringLocalizerExtension { Text = "one", PluralText = "many", Count = 2, Namespace = "EXTRA" })
            .Should().Be("2 extra items");
        Live(new PluralStringLocalizerExtension { Text = "one", PluralText = "many", Count = 2, Namespace = "EXTRA" })
            .Should().Be("2 extra items");
    }

    [Fact]
    public void Plural_BothPathsEchoAMissingKeyUnescaped()
    {
        UseProvider("en-US");

        OneOff(new PluralStringLocalizerExtension { Text = " {0} fish &amp; chips ", PluralText = "unknown", Count = 1 })
            .Should().Be("1 fish & chips");
        Live(new PluralStringLocalizerExtension { Text = " {0} fish &amp; chips ", PluralText = "unknown", Count = 1 })
            .Should().Be("1 fish & chips");
    }

    [Fact]
    public void Plural_AMissingCountReadsAsZero()
    {
        UseProvider("en-US");

        OneOff(new PluralStringLocalizerExtension { Text = "one", PluralText = "many" }).Should().Be("0 items left");
        Live(new PluralStringLocalizerExtension { Text = "one", PluralText = "many" }).Should().Be("0 items left");
    }

    [Fact]
    public void Plural_OneOffPathAppliesTheOuterStringFormat()
    {
        UseProvider("en-US");

        OneOff(new PluralStringLocalizerExtension { Text = "one", PluralText = "many", Count = 4, StringFormat = "[{0}]" })
            .Should().Be("[4 items left]");
    }

    [Fact]
    public void Text_OneOffPathKeepsArgumentOrderWhenOneIsSkipped()
    {
        // Arguments are collected in declaration order and an unset one is skipped rather than left as a hole,
        // so Arg and Arg3 fill {0} and {1}.
        UseProvider("en-US");

        BindingBase binding = new StringLocalizerExtension { Text = "fullname", Arg = "Pham", Arg3 = "Hung", Live = false }
            .ProvideValue(null!);

        binding.Should().BeOfType<Binding>().Which.Source.Should().Be("Hello Pham Hung");
    }

    [Fact]
    public void Text_OneOffPathPassesEveryArgument()
    {
        UseProvider("en-US");

        BindingBase binding = new StringLocalizerExtension { Text = "three", Arg2 = 1, Arg4 = 2, Arg5 = 3, Live = false }
            .ProvideValue(null!);

        binding.Should().BeOfType<Binding>().Which.Source.Should().Be("1-2-3");
    }
}
