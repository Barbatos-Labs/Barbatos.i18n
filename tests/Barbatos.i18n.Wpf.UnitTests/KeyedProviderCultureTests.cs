// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

using System.Globalization;
using AwesomeAssertions;
using Barbatos.i18n;
using Barbatos.i18n.Wpf;

namespace Barbatos.i18n.Wpf.UnitTests;

/// <summary>
/// A culture change must reach every registered provider, not only the default-keyed one. While it did not, an
/// application wiring localization without the DependencyInjection package saw half its screen switch language
/// and every string resolved through a ProviderKey stay behind.
/// </summary>
[Collection("Sequential")]
public sealed class KeyedProviderCultureTests : IDisposable
{
    private static readonly CultureInfo English = new("en-US");
    private static readonly CultureInfo Vietnamese = new("vi-VN");

    private readonly CultureInfo _originalUiCulture = CultureInfo.CurrentUICulture;
    private readonly CultureInfo _originalCulture = CultureInfo.CurrentCulture;

    public KeyedProviderCultureTests()
    {
        LocalizationProviderFactory.SetInstance(Build(), string.Empty);
        LocalizationProviderFactory.SetInstance(Build(), "Secondary");
        WpfLocalization.Initialize(null!);
    }

    public void Dispose()
    {
        LocalizationProviderFactory.SetInstance(null!, string.Empty);
        LocalizationProviderFactory.SetInstance(null!, "Secondary");
        CultureInfo.CurrentUICulture = _originalUiCulture;
        CultureInfo.CurrentCulture = _originalCulture;
    }

    private static ILocalizationProvider Build()
    {
        var builder = new LocalizationBuilder();

        builder.AddLocalization(new LocalizationSet("extra", English,
            new Dictionary<LocalizationKey, string?> { { "bonus", "English bonus" } }));
        builder.AddLocalization(new LocalizationSet("extra", Vietnamese,
            new Dictionary<LocalizationKey, string?> { { "bonus", "Vietnamese bonus" } }));
        builder.SetCulture(English);

        return builder.Build();
    }

    private static object? Resolve(string providerKey) =>
        new StringLocalizerExtension { Text = "bonus", Namespace = "extra", ProviderKey = providerKey, Live = false }
            .ProvideValue(null!);

    [Fact]
    public void SetCulture_MovesEveryRegisteredProvider_NotOnlyTheDefaultOne()
    {
        new LocalizationCultureManager().SetCulture(Vietnamese);

        Resolve(string.Empty).Should().Be("Vietnamese bonus");
        Resolve("Secondary").Should().Be(
            "Vietnamese bonus",
            "a provider registered under a ProviderKey has to follow the culture like every other one");
    }

    [Fact]
    public void GetSupportedCultures_CoversEveryRegisteredProvider()
    {
        new LocalizationCultureManager().GetSupportedCultures()
            .Should().Contain(English).And.Contain(Vietnamese);
    }

    [Fact]
    public void GetAllInstances_SkipsClearedRegistrations()
    {
        LocalizationProviderFactory.SetInstance(null!, "Cleared");

        LocalizationProviderFactory.GetAllInstances().Should().NotContainNulls();
    }
}
