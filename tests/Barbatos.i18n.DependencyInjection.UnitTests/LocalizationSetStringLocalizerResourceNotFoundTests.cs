// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

namespace Barbatos.i18n.DependencyInjection.UnitTests;

/// <summary>
/// A missing key must report ResourceNotFound, otherwise tooling that audits for untranslated strings sees a
/// key echoed back as if it were a real translation.
/// </summary>
public sealed class LocalizationSetStringLocalizerResourceNotFoundTests
{
    private static LocalizationSetStringLocalizer BuildLocalizer() =>
        new(
            new LocalizationSet(
                null,
                new CultureInfo("en-US"),
                new Dictionary<LocalizationKey, string?>
                {
                    { "greeting", "Hello" },
                    { "greetingwithname", "Hello {0}" }
                }
            )
        );

    [Fact]
    public void Indexer_ReportsResourceNotFound_ForAMissingKey()
    {
        LocalizedString result = BuildLocalizer()["missing"];

        result.Value.Should().Be("missing");
        result.ResourceNotFound.Should().BeTrue();
    }

    [Fact]
    public void Indexer_DoesNotReportResourceNotFound_ForAKnownKey()
    {
        LocalizedString result = BuildLocalizer()["greeting"];

        result.Value.Should().Be("Hello");
        result.ResourceNotFound.Should().BeFalse();
    }

    [Fact]
    public void IndexerWithArguments_ReportsResourceNotFound_ForAMissingKey()
    {
        LocalizedString result = BuildLocalizer()["missing", "Hung"];

        result.Value.Should().Be("missing");
        result.ResourceNotFound.Should().BeTrue();
    }

    [Fact]
    public void IndexerWithArguments_FormatsAndDoesNotReportResourceNotFound_ForAKnownKey()
    {
        LocalizedString result = BuildLocalizer()["greetingwithname", "Hung"];

        result.Value.Should().Be("Hello Hung");
        result.ResourceNotFound.Should().BeFalse();
    }
}
