// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

namespace Barbatos.i18n.Maui;

/// <summary>
/// Provides extension methods for the <see cref="MauiAppBuilder"/> class.
/// </summary>
public static class ApplicationExtensions
{
    /// <summary>
    /// Configures the application to use a string localizer.
    /// </summary>
    /// <param name="builder">The application to configure.</param>
    /// <param name="configure">A delegate to configure the localization builder.</param>
    /// <returns>The configured application.</returns>
    public static MauiAppBuilder UseBarbatosLocalization(this MauiAppBuilder builder, Action<LocalizationBuilder> configure)
    {
        var locBuilder = new LocalizationBuilder();
        configure(locBuilder);
        var provider = locBuilder.Build();

        LocalizationProviderFactory.SetInstance(provider);
        MauiLocalization.Initialize(provider);

        // Register services for DI
        builder.Services.AddSingleton<ILocalizationProvider>(provider);
        builder.Services.AddSingleton<ILocalizationProviderResolver>(new DefaultLocalizationProviderResolver(provider));
        builder.Services.AddSingleton<ILocalizationCultureManager>(new DefaultLocalizationCultureManager(provider));

        return builder;
    }

    // Internal classes to bridge the DI if needed, similar to Barbatos.i18n.DependencyInjection but for MAUI Host
    private sealed class DefaultLocalizationProviderResolver : ILocalizationProviderResolver
    {
        private readonly ILocalizationProvider _provider;
        public DefaultLocalizationProviderResolver(ILocalizationProvider provider) => _provider = provider;
        public ILocalizationProvider? GetProvider(string? key = null) => _provider; // Simplified for single provider
        public IEnumerable<ILocalizationProvider> GetAllProviders() => new[] { _provider };
    }

    private sealed class DefaultLocalizationCultureManager : ILocalizationCultureManager
    {
        private readonly ILocalizationProvider _provider;
        public DefaultLocalizationCultureManager(ILocalizationProvider provider) => _provider = provider;
        public void SetCulture(CultureInfo cultureInfo) => _provider.SetCulture(cultureInfo);
        public void SetCulture(string cultureName) => _provider.SetCulture(new CultureInfo(cultureName));
        public CultureInfo GetCulture() => _provider.GetCulture();
        public LocalizationOptions Options => new LocalizationOptions { SyncFormattingCulture = false };
    }
}
