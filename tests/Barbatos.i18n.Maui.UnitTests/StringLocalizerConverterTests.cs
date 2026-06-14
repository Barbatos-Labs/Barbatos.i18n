// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

using System.Globalization;
using Barbatos.i18n;
using Barbatos.i18n.Maui;
using AwesomeAssertions;

namespace Barbatos.i18n.Maui.UnitTests;

[Collection("Sequential")]
public sealed class StringLocalizerConverterTests : IDisposable
{
    private readonly ILocalizationProvider _provider;

    public StringLocalizerConverterTests()
    {
        var builder = new LocalizationBuilder();
        var set = new LocalizationSet(null, new CultureInfo("vi-VN"), new[]
        {
            new KeyValuePair<LocalizationKey, string?>("price", "Giá bán: {0:C2}")
        });
        builder.AddLocalization(set);
        builder.SetCulture(new CultureInfo("vi-VN"));
        
        _provider = builder.Build();
        LocalizationProviderFactory.SetInstance(_provider, "");
        MauiLocalization.Initialize(null!); // Reset just in case
    }

    public void Dispose()
    {
        LocalizationProviderFactory.SetInstance(null!, "");
    }

    [Fact]
    public void Convert_ShouldFormatUsingCurrentCulture_WhenProviderCultureIsDifferent()
    {
        // Arrange
        // Simulate System Culture = en-US (e.g. FormatCultureBuilder = null)
        var sysCulture = new CultureInfo("en-US");
        var originalCulture = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = sysCulture;
        
        try
        {
            var converter = new StringLocalizerConverter("price", null, "");
            var values = new object[] { 1500000.50m };

            // Act
            // WPF UI element language passes its culture, usually en-US by default.
            var result = converter.Convert(values, typeof(string), null!, sysCulture);

            // Assert
            // It should format using CurrentCulture (sysCulture), NOT the provider culture.
            var expectedFormat = string.Format(sysCulture, "Giá bán: {0:C2}", 1500000.50m);
            result.Should().Be(expectedFormat);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }
}
