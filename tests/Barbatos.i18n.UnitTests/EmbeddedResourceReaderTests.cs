// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

using Barbatos.i18n.IO;

namespace Barbatos.i18n.UnitTests;

/// <summary>
/// Pins how a resource path is turned into a manifest resource name: folder separators become dots, runs of dots
/// collapse to one, and a leading assembly name is optional.
/// </summary>
public sealed class EmbeddedResourceReaderTests
{
    private static readonly Assembly TestAssembly = typeof(EmbeddedResourceReaderTests).Assembly;

    [Theory]
    [InlineData("Resources.Translations-ko-KR.yaml")]
    [InlineData("Resources/Translations-ko-KR.yaml")]
    [InlineData("Resources\\Translations-ko-KR.yaml")]
    [InlineData("Resources//Translations-ko-KR.yaml")]
    [InlineData("Resources..Translations-ko-KR.yaml")]
    [InlineData("Resources...Translations-ko-KR.yaml")]
    [InlineData("Resources/./Translations-ko-KR.yaml")]
    [InlineData("Barbatos.i18n.UnitTests.Resources.Translations-ko-KR.yaml")]
    [InlineData("Barbatos.i18n.UnitTests/Resources/Translations-ko-KR.yaml")]
    [InlineData("Barbatos.i18n.UnitTests..Resources.Translations-ko-KR.yaml")]
    [InlineData("barbatos.i18n.unittests.Resources.Translations-ko-KR.yaml")]
    public void ResolvesEverySpellingOfTheSameResource(string path)
    {
        EmbeddedResourceReader.ReadToEnd(path, TestAssembly).Should().Contain("안녕하세요");
    }

    [Theory]
    [InlineData("Resources.Missing.yaml")]
    [InlineData("Resources.translations-ko-kr.yaml")]
    [InlineData(".Resources.Translations-ko-KR.yaml.")]
    public void ReturnsNullForAResourceThatIsNotThere(string path)
    {
        EmbeddedResourceReader.ReadToEnd(path, TestAssembly).Should().BeNull();
    }
}
