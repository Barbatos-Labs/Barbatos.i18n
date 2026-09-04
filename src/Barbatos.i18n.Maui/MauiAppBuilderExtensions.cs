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

            // Register culture manager over the resolver so that every keyed provider follows the culture
            builder.Services.AddSingleton<ILocalizationCultureManager>(sp =>
                new DefaultLocalizationCultureManager(sp.GetRequiredService<ILocalizationProviderResolver>()));
        }
        else
        {
            resolver = (LocalizationProviderResolver)(resolverDescriptor.ImplementationInstance
                ?? throw new InvalidOperationException("ILocalizationProviderResolver registered is not an instance of LocalizationProviderResolver."));
        }

        resolver.AddProvider(providerKey, provider);

        return builder;
    }

    /// <summary>
    /// The culture manager used when the application does not reference <c>Barbatos.i18n.DependencyInjection</c>.
    /// Mirrors the behaviour of <c>DependencyInjectionLocalizationCultureManager</c>: it applies the culture to the
    /// ambient <see cref="CultureInfo"/> properties, forwards it to every registered provider, and announces the
    /// change through <see cref="LocalizationNotifier"/> so live XAML bindings refresh.
    /// </summary>
    private sealed class DefaultLocalizationCultureManager(ILocalizationProviderResolver resolver) : ILocalizationCultureManager
    {
        public LocalizationOptions Options { get; } = new();

        public void SetCulture(string cultureName) => SetCulture(new CultureInfo(cultureName));

        public void SetCulture(CultureInfo cultureInfo)
        {
            if (cultureInfo is null)
            {
                throw new ArgumentNullException(nameof(cultureInfo));
            }

            CultureInfo targetCulture = cultureInfo;
            if (Options.FormatCultureBuilder is not null)
            {
                targetCulture = Options.FormatCultureBuilder.Invoke((CultureInfo)cultureInfo.Clone()) ?? cultureInfo;
            }

            CultureInfo.CurrentUICulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
            CultureInfo.CurrentCulture = targetCulture;
            CultureInfo.DefaultThreadCurrentCulture = targetCulture;

            foreach (ILocalizationProvider provider in resolver.GetAllProviders())
            {
                provider.SetCulture(cultureInfo);
            }

            LocalizationNotifier.NotifyCultureChanged(cultureInfo);
        }

        public CultureInfo GetCulture() => resolver.GetProvider()?.GetCulture() ?? CultureInfo.CurrentCulture;

        public IReadOnlyCollection<CultureInfo> GetSupportedCultures()
        {
            CultureInfo[] cultures = resolver.GetAllProviders()
                .SelectMany(p => p.GetLocalizationSets())
                .Select(s => s.Culture)
                .Distinct()
                .ToArray();

            return cultures.Length > 0 ? cultures : ((ILocalizationCultureManager)this).GetOperatingSystemCultures();
        }
    }
}
