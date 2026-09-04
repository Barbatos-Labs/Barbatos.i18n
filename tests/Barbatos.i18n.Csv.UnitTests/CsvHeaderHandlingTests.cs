// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

using System.Globalization;
using AwesomeAssertions;
using Barbatos.i18n;
using Barbatos.i18n.Csv;

namespace Barbatos.i18n.Csv.UnitTests;

/// <summary>
/// The first CSV row always defines the columns. It used to be skipped only when its first cell was spelled
/// "Key", so any other header spelling was registered as a translation entry as well.
/// </summary>
public class CsvHeaderHandlingTests
{
    [Fact]
    public void FromCsv_DoesNotInsertTheHeaderAsData_WhenFirstColumnIsNotNamedKey()
    {
        LocalizationBuilder builder = new();
        builder.FromCsvString("t", "Name,en-US\nGreeting,Hello");

        ILocalizationProvider provider = builder.Build();
        LocalizationSet? set = provider.GetLocalizationSet(new CultureInfo("en-US"), "t");

        _ = set.Should().NotBeNull();
        _ = set!["Greeting"].Should().Be("Hello");
        _ = set!["Name"].Should().BeNull("the header row must not become a translation entry");
    }

    [Fact]
    public void FromCsv_DoesNotInsertTheHeaderAsData_ForSingleCultureFiles()
    {
        LocalizationBuilder builder = new();
        builder.FromCsvString("t", new CultureInfo("en-US"), "Name,Value\nGreeting,Hello");

        ILocalizationProvider provider = builder.Build();
        LocalizationSet? set = provider.GetLocalizationSet(new CultureInfo("en-US"), "t");

        _ = set.Should().NotBeNull();
        _ = set!["Greeting"].Should().Be("Hello");
        _ = set!["Name"].Should().BeNull("the header row must not become a translation entry");
    }

    [Fact]
    public void FromCsv_StillSkipsTheHeader_WhenFirstColumnIsNamedKey()
    {
        LocalizationBuilder builder = new();
        builder.FromCsvString("t", "Key,en-US\nGreeting,Hello");

        ILocalizationProvider provider = builder.Build();
        LocalizationSet? set = provider.GetLocalizationSet(new CultureInfo("en-US"), "t");

        _ = set.Should().NotBeNull();
        _ = set!["Greeting"].Should().Be("Hello");
        _ = set!["Key"].Should().BeNull();
    }

    [Fact]
    public void FromCsv_IgnoresATrailingCommaInTheHeader()
    {
        // A trailing comma used to register a culture named "", which is the sentinel for a single-culture
        // file, so the multi-culture overload rejected the file with a misleading error.
        LocalizationBuilder builder = new();

        Action act = () => builder.FromCsvString("t", "Key,en-US,vi-VN,\nGreeting,Hello,Xin chào");

        _ = act.Should().NotThrow();

        ILocalizationProvider provider = builder.Build();
        _ = provider.GetLocalizationSet(new CultureInfo("en-US"), "t")!["Greeting"].Should().Be("Hello");
        _ = provider.GetLocalizationSet(new CultureInfo("vi-VN"), "t")!["Greeting"].Should().Be("Xin chào");
    }
}
