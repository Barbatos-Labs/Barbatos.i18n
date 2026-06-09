// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

namespace Barbatos.i18n.UnitTests;

public sealed class LocalizationCultureManagerTests
{
    [Fact]
    public void SetCulture_ShouldUpdateCurrentUICulture()
    {
        var manager = new LocalizationCultureManager();
        manager.SetCulture("fr-FR");

        CultureInfo.CurrentUICulture.Name.Should().Be("fr-FR");
        CultureInfo.DefaultThreadCurrentUICulture?.Name.Should().Be("fr-FR");
    }

    [Fact]
    public void SetCulture_WithSyncFormattingCulture_ShouldUpdateCurrentCulture()
    {
        var options = new LocalizationOptions { SyncFormattingCulture = true };
        var manager = new LocalizationCultureManager(options);
        
        manager.SetCulture("de-DE");

        CultureInfo.CurrentCulture.Name.Should().Be("de-DE");
        CultureInfo.DefaultThreadCurrentCulture?.Name.Should().Be("de-DE");
    }

    [Fact]
    public void SetCulture_WithCustomFormattingCultureBuilder_ShouldUseCustomCulture()
    {
        var options = new LocalizationOptions
        {
            SyncFormattingCulture = true,
            CustomFormattingCultureBuilder = _ => new CultureInfo("en-GB")
        };
        var manager = new LocalizationCultureManager(options);
        
        manager.SetCulture("fr-FR");

        CultureInfo.CurrentCulture.Name.Should().Be("en-GB");
    }

    [Fact]
    public void GetCulture_ShouldReturnCurrentCulture_IfNoProvider()
    {
        var manager = new LocalizationCultureManager();
        var culture = manager.GetCulture();
        culture.Should().NotBeNull();
    }
}
