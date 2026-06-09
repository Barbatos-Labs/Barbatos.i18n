// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

namespace Barbatos.i18n.Json.Parsers;

internal static class JsonLocalizationParserFactory
{
    public static IJsonLocalizationParser Create(int majorVersion)
    {
        return majorVersion switch
        {
            1 => new Version1Parser(),
            2 => new Version2Parser(),
            _ => throw new LocalizationBuilderException(
                $"Localization file with schema version \"{majorVersion}\" is not supported."
            )
        };
    }
}
