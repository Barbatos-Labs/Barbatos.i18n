// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

using AwesomeAssertions;
using System.Globalization;

namespace Barbatos.i18n.Json.UnitTests;

public sealed class FromJsonTests_V2
{
    [Fact]
    public void FromJson_V2_ShouldProperlyAddLocalizations_WithDotAndColonSeparators()
    {
        LocalizationBuilder builder = new();

        _ = builder.FromJson("Resources.v2.Translations-en-US.json", new CultureInfo("en-US"));
        _ = builder.FromJson("Resources.v2.Translations-ko-KR.json", new CultureInfo("ko-KR"));

        ILocalizationProvider localizationProvider = builder.Build();

        // 1. Test en-US
        LocalizationSet? enSet = localizationProvider.GetLocalizationSet("en-US");
        _ = enSet.Should().NotBeNull();

        // Top level
        _ = enSet!["greeting"].Should().Be("Welcome to our application!");

        // Level 2 nested
        _ = enSet!["user.profile.fullName"].Should().Be("Full Name");
        _ = enSet!["user:profile:fullName"].Should().Be("Full Name");

        _ = enSet!["user.contact.phoneNumber"].Should().Be("Phone Number");
        _ = enSet!["user:contact:phoneNumber"].Should().Be("Phone Number");
        
        // Level 2 action
        _ = enSet!["actions.save"].Should().Be("Save Changes");
        _ = enSet!["actions:save"].Should().Be("Save Changes");

        // Level 3 nested action
        _ = enSet!["actions.dialog.confirm"].Should().Be("Are you sure you want to proceed?");
        _ = enSet!["actions:dialog:confirm"].Should().Be("Are you sure you want to proceed?");

        // 2. Test ko-KR
        LocalizationSet? koSet = localizationProvider.GetLocalizationSet("ko-KR");
        _ = koSet.Should().NotBeNull();

        // Top level
        _ = koSet!["greeting"].Should().Be("저희 애플리케이션에 오신 것을 환영합니다!");

        // Level 2 nested
        _ = koSet!["user.profile.fullName"].Should().Be("성명");
        _ = koSet!["user:profile:fullName"].Should().Be("성명");

        _ = koSet!["user.contact.phoneNumber"].Should().Be("전화번호");
        _ = koSet!["user:contact:phoneNumber"].Should().Be("전화번호");
        
        // Level 2 action
        _ = koSet!["actions.save"].Should().Be("변경사항 저장");
        _ = koSet!["actions:save"].Should().Be("변경사항 저장");

        // Level 3 nested action
        _ = koSet!["actions.dialog.confirm"].Should().Be("계속 진행하시겠습니까?");
        _ = koSet!["actions:dialog:confirm"].Should().Be("계속 진행하시겠습니까?");
    }

    [Fact]
    public void FromJson_ShouldThrowArgumentException_WhenPathNotEndsWithJson()
    {
        LocalizationBuilder builder = new();
        Action act = () => builder.FromJson("Resources.Translations.txt", new CultureInfo("en-US"));
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void FromJson_ShouldThrowLocalizationBuilderException_WhenResourceNotFound()
    {
        LocalizationBuilder builder = new();
        Action act = () => builder.FromJson("Resources.Missing.json", new CultureInfo("en-US"));
        act.Should().Throw<LocalizationBuilderException>();
    }

    [Fact]
    public void FromJsonString_ShouldParseProperly()
    {
        var json = """
        {
            "version": "2.0",
            "key": "value",
            "nested": {
                "key": "nested value"
            }
        }
        """;
        
        LocalizationBuilder builder = new();
        builder.FromJsonString(json, "testns", new CultureInfo("en-US"));
        
        var provider = builder.Build();
        var set = provider.GetLocalizationSet("en-US", "testns");
        
        set.Should().NotBeNull();
        set!["key"].Should().Be("value");
        set!["nested.key"].Should().Be("nested value");
    }

    [Fact]
    public void FromJsonString_ShouldThrowArgumentNullException_WhenContentsNull()
    {
        LocalizationBuilder builder = new();
        Action act = () => builder.FromJsonString(null!, "testns", new CultureInfo("en-US"));
        act.Should().Throw<ArgumentNullException>();
    }
}
