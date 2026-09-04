// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

using Barbatos.i18n.Json.Models;

namespace Barbatos.i18n.Json.Converters;

/// <summary>
/// JSON converter for the ITranslationsContainer interface.
/// </summary>
internal class TranslationsContainerConverter : JsonConverter<ITranslationsContainer>
{
    public override ITranslationsContainer? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        JsonElement jsonObject = JsonDocument.ParseValue(ref reader).RootElement;

        string version = "1.0";

        foreach (JsonProperty property in jsonObject.EnumerateObject())
        {
            if (string.Equals(property.Name, "Version", StringComparison.OrdinalIgnoreCase))
            {
                // GetString() only accepts a JSON string, so a numeric "version": 2 used to surface as a
                // JsonException. Read whatever spelling was used and let the caller validate it, so a bad
                // version is reported as a LocalizationBuilderException naming the problem.
                version = property.Value.ValueKind == JsonValueKind.String
                    ? property.Value.GetString() ?? version
                    : property.Value.ToString();

                break;
            }
        }

        return new TranslationsContainer(version);
    }

    public override void Write(
        Utf8JsonWriter writer,
        ITranslationsContainer? value,
        JsonSerializerOptions options
    )
    {
        JsonSerializer.Serialize(
            writer,
            new TranslationsContainer(value?.Version ?? "1.0"),
            options
        );
    }
}
