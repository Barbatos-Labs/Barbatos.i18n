// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

using System.Windows.Controls;

namespace Barbatos.i18n.Sample.Wpf;

public partial class HomePage : Page
{
    private static readonly HomeViewModel _viewModel = new();

    public HomePage()
    {
        InitializeComponent();
        DataContext = _viewModel;
    }

    // No code-behind is needed to switch language: HomeViewModel.OnSelectedCultureChanged calls
    // SetLocalizationCulture, LocalizationNotifier tells LocalizationSource, and every {i18n:...}
    // binding re-translates in place. This used to require Navigate(new HomePage()) to redraw the page.
}
