// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

using System.Globalization;
using AwesomeAssertions;
using Barbatos.i18n;
using Barbatos.i18n.Json;

namespace Barbatos.i18n.Json.UnitTests;

/// <summary>
/// The loader reads files with JsonDocument instead of reflection-based serialization, so the behaviours that
/// used to come from JsonSerializerOptions - case-insensitive property names and trailing commas - are pinned
/// here.
/// </summary>
public class JsonDocumentParsingTests
{
    private static LocalizationSet Load(string json)
    {
        LocalizationBuilder builder = new();
        builder.FromJsonString(json, new CultureInfo("en-US"));

        return builder.Build().GetLocalizationSet(new CultureInfo("en-US"), null)!;
    }

    [Theory]
    [InlineData("Version", "Strings", "Name", "Value")]
    [InlineData("version", "strings", "name", "value")]
    [InlineData("VERSION", "STRINGS", "NAME", "VALUE")]
    public void Version1_PropertyNamesAreCaseInsensitive(string version, string strings, string name, string value)
    {
        LocalizationSet set = Load(
            $"{{\"{version}\":\"1.0\",\"{strings}\":[{{\"{name}\":\"Greeting\",\"{value}\":\"Hello\"}}]}}");

        _ = set["Greeting"].Should().Be("Hello");
    }

    [Fact]
    public void TrailingCommasAreStillAccepted()
    {
        LocalizationSet set = Load("{\"version\": \"2.0\", \"Greeting\": \"Hello\", }");

        _ = set["Greeting"].Should().Be("Hello");
    }

    [Fact]
    public void Version1_EntryWithANullValue_KeepsTheKey()
    {
        LocalizationSet set = Load("{\"version\":\"1.0\",\"strings\":[{\"name\":\"Greeting\",\"value\":null}]}");

        _ = set["Greeting"].Should().BeNull();
    }

    [Fact]
    public void Version1_EntryWithANonStringValue_ThrowsLocalizationBuilderException()
    {
        Action act = () => Load("{\"version\":\"1.0\",\"strings\":[{\"name\":\"Greeting\",\"value\":42}]}");

        _ = act.Should().Throw<LocalizationBuilderException>().WithMessage("*value*");
    }

    [Fact]
    public void Version1_StringsThatIsNotAnArray_ThrowsLocalizationBuilderException()
    {
        Action act = () => Load("{\"version\":\"1.0\",\"strings\":{\"Greeting\":\"Hello\"}}");

        _ = act.Should().Throw<LocalizationBuilderException>().WithMessage("*strings*");
    }

    [Fact]
    public void NonObjectRoot_ThrowsLocalizationBuilderException()
    {
        Action act = () => Load("[{\"name\":\"Greeting\"}]");

        _ = act.Should().Throw<LocalizationBuilderException>().WithMessage("*root*");
    }

    [Fact]
    public void FileWithoutAVersion_IsTreatedAsVersion1()
    {
        LocalizationSet set = Load("{\"strings\":[{\"name\":\"Greeting\",\"value\":\"Hello\"}]}");

        _ = set["Greeting"].Should().Be("Hello");
    }
}
