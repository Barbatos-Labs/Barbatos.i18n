// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

namespace Barbatos.i18n.DependencyInjection;

/// <summary>
/// Provides a localization set-based implementation of the <see cref="IStringLocalizer"/> interface.
/// </summary>
/// <remarks>
/// This class uses a <see cref="LocalizationSet"/> to retrieve localized strings.
/// </remarks>
public class LocalizationSetStringLocalizer(LocalizationSet _localizationSet) : IStringLocalizer
{
    /// <inheritdoc />
    public LocalizedString this[string name] => this[name, []];

    /// <inheritdoc />
    public LocalizedString this[string name, params object[] arguments] =>
        LocalizeString(name, arguments);

    /// <inheritdoc />
    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) =>
        LocalizationSetLookup.AllStrings(_localizationSet);

    /// <summary>
    /// Fills placeholders in a string with the provided values.
    /// </summary>
    /// <param name="name">The string with placeholders.</param>
    /// <param name="placeholders">The values to fill the placeholders with.</param>
    /// <returns>The string with filled placeholders.</returns>
    private LocalizedString LocalizeString(string name, object[] placeholders) =>
        LocalizationSetLookup.Localize(_localizationSet, name, placeholders);
}

/// <summary>
/// Provides a provider-based implementation of the <see cref="IStringLocalizer{TResource}"/> interface.
/// </summary>
/// <typeparam name="TResource">The type of the resource used for localization.</typeparam>
/// <remarks>
/// This class uses an <see cref="ILocalizationProvider"/> to retrieve localization sets,
/// and an <see cref="ILocalizationCultureManager"/> to manage the current culture.
/// </remarks>
public class ProviderBasedStringLocalizer<TResource>(
    ILocalizationProvider localizations,
    ILocalizationCultureManager cultureManager
) : IStringLocalizer<TResource>
{
    private static readonly string? ResourceName = typeof(TResource).FullName?.ToLowerInvariant();

    /// <inheritdoc />
    public LocalizedString this[string name] => this[name, []];

    /// <inheritdoc />
    public LocalizedString this[string name, params object[] arguments] =>
        LocalizeString(name, arguments);

    /// <inheritdoc />
    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
    {
        CultureInfo culture = cultureManager.GetCulture();

        return LocalizationSetLookup.AllStrings(
            localizations.GetLocalizationSet(culture, ResourceName),
            culture,
            includeParentCultures);
    }

    /// <summary>
    /// Fills placeholders in a string with the provided values.
    /// </summary>
    /// <param name="name">The string with placeholders.</param>
    /// <param name="placeholders">The values to fill the placeholders with.</param>
    /// <returns>The string with filled placeholders.</returns>
    private LocalizedString LocalizeString(string name, object[] placeholders) =>
        LocalizationSetLookup.Localize(
            localizations.GetLocalizationSet(cultureManager.GetCulture(), ResourceName),
            name,
            placeholders);
}
