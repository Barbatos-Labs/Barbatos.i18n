// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

namespace Barbatos.i18n.UnitTests;

[Collection("Sequential")]
public sealed class LocalizationCultureManagerTests : IDisposable
{
    private readonly CultureInfo _originalCulture;
    private readonly CultureInfo _originalUICulture;
    private readonly CultureInfo? _originalDefaultCulture;
    private readonly CultureInfo? _originalDefaultUICulture;

    public LocalizationCultureManagerTests()
    {
        _originalCulture = CultureInfo.CurrentCulture;
        _originalUICulture = CultureInfo.CurrentUICulture;
        _originalDefaultCulture = CultureInfo.DefaultThreadCurrentCulture;
        _originalDefaultUICulture = CultureInfo.DefaultThreadCurrentUICulture;
        
        LocalizationProviderFactory.SetInstance(null!);
    }

    public void Dispose()
    {
        CultureInfo.CurrentCulture = _originalCulture;
        CultureInfo.CurrentUICulture = _originalUICulture;
        CultureInfo.DefaultThreadCurrentCulture = _originalDefaultCulture;
        CultureInfo.DefaultThreadCurrentUICulture = _originalDefaultUICulture;
        
        LocalizationProviderFactory.SetInstance(null!);
    }

    [Fact]
    public void SetCulture_ShouldUpdateCurrentCulture_ByDefault()
    {
        var options = new LocalizationOptions();
        var manager = new LocalizationCultureManager(options);

        manager.SetCulture("fr-FR");

        CultureInfo.CurrentUICulture.Name.Should().Be("fr-FR");
        CultureInfo.DefaultThreadCurrentUICulture?.Name.Should().Be("fr-FR");
        
        CultureInfo.CurrentCulture.Name.Should().Be("fr-FR");
        CultureInfo.DefaultThreadCurrentCulture?.Name.Should().Be("fr-FR");
    }

    [Fact]
    public void SetCulture_ShouldNotUpdateCurrentCulture_WhenBuilderReturnsCurrentCulture()
    {
        var options = new LocalizationOptions { FormatCultureBuilder = _ => CultureInfo.CurrentCulture };
        var manager = new LocalizationCultureManager(options);
        
        var prevCulture = CultureInfo.CurrentCulture;
        var prevDefaultCulture = CultureInfo.DefaultThreadCurrentCulture;

        manager.SetCulture("ko-KR");

        CultureInfo.CurrentUICulture.Name.Should().Be("ko-KR");
        CultureInfo.CurrentCulture.Should().Be(prevCulture);
    }

    [Fact]
    public void SetCulture_ShouldUseCustomCulture_WhenCustomBuilderIsProvided()
    {
        var options = new LocalizationOptions
        {
            FormatCultureBuilder = _ => new CultureInfo("vi-VN")
        };
        var manager = new LocalizationCultureManager(options);
        
        manager.SetCulture("en-US");

        CultureInfo.CurrentUICulture.Name.Should().Be("en-US");
        CultureInfo.DefaultThreadCurrentUICulture?.Name.Should().Be("en-US");
        CultureInfo.CurrentCulture.Name.Should().Be("vi-VN");
        CultureInfo.DefaultThreadCurrentCulture?.Name.Should().Be("vi-VN");

        // Verify standard formatting actually respects the "vi-VN" culture
        var price = 1234.56m;
        var date = new DateTime(2026, 6, 14, 15, 30, 0);

        // vi-VN uses ₫ for currency and dd/MM/yyyy for dates
        string.Format("{0:C2}", price).Should().Contain("₫");
        string.Format("{0:d}", date).Should().Contain("14/06/2026");
        string.Format("{0:N2}", price).Should().Be("1.234,56");
    }
    
    [Fact]
    public void SetCulture_ShouldUpdateProviderCulture_WhenProviderIsRegistered()
    {
        var provider = new LocalizationProvider(new CultureInfo("en-US"), []);
        LocalizationProviderFactory.SetInstance(provider);
        
        var manager = new LocalizationCultureManager();
        manager.SetCulture("zh-CN");
        
        provider.GetCulture().Name.Should().Be("zh-CN");
    }

    [Fact]
    public void GetCulture_ShouldReturnProviderCulture_WhenProviderIsRegistered()
    {
        var provider = new LocalizationProvider(new CultureInfo("ko-KR"), []);
        LocalizationProviderFactory.SetInstance(provider);
        
        var manager = new LocalizationCultureManager();
        var culture = manager.GetCulture();
        
        culture.Name.Should().Be("ko-KR");
    }

    [Fact]
    public void GetCulture_ShouldReturnCurrentCulture_WhenNoProviderIsRegistered()
    {
        CultureInfo.CurrentCulture = new CultureInfo("zh-CN");
        var manager = new LocalizationCultureManager();
        
        var culture = manager.GetCulture();
        
        culture.Name.Should().Be("zh-CN");
    }
}
