// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Barbatos.i18n.Wpf;

namespace Barbatos.i18n.Sample.Wpf;

public partial class HomePage : Page
{
    private static readonly HomeViewModel _viewModel = new();

    public HomePage()
    {
        InitializeComponent();
        DataContext = _viewModel;
    }

    private void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (IsLoaded && e.AddedItems.Count > 0 && e.AddedItems[0] is CultureInfo selectedCulture)
        {
            Dispatcher.BeginInvoke(() =>
            {
                (Application.Current as App)?.ServiceProvider.SetLocalizationCulture(selectedCulture);
                NavigationService?.Navigate(new HomePage());
            }, DispatcherPriority.Normal);
        }
    }
}
