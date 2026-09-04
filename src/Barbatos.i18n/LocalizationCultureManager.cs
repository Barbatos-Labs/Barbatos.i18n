// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

namespace Barbatos.i18n;

/// <summary>
/// Provides functionality to manage the current culture for localization.
/// </summary>
public class LocalizationCultureManager : ILocalizationCultureManager
{
    /// <inheritdoc />
    public LocalizationOptions Options { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalizationCultureManager"/> class.
    /// </summary>
    /// <param name="options">Optional localization options. If null, default options are used.</param>
    public LocalizationCultureManager(LocalizationOptions? options = null)
    {
        Options = options ?? new LocalizationOptions();
    }

    /// <inheritdoc />
    public void SetCulture(string cultureName) => SetCulture(new CultureInfo(cultureName));

    /// <inheritdoc />
    public void SetCulture(CultureInfo culture)
    {
        if (culture == null)
            throw new ArgumentNullException(nameof(culture));

        CultureInfo targetCulture = culture;
        if (Options.FormatCultureBuilder is not null)
        {
            targetCulture = Options.FormatCultureBuilder.Invoke((CultureInfo)culture.Clone()) ?? culture;
        }

        CultureInfo.CurrentUICulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        CultureInfo.CurrentCulture = targetCulture;
        CultureInfo.DefaultThreadCurrentCulture = targetCulture;

        // Every provider, not just the default one: a provider registered under a ProviderKey would otherwise
        // keep serving the culture it was built with while the live bindings around it switch language.
        foreach (ILocalizationProvider provider in LocalizationProviderFactory.GetAllInstances())
        {
            provider.SetCulture(culture);
        }

        LocalizationNotifier.NotifyCultureChanged(culture, targetCulture);
    }

    /// <inheritdoc />
    public CultureInfo GetCulture()
    {
        if (LocalizationProviderFactory.GetInstance()?.GetCulture() is { } culture)
        {
            return culture;
        }

        return Options.FormatCultureBuilder?.Invoke((CultureInfo)CultureInfo.CurrentCulture.Clone())
               ?? CultureInfo.CurrentCulture;
    }

    /// <inheritdoc />
    public IReadOnlyCollection<CultureInfo> GetSupportedCultures()
    {
        CultureInfo[] cultures = LocalizationProviderFactory.GetAllInstances()
            .SelectMany(p => p.GetLocalizationSets())
            .Select(s => s.Culture)
            .Distinct()
            .ToArray();

        return cultures.Length > 0 ? cultures : ((ILocalizationCultureManager)this).GetOperatingSystemCultures();
    }
}