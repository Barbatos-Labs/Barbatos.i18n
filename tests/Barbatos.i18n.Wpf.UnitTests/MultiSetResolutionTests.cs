// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

using System.Globalization;
using Barbatos.i18n;
using Barbatos.i18n.Wpf;
using AwesomeAssertions;

namespace Barbatos.i18n.Wpf.UnitTests;

/// <summary>
/// An extension written without a Namespace argument must find its key in whichever registered file holds it.
/// Resolving to a single set instead broke the WPF sample outright: its INI file is registered under a name
/// derived from the file name, while the only unnamed set is the handful of keys a YAML file contributes to its
/// implicit default namespace, so nearly every string rendered as its raw key.
/// </summary>
[Collection("Sequential")]
public sealed class MultiSetResolutionTests : IDisposable
{
    private static readonly CultureInfo Culture = new("en-US");

    public MultiSetResolutionTests()
    {
        var builder = new LocalizationBuilder();

        // Registered first, and named - this is the shape FromIni/FromJson/FromCsv produce.
        builder.AddLocalization(new LocalizationSet("en-us", Culture, new Dictionary<LocalizationKey, string?>
        {
            { "title", "Main Title" },
            { "greeting", "Hello {0}" },
            { "oneapple", "One apple" },
            { "manyapples", "{0} apples" }
        }));

        // Registered later and unnamed - the incidental default namespace a YAML file contributes.
        builder.AddLocalization(new LocalizationSet(null, Culture, new Dictionary<LocalizationKey, string?>
        {
            { "title", "Settings Title" },
            { "theme", "Theme" }
        }));

        builder.AddLocalization(new LocalizationSet("errors", Culture, new Dictionary<LocalizationKey, string?>
        {
            { "networkerror", "Network failed" }
        }));

        builder.SetCulture(Culture);

        LocalizationProviderFactory.SetInstance(builder.Build(), "");
        WpfLocalization.Initialize(null!);
        new LocalizationCultureManager().SetCulture(Culture);
    }

    public void Dispose() => LocalizationProviderFactory.SetInstance(null!, "");

    private static object? Resolve(string key, string? ns = null) =>
        new StringLocalizerExtension { Text = key, Namespace = ns, Live = false }.ProvideValue(null!);

    [Fact]
    public void AKeyInANamedSet_ResolvesWithoutANamespace()
    {
        Resolve("greeting").Should().Be("Hello {0}");
    }

    [Fact]
    public void AKeyInAnyRegisteredSet_ResolvesWithoutANamespace()
    {
        Resolve("networkerror").Should().Be("Network failed");
        Resolve("theme").Should().Be("Theme");
    }

    [Fact]
    public void ADuplicatedKey_ResolvesToTheSetRegisteredFirst()
    {
        Resolve("title").Should().Be(
            "Main Title",
            "registration order decides, so an incidental unnamed set cannot outrank the first registered file");
    }

    [Fact]
    public void ANamespaceStillScopesTheLookup()
    {
        Resolve("networkerror", "errors").Should().Be("Network failed");
        Resolve("greeting", "errors").Should().Be("greeting", "a scoped lookup must not leak into other sets");
    }

    [Fact]
    public void AnUnknownKey_RendersAsItself()
    {
        Resolve("nosuchkey").Should().Be("nosuchkey");
    }

    [Fact]
    public void FormatArgumentsStillApply()
    {
        new StringLocalizerExtension { Text = "greeting", Arg = "Hung", Live = false }
            .ProvideValue(null!).Should().Be("Hello Hung");
    }

    [Fact]
    public void PluralResolvesAcrossSetsToo()
    {
        new PluralStringLocalizerExtension { Text = "OneApple", PluralText = "ManyApples", Count = 5, Live = false }
            .ProvideValue(null!).Should().Be("5 apples");
    }
}
