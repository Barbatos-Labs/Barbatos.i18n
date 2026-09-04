// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

using System.ComponentModel;
using System.Globalization;
using Barbatos.i18n;
using Barbatos.i18n.Maui;
using AwesomeAssertions;

namespace Barbatos.i18n.Maui.UnitTests;

/// <summary>
/// Covers the live-localization plumbing: the culture slot the markup extensions prepend to their multi-binding,
/// keys supplied by a binding, and the observable that drives re-evaluation.
/// </summary>
[Collection("Sequential")]
public sealed class LiveLocalizationTests : IDisposable
{
    private readonly CultureInfo _originalCulture;

    public LiveLocalizationTests()
    {
        _originalCulture = CultureInfo.CurrentCulture;

        var builder = new LocalizationBuilder();
        var set = new LocalizationSet(null, new CultureInfo("vi-VN"), new[]
        {
            new KeyValuePair<LocalizationKey, string?>("price", "Giá bán: {0:C2}"),
            new KeyValuePair<LocalizationKey, string?>("greeting", "Xin chào {0}"),
            new KeyValuePair<LocalizationKey, string?>("apple", "Một quả táo ({0})"),
            new KeyValuePair<LocalizationKey, string?>("apples", "Nhiều quả táo ({0})"),
            new KeyValuePair<LocalizationKey, string?>("active", "Đang bán"),
            new KeyValuePair<LocalizationKey, string?>("archived", "Đã lưu trữ")
        });
        builder.AddLocalization(set);
        builder.SetCulture(new CultureInfo("vi-VN"));

        LocalizationProviderFactory.SetInstance(builder.Build(), "");
        MauiLocalization.Initialize(null!);
    }

    public void Dispose()
    {
        CultureInfo.CurrentCulture = _originalCulture;
        LocalizationProviderFactory.SetInstance(null!, "");
    }

    [Fact]
    public void Convert_ShouldSkipTheCultureSlot_WhenTheKeyIsStatic()
    {
        var converter = new StringLocalizerConverter("price", null, "", hasCultureSlot: true, keyFromBinding: false);
        var values = new object[] { new CultureInfo("vi-VN"), 1500000.50m };

        var result = converter.Convert(values, typeof(string), null!, CultureInfo.CurrentCulture);

        result.Should().Be(string.Format(CultureInfo.CurrentCulture, "Giá bán: {0:C2}", 1500000.50m));
    }

    [Fact]
    public void Convert_ShouldReadTheKeyFromTheBoundValue()
    {
        var converter = new StringLocalizerConverter(null, null, "", hasCultureSlot: true, keyFromBinding: true);
        var values = new object[] { new CultureInfo("vi-VN"), "greeting", "Hùng" };

        var result = converter.Convert(values, typeof(string), null!, CultureInfo.CurrentCulture);

        result.Should().Be("Xin chào Hùng");
    }

    [Fact]
    public void Convert_ShouldUseAnEnumMemberNameAsTheKey()
    {
        var converter = new StringLocalizerConverter(null, null, "", hasCultureSlot: true, keyFromBinding: true);
        var values = new object[] { new CultureInfo("vi-VN"), SampleStatus.Active };

        var result = converter.Convert(values, typeof(string), null!, CultureInfo.CurrentCulture);

        result.Should().Be("Đang bán");
    }

    [Fact]
    public void Convert_ShouldUseAnEnumMemberNameAsTheKey_RegardlessOfCasing()
    {
        var converter = new StringLocalizerConverter(null, null, "", hasCultureSlot: true, keyFromBinding: true);
        var values = new object[] { new CultureInfo("vi-VN"), SampleStatus.ARCHIVED };

        var result = converter.Convert(values, typeof(string), null!, CultureInfo.CurrentCulture);

        result.Should().Be("Đã lưu trữ");
    }

    [Fact]
    public void Convert_ShouldReturnEmpty_WhenTheBoundKeyIsMissing()
    {
        var converter = new StringLocalizerConverter(null, null, "", hasCultureSlot: true, keyFromBinding: true);
        var values = new object[] { new CultureInfo("vi-VN"), null! };

        var result = converter.Convert(values, typeof(string), null!, CultureInfo.CurrentCulture);

        result.Should().Be(string.Empty);
    }

    [Fact]
    public void Convert_ShouldReturnTheBoundKey_WhenNoTranslationExists()
    {
        var converter = new StringLocalizerConverter(null, null, "", hasCultureSlot: true, keyFromBinding: true);
        var values = new object[] { new CultureInfo("vi-VN"), "unknown-key" };

        var result = converter.Convert(values, typeof(string), null!, CultureInfo.CurrentCulture);

        result.Should().Be("unknown-key");
    }

    [Fact]
    public void PluralConvert_ShouldUseTheStaticCount_WhenNoValueCarriesOne()
    {
        var converter = new PluralStringLocalizerConverter(
            "apple", "apples", null, "", hasCultureSlot: true, keyFromBinding: false, pluralKeyFromBinding: false, staticCount: 5);
        var values = new object[] { new CultureInfo("vi-VN") };

        var result = converter.Convert(values, typeof(string), null!, CultureInfo.CurrentCulture);

        result.Should().Be("Nhiều quả táo (5)");
    }

    [Fact]
    public void PluralConvert_ShouldPreferTheBoundCountOverTheStaticOne()
    {
        var converter = new PluralStringLocalizerConverter(
            "apple", "apples", null, "", hasCultureSlot: true, keyFromBinding: false, pluralKeyFromBinding: false, staticCount: 5);
        var values = new object[] { new CultureInfo("vi-VN"), 1 };

        var result = converter.Convert(values, typeof(string), null!, CultureInfo.CurrentCulture);

        result.Should().Be("Một quả táo (1)");
    }

    [Fact]
    public void PluralConvert_ShouldReadBothKeysFromBoundValues()
    {
        var converter = new PluralStringLocalizerConverter(
            null, null, null, "", hasCultureSlot: true, keyFromBinding: true, pluralKeyFromBinding: true, staticCount: null);
        var values = new object[] { new CultureInfo("vi-VN"), "apple", "apples", 3 };

        var result = converter.Convert(values, typeof(string), null!, CultureInfo.CurrentCulture);

        result.Should().Be("Nhiều quả táo (3)");
    }

    [Fact]
    public void PluralExtension_NonLivePath_ShouldFallBackToText_WhenPluralTextIsMissingAndCountIsPlural()
    {
        var extension = new PluralStringLocalizerExtension { Text = "apple", Count = 5, Live = false };

        var binding = (Binding)extension.ProvideValue(null!);

        binding.Source.Should().Be("Một quả táo (5)");
    }

    [Fact]
    public void PluralExtension_NonLivePath_ShouldFallBackToPluralText_WhenTextIsMissingAndCountIsSingular()
    {
        var extension = new PluralStringLocalizerExtension { PluralText = "apples", Count = 1, Live = false };

        var binding = (Binding)extension.ProvideValue(null!);

        binding.Source.Should().Be("Nhiều quả táo (1)");
    }

    [Fact]
    public void LocalizationSource_ShouldRaisePropertyChanged_WhenTheCultureChanges()
    {
        LocalizationSource source = LocalizationSource.Instance;
        string? propertyName = null;

        void Handler(object? sender, PropertyChangedEventArgs e) => propertyName = e.PropertyName;

        source.PropertyChanged += Handler;

        try
        {
            LocalizationNotifier.NotifyCultureChanged(new CultureInfo("ko-KR"));
        }
        finally
        {
            source.PropertyChanged -= Handler;
        }

        propertyName.Should().Be(nameof(LocalizationSource.Culture));
        source.Culture.Name.Should().Be("ko-KR");
    }

    /// <summary>
    /// Deliberately mixed casing: <see cref="LocalizationKey"/> lowercases every key, so the member name matches
    /// its resource entry however the enum is written.
    /// </summary>
    private enum SampleStatus
    {
        Active,
        ARCHIVED
    }
}
