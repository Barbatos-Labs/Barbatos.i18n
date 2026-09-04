// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

namespace Barbatos.i18n.UnitTests;

/// <summary>
/// The constructor takes IEnumerable, so a caller can hand it a deferred query. Storing that sequence unmaterialized
/// made every lookup re-run it, and lookups are on the hot path: one culture change re-evaluates every live binding.
/// </summary>
public sealed class LocalizationProviderMaterializationTests
{
    private static readonly CultureInfo Culture = new("en-US");

    [Fact]
    public void DeferredQuery_IsEnumeratedOnlyOnce_RegardlessOfLookupCount()
    {
        int enumerations = 0;

        IEnumerable<LocalizationSet> Deferred()
        {
            enumerations++;
            yield return new LocalizationSet(
                null,
                Culture,
                new Dictionary<LocalizationKey, string?> { { "greeting", "Hello" } });
        }

        var provider = new LocalizationProvider(Culture, Deferred());

        for (int i = 0; i < 5; i++)
        {
            provider.GetLocalizationSet(Culture, null).Should().NotBeNull();
            provider.GetLocalizationSets(Culture).Count().Should().Be(1);
        }

        enumerations.Should().Be(1, "the sets are copied once in the constructor");
    }

    [Fact]
    public void MutatingTheSourceCollectionAfterConstruction_DoesNotChangeTheProvider()
    {
        List<LocalizationSet> sets =
        [
            new(null, Culture, new Dictionary<LocalizationKey, string?> { { "greeting", "Hello" } })
        ];

        var provider = new LocalizationProvider(Culture, sets);

        sets.Clear();

        provider.GetLocalizationSet(Culture, null).Should().NotBeNull();
        provider.GetLocalizationSets().Count().Should().Be(1);
    }

    [Fact]
    public void NullSets_ThrowArgumentNullException()
    {
        Action act = () => new LocalizationProvider(Culture, null!);

        act.Should().Throw<ArgumentNullException>();
    }
}
