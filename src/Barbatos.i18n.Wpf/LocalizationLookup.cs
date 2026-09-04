// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

namespace Barbatos.i18n.Wpf;

/// <summary>
/// Resolves translations for the markup extensions and converters.
/// </summary>
/// <remarks>
/// This sits on the hot path: every live binding runs it again on each culture change, so each provider registry
/// is consulted exactly once per lookup rather than once per value read.
/// </remarks>
internal static class LocalizationLookup
{
    /// <summary>
    /// Resolves a key and applies its format arguments.
    /// </summary>
    /// <param name="providerKey">The provider key.</param>
    /// <param name="normalizedNamespace">The lower-cased namespace, or null to search every set.</param>
    /// <param name="key">The localization key.</param>
    /// <param name="formatProvider">The culture used to format the arguments.</param>
    /// <param name="arguments">The format arguments, or null when the key takes none.</param>
    /// <returns>The formatted translation, or null when no set carries the key.</returns>
    internal static string? ResolveFormatted(
        string providerKey,
        string? normalizedNamespace,
        LocalizationKey key,
        IFormatProvider formatProvider,
        object?[]? arguments
    )
    {
        string? value = ResolveValue(providerKey, normalizedNamespace, key);

        if (value is null)
        {
            return null;
        }

        return arguments is null || arguments.Length == 0
            ? value
            : string.Format(formatProvider, value, arguments);
    }

    /// <summary>
    /// Resolves a key to its raw translation.
    /// </summary>
    /// <param name="providerKey">The provider key.</param>
    /// <param name="normalizedNamespace">The lower-cased namespace, or null to search every set.</param>
    /// <param name="key">The localization key.</param>
    /// <returns>The translation, or null when no set carries the key.</returns>
    /// <remarks>
    /// Omitting the namespace means "find this key wherever it lives", which is what an extension written without
    /// a Namespace argument expects and what ICompositeStringLocalizer already does. Resolving to a single set
    /// instead would tie the result to one file, so a key living in any other registered file would render as its
    /// raw name. Sets are searched in registration order, so a key defined in more than one of them resolves to
    /// whichever file was registered first.
    /// </remarks>
    internal static string? ResolveValue(string providerKey, string? normalizedNamespace, LocalizationKey key)
    {
        ILocalizationProvider? provider = WpfLocalization.GetProvider(providerKey);
        ILocalizationProvider? fallback = LocalizationProviderFactory.GetInstance(providerKey);

        return Search(provider, normalizedNamespace, key) ?? Search(fallback, normalizedNamespace, key);
    }

    /// <summary>
    /// Lower-cases a namespace once, so the hot path does not allocate a new string on every value read.
    /// </summary>
    /// <param name="value">The namespace as written in XAML.</param>
    /// <returns>The normalized namespace, or null when none was given.</returns>
    internal static string? NormalizeNamespace(string? value) => value?.ToLowerInvariant();

    /// <summary>
    /// Searches one provider, walking the culture fallback chain.
    /// </summary>
    /// <param name="provider">The provider to search, or null.</param>
    /// <param name="normalizedNamespace">The lower-cased namespace, or null to search every set.</param>
    /// <param name="key">The localization key.</param>
    /// <returns>The translation, or null.</returns>
    private static string? Search(ILocalizationProvider? provider, string? normalizedNamespace, LocalizationKey key)
    {
        if (provider is null)
        {
            return null;
        }

        foreach (CultureInfo candidate in CultureFallback.EnumerateChain(provider.GetCulture()))
        {
            if (normalizedNamespace is not null)
            {
                // GetLocalizationSet walks the chain itself, so the sets are filtered here instead to keep this
                // loop the single place the chain is walked.
                foreach (LocalizationSet set in provider.GetLocalizationSets(candidate))
                {
                    if (string.Equals(set.Name, normalizedNamespace, StringComparison.OrdinalIgnoreCase)
                        && set[key] is { } scoped)
                    {
                        return scoped;
                    }
                }

                continue;
            }

            // Registration order decides: the first set that carries the key wins. Preferring the unnamed set
            // would let an incidental one - a YAML file's implicit default namespace, say - outrank the file the
            // application registered first.
            foreach (LocalizationSet set in provider.GetLocalizationSets(candidate))
            {
                if (set[key] is { } match)
                {
                    return match;
                }
            }
        }

        return null;
    }
}
