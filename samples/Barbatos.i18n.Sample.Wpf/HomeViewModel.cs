// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Barbatos.i18n.Wpf;

namespace Barbatos.i18n.Sample.Wpf;

public partial class HomeViewModel : ObservableObject
{
    public ObservableCollection<CultureInfo> SupportedCultures { get; } = new(
        [..WpfLocalization.GetCultureManager()?.GetSupportedCultures() ?? [CultureInfo.CurrentCulture], new CultureInfo("zh-CN")]);

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
            // Switches the UI language. The DI layer keeps the formatting culture in sync under the hood.
            // Every {i18n:...} binding re-evaluates through LocalizationSource, so the page does not need
            // re-navigating and CurrentDate / Price / Distance no longer need a manual OnPropertyChanged.
            System.Windows.Application.Current.SetLocalizationCulture(value);
        }
    }

    /// <summary>
    /// Raw localization keys, translated by the item template. No converter resource to declare.
    /// </summary>
    public ObservableCollection<string> AvailableOptions { get; } = new()
    {
        "ComboBoxItem1",
        "ComboBoxItem2",
        "ComboBoxItem3"
    };

    /// <summary>
    /// Enum members double as localization keys: OrderStatus.Active resolves the "Active" entry.
    /// </summary>
    public IReadOnlyList<OrderStatus> StatusOptions { get; } = Enum.GetValues<OrderStatus>();

    /// <summary>
    /// Rows whose translation key lives in the item itself. This is what BindText is for.
    /// </summary>
    public ObservableCollection<ProductRow> Products { get; } = new()
    {
        new ProductRow("Barbatos Keyboard", OrderStatus.Active, 12),
        new ProductRow("Barbatos Mouse", OrderStatus.Pending, 1),
        new ProductRow("Barbatos Headset", OrderStatus.Archived, 0)
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
        var localizer = ((App)System.Windows.Application.Current).ServiceProvider.GetRequiredService<Barbatos.i18n.DependencyInjection.ICompositeStringLocalizer>();

        string title = localizer["MessageTitle"];
        string message = localizer["MessageContent", UserName];

        System.Windows.MessageBox.Show(message, title, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
    }
}

/// <summary>
/// Status is bound straight to the markup extension: the enum member name is the localization key.
/// </summary>
public record ProductRow(string Name, OrderStatus Status, int Stock);

/// <summary>
/// Each member name matches a key in the localization files, so the enum needs no lookup table.
/// </summary>
public enum OrderStatus
{
    Active,
    Pending,
    Archived
}
