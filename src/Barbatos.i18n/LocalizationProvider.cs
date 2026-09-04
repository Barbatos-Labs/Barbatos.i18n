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

    private CultureInfo _currentCulture;

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalizationProvider"/> class.
    /// </summary>
    /// <param name="currentCulture">The culture the provider starts on.</param>
    /// <param name="localizationSets">The sets this provider serves.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="localizationSets"/> is null.</exception>
    /// <remarks>
    /// The sets are copied up front. Holding the sequence itself would re-run a deferred query on every lookup,
    /// and lookups sit on the hot path: each culture change re-evaluates every live binding on screen.
    /// </remarks>
    public LocalizationProvider(CultureInfo currentCulture, IEnumerable<LocalizationSet> localizationSets)
    {
        if (localizationSets is null)
        {
            throw new ArgumentNullException(nameof(localizationSets));
        }

        _currentCulture = currentCulture;
        _localizationSets = localizationSets.ToArray();
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
        if (name is null)
        {
            // A null name asks for the default set. Match the unnamed set first: without this, callers that
            // specifically want the default one - the XAML extensions with no Namespace argument, or
            // CompositeStringLocalizer's default-set step - received whichever named set happened to be
            // enumerated first, and reported keys that do live in the default set as missing.
            return _localizationSets.FirstOrDefault(s => s.Culture.Equals(culture) && s.Name is null)
                ?? _localizationSets.FirstOrDefault(s => s.Culture.Equals(culture));
        }

        return _localizationSets.FirstOrDefault(s => s.Culture.Equals(culture) && string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase));
    }

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
    public IEnumerable<LocalizationSet> GetLocalizationSets(CultureInfo culture) =>
        _localizationSets.Where(s => s.Culture.Equals(culture));
}
