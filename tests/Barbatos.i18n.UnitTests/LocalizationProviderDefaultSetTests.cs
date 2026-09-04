// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

namespace Barbatos.i18n.UnitTests;

/// <summary>
/// Asking for a null name means "the default set". It used to filter on culture alone, so whichever named set
/// happened to be enumerated first was returned and keys living in the default set were reported missing.
/// </summary>
public sealed class LocalizationProviderDefaultSetTests
{
    private static readonly CultureInfo Culture = new("en-US");

    private static LocalizationSet Named() =>
        new("errors", Culture, new Dictionary<LocalizationKey, string?> { { "networkerror", "Network failed" } });

    private static LocalizationSet Unnamed() =>
        new(null, Culture, new Dictionary<LocalizationKey, string?> { { "greeting", "Hello" } });

    [Fact]
    public void NullName_PrefersTheUnnamedSet_EvenWhenANamedSetComesFirst()
    {
        var provider = new LocalizationProvider(Culture, [Named(), Unnamed()]);

        LocalizationSet? resolved = provider.GetLocalizationSet(Culture, null);

        resolved.Should().NotBeNull();
        resolved!.Name.Should().BeNull();
        resolved["greeting"].Should().Be("Hello");
    }

    [Fact]
    public void NullName_PrefersTheUnnamedSet_RegardlessOfRegistrationOrder()
    {
        var provider = new LocalizationProvider(Culture, [Unnamed(), Named()]);

        provider.GetLocalizationSet(Culture, null)!.Name.Should().BeNull();
    }

    [Fact]
    public void NullName_FallsBackToTheFirstSet_WhenNoUnnamedSetIsRegistered()
    {
        // Back-compat: a provider holding only named sets keeps answering with one of them, which is what
        // the single-argument GetLocalizationSet(cultureName) overload has always relied on.
        var provider = new LocalizationProvider(Culture, [Named()]);

        LocalizationSet? resolved = provider.GetLocalizationSet(Culture, null);

        resolved.Should().NotBeNull();
        resolved!.Name.Should().Be("errors");
    }

    [Fact]
    public void NamedLookup_IsUnaffected()
    {
        var provider = new LocalizationProvider(Culture, [Named(), Unnamed()]);

        provider.GetLocalizationSet(Culture, "errors")!["networkerror"].Should().Be("Network failed");
        provider.GetLocalizationSet(Culture, "ERRORS")!.Name.Should().Be("errors");
        provider.GetLocalizationSet(Culture, "missing").Should().BeNull();
    }

    [Fact]
    public void NullName_ReturnsNull_WhenTheCultureHasNoSets()
    {
        var provider = new LocalizationProvider(Culture, [Named(), Unnamed()]);

        provider.GetLocalizationSet(new CultureInfo("ko-KR"), null).Should().BeNull();
    }
}
