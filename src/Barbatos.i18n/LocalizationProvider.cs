// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

namespace Barbatos.i18n;

/// <summary>
/// Provides functionality to retrieve localization sets for specific cultures.
/// </summary>
public class LocalizationProvider(
    CultureInfo _currentCulture,
    IEnumerable<LocalizationSet> _localizationSets
) : ILocalizationProvider
{
    /// <inheritdoc />
    public LocalizationSet? GetLocalizationSet(string cultureName) => GetLocalizationSet(new CultureInfo(cultureName), default);

    /// <inheritdoc />
    public LocalizationSet? GetLocalizationSet(string cultureName, string name) => GetLocalizationSet(new CultureInfo(cultureName), name);

    /// <inheritdoc />
    public LocalizationSet? GetLocalizationSet(CultureInfo culture, string? name)
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
