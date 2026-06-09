// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

using System.Collections.Concurrent;

namespace Barbatos.i18n;

/// <summary>
/// Provides a factory for managing the current instance of the <see cref="LocalizationProvider"/>.
/// </summary>
public static class LocalizationProviderFactory
{
    private static readonly ConcurrentDictionary<string, ILocalizationProvider> _instances = new();

    /// <summary>
    /// Gets the current instance of the <see cref="ILocalizationProvider"/>.
    /// </summary>
    /// <returns>The current instance of the <see cref="ILocalizationProvider"/>, or null if no instance has been set.</returns>
    public static ILocalizationProvider? GetInstance()
    {
        return GetInstance(string.Empty);
    }

    /// <summary>
    /// Gets the current instance of the <see cref="ILocalizationProvider"/>.
    /// </summary>
    /// <returns>The current instance of the <see cref="ILocalizationProvider"/>, or null if no instance has been set.</returns>
    public static ILocalizationProvider? GetInstance(string key)
    {
        _ = _instances.TryGetValue(key, out ILocalizationProvider instance);

        return instance;
    }

    /// <summary>
    /// Sets the current instance of the <see cref="ILocalizationProvider"/>.
    /// </summary>
    /// <param name="provider">The <see cref="ILocalizationProvider"/> to set as the current instance.</param>
    public static void SetInstance(ILocalizationProvider provider)
    {
        SetInstance(provider, string.Empty);
    }

    /// <summary>
    /// Sets the current instance of the <see cref="ILocalizationProvider"/>.
    /// </summary>
    /// <param name="provider">The <see cref="ILocalizationProvider"/> to set as the current instance.</param>
    public static void SetInstance(ILocalizationProvider provider, string key)
    {
        _ = _instances.AddOrUpdate(key, provider, (_, _) => provider);
    }
}

