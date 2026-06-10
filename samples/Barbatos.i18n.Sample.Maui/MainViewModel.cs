// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Barbatos.i18n.Maui;

namespace Barbatos.i18n.Sample.Maui;

public partial class MainViewModel : ObservableObject
{
    public ObservableCollection<CultureInfo> SupportedCultures { get; } = new()
    {
        new CultureInfo("en-US"),
        new CultureInfo("vi-VN"),
        new CultureInfo("ko-KR"),
    };

    [ObservableProperty]
    private CultureInfo _selectedCulture;

    partial void OnSelectedCultureChanged(CultureInfo value)
    {
        if (value != null)
        {
            MauiLocalization.GetProvider()?.SetCulture(value);

            // Force property refresh for culture-sensitive bindings
            OnPropertyChanged(nameof(CurrentDate));
            OnPropertyChanged(nameof(Price));
            OnPropertyChanged(nameof(Distance));

            // Broadcast the change so the App can reload the UI
            WeakReferenceMessenger.Default.Send(new CultureChangedMessage(value));
        }
    }

    public ObservableCollection<string> PickerItems { get; } = new()
    {
        "PickerItem1",
        "PickerItem2",
        "PickerItem3"
    };

    [ObservableProperty]
    private int _appleCount = 1;

    [ObservableProperty]
    private string _userName = "John Doe";

    [ObservableProperty]
    private string _firstName = "John";

    [ObservableProperty]
    private string _lastName = "Smith";

    public DateTime CurrentDate { get; } = DateTime.Now;

    public decimal Price { get; } = 1500000.50m;

    public double Distance { get; } = 12345.678;

    [RelayCommand]
    private void IncrementApples() => AppleCount++;

    [RelayCommand]
    private void DecrementApples()
    {
        if (AppleCount > 0)
            AppleCount--;
    }

    public MainViewModel()
    {
        var currentCulture = CultureInfo.CurrentUICulture;
        _selectedCulture = SupportedCultures.FirstOrDefault(c => c.Name == currentCulture.Name) ?? SupportedCultures[0];
    }
}

public record CultureChangedMessage(CultureInfo Culture);
