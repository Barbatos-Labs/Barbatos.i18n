// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

namespace Barbatos.i18n.Wpf;

/// <summary>
/// Provides extension methods for the <see cref="Application"/> class.
/// </summary>
public static class ApplicationExtensions
{
    private static bool _isLanguageOverridden = false;

    /// <summary>
    /// Configures the application to use a string localizer.
    /// </summary>
    /// <param name="app">The application to configure.</param>
    /// <param name="configure">A delegate to configure the localization builder.</param>
    /// <returns>The configured application.</returns>
    public static Application UseStringLocalizer(
        this Application app,
        Action<LocalizationBuilder> configure
    )
    {
        LocalizationBuilder builder = new();

        configure(builder);

        LocalizationProviderFactory.SetInstance(builder.Build());

        return app;
    }

    /// <summary>
    /// Connects the WPF application to the provided Dependency Injection container.
    /// </summary>
    /// <param name="serviceProvider">The service provider from the DI container.</param>
    /// <returns>The service provider, for chaining.</returns>
    public static IServiceProvider UseWpfLocalization(this IServiceProvider serviceProvider)
    {
        WpfLocalization.Initialize(serviceProvider);
        return serviceProvider;
    }

    /// <summary>
    /// Sets the culture for localization in the application (Legacy).
    /// </summary>
    /// <param name="app">The application to set the culture for.</param>
    /// <param name="culture">The culture to set.</param>
    /// <returns>The application with the set culture.</returns>
    public static Application SetLocalizationCulture(this Application app, CultureInfo culture)
    {
        if (WpfLocalization.ServiceProvider?.GetService(typeof(ILocalizationCultureManager)) is ILocalizationCultureManager diManager)
        {
            diManager.SetCulture(culture);
        }
        else
        {
            // Fallback for non-DI applications
            var fallbackManager = new LocalizationCultureManager();
            fallbackManager.SetCulture(culture);
        }

        UpdateWpfLanguageProperty(CultureInfo.CurrentCulture);
        return app;
    }

    /// <summary>
    /// Sets the localization culture for all providers registered in the DI container.
    /// </summary>
    /// <param name="serviceProvider">The service provider from the DI container.</param>
    /// <param name="culture">The culture to set.</param>
    /// <returns>The service provider, for chaining.</returns>
    public static IServiceProvider SetLocalizationCulture(this IServiceProvider serviceProvider, CultureInfo culture)
    {
        var cultureManager = serviceProvider.GetService(typeof(ILocalizationCultureManager)) as ILocalizationCultureManager;
        cultureManager?.SetCulture(culture);

        UpdateWpfLanguageProperty(CultureInfo.CurrentCulture);
        return serviceProvider;
    }

    /// <summary>
    /// Overrides the WPF FrameworkElement.Language property to ensure standard WPF bindings (StringFormat) respect the culture.
    /// </summary>
    private static void UpdateWpfLanguageProperty(CultureInfo targetCulture)
    {
        if (!_isLanguageOverridden)
        {
            try
            {
                FrameworkElement.LanguageProperty.OverrideMetadata(
                    typeof(FrameworkElement),
                    new FrameworkPropertyMetadata(
                        XmlLanguage.GetLanguage(targetCulture.IetfLanguageTag)));
                _isLanguageOverridden = true;
            }
            catch (ArgumentException)
            {
                // Metadata already overridden by another library or previous call
                _isLanguageOverridden = true;
            }
        }

        // If the window is already active, we must update its Language property directly
        if (Application.Current?.MainWindow is not null)
        {
            Application.Current.MainWindow.Language = XmlLanguage.GetLanguage(targetCulture.IetfLanguageTag);
        }
    }
}
