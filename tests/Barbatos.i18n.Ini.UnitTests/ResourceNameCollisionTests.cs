// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

using System.Globalization;
using AwesomeAssertions;
using Barbatos.i18n;
using Barbatos.i18n.Ini;

namespace Barbatos.i18n.Ini.UnitTests;

/// <summary>
/// A resource path may be written with or without the assembly-name prefix, so the reader strips it before
/// prepending it. Stripping every occurrence rather than the leading one corrupts any path in which the
/// assembly name appears again - a real hazard for a short assembly name such as "App" or "Core", whose name
/// easily turns up inside a folder or file name.
/// </summary>
public sealed class ResourceNameCollisionTests
{
    private static readonly CultureInfo Culture = new("en-US");

    [Fact]
    public void APathRepeatingTheAssemblyName_StillResolves()
    {
        LocalizationBuilder builder = new();

        builder.FromIni("Resources.Barbatos.i18n.Ini.UnitTests-en-US.ini", Culture);
        builder.SetCulture(Culture);

        // The set name is beside the point here - what matters is that the resource was found at all.
        builder.Build().GetLocalizationSets(Culture)
            .Select(s => s["Greeting"])
            .Should().Contain("Hello from a colliding name");
    }

    [Fact]
    public void ThePathMayStillCarryTheAssemblyNameAsItsPrefix()
    {
        LocalizationBuilder builder = new();

        builder.FromIni("Barbatos.i18n.Ini.UnitTests.Resources.Translations-en-US.ini", Culture);
        builder.SetCulture(Culture);

        builder.Build().GetLocalizationSet(Culture, "translations").Should().NotBeNull();
    }
}
