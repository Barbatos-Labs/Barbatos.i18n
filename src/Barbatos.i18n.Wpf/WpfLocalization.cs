// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

namespace Barbatos.i18n.Wpf;

/// <summary>
/// Provides a global integration point (Service Locator Bridge) for Dependency Injection in WPF.
/// Because XAML <see cref="System.Windows.Markup.MarkupExtension"/> (like StringLocalizerExtension) 
/// are instantiated by the XAML parser using their default constructors, they cannot use standard 
/// constructor injection. This class bridges the gap by holding a static reference to the application's 
/// <see cref="IServiceProvider"/>, allowing XAML extensions to resolve localization providers from the DI container.
/// </summary>
public static class WpfLocalization
{
    /// <summary>
    /// Gets the service provider used for WPF localization.
    /// </summary>
    public static IServiceProvider? ServiceProvider { get; private set; }

    /// <summary>
    /// Initializes WPF localization with the specified service provider.
    /// </summary>
    /// <param name="serviceProvider">The service provider to use.</param>
    public static void Initialize(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    /// <summary>
    /// Gets a localization provider from the dependency injection container by its key.
    /// </summary>
    /// <param name="key">The key of the provider. If null, the default provider is returned.</param>
    /// <returns>The <see cref="ILocalizationProvider"/> if found; otherwise, null.</returns>
    public static ILocalizationProvider? GetProvider(string? key = null)
    {
        if (ServiceProvider == null) return null;

        var resolver = ServiceProvider.GetService(typeof(ILocalizationProviderResolver)) as ILocalizationProviderResolver;
        if (resolver != null)
        {
            return resolver.GetProvider(key);
        }

        return null;
    }
}
