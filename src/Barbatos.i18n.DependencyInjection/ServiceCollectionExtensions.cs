// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

namespace Barbatos.i18n.DependencyInjection;

/// <summary>
/// Provides extension methods for the <see cref="IServiceCollection"/> interface.
/// </summary>
public static class ServiceCollectionExtensions
{

    /// <summary>
    /// Adds the string localizer and related services to the specified <see cref="IServiceCollection"/> as the default provider.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configure">A delegate to configure the localization builder.</param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    public static IServiceCollection AddStringLocalizer(
        this IServiceCollection services,
        Action<LocalizationBuilder> configure
    )
    {
        return services.AddStringLocalizer((string?)null, configure);
    }

    /// <summary>
    /// Configures the global localization options.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configureOptions">A delegate to configure localization options.</param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    public static IServiceCollection ConfigureLocalizationOptions(
        this IServiceCollection services,
        Action<LocalizationOptions> configureOptions
    )
    {
        var optionsDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(LocalizationOptions));
        LocalizationOptions options;

        if (optionsDescriptor != null)
        {
            // ImplementationInstance is null for a type- or factory-based registration, and casting null to a
            // reference type succeeds silently, so the failure used to surface as a NullReferenceException
            // inside the caller's own configure delegate.
            if (optionsDescriptor.ImplementationInstance is not LocalizationOptions registeredOptions)
            {
                throw new InvalidOperationException(
                    $"{nameof(LocalizationOptions)} is already registered in a form this method cannot read. "
                        + $"Register it as an instance - services.AddSingleton(new {nameof(LocalizationOptions)}(...)) - "
                        + $"or call {nameof(ConfigureLocalizationOptions)} before registering it yourself."
                );
            }

            options = registeredOptions;
            services.Remove(optionsDescriptor);
        }
        else
        {
            options = new LocalizationOptions();
        }

        configureOptions(options);
        services.AddSingleton(options);
        
        return services;
    }

    /// <summary>
    /// Adds the string localizer and related services to the specified <see cref="IServiceCollection"/> with a specific provider key.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="providerKey">The key to associate with the provider. If null, it becomes the default provider.</param>
    /// <param name="configure">A delegate to configure the localization builder.</param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    public static IServiceCollection AddStringLocalizer(
        this IServiceCollection services,
        string? providerKey,
        Action<LocalizationBuilder> configure
    )
    {
        if (!services.Any(d => d.ServiceType == typeof(LocalizationOptions)))
        {
            services.AddSingleton(new LocalizationOptions());
        }

        DependencyInjectionLocalizationBuilder builder = new(services);

        configure(builder);

        var provider = builder.Build();

        var resolverDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(ILocalizationProviderResolver));
        LocalizationProviderResolver resolver;

        if (resolverDescriptor is null)
        {
            resolver = new LocalizationProviderResolver();
            services.AddSingleton<ILocalizationProviderResolver>(resolver);

            services.AddTransient<IStringLocalizerFactory, ProviderBasedStringLocalizerFactory>();
            services.AddTransient<ILocalizationCultureManager, DependencyInjectionLocalizationCultureManager>();
            services.AddTransient<ILocalizationProvider>(sp =>
                sp.GetRequiredService<ILocalizationProviderResolver>().GetProvider() ?? throw new InvalidOperationException("Default localization provider not found."));
            services.AddTransient<IStringLocalizer>(sp => 
                new ProviderBasedStringLocalizer(
                    sp.GetRequiredService<ILocalizationProvider>(), 
                    sp.GetRequiredService<ILocalizationCultureManager>(), 
                    null));

            // Register ICompositeStringLocalizer (composite interface with fallback across all localization sets)
            services.AddTransient<ICompositeStringLocalizer, CompositeStringLocalizer>();
            services.AddTransient(typeof(ICompositeStringLocalizer<>), typeof(CompositeStringLocalizer<>));
        }
        else
        {
            // Providers are handed to the resolver through LocalizationProviderResolver.AddProvider, which the
            // ILocalizationProviderResolver interface does not expose, so a foreign implementation cannot be
            // extended here. Say so instead of failing with NullReferenceException or InvalidCastException.
            if (resolverDescriptor.ImplementationInstance is not LocalizationProviderResolver registeredResolver)
            {
                throw new InvalidOperationException(
                    $"{nameof(ILocalizationProviderResolver)} is already registered in a form this method cannot extend. "
                        + $"Register it as a {nameof(LocalizationProviderResolver)} instance, or add every provider "
                        + $"through {nameof(AddStringLocalizer)} so the resolver is created here."
                );
            }

            resolver = registeredResolver;
        }

        resolver.AddProvider(providerKey, provider);

        return services;
    }
}