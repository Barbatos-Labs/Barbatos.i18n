// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

using System.Globalization;

namespace Barbatos.i18n.DependencyInjection;

/// <summary>
/// Provides functionality to manage the current culture for localization using dependency injection.
/// </summary>
public class DependencyInjectionLocalizationCultureManager : ILocalizationCultureManager
{
    private readonly ILocalizationProviderResolver _resolver;
    /// <inheritdoc />
    public LocalizationOptions Options { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DependencyInjectionLocalizationCultureManager"/> class.
    /// </summary>
    /// <param name="resolver">The localization provider resolver.</param>
    /// <param name="options">Global localization options.</param>
    public DependencyInjectionLocalizationCultureManager(ILocalizationProviderResolver resolver, LocalizationOptions options)
    {
        _resolver = resolver;
        Options = options;
    }

    /// <inheritdoc />
    public void SetCulture(string cultureName) => SetCulture(new CultureInfo(cultureName));

    /// <inheritdoc />
    public void SetCulture(CultureInfo culture)
    {
        foreach (var provider in _resolver.GetAllProviders())
        {
            provider.SetCulture(culture);
        }

        CultureInfo.CurrentUICulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;

        if (Options.SyncFormattingCulture)
        {
            CultureInfo targetCulture = Options.CustomFormattingCultureBuilder?.Invoke(culture) ?? culture;
            CultureInfo.CurrentCulture = targetCulture;
            CultureInfo.DefaultThreadCurrentCulture = targetCulture;
        }
    }

    /// <inheritdoc />
    public CultureInfo GetCulture()
    {
        return _resolver.GetProvider()?.GetCulture() ?? CultureInfo.CurrentCulture;
    }
}
