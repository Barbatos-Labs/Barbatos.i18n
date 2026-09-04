// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

using System.Runtime.CompilerServices;

namespace Barbatos.i18n.Wpf;

/// <summary>
/// Provides extension methods for the <see cref="Application"/> class.
/// </summary>
public static class ApplicationExtensions
{
    private static bool _isLanguageOverridden = false;

    /// <summary>
    /// The language of the most recently applied culture, used for windows opened after that change.
    /// </summary>
    private static XmlLanguage? _currentLanguage;

    /// <summary>
    /// The windows whose language this library owns, so an application-set language is never overwritten.
    /// </summary>
    /// <remarks>
    /// A <see cref="ConditionalWeakTable{TKey, TValue}"/> keeps no strong reference, so a closed window is
    /// collected as usual.
    /// </remarks>
    private static readonly ConditionalWeakTable<Window, object> _managedWindows = new();

    /// <summary>
    /// The value stored for every tracked window; only the presence of the entry carries meaning.
    /// </summary>
    private static readonly object ManagedWindow = new();

    /// <summary>
    /// Configures the application to use a string localizer.
    /// </summary>
    /// <param name="app">The application to configure.</param>
    /// <param name="configure">A delegate to configure the localization builder.</param>
    /// <returns>The configured application.</returns>
    public static Application UseStringLocalizer(this Application app, Action<LocalizationBuilder> configure)
    {
        return app.UseStringLocalizer(null, configure);
    }

    /// <summary>
    /// Configures the application to use a string localizer with a specific provider key.
    /// </summary>
    /// <param name="app">The application to configure.</param>
    /// <param name="providerKey">The key to associate with the provider. If null, it becomes the default provider.</param>
    /// <param name="configure">A delegate to configure the localization builder.</param>
    /// <returns>The configured application.</returns>
    public static Application UseStringLocalizer(this Application app, string? providerKey, Action<LocalizationBuilder> configure)
    {
        var locBuilder = new LocalizationBuilder();
        configure(locBuilder);
        var provider = locBuilder.Build();

        LocalizationProviderFactory.SetInstance(provider, providerKey ?? string.Empty);

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
    /// Points every window's <see cref="FrameworkElement.Language"/> at the given culture, so that standard WPF
    /// bindings and <c>StringFormat</c> respect it.
    /// </summary>
    /// <param name="targetCulture">The culture to apply.</param>
    /// <remarks>
    /// WPF allows <see cref="System.Windows.DependencyProperty.OverrideMetadata(Type, PropertyMetadata)"/> only
    /// once per property per type, so the metadata default is frozen at whatever culture was current the first
    /// time this ran. Relying on it alone left every visual root other than the main window - a second window, a
    /// popup, a context menu, a tooltip - on that first culture forever. Each open window is therefore assigned
    /// explicitly, and a class handler does the same for windows opened later.
    /// </remarks>
    private static void UpdateWpfLanguageProperty(CultureInfo targetCulture)
    {
        XmlLanguage language = XmlLanguage.GetLanguage(targetCulture.IetfLanguageTag);

        if (!_isLanguageOverridden)
        {
            try
            {
                FrameworkElement.LanguageProperty.OverrideMetadata(
                    typeof(FrameworkElement),
                    new FrameworkPropertyMetadata(language));
            }
            catch (ArgumentException)
            {
                // Metadata already overridden by another library or previous call.
            }

            // A window can be created after the last culture change, in which case it starts from the frozen
            // metadata default. Registering once here keeps those windows in step too.
            EventManager.RegisterClassHandler(
                typeof(Window),
                FrameworkElement.LoadedEvent,
                new RoutedEventHandler(OnWindowLoaded));

            _isLanguageOverridden = true;
        }

        _currentLanguage = language;

        Application? application = Application.Current;

        if (application is null)
        {
            return;
        }

        // Windows has thread affinity; touching it from a background thread would throw.
        if (!application.Dispatcher.CheckAccess())
        {
            _ = application.Dispatcher.BeginInvoke(new Action(ApplyLanguageToOpenWindows));
            return;
        }

        ApplyLanguageToOpenWindows();
    }

    /// <summary>
    /// Assigns the current language to every open window.
    /// </summary>
    private static void ApplyLanguageToOpenWindows()
    {
        if (_currentLanguage is null || Application.Current is not { } application)
        {
            return;
        }

        foreach (Window window in application.Windows)
        {
            ApplyLanguage(window);
        }
    }

    /// <summary>
    /// Applies the current language to a window that was opened after the last culture change.
    /// </summary>
    /// <param name="sender">The window being loaded.</param>
    /// <param name="e">The event data.</param>
    private static void OnWindowLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is Window window)
        {
            ApplyLanguage(window);
        }
    }

    /// <summary>
    /// Points one window's <see cref="FrameworkElement.Language"/> at the current culture, unless the
    /// application set that window's language itself.
    /// </summary>
    /// <param name="window">The window to update.</param>
    /// <remarks>
    /// A window whose XAML says <c>Language="de-DE"</c> is asking for formatting that does not follow the UI
    /// language - a report preview, say - so overwriting it would silently break that window. A local value is
    /// therefore adopted only the first time it is seen, and only when the application left it unset; from then
    /// on the window is tracked and keeps following the culture. The table holds weak references, so a closed
    /// window is still collected.
    /// </remarks>
    private static void ApplyLanguage(Window window)
    {
        if (_currentLanguage is null)
        {
            return;
        }

        if (!_managedWindows.TryGetValue(window, out _))
        {
            if (window.ReadLocalValue(FrameworkElement.LanguageProperty) != DependencyProperty.UnsetValue)
            {
                return;
            }

            _managedWindows.Add(window, ManagedWindow);
        }

        window.Language = _currentLanguage;
    }
}
