// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Barbatos.i18n.Maui;

namespace Barbatos.i18n.Sample.Maui;

public partial class MainViewModel : ObservableObject
{
    public ObservableCollection<CultureInfo> SupportedCultures { get; } = new(
        [..MauiLocalization.GetCultureManager()?.GetSupportedCultures() ?? [CultureInfo.CurrentCulture], new CultureInfo("zh-CN")]);

    [ObservableProperty]
    private CultureInfo _selectedCulture;

    partial void OnSelectedCultureChanged(CultureInfo value)
    {
        if (value != null)
        {
            // Every {i18n:...} binding re-evaluates through LocalizationSource, so the AppShell does not need
            // rebuilding and CurrentDate / Price / Distance no longer need a manual OnPropertyChanged.
            MauiLocalization.GetCultureManager()?.SetCulture(value);
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

    [RelayCommand]
    private async Task ShowMessageAsync()
    {
        var provider = MauiLocalization.GetProvider();
        if (provider == null) return;

        var culture = provider.GetCulture();
        var set = provider.GetLocalizationSet(culture, null);
        if (set == null) return;

        string title = set["MessageTitle"] ?? "Hello from Code-Behind";
        string message = set.Format("MessageContent", UserName) ?? $"Welcome to Barbatos.i18n, {UserName}!";
        string ok = set["Ok"] ?? "OK";

        if (Shell.Current != null)
        {
            await Shell.Current.DisplayAlertAsync(title, message, ok);
        }
    }

    [RelayCommand]
    private async Task ShowConfirmationAsync()
    {
        var provider = MauiLocalization.GetProvider();
        if (provider == null) return;

        var culture = provider.GetCulture();
        var set = provider.GetLocalizationSet(culture, null);
        if (set == null) return;

        string title = set["QuestionTitle"] ?? "Confirmation";
        string message = set["QuestionContent"] ?? "Are you sure you want to proceed?";
        string yes = set["Yes"] ?? "Yes";
        string no = set["No"] ?? "No";

        if (Shell.Current != null)
        {
            bool result = await Shell.Current.DisplayAlertAsync(title, message, yes, no);

            string responseTitle = set["MessageTitle"] ?? "Result";
            string responseMessage = result ? (set["Yes"] ?? "Yes") : (set["No"] ?? "No");
            string ok = set["Ok"] ?? "OK";
            await Shell.Current.DisplayAlertAsync(responseTitle, responseMessage, ok);
        }
    }

    [RelayCommand]
    private async Task ShowPromptAsync()
    {
        var provider = MauiLocalization.GetProvider();
        if (provider == null) return;

        var culture = provider.GetCulture();
        var set = provider.GetLocalizationSet(culture, null);
        if (set == null) return;

        string title = set["PromptTitle"] ?? "Input Name";
        string message = set["PromptContent"] ?? "Please enter your name:";
        string ok = set["Ok"] ?? "OK";
        string cancel = set["Cancel"] ?? "Cancel";
        string placeholder = set["PromptPlaceholder"] ?? "Type name here...";

        if (Shell.Current != null)
        {
            string result = await Shell.Current.DisplayPromptAsync(title, message, ok, cancel, placeholder);
            if (!string.IsNullOrWhiteSpace(result))
            {
                UserName = result;
            }
        }
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
