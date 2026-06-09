// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

namespace Barbatos.i18n.DependencyInjection.UnitTests;

public sealed class LocalizationSetStringLocalizerTests
{
    [Fact]
    public void Indexer_ReturnsLocalizedString()
    {
        var set = new LocalizationSet(
            "namespace",
            new CultureInfo("en-US"),
            new Dictionary<LocalizationKey, string> { { "key1", "value1" } }!
        );

        var localizer = new LocalizationSetStringLocalizer(set);

        var result = localizer["key1"];
        result.Name.Should().Be("key1");
        result.Value.Should().Be("value1");
        result.ResourceNotFound.Should().BeFalse();
    }

    [Fact]
    public void Indexer_ReturnsKeyIfNotFound()
    {
        var set = new LocalizationSet(
            "namespace",
            new CultureInfo("en-US"),
            []
        );

        var localizer = new LocalizationSetStringLocalizer(set);

        var result = localizer["missing"];
        result.Name.Should().Be("missing");
        result.Value.Should().Be("missing");
    }

    [Fact]
    public void IndexerWithArguments_FormatsString()
    {
        var set = new LocalizationSet(
            "namespace",
            new CultureInfo("en-US"),
            new Dictionary<LocalizationKey, string> { { "key1", "Hello {0}" } }!
        );

        var localizer = new LocalizationSetStringLocalizer(set);

        var result = localizer["key1", "World"];
        result.Value.Should().Be("Hello World");
    }

    [Fact]
    public void GetAllStrings_ReturnsAllStringsInSet()
    {
        var set = new LocalizationSet(
            "namespace",
            new CultureInfo("en-US"),
            new Dictionary<LocalizationKey, string>
            {
                { "key1", "value1" },
                { "key2", "value2" }
            }!
        );

        var localizer = new LocalizationSetStringLocalizer(set);

        var result = localizer.GetAllStrings(true).ToList();
        result.Should().HaveCount(2);
        result.Should().Contain(x => x.Name == "key1" && x.Value == "value1");
        result.Should().Contain(x => x.Name == "key2" && x.Value == "value2");
    }
}
