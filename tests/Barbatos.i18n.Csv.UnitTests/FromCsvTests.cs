// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

using System.Globalization;
using AwesomeAssertions;
using Barbatos.i18n;
using Barbatos.i18n.Csv;

namespace Barbatos.i18n.Csv.UnitTests;

public class FromCsvTests
{
    [Fact]
    public void FromCsv_Should_ParseAndMapLocalizationKeysCorrectly()
    {
        LocalizationBuilder builder = new();
        builder.FromCsv("Resources.Translations-en-US.csv", new CultureInfo("en-US"));
        builder.FromCsv("Resources.Translations-ko-KR.csv", new CultureInfo("ko-KR"));

        ILocalizationProvider provider = builder.Build();

        // Assert for English
        LocalizationSet? enSet = provider.GetLocalizationSet("en-US");
        _ = enSet.Should().NotBeNull();
        _ = enSet!["User.FullName"].Should().Be("Full Name");
        _ = enSet!["User.Address"].Should().Be("Home Address");
        _ = enSet!["Messages.Error.InvalidEmail"].Should().Be("Invalid email address.");
        _ = enSet!["Messages.Error.Required"].Should().Be("This field is required.");
        _ = enSet!["Global.Greeting"].Should().Be("Welcome to our application!");
        _ = enSet!["Advanced.Timeout.Max"].Should().Be("30");
        _ = enSet!["Advanced.Message"].Should().Be("Hello, this contains a comma, and quotes too!");

        // Assert for Korean
        LocalizationSet? koSet = provider.GetLocalizationSet("ko-KR");
        _ = koSet.Should().NotBeNull();
        _ = koSet!["User.FullName"].Should().Be("성명");
        _ = koSet!["User.Address"].Should().Be("자택 주소");
        _ = koSet!["Messages.Error.InvalidEmail"].Should().Be("이메일 주소가 올바르지 않습니다.");
        _ = koSet!["Messages.Error.Required"].Should().Be("필수 입력 항목입니다.");
        _ = koSet!["Global.Greeting"].Should().Be("저희 애플리케이션에 오신 것을 환영합니다!");
        _ = koSet!["Advanced.Timeout.Max"].Should().Be("30");
        _ = koSet!["Advanced.Message"].Should().Be("안녕하세요, 쉼표가 있습니다!");
    }

    [Fact]
    public void FromCsv_WithMultiCulture_Should_ParseAllCultures()
    {
        LocalizationBuilder builder = new();
        builder.FromCsv("Resources.Translations-Multi.csv"); // No CultureInfo provided

        ILocalizationProvider provider = builder.Build();

        // Assert for English
        LocalizationSet? enSet = provider.GetLocalizationSet("en-US");
        _ = enSet.Should().NotBeNull();
        _ = enSet!["User.FullName"].Should().Be("Full Name");
        _ = enSet!["Messages.Error.Required"].Should().Be("This field is required.");
        _ = enSet!["Advanced.Message"].Should().Be("Hello, this contains a comma, and quotes too!");

        // Assert for Korean
        LocalizationSet? koSet = provider.GetLocalizationSet("ko-KR");
        _ = koSet.Should().NotBeNull();
        _ = koSet!["User.FullName"].Should().Be("성명");
        _ = koSet!["Messages.Error.Required"].Should().Be("필수 입력 항목입니다.");
        _ = koSet!["Advanced.Message"].Should().Be("안녕하세요, 쉼표가 있습니다!");
    }

    [Fact]
    public void FromCsv_ShouldThrowArgumentException_WhenPathNotEndsWithCsv()
    {
        LocalizationBuilder builder = new();
        Action act = () => builder.FromCsv("Resources.Translations.txt", new CultureInfo("en-US"));
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void FromCsv_MultiCulture_ShouldThrowArgumentException_WhenPathNotEndsWithCsv()
    {
        LocalizationBuilder builder = new();
        Action act = () => builder.FromCsv("Resources.Translations.txt");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void FromCsv_ShouldThrowLocalizationBuilderException_WhenResourceNotFound()
    {
        LocalizationBuilder builder = new();
        Action act = () => builder.FromCsv("Resources.Missing.csv", new CultureInfo("en-US"));
        act.Should().Throw<LocalizationBuilderException>();
    }

    [Fact]
    public void FromCsvString_ShouldParseAndMapLocalizationKeysCorrectly()
    {
        var csv = """
        Key,Value
        "TestKey","TestValue"
        """;
        
        LocalizationBuilder builder = new();
        builder.FromCsvString("testname", new CultureInfo("en-US"), csv);
        
        var provider = builder.Build();
        var set = provider.GetLocalizationSet("en-US");
        
        set.Should().NotBeNull();
        set!["TestKey"].Should().Be("TestValue");
    }

    [Fact]
    public void FromCsvString_MultiCulture_ShouldParseAllCultures()
    {
        var csv = """
        Key,en-US,ko-KR
        "TestKey","TestValueEn","TestValueKo"
        """;
        
        LocalizationBuilder builder = new();
        builder.FromCsvString("testname", csv);
        
        var provider = builder.Build();
        var setEn = provider.GetLocalizationSet("en-US");
        var setKo = provider.GetLocalizationSet("ko-KR");
        
        setEn!["TestKey"].Should().Be("TestValueEn");
        setKo!["TestKey"].Should().Be("TestValueKo");
    }

    [Fact]
    public void FromCsvString_ShouldThrowArgumentNullException_WhenContentsNull()
    {
        LocalizationBuilder builder = new();
        Action act = () => builder.FromCsvString("testname", new CultureInfo("en-US"), null);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void FromCsvString_MultiCulture_ShouldThrowArgumentNullException_WhenContentsNull()
    {
        LocalizationBuilder builder = new();
        Action act = () => builder.FromCsvString("testname", null);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void CsvLocalizationParser_DuplicateKeys_ThrowsLocalizationBuilderException()
    {
        var csv = """
        Key,Value
        "TestKey","Value1"
        "TestKey","Value2"
        """;
        
        LocalizationBuilder builder = new();
        Action act = () => builder.FromCsvString("testname", new CultureInfo("en-US"), csv);
        act.Should().Throw<LocalizationBuilderException>();
    }
}
