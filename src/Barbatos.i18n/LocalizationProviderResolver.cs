// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

using System.Collections.Concurrent;

namespace Barbatos.i18n;

/// <summary>
/// A default implementation of <see cref="ILocalizationProviderResolver"/> that manages localization providers.
/// </summary>
public class LocalizationProviderResolver : ILocalizationProviderResolver
{
    private readonly ConcurrentDictionary<string, ILocalizationProvider> _providers = new();

    /// <summary>
    /// Adds or updates a localization provider with the specified key.
    /// </summary>
    /// <param name="key">The key associated with the provider. If null, an empty string is used as the default key.</param>
    /// <param name="provider">The localization provider to register.</param>
    public void AddProvider(string? key, ILocalizationProvider provider)
    {
        _providers.AddOrUpdate(key ?? string.Empty, provider, (_, _) => provider);
    }

    /// <inheritdoc />
    public ILocalizationProvider? GetProvider(string? key = null)
    {
        _providers.TryGetValue(key ?? string.Empty, out var provider);
        return provider;
    }

    /// <inheritdoc />
    public IEnumerable<ILocalizationProvider> GetAllProviders()
    {
        return _providers.Values;
    }
}
