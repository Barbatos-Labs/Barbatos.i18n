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
/// An extension written without a Namespace means "find this key wherever it lives", so the lookup searches
/// every registered set in registration order. Resolving to a single set instead broke the WPF sample outright,
/// with nearly every string rendering as its raw key. MAUI carries a mirror of that lookup and had nothing
/// pinning it: no MAUI test registered more than one set or passed a Namespace at all, so the same regression
/// could have landed here unnoticed. This mirrors MultiSetResolutionTests on the WPF side.
/// </summary>
[Collection("Sequential")]
public sealed class MultiSetResolutionTests : IDisposable
{
    private static readonly CultureInfo Culture = new("en-US");

    public MultiSetResolutionTests()
    {
        var builder = new LocalizationBuilder();

        // Registered first and named - the shape FromIni/FromJson/FromCsv produce.
        builder.AddLocalization(new LocalizationSet("locales", Culture, new Dictionary<LocalizationKey, string?>
        {
            { "title", "Main Title" },
            { "greeting", "Hello" }
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

        LocalizationProviderFactory.SetInstance(builder.Build(), string.Empty);
        MauiLocalization.Initialize(null!);
    }

    public void Dispose() => LocalizationProviderFactory.SetInstance(null!, string.Empty);

    private static string Resolve(string key, string? ns = null) =>
        (string)new StringLocalizerConverter(key, ns, string.Empty)
            .Convert([], typeof(string), null!, Culture);

    [Fact]
    public void AKeyInANamedSet_ResolvesWithoutANamespace()
    {
        Resolve("greeting").Should().Be("Hello");
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
    public void ANamespaceIsMatchedCaseInsensitively()
    {
        Resolve("networkerror", "ERRORS").Should().Be("Network failed");
    }
}
