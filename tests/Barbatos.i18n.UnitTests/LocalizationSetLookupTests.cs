// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

using Barbatos.i18n.IO;
using Barbatos.i18n.UnitTests.Resources;

namespace Barbatos.i18n.UnitTests;

/// <summary>
/// The set indexer guesses a key from the caller's property name so that a generated RESX designer works, but
/// the value actually passed has to win. Guessing first meant set[row.Status], with a Status of "Active",
/// silently returned the translation of the "Status" column header instead.
/// </summary>
public sealed class LocalizationSetLookupTests
{
    private sealed record Row(string Status);

    private static LocalizationSet Set() =>
        new("s", new CultureInfo("en-US"), new Dictionary<LocalizationKey, string?>
        {
            { "active", "Currently selling" },
            { "status", "Status column header" }
        });

    [Fact]
    public void APropertysValue_ResolvesAsTheKey_NotThePropertyName()
    {
        var row = new Row("Active");

        Set()[row.Status].Should().Be("Currently selling");
    }

    [Fact]
    public void APropertyName_StillResolvesWhenItsValueIsNotAKey()
    {
        // This is the RESX pattern: the designer property returns a translation, never a key.
        var row = new Row("no such key");

        Set()[row.Status].Should().Be("Status column header");
    }

    [Fact]
    public void ALiteralIsNeverTreatedAsAPropertyPath()
    {
        Set()["no such key"].Should().BeNull();
    }

    [Fact]
    public void TheResxDesignerPatternStillWorks()
    {
        LocalizationBuilder builder = new();
        builder.FromResource<TestResource>(new CultureInfo("ko-KR"));

        LocalizationSet? set = builder.Build().GetLocalizationSet(new CultureInfo("ko-KR"), null);

        set![TestResource.Test].Should().Be("안녕하세요");
    }
}
