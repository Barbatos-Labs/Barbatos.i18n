// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

namespace Barbatos.i18n;

/// <summary>
/// Options for configuring localization behavior globally via Dependency Injection.
/// </summary>
public class LocalizationOptions
{
    /// <summary>
    /// If true, automatically updates CultureInfo.CurrentCulture and CultureInfo.CurrentUICulture 
    /// whenever the UI localization language changes.
    /// Default is true.
    /// </summary>
    public bool SyncFormattingCulture { get; set; } = true;

    /// <summary>
    /// A custom builder function to modify or replace the formatting culture 
    /// whenever the localization culture changes. The parameter is the current localization culture.
    /// The returned culture will be set as CultureInfo.CurrentCulture.
    /// </summary>
    public Func<CultureInfo, CultureInfo>? CustomFormattingCultureBuilder { get; set; }
}
