// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

using AwesomeAssertions;
using System.Globalization;

namespace Barbatos.i18n.Json.UnitTests;

public sealed class FromJsonTests_V1
{
    [Fact]
    public void FromJson_V1_ShouldProperlyAddLocalizations()
    {
        LocalizationBuilder builder = new();

        _ = builder.FromJson("Resources.v1.Translations-ko-KR.json", new CultureInfo("ko-KR"));
        _ = builder.FromJson("Resources.v1.Translations-en-US.json", new CultureInfo("en-US"));

        ILocalizationProvider localizationProvider = builder.Build();

        LocalizationSet? localizationSet = localizationProvider.GetLocalizationSet("ko-KR");

        _ = localizationSet!["Test"].Should().Be("영어로 테스트");
    }
}
