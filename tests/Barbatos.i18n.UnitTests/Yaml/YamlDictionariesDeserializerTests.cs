// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

using Barbatos.i18n.Yaml;

namespace Barbatos.i18n.UnitTests.Yaml;

public sealed class YamlDictionariesDeserializerTests
{
    [Fact]
    public void FromString_ShouldProperlyDecodeString()
    {
        const string input = """
            # Comment
            main.languages: Languages
            main.hello: "Hello world"

            namespace.test:
                main.languages: Languages in namespace.test #yet another comment 
                main.hello: 'Hello world in namespace.test'

            # Some comment
            other.namespace:
                main.languages: Languages in other.namespace #yet another comment 
                main.hello: 'Hello world in other.namespace'
            """;

        IDictionary<string, IDictionary<string, string>> result =
            YamlDictionariesDeserializer.FromString(input);

        _ = result.Should().ContainKey("default").WhoseValue.ContainsKey("main.languages");
        _ = result.Should().ContainKey("namespace.test").WhoseValue.ContainsKey("main.languages");
        _ = result.Should().ContainKey("other.namespace").WhoseValue.ContainsKey("main.languages");
    }
}
