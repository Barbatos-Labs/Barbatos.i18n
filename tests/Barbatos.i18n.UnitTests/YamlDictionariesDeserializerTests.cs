// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

using Barbatos.i18n.Yaml;

namespace Barbatos.i18n.UnitTests;

public sealed class YamlDictionariesDeserializerTests
{
    [Fact]
    public void Parse_KeepsKey_WhenValueEndsWithColon()
    {
        // Labels ending in a colon are everywhere in UI localization; they must not be mistaken for a section.
        IDictionary<string, IDictionary<string, string>> result =
            YamlDictionariesDeserializer.FromString("EnterName: Enter Name:\nOther: ok");

        result["default"].Should().ContainKey("EnterName");
        result["default"]["EnterName"].Should().Be("Enter Name:");
        result["default"]["Other"].Should().Be("ok");
    }

    [Fact]
    public void Parse_DoesNotCreateNamespace_FromAValueThatEndsWithColon()
    {
        IDictionary<string, IDictionary<string, string>> result =
            YamlDictionariesDeserializer.FromString("EnterName: Enter Name:\nOther: ok");

        result.Should().NotContainKey("EnterName: Enter Name");
    }

    [Fact]
    public void Parse_StillTreatsABareKeyWithNothingAfterTheColonAsANamespace()
    {
        IDictionary<string, IDictionary<string, string>> result =
            YamlDictionariesDeserializer.FromString("Settings:\n  Title: Application Settings\n  Theme: Theme");

        result.Should().ContainKey("Settings");
        result["Settings"]["Title"].Should().Be("Application Settings");
        result["Settings"]["Theme"].Should().Be("Theme");
    }

    [Theory]
    [InlineData("'")]
    [InlineData("\"")]
    public void Parse_DoesNotThrow_WhenValueIsASingleQuoteCharacter(string quote)
    {
        Action act = () => YamlDictionariesDeserializer.FromString($"Apostrophe: {quote}");

        act.Should().NotThrow();
    }

    [Fact]
    public void Parse_KeepsASingleQuoteCharacterAsTheValue()
    {
        IDictionary<string, IDictionary<string, string>> result =
            YamlDictionariesDeserializer.FromString("Apostrophe: '");

        result["default"]["Apostrophe"].Should().Be("'");
    }

    [Fact]
    public void Parse_StillStripsMatchingSurroundingQuotes()
    {
        IDictionary<string, IDictionary<string, string>> result =
            YamlDictionariesDeserializer.FromString("Quoted: \"Enter Name:\"\nSingle: 'hello'");

        result["default"]["Quoted"].Should().Be("Enter Name:");
        result["default"]["Single"].Should().Be("hello");
    }
}
