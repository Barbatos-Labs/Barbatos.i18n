// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

using System.Globalization;
using Barbatos.i18n;
using Barbatos.i18n.Wpf;
using AwesomeAssertions;

namespace Barbatos.i18n.Wpf.UnitTests;

/// <summary>
/// CultureInfo.CurrentCulture is per-thread, and ILocalizationCultureManager.SetCulture can only set it on the
/// thread that called it. A UI thread holding an explicit culture - which any earlier on-thread SetCulture gives
/// it - therefore kept formatting dates and numbers with the previous culture when a later switch came from a
/// background thread, even though translations updated. LocalizationSource now adopts the culture on whichever
/// thread handles the notification, which in an application is the dispatcher thread the converters run on.
/// </summary>
/// <remarks>
/// The dispatcher hop itself is not exercised here: reaching it requires Application.Current, and creating a WPF
/// Application is process-wide and irreversible, which would strand every later test. What is covered is the
/// behaviour the hop delivers - that handling a culture change adopts both cultures on the handling thread, and
/// that the formatting culture travels separately from the lookup culture.
/// </remarks>
[Collection("Sequential")]
public sealed class CrossThreadCultureTests : IDisposable
{
    private readonly CultureInfo _originalCulture = CultureInfo.CurrentCulture;
    private readonly CultureInfo _originalUICulture = CultureInfo.CurrentUICulture;

    public CrossThreadCultureTests()
    {
        // Ensure the singleton is subscribed before any notification is raised.
        _ = LocalizationSource.Instance;

        // Stand in for a thread that already holds an explicit culture.
        CultureInfo.CurrentCulture = new CultureInfo("vi-VN");
        CultureInfo.CurrentUICulture = new CultureInfo("vi-VN");
    }

    public void Dispose()
    {
        CultureInfo.CurrentCulture = _originalCulture;
        CultureInfo.CurrentUICulture = _originalUICulture;
    }

    [Fact]
    public void HandlingACultureChange_AdoptsTheCultureOnTheHandlingThread()
    {
        LocalizationNotifier.NotifyCultureChanged(new CultureInfo("ko-KR"));

        CultureInfo.CurrentUICulture.Name.Should().Be("ko-KR");
        CultureInfo.CurrentCulture.Name.Should().Be("ko-KR");
        LocalizationSource.Instance.Culture.Name.Should().Be("ko-KR");
    }

    [Fact]
    public void HandlingACultureChange_AdoptsTheFormattingCultureSeparately()
    {
        var lookup = new CultureInfo("ko-KR");
        var formatting = (CultureInfo)lookup.Clone();
        formatting.NumberFormat.CurrencySymbol = "###";

        LocalizationNotifier.NotifyCultureChanged(lookup, formatting);

        CultureInfo.CurrentUICulture.Name.Should().Be("ko-KR", "lookups use the untransformed culture");
        CultureInfo.CurrentCulture.NumberFormat.CurrencySymbol.Should().Be(
            "###",
            "a FormatCultureBuilder result must survive the hop, not be collapsed into the lookup culture");
    }

    [Fact]
    public void ARefreshWithoutACultureChange_LeavesTheAmbientCultureAlone()
    {
        LocalizationSource.Instance.Refresh();

        CultureInfo.CurrentUICulture.Name.Should().Be("vi-VN");
        CultureInfo.CurrentCulture.Name.Should().Be("vi-VN");
    }

    [Fact]
    public void SetCultureCarriesTheFormattingCultureToTheNotification()
    {
        var options = new LocalizationOptions
        {
            FormatCultureBuilder = culture =>
            {
                culture.NumberFormat.CurrencySymbol = "###";
                return culture;
            }
        };

        CultureInfo? observedFormatCulture = null;

        void Handler(object? sender, LocalizationChangedEventArgs e) => observedFormatCulture = e.FormatCulture;

        LocalizationNotifier.CultureChanged += Handler;

        try
        {
            new LocalizationCultureManager(options).SetCulture("ko-KR");
        }
        finally
        {
            LocalizationNotifier.CultureChanged -= Handler;
        }

        observedFormatCulture.Should().NotBeNull();
        observedFormatCulture!.NumberFormat.CurrencySymbol.Should().Be("###");
    }
}
