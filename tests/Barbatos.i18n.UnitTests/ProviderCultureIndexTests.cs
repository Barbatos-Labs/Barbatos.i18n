// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

namespace Barbatos.i18n.UnitTests;

/// <summary>
/// The provider indexes its sets by culture so a lookup does not rescan every set for each culture it tries.
/// The index has to preserve exactly what the previous linear scan matched, registration order included, since
/// registration order is what decides precedence when a key lives in more than one set.
/// </summary>
public sealed class ProviderCultureIndexTests
{
    private static readonly CultureInfo English = new("en-US");
    private static readonly CultureInfo Neutral = new("en");
    private static readonly CultureInfo Vietnamese = new("vi-VN");

    private static LocalizationProvider Build() =>
        new(English, [
            new LocalizationSet("first", English, new Dictionary<LocalizationKey, string?> { { "shared", "First" } }),
            new LocalizationSet("second", English, new Dictionary<LocalizationKey, string?> { { "shared", "Second" } }),
            new LocalizationSet(null, English, new Dictionary<LocalizationKey, string?> { { "shared", "Unnamed" } }),
            new LocalizationSet("neutral", Neutral, new Dictionary<LocalizationKey, string?> { { "only", "Neutral" } }),
            new LocalizationSet("vi", Vietnamese, new Dictionary<LocalizationKey, string?> { { "shared", "Viet" } })
        ]);

    [Fact]
    public void GetLocalizationSets_ReturnsOnlyTheExactCulture_InRegistrationOrder()
    {
        Build().GetLocalizationSets(English).Select(s => s.Name)
            .Should().Equal("first", "second", null);
    }

    [Fact]
    public void GetLocalizationSets_ForAnUnregisteredCulture_IsEmpty()
    {
        Build().GetLocalizationSets(new CultureInfo("ko-KR")).Should().BeEmpty();
    }

    [Fact]
    public void GetLocalizationSets_MatchesACultureInstanceItWasNotBuiltWith()
    {
        // The index is keyed by name, so a different CultureInfo instance for the same culture still matches.
        Build().GetLocalizationSets(new CultureInfo("en-US")).Should().HaveCount(3);
    }

    [Fact]
    public void GetLocalizationSet_ByName_IsCaseInsensitive()
    {
        Build().GetLocalizationSet(English, "FIRST")!["shared"].Should().Be("First");
    }

    [Fact]
    public void GetLocalizationSet_WithoutAName_PrefersTheUnnamedSet()
    {
        Build().GetLocalizationSet(English, null)!["shared"].Should().Be("Unnamed");
    }

    [Fact]
    public void GetLocalizationSet_FallsBackThroughParentCultures()
    {
        Build().GetLocalizationSet(English, "neutral")!["only"].Should().Be("Neutral");
    }

    [Fact]
    public void GetLocalizationSets_WithoutACulture_StillReturnsEveryRegisteredSet()
    {
        Build().GetLocalizationSets().Should().HaveCount(5);
    }
}
