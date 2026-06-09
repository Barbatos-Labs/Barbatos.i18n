// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

namespace Barbatos.i18n.DependencyInjection.UnitTests;

public sealed class ProviderBasedStringLocalizerTests
{
    [Fact]
    public void Indexer_ReturnsLocalizedString()
    {
        var cultureManager = new LocalizationCultureManager();
        cultureManager.SetCulture(new CultureInfo("en-US"));

        var localizations = new LocalizationProvider(
            new CultureInfo("en-US"),
            [
                new LocalizationSet(
                    "testbase",
                    new CultureInfo("en-US"),
                    new Dictionary<LocalizationKey, string> { { "key", "value" } }!
                )
            ]
        );

        var localizer = new ProviderBasedStringLocalizer(localizations, cultureManager, "testbase");

        var result = localizer["key"];
        result.Name.Should().Be("key");
        result.Value.Should().Be("value");
    }

    [Fact]
    public void IndexerWithArgs_ReturnsFormattedString()
    {
        var cultureManager = new LocalizationCultureManager();
        cultureManager.SetCulture(new CultureInfo("en-US"));

        var localizations = new LocalizationProvider(
            new CultureInfo("en-US"),
            [
                new LocalizationSet(
                    "testbase",
                    new CultureInfo("en-US"),
                    new Dictionary<LocalizationKey, string> { { "greeting", "Hello {0}" } }!
                )
            ]
        );

        var localizer = new ProviderBasedStringLocalizer(localizations, cultureManager, "testbase");

        var result = localizer["greeting", "John"];
        result.Value.Should().Be("Hello John");
    }

    [Fact]
    public void Indexer_ReturnsKey_IfLocalizationSetNotFound()
    {
        var cultureManager = new LocalizationCultureManager();
        cultureManager.SetCulture(new CultureInfo("en-US"));

        var localizations = new LocalizationProvider(
            new CultureInfo("en-US"),
            []
        );

        var localizer = new ProviderBasedStringLocalizer(localizations, cultureManager, "testbase");

        var result = localizer["missing"];
        result.Value.Should().Be("missing");
        result.ResourceNotFound.Should().BeTrue();
    }

    [Fact]
    public void Indexer_ReturnsKey_IfKeyNotFoundInLocalizationSet()
    {
        var cultureManager = new LocalizationCultureManager();
        cultureManager.SetCulture(new CultureInfo("en-US"));

        var localizations = new LocalizationProvider(
            new CultureInfo("en-US"),
            [
                new LocalizationSet(
                    "testbase",
                    new CultureInfo("en-US"),
                    new Dictionary<LocalizationKey, string> { { "key", "value" } }!
                )
            ]
        );

        var localizer = new ProviderBasedStringLocalizer(localizations, cultureManager, "testbase");

        var result = localizer["missing", "arg"];
        result.Value.Should().Be("missing");
        result.ResourceNotFound.Should().BeTrue();
    }

    [Fact]
    public void GetAllStrings_ReturnsAllStrings()
    {
        var cultureManager = new LocalizationCultureManager();
        cultureManager.SetCulture(new CultureInfo("en-US"));

        var localizations = new LocalizationProvider(
            new CultureInfo("en-US"),
            [
                new LocalizationSet(
                    "testbase",
                    new CultureInfo("en-US"),
                    new Dictionary<LocalizationKey, string>
                    {
                        { "k1", "v1" },
                        { "k2", "v2" }
                    }!
                )
            ]
        );

        var localizer = new ProviderBasedStringLocalizer(localizations, cultureManager, "testbase");

        var result = localizer.GetAllStrings(false).ToList();
        result.Should().HaveCount(2);
        result.Should().Contain(x => x.Name == "k1" && x.Value == "v1");
        result.Should().Contain(x => x.Name == "k2" && x.Value == "v2");
    }

    [Fact]
    public void GetAllStrings_ReturnsEmpty_IfLocalizationSetNotFound()
    {
        var cultureManager = new LocalizationCultureManager();
        cultureManager.SetCulture(new CultureInfo("en-US"));

        var localizations = new LocalizationProvider(
            new CultureInfo("en-US"),
            []
        );

        var localizer = new ProviderBasedStringLocalizer(localizations, cultureManager, "testbase");

        var result = localizer.GetAllStrings(false).ToList();
        result.Should().BeEmpty();
    }
}
