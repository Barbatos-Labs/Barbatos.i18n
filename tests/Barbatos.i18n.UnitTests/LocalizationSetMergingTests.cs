// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

namespace Barbatos.i18n.UnitTests;

/// <summary>
/// Two files can legitimately belong to one namespace - a YAML file's root-level keys and an INI file named
/// after nothing but its culture are both in the default one. Refusing that aborted startup with
/// "Localization \"\" for culture en-US already exists", so the sets are merged instead.
/// </summary>
public sealed class LocalizationSetMergingTests
{
    private static readonly CultureInfo Culture = new("en-US");

    private static LocalizationSet Set(string? name, params (string Key, string Value)[] entries) =>
        new(name, Culture, entries.ToDictionary(e => new LocalizationKey(e.Key), e => (string?)e.Value));

    [Fact]
    public void TwoSetsSharingANameAndCulture_AreMerged()
    {
        LocalizationBuilder builder = new();
        builder.AddLocalization(Set(null, ("greeting", "Hello")));
        builder.AddLocalization(Set(null, ("farewell", "Bye")));
        builder.SetCulture(Culture);

        LocalizationSet? merged = builder.Build().GetLocalizationSet(Culture, null);

        merged!["greeting"].Should().Be("Hello");
        merged["farewell"].Should().Be("Bye");
    }

    [Fact]
    public void NamedSetsMergeToo()
    {
        LocalizationBuilder builder = new();
        builder.AddLocalization(Set("errors", ("network", "Network failed")));
        builder.AddLocalization(Set("errors", ("disk", "Disk full")));
        builder.SetCulture(Culture);

        LocalizationSet? merged = builder.Build().GetLocalizationSet(Culture, "errors");

        merged!["network"].Should().Be("Network failed");
        merged["disk"].Should().Be("Disk full");
    }

    [Fact]
    public void AKeyInBoth_KeepsTheValueRegisteredFirst()
    {
        LocalizationBuilder builder = new();
        builder.AddLocalization(Set(null, ("shared", "First")));
        builder.AddLocalization(Set(null, ("shared", "Second")));
        builder.SetCulture(Culture);

        builder.Build().GetLocalizationSet(Culture, null)!["shared"].Should().Be(
            "First",
            "merging follows the same registration order a lookup does");
    }

    [Fact]
    public void MergingKeepsThePositionOfTheSetRegisteredFirst()
    {
        LocalizationBuilder builder = new();
        builder.AddLocalization(Set("first", ("a", "A")));
        builder.AddLocalization(Set("second", ("b", "B")));
        builder.AddLocalization(Set("first", ("c", "C")));
        builder.SetCulture(Culture);

        builder.Build().GetLocalizationSets(Culture).Select(s => s.Name)
            .Should().Equal("first", "second");
    }

    [Fact]
    public void SetsForDifferentCultures_AreNotMerged()
    {
        LocalizationBuilder builder = new();
        builder.AddLocalization(Set(null, ("greeting", "Hello")));
        builder.AddLocalization(new LocalizationSet(null, new CultureInfo("vi-VN"),
            new Dictionary<LocalizationKey, string?> { { "greeting", "Xin chao" } }));
        builder.SetCulture(Culture);

        ILocalizationProvider provider = builder.Build();

        provider.GetLocalizationSet(Culture, null)!["greeting"].Should().Be("Hello");
        provider.GetLocalizationSet(new CultureInfo("vi-VN"), null)!["greeting"].Should().Be("Xin chao");
    }

    [Fact]
    public void AMergedSet_IsDictionaryBacked()
    {
        LocalizationBuilder builder = new();
        builder.AddLocalization(new LocalizationSet(null, Culture,
            new[] { new KeyValuePair<LocalizationKey, string?>("a", "A") }));
        builder.AddLocalization(Set(null, ("b", "B")));
        builder.SetCulture(Culture);

        builder.Build().GetLocalizationSet(Culture, null)!.Strings
            .Should().BeAssignableTo<IReadOnlyDictionary<LocalizationKey, string?>>();
    }

    [Fact]
    public void ANullSet_Throws()
    {
        FluentActions.Invoking(() => new LocalizationBuilder().AddLocalization(null!))
            .Should().Throw<ArgumentNullException>();
    }
}
