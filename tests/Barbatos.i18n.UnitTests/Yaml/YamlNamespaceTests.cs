// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

using Barbatos.i18n.Yaml;

namespace Barbatos.i18n.UnitTests.Yaml;

/// <summary>
/// A header names a namespace, and the keys under it belong to that namespace only; keys at the root level belong
/// to the default namespace. The keys under a header used to be copied into the default namespace as well, so the
/// C# localizers - which read the default set first - answered from a YAML namespace while the XAML lookup, which
/// follows registration order, answered from another file, and two namespaces sharing a key left the default
/// namespace holding whichever came last.
/// </summary>
public sealed class YamlNamespaceTests
{
    private static readonly CultureInfo Culture = new("en-US");

    [Fact]
    public void KeysUnderAHeaderAreNotCopiedIntoTheDefaultNamespace()
    {
        IDictionary<string, IDictionary<string, string>> result =
            YamlDictionariesDeserializer.FromString("Root: Root value\nSettings:\n  Title: Application Settings\n");

        result["default"].Should().ContainKey("Root").And.NotContainKey("Title");
        result["Settings"]["Title"].Should().Be("Application Settings");
    }

    [Fact]
    public void AFileWithOnlyHeadersHasNoDefaultNamespace()
    {
        IDictionary<string, IDictionary<string, string>> result =
            YamlDictionariesDeserializer.FromString("Settings:\n  Title: Application Settings\n");

        result.Should().ContainKey("Settings").And.NotContainKey("default");
    }

    [Fact]
    public void TwoNamespacesKeepTheirOwnValueForTheSameKey()
    {
        IDictionary<string, IDictionary<string, string>> result =
            YamlDictionariesDeserializer.FromString("Settings:\n  Title: Settings title\nAbout:\n  Title: About title\n");

        result["Settings"]["Title"].Should().Be("Settings title");
        result["About"]["Title"].Should().Be("About title");
        result.Should().NotContainKey("default");
    }

    [Fact]
    public void ARootKeyAfterAHeaderBlockBelongsToTheDefaultNamespace()
    {
        // Ending a namespace used to require a line indented less than its header, which can never happen for a
        // header in column 0, so a root key written after a block was filed under that block's header.
        IDictionary<string, IDictionary<string, string>> result =
            YamlDictionariesDeserializer.FromString("Settings:\n  Title: Application Settings\nFooter: Made with Barbatos\n");

        result["default"]["Footer"].Should().Be("Made with Barbatos");
        result["Settings"].Should().ContainKey("Title").And.NotContainKey("Footer");
    }

    [Fact]
    public void FromYaml_RegistersRootKeysAsTheDefaultSetAndEachHeaderAsItsOwnSet()
    {
        LocalizationBuilder builder = new();
        builder.SetCulture(Culture);
        builder.FromYaml("Resources.Settings-en-US.yaml", Culture);

        ILocalizationProvider provider = builder.Build();

        LocalizationSet defaultSet = provider.GetLocalizationSets(Culture).Single(set => set.Name is null);
        defaultSet.Strings.Select(pair => pair.Key.ToString()).Should().BeEquivalentTo("appname", "footer");

        provider.GetLocalizationSet(Culture, "settings")!["Title"].Should().Be("Application Settings");
        provider.GetLocalizationSet(Culture, "settings")!["Theme"].Should().Be("Dark");
        provider.GetLocalizationSet(Culture, "about")!["Title"].Should().Be("About this app");
    }
}
