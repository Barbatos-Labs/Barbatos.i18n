// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

using AwesomeAssertions;
using System.Globalization;

namespace Barbatos.i18n.Ini.UnitTests;

public sealed class FromIniTests
{
    [Fact]
    public void FromIni_ShouldProperlyAddLocalizations()
    {
        // Arrange
        LocalizationBuilder builder = new();
        string englishPath = "Resources.Translations-en-US.ini";
        string koreanPath = "Resources.Translations-ko-KR.ini";

        // Act
        builder.FromIni(englishPath, new CultureInfo("en-US"));
        builder.FromIni(koreanPath, new CultureInfo("ko-KR"));

        ILocalizationProvider provider = builder.Build();

        // Assert for English
        LocalizationSet? enSet = provider.GetLocalizationSet("en-US");
        _ = enSet.Should().NotBeNull();
        _ = enSet!["User.FullName"].Should().Be("Full Name");
        _ = enSet!["User.Address"].Should().Be("Home Address");
        _ = enSet!["Messages.Error.InvalidEmail"].Should().Be("Invalid email address.");
        _ = enSet!["Messages:Error:InvalidEmail"].Should().Be("Invalid email address.");
        _ = enSet!["Messages.Error:InvalidEmail"].Should().Be("Invalid email address.");
        _ = enSet!["Messages:Error.InvalidEmail"].Should().Be("Invalid email address.");
        _ = enSet!["Messages.Error.Required"].Should().Be("This field is required.");
        _ = enSet!["Global.Greeting"].Should().Be("Welcome to our application!");
        _ = enSet!["Advanced.Timeout.Max"].Should().Be("30");
        _ = enSet!["Advanced:Timeout:Max"].Should().Be("30"); // Verify alternate syntax works

        // Assert for Korean
        LocalizationSet? koSet = provider.GetLocalizationSet("ko-KR");
        _ = koSet.Should().NotBeNull();
        _ = koSet!["User.FullName"].Should().Be("성명");
        _ = koSet!["User.Address"].Should().Be("자택 주소");
        _ = koSet!["Messages.Error.InvalidEmail"].Should().Be("이메일 주소가 올바르지 않습니다.");
        _ = koSet!["Messages:Error:InvalidEmail"].Should().Be("이메일 주소가 올바르지 않습니다.");
        _ = koSet!["Messages.Error:InvalidEmail"].Should().Be("이메일 주소가 올바르지 않습니다.");
        _ = koSet!["Messages:Error.InvalidEmail"].Should().Be("이메일 주소가 올바르지 않습니다.");
        _ = koSet!["Messages.Error.Required"].Should().Be("필수 입력 항목입니다.");
        _ = koSet!["Global.Greeting"].Should().Be("저희 애플리케이션에 오신 것을 환영합니다!");
        _ = koSet!["Advanced.Timeout.Max"].Should().Be("30");
        _ = koSet!["Advanced:Timeout:Max"].Should().Be("30");
    }

    [Fact]
    public void FromIni_ShouldThrowArgumentException_WhenPathNotEndsWithIni()
    {
        LocalizationBuilder builder = new();
        Action act = () => builder.FromIni("Resources.Translations.txt", new CultureInfo("en-US"));
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void FromIni_ShouldThrowLocalizationBuilderException_WhenResourceNotFound()
    {
        LocalizationBuilder builder = new();
        Action act = () => builder.FromIni("Resources.Missing.ini", new CultureInfo("en-US"));
        act.Should().Throw<LocalizationBuilderException>();
    }

    [Fact]
    public void FromIniString_ShouldParseProperly()
    {
        var ini = """
        [Section]
        Key=Value
        """;
        
        LocalizationBuilder builder = new();
        builder.FromIniString("testns", new CultureInfo("en-US"), ini);
        
        var provider = builder.Build();
        var set = provider.GetLocalizationSet("en-US", "testns");
        
        set.Should().NotBeNull();
        set!["Section.Key"].Should().Be("Value");
    }

    [Fact]
    public void FromIniString_ShouldThrowArgumentNullException_WhenContentsNull()
    {
        LocalizationBuilder builder = new();
        Action act = () => builder.FromIniString("testns", new CultureInfo("en-US"), null);
        act.Should().Throw<ArgumentNullException>();
    }
}
