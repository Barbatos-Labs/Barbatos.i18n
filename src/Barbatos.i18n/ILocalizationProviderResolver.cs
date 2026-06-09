// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

namespace Barbatos.i18n;

/// <summary>
/// Provides a mechanism for resolving localization providers by key.
/// </summary>
public interface ILocalizationProviderResolver
{
    /// <summary>
    /// Gets the localization provider associated with the specified key.
    /// </summary>
    /// <param name="key">The key of the provider to retrieve. If null or empty, retrieves the default provider.</param>
    /// <returns>The <see cref="ILocalizationProvider"/> if found; otherwise, null.</returns>
    ILocalizationProvider? GetProvider(string? key = null);

    /// <summary>
    /// Gets all registered localization providers.
    /// </summary>
    /// <returns>An enumerable of all registered <see cref="ILocalizationProvider"/> instances.</returns>
    IEnumerable<ILocalizationProvider> GetAllProviders();
}
