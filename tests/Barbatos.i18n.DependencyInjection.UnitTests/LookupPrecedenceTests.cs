// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

namespace Barbatos.i18n.DependencyInjection.UnitTests;

/// <summary>
/// Pins which set answers when a key lives in more than one, and when a localizer falls back to the default set
/// at all. The localizers differ here on purpose - the non-generic ProviderBasedStringLocalizer falls back to the
/// default set, its generic sibling does not - and nothing else checked those differences.
/// </summary>
public sealed class LookupPrecedenceTests
{
    private static readonly CultureInfo Culture = new("en-US");

    private sealed class FixedCultureManager : ILocalizationCultureManager
    {
        public LocalizationOptions Options { get; } = new();

        public void SetCulture(string cultureName) => throw new NotSupportedException();

        public void SetCulture(CultureInfo culture) => throw new NotSupportedException();

        public CultureInfo GetCulture() => Culture;

        public IReadOnlyCollection<CultureInfo> GetSupportedCultures() => [Culture];
    }

    private static LocalizationSet Set(string? name, params (string Key, string Value)[] strings)
    {
        Dictionary<LocalizationKey, string?> dictionary = new();

        foreach ((string key, string value) in strings)
        {
            dictionary[key] = value;
        }

        return new LocalizationSet(name, Culture, dictionary);
    }

    /// <summary>
    /// A named set registered before the default one, both carrying "shared", plus a set scoped to
    /// <see cref="ScopedResource"/>.
    /// </summary>
    private static LocalizationProvider Provider() => new(
        Culture,
        [
            Set("other", ("shared", "from other"), ("onlyother", "other {0}")),
            Set(null, ("shared", "from default"), ("onlydefault", "default {0}")),
            Set(typeof(ScopedResource).FullName!.ToLowerInvariant(), ("shared", "from scoped"), ("onlyscoped", "scoped {0}"))
        ]);

    [Fact]
    public void Composite_PrefersTheDefaultSetOverAnEarlierNamedOne()
    {
        var localizer = new CompositeStringLocalizer(Provider(), new FixedCultureManager());

        localizer["shared"].Value.Should().Be("from default");
        localizer["onlyother", "x"].Value.Should().Be("other x");
        localizer["onlyother", "x"].ResourceNotFound.Should().BeFalse();
        localizer["absent", "x"].ResourceNotFound.Should().BeTrue();
        localizer["absent", "x"].Value.Should().Be("absent");
    }

    [Fact]
    public void CompositeOfT_PrefersTheScopedSetThenTheDefaultSetThenTheRest()
    {
        var localizer = new CompositeStringLocalizer<ScopedResource>(Provider(), new FixedCultureManager());

        localizer["shared"].Value.Should().Be("from scoped");
        localizer["onlydefault", 1].Value.Should().Be("default 1");
        localizer["onlyother", 2].Value.Should().Be("other 2");
        localizer["absent"].ResourceNotFound.Should().BeTrue();
    }

    [Fact]
    public void ProviderBased_FallsBackToTheDefaultSet_WhenItsOwnSetIsMissing()
    {
        var localizer = new ProviderBasedStringLocalizer(Provider(), new FixedCultureManager(), "nosuchset");

        localizer["onlydefault", "y"].Value.Should().Be("default y");
        localizer["onlyother"].ResourceNotFound.Should().BeTrue();
    }

    [Fact]
    public void ProviderBased_UsesItsOwnSet_WhenItExists()
    {
        var localizer = new ProviderBasedStringLocalizer(Provider(), new FixedCultureManager(), "other");

        localizer["shared"].Value.Should().Be("from other");
        localizer["onlydefault"].ResourceNotFound.Should().BeTrue("a set that exists is not topped up from the default one");
    }

    [Fact]
    public void ProviderBasedOfT_DoesNotFallBackToTheDefaultSet()
    {
        var localizer = new ProviderBasedStringLocalizer<UnregisteredResource>(Provider(), new FixedCultureManager());

        LocalizedString result = localizer["onlydefault"];

        result.ResourceNotFound.Should().BeTrue();
        result.Value.Should().Be("onlydefault");
    }

    [Fact]
    public void ProviderBasedOfT_ReadsTheSetNamedAfterTheResource()
    {
        var localizer = new ProviderBasedStringLocalizer<ScopedResource>(Provider(), new FixedCultureManager());

        localizer["onlyscoped", "z"].Value.Should().Be("scoped z");
        localizer.GetAllStrings(includeParentCultures: false).Should().HaveCount(2);
    }

    [Fact]
    public void LocalizationSetLocalizer_FormatsOnlyWhenArgumentsAreGiven()
    {
        var localizer = new LocalizationSetStringLocalizer(Set(null, ("literal", "100% {0}")));

        localizer["literal"].Value.Should().Be("100% {0}", "without arguments the text is returned as written");
        localizer["literal", "done"].Value.Should().Be("100% done");
    }

    /// <summary>
    /// Scopes a generic localizer to the set named after this type.
    /// </summary>
    public sealed class ScopedResource;

    /// <summary>
    /// A resource type that no set is named after.
    /// </summary>
    public sealed class UnregisteredResource;
}
