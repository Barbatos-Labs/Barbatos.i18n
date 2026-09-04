// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

namespace Barbatos.i18n;

/// <summary>
/// Provides functionality to retrieve localization sets for specific cultures.
/// </summary>
public class LocalizationProvider : ILocalizationProvider
{
    private readonly LocalizationSet[] _localizationSets;

    /// <summary>
    /// The sets grouped by culture name, so a lookup does not rescan every set for each culture it tries.
    /// </summary>
    /// <remarks>
    /// Lookups sit on the hot path: one culture change re-evaluates every live binding on screen, and each of
    /// those walks the culture fallback chain. Scanning the whole array per chain step made that cost grow with
    /// the number of registered files. Sets are copied and immutable, so the index can be built once.
    /// </remarks>
    private readonly Dictionary<string, LocalizationSet[]> _setsByCulture;

    private CultureInfo _currentCulture;

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalizationProvider"/> class.
    /// </summary>
    /// <param name="currentCulture">The culture the provider starts on.</param>
    /// <param name="localizationSets">The sets this provider serves.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="localizationSets"/> is null.</exception>
    /// <remarks>
    /// The sets, and the strings inside them, are copied up front. Holding the sequences themselves would re-run
    /// a deferred query on every lookup, and lookups sit on the hot path: each culture change re-evaluates every
    /// live binding on screen. A YAML file's strings arrived as a LINQ projection, so every key read re-ran it.
    /// </remarks>
    public LocalizationProvider(CultureInfo currentCulture, IEnumerable<LocalizationSet> localizationSets)
    {
        if (localizationSets is null)
        {
            throw new ArgumentNullException(nameof(localizationSets));
        }

        _currentCulture = currentCulture;
        _localizationSets = localizationSets.Select(Materialize).ToArray();

        // CultureInfo.Equals compares names ordinally, so grouping by name preserves the previous matching
        // exactly while making the per-culture lookup a hash probe. Registration order is kept within a group,
        // which is what decides precedence when a key lives in more than one set.
        _setsByCulture = _localizationSets
            .GroupBy(s => s.Culture.Name, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.ToArray(), StringComparer.Ordinal);
    }

    /// <summary>
    /// Copies a set's strings into a dictionary so that every key read is a hash probe.
    /// </summary>
    /// <param name="set">The set as it was registered.</param>
    /// <returns>The set, backed by a dictionary.</returns>
    /// <remarks>
    /// A set already backed by a dictionary is returned untouched. Otherwise the strings are copied, and the
    /// first entry for a key wins, matching the order the previous linear scan reported.
    /// </remarks>
    private static LocalizationSet Materialize(LocalizationSet set)
    {
        if (set.Strings is IReadOnlyDictionary<LocalizationKey, string?> or IDictionary<LocalizationKey, string?>)
        {
            return set;
        }

        Dictionary<LocalizationKey, string?> strings = new();

        foreach (KeyValuePair<LocalizationKey, string?> pair in set.Strings)
        {
            _ = strings.TryAdd(pair.Key, pair.Value);
        }

        return set with { Strings = strings };
    }

    /// <inheritdoc />
    public LocalizationSet? GetLocalizationSet(string cultureName) => GetLocalizationSet(new CultureInfo(cultureName), default);

    /// <inheritdoc />
    public LocalizationSet? GetLocalizationSet(string cultureName, string name) => GetLocalizationSet(new CultureInfo(cultureName), name);

    /// <inheritdoc />
    public LocalizationSet? GetLocalizationSet(CultureInfo culture, string? name)
    {
        // Specificity wins over naming: each culture in the chain is fully considered before falling back to a
        // less specific one, so an exact-culture match is never lost to a parent's set.
        foreach (CultureInfo candidate in CultureFallback.EnumerateChain(culture))
        {
            if (MatchSet(candidate, name) is { } match)
            {
                return match;
            }
        }

        return null;
    }

    /// <summary>
    /// Finds the set for one exact culture.
    /// </summary>
    /// <param name="culture">The culture to match exactly.</param>
    /// <param name="name">The set name, or null for the default set.</param>
    /// <returns>The matching set, or null.</returns>
    private LocalizationSet? MatchSet(CultureInfo culture, string? name)
    {
        LocalizationSet[] candidates = SetsFor(culture);

        if (name is null)
        {
            // A null name asks for the default set. Match the unnamed set first: without this, callers that
            // specifically want the default one - CompositeStringLocalizer's default-set step, say - received
            // whichever named set happened to be enumerated first, and reported keys that do live in the
            // default set as missing. The XAML extensions do not come through here; with no Namespace they
            // search every set for the key instead.
            foreach (LocalizationSet set in candidates)
            {
                if (set.Name is null)
                {
                    return set;
                }
            }

            return candidates.Length > 0 ? candidates[0] : null;
        }

        foreach (LocalizationSet set in candidates)
        {
            if (string.Equals(set.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                return set;
            }
        }

        return null;
    }

    /// <summary>
    /// Gets the sets registered for one exact culture.
    /// </summary>
    /// <param name="culture">The culture to look up.</param>
    /// <returns>The sets in registration order, or an empty array when the culture has none.</returns>
    private LocalizationSet[] SetsFor(CultureInfo culture) =>
        _setsByCulture.TryGetValue(culture.Name, out LocalizationSet[]? sets) ? sets : [];

    /// <inheritdoc />
    public CultureInfo GetCulture()
    {
        return _currentCulture;
    }

    /// <inheritdoc />
    public void SetCulture(CultureInfo cultureInfo)
    {
        _currentCulture = cultureInfo;
    }

    /// <inheritdoc />
    public IEnumerable<LocalizationSet> GetLocalizationSets() => _localizationSets;

    /// <inheritdoc />
    public IEnumerable<LocalizationSet> GetLocalizationSets(CultureInfo culture) => SetsFor(culture);
}
