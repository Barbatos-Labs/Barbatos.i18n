// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

namespace Barbatos.i18n.Maui;

/// <summary>
/// Provides a global integration point (Service Locator Bridge) for Dependency Injection in MAUI.
/// </summary>
public static class MauiLocalization
{
    /// <summary>
    /// Gets the service provider used for MAUI localization.
    /// </summary>
    public static IServiceProvider? ServiceProvider { get; private set; }

    private static ILocalizationProvider? _directProvider;

    /// <summary>
    /// Initializes MAUI localization with the specified service provider.
    /// </summary>
    /// <param name="serviceProvider">The service provider to use.</param>
    public static void Initialize(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    /// <summary>
    /// Initializes MAUI localization with the specified provider.
    /// </summary>
    /// <param name="provider">The provider to use.</param>
    public static void Initialize(ILocalizationProvider provider)
    {
        _directProvider = provider;
    }

    /// <summary>
    /// Gets a localization provider from the dependency injection container by its key.
    /// </summary>
    /// <param name="key">The key of the provider. If null, the default provider is returned.</param>
    /// <returns>The <see cref="ILocalizationProvider"/> if found; otherwise, null.</returns>
    public static ILocalizationProvider? GetProvider(string? key = null)
    {
        if (_directProvider != null) return _directProvider;

        if (ServiceProvider == null) return null;

        var resolver = ServiceProvider.GetService(typeof(ILocalizationProviderResolver)) as ILocalizationProviderResolver;
        if (resolver != null)
        {
            return resolver.GetProvider(key);
        }

        return null;
    }
}
