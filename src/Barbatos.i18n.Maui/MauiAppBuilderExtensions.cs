// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

namespace Barbatos.i18n.Maui;

/// <summary>
/// Provides extension methods for the <see cref="MauiAppBuilder"/> class.
/// </summary>
public static class MauiAppBuilderExtensions
{
    /// <summary>
    /// Configures the application to use a string localizer.
    /// </summary>
    /// <param name="builder">The application to configure.</param>
    /// <param name="configure">A delegate to configure the localization builder.</param>
    /// <returns>The configured application.</returns>
    public static MauiAppBuilder UseStringLocalizer(this MauiAppBuilder builder, Action<LocalizationBuilder> configure)
    {
        return builder.UseStringLocalizer(null, configure);
    }

    /// <summary>
    /// Configures the application to use a string localizer with a specific provider key.
    /// </summary>
    /// <param name="builder">The application to configure.</param>
    /// <param name="providerKey">The key to associate with the provider. If null, it becomes the default provider.</param>
    /// <param name="configure">A delegate to configure the localization builder.</param>
    /// <returns>The configured application.</returns>
    public static MauiAppBuilder UseStringLocalizer(this MauiAppBuilder builder, string? providerKey, Action<LocalizationBuilder> configure)
    {
        var locBuilder = new LocalizationBuilder();
        configure(locBuilder);
        var provider = locBuilder.Build();

        LocalizationProviderFactory.SetInstance(provider, providerKey ?? string.Empty);

        // Check if ILocalizationProviderResolver is already registered
        var resolverDescriptor = builder.Services.FirstOrDefault(d => d.ServiceType == typeof(ILocalizationProviderResolver));
        LocalizationProviderResolver resolver;

        if (resolverDescriptor is null)
        {
            resolver = new LocalizationProviderResolver();
            builder.Services.AddSingleton<ILocalizationProviderResolver>(resolver);

            // Register default provider to ILocalizationProvider
            builder.Services.AddSingleton<ILocalizationProvider>(sp =>
                sp.GetRequiredService<ILocalizationProviderResolver>().GetProvider()
                ?? throw new InvalidOperationException("Default localization provider not found."));

            // Register culture manager using the default provider
            builder.Services.AddSingleton<ILocalizationCultureManager>(sp =>
                new DefaultLocalizationCultureManager(sp.GetRequiredService<ILocalizationProvider>()));
        }
        else
        {
            resolver = (LocalizationProviderResolver)(resolverDescriptor.ImplementationInstance
                ?? throw new InvalidOperationException("ILocalizationProviderResolver registered is not an instance of LocalizationProviderResolver."));
        }

        resolver.AddProvider(providerKey, provider);

        return builder;
    }

    private sealed class DefaultLocalizationCultureManager : ILocalizationCultureManager
    {
        private readonly ILocalizationProvider _provider;
        public DefaultLocalizationCultureManager(ILocalizationProvider provider) => _provider = provider;
        public void SetCulture(CultureInfo cultureInfo) => _provider.SetCulture(cultureInfo);
        public void SetCulture(string cultureName) => _provider.SetCulture(new CultureInfo(cultureName));
        public CultureInfo GetCulture() => _provider.GetCulture();
        public LocalizationOptions Options => new LocalizationOptions();
    }
}
