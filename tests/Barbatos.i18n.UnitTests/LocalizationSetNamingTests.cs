// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

using Barbatos.i18n.IO;

namespace Barbatos.i18n.UnitTests;

/// <summary>
/// A file-backed set is named after its file. A file named after nothing but its culture belongs in the default
/// namespace: naming it after the culture put each language in a differently named set, so Namespace='...' had
/// no name that spanned the cultures and a baseName-scoped IStringLocalizer resolved for one language only.
/// </summary>
public sealed class LocalizationSetNamingTests
{
    [Theory]
    [InlineData("Locales.Translations-en-US.ini", "en-US", "translations")]
    [InlineData("Locales.Errors-vi-VN.json", "vi-VN", "errors")]
    [InlineData("Resources.v1.Translations-en-US.ini", "en-US", "translations")]
    [InlineData("Locales.Errors.csv", "en-US", "errors")]
    [InlineData("Locales.MIXEDCase-EN-us.ini", "en-US", "mixedcase")]
    public void ANamedFile_KeepsItsName(string path, string culture, string expected)
    {
        LocalizationSetNaming.DeriveName(path, new CultureInfo(culture)).Should().Be(expected);
    }

    [Theory]
    [InlineData("Locales.en-US.ini", "en-US")]
    [InlineData("Locales.vi-VN.json", "vi-VN")]
    [InlineData("Locales.EN-us.csv", "en-US")]
    [InlineData("Locales.en.ini", "en-US")]
    public void AFileNamedAfterItsCulture_TakesItsFolderName(string path, string culture)
    {
        LocalizationSetNaming.DeriveName(path, new CultureInfo(culture)).Should().Be("locales");
    }

    [Fact]
    public void AFileWithNoFolder_KeepsItsNameAsWritten()
    {
        // Nothing better to fall back on, and returning no name would collide with another default-namespace set.
        LocalizationSetNaming.DeriveName("en-US.ini", new CultureInfo("en-US")).Should().Be("en-us");
    }

    [Fact]
    public void AllCulturesOfTheSameLayout_ShareOneNamespace()
    {
        string?[] names =
        [
            LocalizationSetNaming.DeriveName("Locales.en-US.ini", new CultureInfo("en-US")),
            LocalizationSetNaming.DeriveName("Locales.vi-VN.ini", new CultureInfo("vi-VN")),
            LocalizationSetNaming.DeriveName("Locales.ko-KR.ini", new CultureInfo("ko-KR"))
        ];

        names.Should().AllBe("locales", "one layout must not produce three namespaces");
    }

    [Fact]
    public void AHyphenatedName_IsNotTruncatedUnderTheInvariantCulture()
    {
        // The multi-culture CSV overload derives its name against the invariant culture, whose empty name would
        // otherwise make the suffix strip search for a bare "-".
        LocalizationSetNaming.DeriveName("Locales.My-Errors.csv", CultureInfo.InvariantCulture)
            .Should().Be("my-errors");
    }

    [Fact]
    public void ANullArgument_Throws()
    {
        FluentActions.Invoking(() => LocalizationSetNaming.DeriveName(null!, CultureInfo.InvariantCulture))
            .Should().Throw<ArgumentNullException>();

        FluentActions.Invoking(() => LocalizationSetNaming.DeriveName("a.ini", null!))
            .Should().Throw<ArgumentNullException>();
    }
}
