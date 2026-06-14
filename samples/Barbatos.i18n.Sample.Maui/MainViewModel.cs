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
            MauiLocalization.GetCultureManager()?.SetCulture(value);

            // Force property refresh for culture-sensitive bindings
            OnPropertyChanged(nameof(CurrentDate));
            OnPropertyChanged(nameof(Price));
            OnPropertyChanged(nameof(Distance));

            // Broadcast the change so the App can reload the UI
            WeakReferenceMessenger.Default.Send(new CultureChangedMessage(value));
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

public record CultureChangedMessage(CultureInfo Culture);
