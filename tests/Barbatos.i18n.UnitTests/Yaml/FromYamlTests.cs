// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

using Barbatos.i18n.Yaml;

namespace Barbatos.i18n.UnitTests.Yaml;

public sealed class FromYamlTests
{
    [Fact]
    public void FromYaml_ShouldProperlyAddLocalizations()
    {
        LocalizationBuilder builder = new();
        builder.SetCulture(new CultureInfo("ko-KR"));

        _ = builder.FromYaml(
            "Barbatos.i18n.UnitTests.Resources.Translations-ko-KR.yaml",
            new CultureInfo("ko-KR")
        );

        ILocalizationProvider localizationProvider = builder.Build();

        LocalizationSet? defaultSet = localizationProvider.GetLocalizationSet(
            new CultureInfo("ko-KR"),
            default
        );

        _ = defaultSet!
            .Strings.First(x => x.Key == "main.hello")
            .Value.Should()
            .Be("안녕하세요");

        LocalizationSet? namespacetestSet = localizationProvider.GetLocalizationSet(
            new CultureInfo("ko-KR"),
            "namespace.test"
        );

        _ = namespacetestSet!["main.hello"].Should().Be("안녕하세요 in namespace.test");
    }
}
