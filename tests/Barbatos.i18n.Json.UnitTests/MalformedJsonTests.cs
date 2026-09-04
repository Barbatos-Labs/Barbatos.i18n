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
/// A malformed translation file must surface as <see cref="LocalizationBuilderException"/>, the type this
/// package already uses for a missing resource, a duplicate key and an unsupported schema version. These paths
/// used to escape as NullReferenceException, ArgumentException, ArgumentNullException or JsonException.
/// </summary>
public class MalformedJsonTests
{
    private static Action Load(string json) =>
        () => new LocalizationBuilder().FromJsonString(json, new CultureInfo("en-US"));

    [Fact]
    public void Version1_WithoutStringsArray_ThrowsLocalizationBuilderException()
    {
        Load("{\"version\": \"1.0\"}")
            .Should().Throw<LocalizationBuilderException>()
            .WithMessage("*strings*");
    }

    [Fact]
    public void Version1_WithNullEntryName_ThrowsLocalizationBuilderException()
    {
        Load("{\"version\":\"1.0\",\"strings\":[{\"name\":null,\"value\":\"v\"}]}")
            .Should().Throw<LocalizationBuilderException>()
            .WithMessage("*name*");
    }

    [Fact]
    public void UnreadableVersion_ThrowsLocalizationBuilderException()
    {
        Load("{\"version\": \"abc\", \"A\": \"a\"}")
            .Should().Throw<LocalizationBuilderException>()
            .WithMessage("*abc*");
    }

    [Theory]
    [InlineData("\"2\"")]
    [InlineData("2")]
    public void BareMajorVersion_IsAccepted(string version)
    {
        // Both a bare string "2" and a numeric 2 previously threw - ArgumentException and JsonException
        // respectively - even though the intent is unambiguous.
        LocalizationBuilder builder = new();
        builder.FromJsonString($"{{\"version\": {version}, \"Greeting\": \"Hello\"}}", new CultureInfo("en-US"));

        LocalizationSet? set = builder.Build().GetLocalizationSet(new CultureInfo("en-US"), null);

        set.Should().NotBeNull();
        set!["Greeting"].Should().Be("Hello");
    }

    [Fact]
    public void WellFormedVersion2_StillLoads()
    {
        LocalizationBuilder builder = new();
        builder.FromJsonString("{\"version\": \"2.0\", \"Errors\": {\"Network\": \"Failed\"}}", new CultureInfo("en-US"));

        LocalizationSet? set = builder.Build().GetLocalizationSet(new CultureInfo("en-US"), null);

        set!["Errors.Network"].Should().Be("Failed");
    }

    [Fact]
    public void WellFormedVersion1_StillLoads()
    {
        LocalizationBuilder builder = new();
        builder.FromJsonString(
            "{\"version\":\"1.0\",\"strings\":[{\"name\":\"Greeting\",\"value\":\"Hello\"}]}",
            new CultureInfo("en-US"));

        LocalizationSet? set = builder.Build().GetLocalizationSet(new CultureInfo("en-US"), null);

        set!["Greeting"].Should().Be("Hello");
    }

    [Fact]
    public void UnsupportedVersion_StillThrowsLocalizationBuilderException()
    {
        Load("{\"version\": \"9.0\", \"A\": \"a\"}")
            .Should().Throw<LocalizationBuilderException>();
    }
}
