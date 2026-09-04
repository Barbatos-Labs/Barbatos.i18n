// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

namespace Barbatos.i18n.Wpf;

/// <summary>
/// Resolves the localization set that backs a markup extension or converter.
/// </summary>
/// <remarks>
/// This sits on the hot path: every live binding runs it again on each culture change, so each provider registry
/// is consulted exactly once per lookup rather than once per value read.
/// </remarks>
internal static class LocalizationLookup
{
    /// <summary>
    /// Gets the localization set for the given provider key and namespace, preferring the provider registered in
    /// the dependency injection container and falling back to the statically registered one.
    /// </summary>
    /// <param name="providerKey">The provider key.</param>
    /// <param name="normalizedNamespace">The lower-cased namespace, or null for the default set.</param>
    /// <returns>The matching set, or null when neither registry can supply one.</returns>
    internal static LocalizationSet? ResolveSet(string providerKey, string? normalizedNamespace)
    {
        ILocalizationProvider? provider = WpfLocalization.GetProvider(providerKey);
        ILocalizationProvider? fallback = LocalizationProviderFactory.GetInstance(providerKey);

        if (provider is null && fallback is null)
        {
            return null;
        }

        CultureInfo culture = provider?.GetCulture() ?? fallback?.GetCulture() ?? CultureInfo.CurrentUICulture;

        return provider?.GetLocalizationSet(culture, normalizedNamespace)
            ?? fallback?.GetLocalizationSet(culture, normalizedNamespace);
    }

    /// <summary>
    /// Lower-cases a namespace once, so the hot path does not allocate a new string on every value read.
    /// </summary>
    /// <param name="value">The namespace as written in XAML.</param>
    /// <returns>The normalized namespace, or null when none was given.</returns>
    internal static string? NormalizeNamespace(string? value) => value?.ToLowerInvariant();
}
