// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

namespace Barbatos.i18n.Sample.Wpf.Locales;

public class Strings
{
    /// <summary>
    /// Returns the key name "Test" for use with XAML markup extensions.
    /// Usage: {i18n:StringLocalizer {x:Static locales:Strings.Test}, Namespace='Strings'}
    /// </summary>
    public static string Test => nameof(Test);
}
