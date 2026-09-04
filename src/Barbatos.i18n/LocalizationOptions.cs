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

    /// <summary>
    /// Gets or sets whether lookups follow <see cref="CultureInfo.CurrentUICulture"/> instead of the culture the
    /// provider was last set to. Off by default.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A provider's culture is process-wide state, which suits an application with one user and one language at a
    /// time. A server does not fit that shape: turning the request's language into a call to
    /// <c>SetCulture</c> lets concurrent requests overwrite each other, and a request can be answered in
    /// another request's language.
    /// </para>
    /// <para>
    /// Turn this on in ASP.NET Core, where <c>UseRequestLocalization</c> already establishes
    /// <see cref="CultureInfo.CurrentUICulture"/> per request and it flows with the async context. No middleware
    /// then needs to call <c>SetCulture</c> at all.
    /// </para>
    /// </remarks>
    public bool UseAmbientCulture { get; set; }
}
