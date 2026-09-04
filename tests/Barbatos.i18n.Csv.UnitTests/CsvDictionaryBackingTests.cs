// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

using System.Globalization;
using AwesomeAssertions;
using Barbatos.i18n;

namespace Barbatos.i18n.Csv.UnitTests;

/// <summary>
/// LocalizationSet indexes a set by key on every lookup and only takes its O(1) path when the strings are
/// dictionary-backed. The CSV parser accumulated entries in a List, so a CSV-backed set was alone in scanning
/// all of its entries for every key - on a path that re-runs for every live binding on every culture change.
/// </summary>
public sealed class CsvDictionaryBackingTests
{
    private const string SingleCulture = "Key,Value\nGreeting,Hello\nFarewell,Bye\n";
    private const string MultiCulture = "Key,en-US,vi-VN\nGreeting,Hello,Xin chao\n";

    [Fact]
    public void ASingleCultureFile_ProducesADictionaryBackedSet()
    {
        LocalizationBuilder builder = new();
        builder.FromCsvString("t", new CultureInfo("en-US"), SingleCulture);

        LocalizationSet? set = builder.Build().GetLocalizationSet(new CultureInfo("en-US"), "t");

        set!.Strings.Should().BeAssignableTo<IReadOnlyDictionary<LocalizationKey, string?>>();
        set["Greeting"].Should().Be("Hello");
        set["Farewell"].Should().Be("Bye");
    }

    [Fact]
    public void AMultiCultureFile_ProducesDictionaryBackedSets()
    {
        LocalizationBuilder builder = new();
        builder.FromCsvString("t", MultiCulture);

        ILocalizationProvider provider = builder.Build();

        foreach (string culture in new[] { "en-US", "vi-VN" })
        {
            LocalizationSet? set = provider.GetLocalizationSet(new CultureInfo(culture), "t");
            set!.Strings.Should().BeAssignableTo<IReadOnlyDictionary<LocalizationKey, string?>>();
        }

        provider.GetLocalizationSet(new CultureInfo("vi-VN"), "t")!["Greeting"].Should().Be("Xin chao");
    }
}
