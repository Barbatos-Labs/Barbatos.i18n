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
/// The converter reads the multi-binding values by position - [culture slot?] [key?] [plural key?] [args|count?] -
/// and the extension's constructor flags declare which leading slots it reserved. Add a binding without updating
/// the flags and every value shifts by one, producing wrong or blank text rather than an exception.
/// </summary>
/// <remarks>
/// Every other MAUI test builds a converter with hand-written flags, which cannot catch a disagreement because
/// it never runs the extension that sets them. These drive the real extension, take the multi-binding it emits,
/// and feed the converter values in that binding's own order - so the two halves are checked against each other
/// rather than against an assumption.
/// </remarks>
[Collection("Sequential")]
public sealed class SlotContractTests : IDisposable
{
    private static readonly CultureInfo Culture = new("en-US");

    public SlotContractTests()
    {
        var builder = new LocalizationBuilder();
        builder.AddLocalization(new LocalizationSet(null, Culture, new Dictionary<LocalizationKey, string?>
        {
            { "greeting", "Hello" },
            { "greetingwithname", "Hello {0}" },
            { "fullname", "Hello {0} {1}" },
            { "oneapple", "One apple" },
            { "manyapples", "{0} apples" }
        }));
        builder.SetCulture(Culture);

        LocalizationProviderFactory.SetInstance(builder.Build(), string.Empty);
        MauiLocalization.Initialize(null!);
    }

    public void Dispose() => LocalizationProviderFactory.SetInstance(null!, string.Empty);

    /// <summary>
    /// Runs a multi-binding's own converter over values supplied in that binding's order.
    /// </summary>
    private static string Run(BindingBase binding, params object?[] values)
    {
        var multi = binding.Should().BeOfType<MultiBinding>().Subject;

        multi.Bindings.Should().HaveCount(
            values.Length,
            "the converter reads by position, so a value has to exist for every binding the extension emitted");

        return (string)multi.Converter.Convert(values!, typeof(string), null!, Culture);
    }

    [Fact]
    public void StaticKey_ReservesOnlyTheCultureSlot()
    {
        BindingBase binding = new StringLocalizerExtension { Text = "greeting" }.ProvideValue(null!);

        Run(binding, Culture).Should().Be("Hello");
    }

    [Fact]
    public void BoundKey_ReservesTheCultureSlotThenTheKey()
    {
        BindingBase binding = new StringLocalizerExtension { BindText = new Binding("Status") }.ProvideValue(null!);

        Run(binding, Culture, "greeting").Should().Be("Hello");
    }

    [Fact]
    public void StaticKeyWithOneArgument_PutsTheArgumentAfterTheCultureSlot()
    {
        BindingBase binding = new StringLocalizerExtension { Text = "greetingwithname", Arg = "Hung" }
            .ProvideValue(null!);

        Run(binding, Culture, "Hung").Should().Be("Hello Hung");
    }

    [Fact]
    public void BoundKeyWithTwoArguments_OrdersCultureThenKeyThenArguments()
    {
        BindingBase binding = new StringLocalizerExtension
        {
            BindText = new Binding("Key"),
            BindArg = new Binding("First"),
            BindArg2 = new Binding("Last")
        }.ProvideValue(null!);

        Run(binding, Culture, "fullname", "Pham", "Hung").Should().Be("Hello Pham Hung");
    }

    [Fact]
    public void OptingOutOfLive_ResolvesOnceInsteadOfEmittingSlots()
    {
        // With nothing bound and no culture slot there is no positional contract left to honour, so the
        // extension resolves the string up front and hands over a plain Binding carrying the result.
        BindingBase binding = new StringLocalizerExtension { Text = "greetingwithname", Arg = "Hung", Live = false }
            .ProvideValue(null!);

        binding.Should().BeOfType<Binding>().Which.Source.Should().Be("Hello Hung");
    }

    [Fact]
    public void OptingOutOfLive_StillEmitsSlots_WhenSomethingIsBound()
    {
        // A bound argument means the value is not known yet, so the multi-binding survives - without the
        // culture slot, which puts the argument first.
        BindingBase binding = new StringLocalizerExtension
        {
            Text = "greetingwithname",
            BindArg = new Binding("UserName"),
            Live = false
        }.ProvideValue(null!);

        Run(binding, "Hung").Should().Be("Hello Hung");
    }

    [Fact]
    public void Plural_OrdersCultureThenBothKeysThenCount()
    {
        BindingBase binding = new PluralStringLocalizerExtension
        {
            BindText = new Binding("One"),
            BindPluralText = new Binding("Many"),
            BindCount = new Binding("Count")
        }.ProvideValue(null!);

        Run(binding, Culture, "oneapple", "manyapples", 5).Should().Be("5 apples");
    }

    [Fact]
    public void Plural_WithStaticKeys_PutsTheBoundCountAfterTheCultureSlot()
    {
        BindingBase binding = new PluralStringLocalizerExtension
        {
            Text = "oneapple",
            PluralText = "manyapples",
            BindCount = new Binding("Count")
        }.ProvideValue(null!);

        Run(binding, Culture, 5).Should().Be("5 apples");
    }
}
