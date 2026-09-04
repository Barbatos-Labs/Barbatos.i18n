// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

namespace Barbatos.i18n.UnitTests;

/// <summary>
/// Translations registered for a neutral culture must serve its specific children, the way ResourceManager
/// falls back from a satellite assembly. Lookups used to match the culture exactly, so an application
/// registering "en" and running under an ambient "en-GB" found nothing and rendered every key raw.
/// </summary>
public sealed class CultureFallbackTests
{
    [Fact]
    public void EnumerateChain_WalksFromSpecificToInvariant()
    {
        string[] chain = CultureFallback.EnumerateChain(new CultureInfo("en-GB"))
            .Select(c => c.Name)
            .ToArray();

        chain.Should().Equal("en-GB", "en", string.Empty);
    }

    [Fact]
    public void EnumerateChain_TerminatesOnTheInvariantCulture()
    {
        // The invariant culture is its own parent, so a naive walk would never end.
        string[] chain = CultureFallback.EnumerateChain(CultureInfo.InvariantCulture)
            .Select(c => c.Name)
            .ToArray();

        chain.Should().Equal(string.Empty);
    }

    [Fact]
    public void EnumerateChain_Throws_ForNull()
    {
        Action act = () => CultureFallback.EnumerateChain(null!).ToArray();

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Lookup_FallsBackFromSpecificToNeutralCulture()
    {
        var provider = new LocalizationProvider(new CultureInfo("en"), [NeutralEnglish()]);

        LocalizationSet? set = provider.GetLocalizationSet(new CultureInfo("en-GB"), null);

        set.Should().NotBeNull();
        set!["greeting"].Should().Be("Hello");
    }

    [Fact]
    public void Lookup_PrefersTheExactCulture_OverAParent()
    {
        var provider = new LocalizationProvider(new CultureInfo("en"), [NeutralEnglish(), BritishEnglish()]);

        provider.GetLocalizationSet(new CultureInfo("en-GB"), null)!["greeting"]
            .Should().Be("Good day", "an exact match must never lose to a parent");

        provider.GetLocalizationSet(new CultureInfo("en-US"), null)!["greeting"]
            .Should().Be("Hello", "a sibling culture still falls back to the neutral parent");
    }

    [Fact]
    public void Lookup_FallsBackToTheInvariantCulture()
    {
        var provider = new LocalizationProvider(
            CultureInfo.InvariantCulture,
            [
                new LocalizationSet(
                    null,
                    CultureInfo.InvariantCulture,
                    new Dictionary<LocalizationKey, string?> { { "greeting", "Hello" } })
            ]);

        provider.GetLocalizationSet(new CultureInfo("vi-VN"), null)!["greeting"].Should().Be("Hello");
    }

    [Fact]
    public void Lookup_HonoursTheSetNameAtEachLevelOfTheChain()
    {
        var provider = new LocalizationProvider(new CultureInfo("en"), [NeutralErrors()]);

        provider.GetLocalizationSet(new CultureInfo("en-GB"), "errors")!["networkerror"]
            .Should().Be("Network failed");

        provider.GetLocalizationSet(new CultureInfo("en-GB"), "missing").Should().BeNull();
    }

    [Fact]
    public void Lookup_ReturnsNull_WhenNoCultureInTheChainHasASet()
    {
        var provider = new LocalizationProvider(new CultureInfo("en"), [NeutralEnglish()]);

        provider.GetLocalizationSet(new CultureInfo("vi-VN"), null).Should().BeNull();
    }

    [Fact]
    public void GetLocalizationSets_StaysExact_SoCallersCanStillScopeToOneCulture()
    {
        var provider = new LocalizationProvider(new CultureInfo("en"), [NeutralEnglish()]);

        provider.GetLocalizationSets(new CultureInfo("en-GB")).Should().BeEmpty();
        provider.GetLocalizationSets(new CultureInfo("en")).Should().ContainSingle();
    }

    private static LocalizationSet NeutralEnglish() =>
        new(null, new CultureInfo("en"), new Dictionary<LocalizationKey, string?> { { "greeting", "Hello" } });

    private static LocalizationSet BritishEnglish() =>
        new(null, new CultureInfo("en-GB"), new Dictionary<LocalizationKey, string?> { { "greeting", "Good day" } });

    private static LocalizationSet NeutralErrors() =>
        new("errors", new CultureInfo("en"), new Dictionary<LocalizationKey, string?> { { "networkerror", "Network failed" } });
}
