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
    /// Gets or sets a custom builder to modify the formatting culture before it is set.
    /// If provided, <see cref="CultureInfo.CurrentCulture"/> will be updated alongside <see cref="CultureInfo.CurrentUICulture"/>.
    /// </summary>
    public Func<CultureInfo, CultureInfo>? FormatCultureBuilder { get; set; }
}
