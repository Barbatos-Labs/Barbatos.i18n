// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

using System.Globalization;
using System.Text.Json;
using AwesomeAssertions;
using Barbatos.i18n;

namespace Barbatos.i18n.Json.UnitTests;

/// <summary>
/// Every failure this package reports is a <see cref="LocalizationBuilderException"/>, so a consumer can catch
/// that one type to name the offending locale file. A syntax error used to escape as a raw
/// <see cref="JsonException"/> instead, defeating exactly that.
/// </summary>
public class InvalidJsonSyntaxTests
{
    private static readonly CultureInfo Culture = new("en-US");

    [Theory]
    [InlineData("{ \"version\": \"2.0\", \"Greeting\": \"Hello\"")]   // unclosed brace
    [InlineData("{ \"version\": \"1.0\", \"strings\": [ { \"name\": ] }")] // malformed array
    [InlineData("not json at all")]
    [InlineData("")]
    public void MalformedContents_ThrowLocalizationBuilderException(string contents)
    {
        var builder = new LocalizationBuilder();

        FluentActions.Invoking(() => builder.FromJsonString(contents, "broken", Culture))
            .Should().Throw<LocalizationBuilderException>()
            .WithMessage("*not valid JSON*");
    }

    [Fact]
    public void MalformedContents_KeepTheParserErrorAsInnerException()
    {
        var builder = new LocalizationBuilder();

        FluentActions.Invoking(() => builder.FromJsonString("{ \"a\": ", "broken", Culture))
            .Should().Throw<LocalizationBuilderException>()
            .WithInnerException<JsonException>("the original parse error stays available for diagnostics");
    }

    [Fact]
    public void WellFormedContents_StillLoad()
    {
        var builder = new LocalizationBuilder();
        builder.FromJsonString("{ \"version\": \"2.0\", \"Greeting\": \"Hello\" }", "ok", Culture);
        builder.SetCulture(Culture);

        builder.Build().GetLocalizationSet(Culture, "ok")!["Greeting"].Should().Be("Hello");
    }
}
