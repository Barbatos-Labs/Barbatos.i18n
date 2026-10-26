// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

namespace Barbatos.i18n.DependencyInjection;

/// <summary>
/// The single-set lookup that every <see cref="IStringLocalizer"/> in this package performs.
/// </summary>
/// <remarks>
/// The localizers differ only in which set they ask for and in what order; reading one set and reporting the
/// result was copied into each of them.
/// </remarks>
internal static class LocalizationSetLookup
{
    /// <summary>
    /// Finds a key in a set, filling its placeholders when arguments were given.
    /// </summary>
    /// <param name="set">The set to search, or null.</param>
    /// <param name="name">The key.</param>
    /// <param name="arguments">The format arguments, possibly empty.</param>
    /// <returns>The translation, or null when there is no set or it does not carry the key.</returns>
    internal static string? Find(LocalizationSet? set, string name, object[] arguments)
    {
        if (set is null)
        {
            return null;
        }

        return (arguments is null || arguments.Length == 0)
            ? set[name]
            : set[name, arguments];
    }

    /// <summary>
    /// Finds a key in a set and reports the outcome as a <see cref="LocalizedString"/>.
    /// </summary>
    /// <param name="set">The set to search, or null.</param>
    /// <param name="name">The key.</param>
    /// <param name="arguments">The format arguments, possibly empty.</param>
    /// <returns>The translation, or the key itself flagged as not found.</returns>
    internal static LocalizedString Localize(LocalizationSet? set, string name, object[] arguments) =>
        ToLocalizedString(name, Find(set, name, arguments));

    /// <summary>
    /// Reports a lookup's outcome as a <see cref="LocalizedString"/>.
    /// </summary>
    /// <param name="name">The key that was looked up.</param>
    /// <param name="value">The translation, or null when none was found.</param>
    /// <returns>The translation, or the key itself flagged as not found.</returns>
    internal static LocalizedString ToLocalizedString(string name, string? value) =>
        // The third argument is what lets callers tell a real translation from a key echoed back, which is how
        // tooling audits for missing strings.
        new(name, value ?? name, value is null);

    /// <summary>
    /// Lists every string of one set.
    /// </summary>
    /// <param name="set">The set to list.</param>
    /// <returns>One entry per key, an untranslated key standing in for its own value.</returns>
    internal static IEnumerable<LocalizedString> AllStrings(LocalizationSet set) =>
        set.Strings.Select(x => new LocalizedString(x.Key, x.Value ?? x.Key));

    /// <summary>
    /// Lists every string of the set a lookup found, honouring <c>includeParentCultures</c>.
    /// </summary>
    /// <param name="set">The set the lookup found, or null.</param>
    /// <param name="culture">The culture the caller asked for.</param>
    /// <param name="includeParentCultures">Whether a set registered for a parent culture may answer.</param>
    /// <returns>The set's strings, or nothing.</returns>
    internal static IEnumerable<LocalizedString> AllStrings(LocalizationSet? set, CultureInfo culture, bool includeParentCultures)
    {
        // The lookup walks parent cultures on its own, so a result may come from a less specific culture than
        // the one asked for. Drop it when the caller said not to include parents.
        if (set is null || (!includeParentCultures && !set.Culture.Equals(culture)))
        {
            return [];
        }

        return AllStrings(set);
    }
}
