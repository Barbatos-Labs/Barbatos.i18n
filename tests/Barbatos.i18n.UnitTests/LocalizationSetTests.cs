// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

namespace Barbatos.i18n.UnitTests;

public class LocalizationSetTests
{
    [Theory]
    [InlineData("en-US", "Date: {0:d}", "Price: {0:C}")]
    [InlineData("vi-VN", "Ngày hiện tại: {0:d}", "Giá bán: {0:C}")]
    [InlineData("ko-KR", "현재 날짜: {0:d}", "가격: {0:C}")]
    [InlineData("zh-CN", "当前日期：{0:d}", "价格：{0:C}")]
    public void Format_WithArguments_UsesCultureOfLocalizationSet(string cultureName, string dateTemplate, string currencyTemplate)
    {
        // Arrange
        var culture = new CultureInfo(cultureName);
        var keys = new Dictionary<LocalizationKey, string?>
        {
            { "DateTest", dateTemplate },
            { "CurrencyTest", currencyTemplate }
        };

        var localizationSet = new LocalizationSet("test", culture, keys);

        var date = new DateTime(2026, 6, 8);
        var price = 1500000.50m;

        // Act
        var formattedDate = localizationSet.Format("DateTest", date);
        var formattedCurrency = localizationSet.Format("CurrencyTest", price);

        // Assert
        // We use string.Format directly with the specific culture to establish the "expected" correct answer natively from .NET
        var expectedDate = string.Format(culture, dateTemplate, date);
        var expectedCurrency = string.Format(culture, currencyTemplate, price);

        Assert.Equal(expectedDate, formattedDate);
        Assert.Equal(expectedCurrency, formattedCurrency);
    }
}
