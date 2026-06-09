// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Barbatos.i18n.Wpf;

namespace Barbatos.i18n.Sample.Wpf;

public partial class HomeViewModel : ObservableObject
{
    public ObservableCollection<CultureInfo> SupportedCultures { get; } = new()
    {
        new CultureInfo("en-US"),
        new CultureInfo("vi-VN"),
        new CultureInfo("ko-KR"),
        new CultureInfo("zh-CN") // Missing locale to test fallback
    };

    [ObservableProperty]
    private CultureInfo _selectedCulture;

    public HomeViewModel()
    {
        var currentCulture = CultureInfo.CurrentUICulture;
        _selectedCulture = SupportedCultures.FirstOrDefault(c => c.Name == currentCulture.Name) ?? SupportedCultures[0];
    }

    partial void OnSelectedCultureChanged(CultureInfo value)
    {
        if (value != null)
        {
            // Thay đổi ngôn ngữ UI. Tính năng Formatting Culture sẽ được thư viện DI tự động đồng bộ ngầm!
            System.Windows.Application.Current.SetLocalizationCulture(value);
            
            // Force property change to refresh the bindings that rely solely on CurrentDate or Price
            OnPropertyChanged(nameof(CurrentDate));
            OnPropertyChanged(nameof(Price));
            OnPropertyChanged(nameof(Distance));
        }
    }
    public ObservableCollection<string> AvailableOptions { get; } = new()
    {
        "ComboBoxItem1",
        "ComboBoxItem2",
        "ComboBoxItem3"
    };
    [ObservableProperty]
    private int _appleCount = 1;

    [ObservableProperty]
    private string _userName = "John Doe";

    [ObservableProperty]
    private string _firstName = "John";

    [ObservableProperty]
    private string _lastName = "Smith";

    [ObservableProperty]
    private DateTime _currentDate = DateTime.Now;

    [ObservableProperty]
    private decimal _price = 1500000.50m;

    [ObservableProperty]
    private double _distance = 12345.678;

    [RelayCommand]
    private void IncrementApples()
    {
        AppleCount++;
    }

    [RelayCommand]
    private void DecrementApples()
    {
        if (AppleCount > 0)
        {
            AppleCount--;
        }
    }

    [RelayCommand]
    private void ShowMessage()
    {
        // Simple: Use ICompositeStringLocalizer to access keys from ANY localization set (RESX, JSON, YAML, INI, CSV)
        // var localizer = ((App)System.Windows.Application.Current).ServiceProvider.GetRequiredService<Barbatos.i18n.DependencyInjection.ICompositeStringLocalizer>();

        // Simple: Use ICompositeStringLocalizer to access keys from ANY localization set (RESX, JSON, YAML, INI, CSV)
        var localizer = ((App)System.Windows.Application.Current).ServiceProvider.GetRequiredService<Barbatos.i18n.DependencyInjection.ICompositeStringLocalizer>();

        string title = localizer["MessageTitle"];
        string message = localizer["MessageContent", UserName];

        System.Windows.MessageBox.Show(message, title, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
    }
}
