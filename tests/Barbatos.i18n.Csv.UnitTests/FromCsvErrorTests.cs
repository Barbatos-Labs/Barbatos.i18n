// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

using System.Globalization;
using System.Reflection;
using AwesomeAssertions;
using Barbatos.i18n;
using Barbatos.i18n.Csv;

namespace Barbatos.i18n.Csv.UnitTests;

/// <summary>
/// The single-culture and multi-culture file overloads validate their input identically. Only the exception
/// types were checked, and only for some of the overloads, so these pin the messages a consumer sees for every
/// way a file can be rejected.
/// </summary>
public sealed class FromCsvErrorTests
{
    private static readonly Assembly TestAssembly = typeof(FromCsvErrorTests).Assembly;

    [Fact]
    public void BothOverloads_RejectAPathThatIsNotACsvFile()
    {
        const string expected = "Parameter path in FromCsv must be path to the CSV file.";

        FluentActions.Invoking(() => new LocalizationBuilder().FromCsv(TestAssembly, "Resources.Translations.txt", new CultureInfo("en-US")))
            .Should().Throw<ArgumentException>().WithMessage(expected);
        FluentActions.Invoking(() => new LocalizationBuilder().FromCsv(TestAssembly, "Resources.Translations.txt"))
            .Should().Throw<ArgumentException>().WithMessage(expected);
    }

    [Fact]
    public void BothOverloads_NameTheMissingResourceAndTheAssembly()
    {
        string expected = $"Resource Resources.Missing.csv not found in assembly {TestAssembly.FullName}.";

        FluentActions.Invoking(() => new LocalizationBuilder().FromCsv(TestAssembly, "Resources.Missing.csv", new CultureInfo("en-US")))
            .Should().Throw<LocalizationBuilderException>().WithMessage(expected);
        FluentActions.Invoking(() => new LocalizationBuilder().FromCsv(TestAssembly, "Resources.Missing.csv"))
            .Should().Throw<LocalizationBuilderException>().WithMessage(expected);
    }

    [Fact]
    public void TheSingleCultureOverload_RejectsAMultiCultureFile()
    {
        FluentActions.Invoking(() => new LocalizationBuilder().FromCsv(TestAssembly, "Resources.Translations-Multi.csv", new CultureInfo("en-US")))
            .Should().Throw<ArgumentException>()
            .WithMessage("The CSV file Resources.Translations-Multi.csv is formatted as a multi-culture file.*");
    }

    [Fact]
    public void TheMultiCultureOverload_RejectsASingleCultureFile()
    {
        FluentActions.Invoking(() => new LocalizationBuilder().FromCsv(TestAssembly, "Resources.Translations-en-US.csv"))
            .Should().Throw<ArgumentException>()
            .WithMessage("The CSV file Resources.Translations-en-US.csv is formatted as a single-culture file.*");
    }
}
