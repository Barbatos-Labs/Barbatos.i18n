// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

namespace Barbatos.i18n.DependencyInjection.UnitTests;

/// <summary>
/// A region culture whose translations are registered for its neutral parent - "Strings-en.json" serving an
/// "en-GB" request, the usual layout on a server. The composite localizers searched every set only for the exact
/// culture, so a key in any named set but the first came back as missing, while the XAML lookup found it.
/// </summary>
public sealed class CompositeCultureFallbackTests
{
    private static readonly CultureInfo Region = new("en-GB");
    private static readonly CultureInfo Neutral = new("en");

    private sealed class FixedCultureManager(CultureInfo culture) : ILocalizationCultureManager
    {
        public LocalizationOptions Options { get; } = new();

        public void SetCulture(string cultureName) => throw new NotSupportedException();

        public void SetCulture(CultureInfo cultureInfo) => throw new NotSupportedException();

        public CultureInfo GetCulture() => culture;

        public IReadOnlyCollection<CultureInfo> GetSupportedCultures() => [culture];
    }

    private static LocalizationSet Set(string? name, CultureInfo culture, params (string Key, string Value)[] strings)
    {
        Dictionary<LocalizationKey, string?> dictionary = new();

        foreach ((string key, string value) in strings)
        {
            dictionary[key] = value;
        }

        return new LocalizationSet(name, culture, dictionary);
    }

    private static LocalizationProvider Provider(params LocalizationSet[] sets) => new(Region, sets);

    [Fact]
    public void FindsAKeyInALaterNamedSetOfTheParentCulture()
    {
        var localizer = new CompositeStringLocalizer(
            Provider(
                Set("errors", Neutral, ("NetworkError", "Network failed")),
                Set("labels", Neutral, ("Save", "Save changes"))),
            new FixedCultureManager(Region));

        localizer["NetworkError"].Value.Should().Be("Network failed");
        localizer["Save"].Value.Should().Be("Save changes");
        localizer["Save"].ResourceNotFound.Should().BeFalse();
    }

    [Fact]
    public void FormatsArgumentsFromAParentCultureSet()
    {
        var localizer = new CompositeStringLocalizer(
            Provider(
                Set("errors", Neutral, ("NetworkError", "Network failed")),
                Set("labels", Neutral, ("Greeting", "Hello {0}"))),
            new FixedCultureManager(Region));

        localizer["Greeting", "Hung"].Value.Should().Be("Hello Hung");
    }

    [Fact]
    public void TheExactCultureStillWinsOverItsParent()
    {
        var localizer = new CompositeStringLocalizer(
            Provider(
                Set("labels", Neutral, ("Colour", "Color"), ("Save", "Save changes")),
                Set("labels-gb", Region, ("Colour", "Colour"))),
            new FixedCultureManager(Region));

        localizer["Colour"].Value.Should().Be("Colour");
        localizer["Save"].Value.Should().Be("Save changes", "a key the exact culture lacks still comes from its parent");
    }

    [Fact]
    public void TheDefaultSetStillComesFirst()
    {
        var localizer = new CompositeStringLocalizer(
            Provider(
                Set("labels", Neutral, ("Title", "From labels")),
                Set(null, Neutral, ("Title", "From the default set"))),
            new FixedCultureManager(Region));

        localizer["Title"].Value.Should().Be("From the default set");
    }

    [Fact]
    public void TheGenericLocalizerFallsBackThroughParentsToo()
    {
        var localizer = new CompositeStringLocalizer<CompositeCultureFallbackTests>(
            Provider(
                Set("errors", Neutral, ("NetworkError", "Network failed")),
                Set("labels", Neutral, ("Save", "Save changes"))),
            new FixedCultureManager(Region));

        localizer["Save"].Value.Should().Be("Save changes");
    }

    [Fact]
    public void AKeyNoCultureInTheChainCarriesIsStillMissing()
    {
        var localizer = new CompositeStringLocalizer(
            Provider(Set("labels", Neutral, ("Save", "Save changes"))),
            new FixedCultureManager(Region));

        localizer["Absent"].ResourceNotFound.Should().BeTrue();
        localizer["Absent"].Value.Should().Be("Absent");
    }
}
