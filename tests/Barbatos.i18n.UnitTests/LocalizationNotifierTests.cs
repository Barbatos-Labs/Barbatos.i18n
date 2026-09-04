// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

namespace Barbatos.i18n.UnitTests;

[Collection("Sequential")]
public sealed class LocalizationNotifierTests : IDisposable
{
    private readonly CultureInfo _originalCulture;
    private readonly CultureInfo _originalUICulture;
    private readonly CultureInfo? _originalDefaultCulture;
    private readonly CultureInfo? _originalDefaultUICulture;

    public LocalizationNotifierTests()
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
    public void NotifyCultureChanged_ShouldRaiseEventCarryingTheCulture()
    {
        CultureInfo? observed = null;
        void Handler(object? sender, LocalizationChangedEventArgs e) => observed = e.Culture;

        LocalizationNotifier.CultureChanged += Handler;

        try
        {
            LocalizationNotifier.NotifyCultureChanged(new CultureInfo("ko-KR"));
        }
        finally
        {
            LocalizationNotifier.CultureChanged -= Handler;
        }

        observed.Should().NotBeNull();
        observed!.Name.Should().Be("ko-KR");
    }

    [Fact]
    public void NotifyCultureChanged_ShouldThrow_WhenCultureIsNull()
    {
        Action act = () => LocalizationNotifier.NotifyCultureChanged(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void NotifyCultureChanged_ShouldNotThrow_WhenNoHandlerIsSubscribed()
    {
        Action act = () => LocalizationNotifier.NotifyCultureChanged(new CultureInfo("vi-VN"));

        act.Should().NotThrow();
    }

    [Fact]
    public void SetCulture_ShouldNotifyAfterTheCultureHasBeenApplied()
    {
        CultureInfo? uiCultureWhenNotified = null;
        void Handler(object? sender, LocalizationChangedEventArgs e) => uiCultureWhenNotified = CultureInfo.CurrentUICulture;

        LocalizationNotifier.CultureChanged += Handler;

        try
        {
            new LocalizationCultureManager().SetCulture("fr-FR");
        }
        finally
        {
            LocalizationNotifier.CultureChanged -= Handler;
        }

        uiCultureWhenNotified.Should().NotBeNull();
        uiCultureWhenNotified!.Name.Should().Be("fr-FR");
    }

    [Fact]
    public void SetCulture_ShouldRaiseOncePerCall()
    {
        int raised = 0;
        void Handler(object? sender, LocalizationChangedEventArgs e) => raised++;

        LocalizationNotifier.CultureChanged += Handler;

        try
        {
            var manager = new LocalizationCultureManager();
            manager.SetCulture("fr-FR");
            manager.SetCulture("ko-KR");
        }
        finally
        {
            LocalizationNotifier.CultureChanged -= Handler;
        }

        raised.Should().Be(2);
    }
}
