// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

using System.Globalization;
using Barbatos.i18n;
using Barbatos.i18n.Maui;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Hosting;

namespace Barbatos.i18n.Maui.UnitTests;

/// <summary>
/// MauiAppBuilderExtensions registers a private culture manager for apps that do not reference the
/// DependencyInjection package. CLAUDE.md requires all three managers to behave alike, but this one had no test
/// at all - it was aligned only by inspection, and it is the one a MAUI app gets by default.
/// </summary>
[Collection("Sequential")]
public sealed class DefaultCultureManagerTests : IDisposable
{
    private static readonly CultureInfo English = new("en-US");
    private static readonly CultureInfo Vietnamese = new("vi-VN");

    private readonly CultureInfo _originalUiCulture = CultureInfo.CurrentUICulture;
    private readonly CultureInfo _originalCulture = CultureInfo.CurrentCulture;

    public void Dispose()
    {
        CultureInfo.CurrentUICulture = _originalUiCulture;
        CultureInfo.CurrentCulture = _originalCulture;
    }

    private static ServiceProvider BuildServices()
    {
        MauiAppBuilder builder = MauiApp.CreateBuilder();

        builder.UseStringLocalizer(loc =>
        {
            loc.AddLocalization(new LocalizationSet(null, English,
                new Dictionary<LocalizationKey, string?> { { "greeting", "Hello" } }));
            loc.AddLocalization(new LocalizationSet(null, Vietnamese,
                new Dictionary<LocalizationKey, string?> { { "greeting", "Xin chao" } }));
            loc.SetCulture(English);
        });

        builder.UseStringLocalizer("Secondary", loc =>
        {
            loc.AddLocalization(new LocalizationSet("extra", English,
                new Dictionary<LocalizationKey, string?> { { "bonus", "Bonus" } }));
            loc.AddLocalization(new LocalizationSet("extra", Vietnamese,
                new Dictionary<LocalizationKey, string?> { { "bonus", "Thuong" } }));
            loc.SetCulture(English);
        });

        return builder.Services.BuildServiceProvider();
    }

    [Fact]
    public void ItIsRegisteredForAnAppWithoutTheDiPackage()
    {
        using ServiceProvider provider = BuildServices();

        provider.GetService<ILocalizationCultureManager>().Should().NotBeNull();
    }

    [Fact]
    public void SetCulture_MovesEveryProvider_NotOnlyTheDefaultOne()
    {
        using ServiceProvider provider = BuildServices();
        var resolver = provider.GetRequiredService<ILocalizationProviderResolver>();

        provider.GetRequiredService<ILocalizationCultureManager>().SetCulture(Vietnamese);

        resolver.GetProvider()!.GetCulture().Name.Should().Be("vi-VN");
        resolver.GetProvider("Secondary")!.GetCulture().Name.Should().Be(
            "vi-VN",
            "a keyed provider has to follow the culture like every other one");
    }

    [Fact]
    public void SetCulture_AppliesTheAmbientCultures()
    {
        using ServiceProvider provider = BuildServices();

        provider.GetRequiredService<ILocalizationCultureManager>().SetCulture(Vietnamese);

        CultureInfo.CurrentUICulture.Name.Should().Be("vi-VN");
        CultureInfo.CurrentCulture.Name.Should().Be("vi-VN");
        CultureInfo.DefaultThreadCurrentUICulture!.Name.Should().Be("vi-VN");
    }

    [Fact]
    public void SetCulture_RaisesTheNotification_AfterMovingTheProviders()
    {
        using ServiceProvider provider = BuildServices();
        var resolver = provider.GetRequiredService<ILocalizationProviderResolver>();

        string? cultureSeenByListener = null;
        void Handler(object? sender, LocalizationChangedEventArgs e) =>
            cultureSeenByListener = resolver.GetProvider()!.GetCulture().Name;

        LocalizationNotifier.CultureChanged += Handler;

        try
        {
            provider.GetRequiredService<ILocalizationCultureManager>().SetCulture(Vietnamese);
        }
        finally
        {
            LocalizationNotifier.CultureChanged -= Handler;
        }

        // The live bindings repaint from this event, so the providers must already be moved when it arrives.
        cultureSeenByListener.Should().Be("vi-VN");
    }

    [Fact]
    public void GetSupportedCultures_CoversEveryRegisteredProvider()
    {
        using ServiceProvider provider = BuildServices();

        provider.GetRequiredService<ILocalizationCultureManager>().GetSupportedCultures()
            .Should().Contain(English).And.Contain(Vietnamese);
    }

    [Fact]
    public void GetCulture_ReportsWhatSetCultureApplied()
    {
        using ServiceProvider provider = BuildServices();
        var manager = provider.GetRequiredService<ILocalizationCultureManager>();

        manager.SetCulture(Vietnamese);

        manager.GetCulture().Name.Should().Be("vi-VN");
    }

    [Fact]
    public void SetCulture_RejectsNull()
    {
        using ServiceProvider provider = BuildServices();

        FluentActions.Invoking(() => provider.GetRequiredService<ILocalizationCultureManager>().SetCulture((CultureInfo)null!))
            .Should().Throw<ArgumentNullException>();
    }
}
