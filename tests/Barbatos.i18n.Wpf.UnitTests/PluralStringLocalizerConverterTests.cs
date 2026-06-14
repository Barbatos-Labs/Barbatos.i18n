// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

using System.Globalization;
using Barbatos.i18n;
using Barbatos.i18n.Wpf;
using AwesomeAssertions;

namespace Barbatos.i18n.Wpf.UnitTests;

[Collection("Sequential")]
public class PluralStringLocalizerConverterTests : IDisposable
{
    private readonly ILocalizationProvider _provider;

    public PluralStringLocalizerConverterTests()
    {
        var builder = new LocalizationBuilder();
        var set = new LocalizationSet(null, new CultureInfo("vi-VN"), new[]
        {
            new KeyValuePair<LocalizationKey, string?>("distance", "Khoảng cách: {0:N2} km"),
            new KeyValuePair<LocalizationKey, string?>("distances", "Khoảng cách: {0:N2} km")
        });
        builder.AddLocalization(set);
        builder.SetCulture(new CultureInfo("vi-VN"));
        
        _provider = builder.Build();
        LocalizationProviderFactory.SetInstance(_provider, "");
        WpfLocalization.Initialize(null!); // Reset just in case
    }

    public void Dispose()
    {
        LocalizationProviderFactory.SetInstance(null!, "");
    }

    [Fact]
    public void Convert_ShouldFormatUsingCurrentCulture_WhenProviderCultureIsDifferent()
    {
        // Arrange
        var sysCulture = new CultureInfo("en-US");
        var originalCulture = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = sysCulture;

        try
        {
            var converter = new PluralStringLocalizerConverter("distance", "distances", null, "");
            var values = new object[] { 12345 }; // Plural count

            // Act
            // WPF provides en-US
            var result = converter.Convert(values, typeof(string), null!, sysCulture);

            // Assert
            // It should format using CurrentCulture (sysCulture), NOT the provider culture.
            var expectedFormat = string.Format(sysCulture, "Khoảng cách: {0:N2} km", 12345);
            result.Should().Be(expectedFormat);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }
}
