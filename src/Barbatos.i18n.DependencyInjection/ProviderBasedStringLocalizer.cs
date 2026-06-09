// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

namespace Barbatos.i18n.DependencyInjection;

/// <summary>
/// Provides a provider-based implementation of the <see cref="IStringLocalizer"/> interface.
/// </summary>
/// <remarks>
/// This class uses an <see cref="ILocalizationProvider"/> to retrieve localization sets,
/// and an <see cref="ILocalizationCultureManager"/> to manage the current culture.
/// </remarks>
public class ProviderBasedStringLocalizer(
    ILocalizationProvider localizations,
    ILocalizationCultureManager cultureManager,
    string? baseName = default
) : IStringLocalizer
{
    /// <inheritdoc />
    public LocalizedString this[string name] => this[name, []];

    /// <inheritdoc />
    public LocalizedString this[string name, params object[] arguments] =>
        LocalizeString(name, arguments);

    /// <inheritdoc />
    public IEnumerable<LocalizedString> GetAllStrings(bool _)
    {
        LocalizationSet? localizationSet = localizations.GetLocalizationSet(cultureManager.GetCulture(), baseName)
            ?? localizations.GetLocalizationSet(cultureManager.GetCulture(), default);

        return localizationSet?.Strings.Select(x => new LocalizedString(x.Key, x.Value ?? x.Key)) ?? [];
    }

    /// <summary>
    /// Fills placeholders in a string with the provided values.
    /// </summary>
    /// <param name="name">The string with placeholders.</param>
    /// <param name="placeholders">The values to fill the placeholders with.</param>
    /// <returns>The string with filled placeholders.</returns>
    private LocalizedString LocalizeString(string name, object[] placeholders)
    {
        LocalizationSet? localizationSet = localizations.GetLocalizationSet(cultureManager.GetCulture(), baseName)
            ?? localizations.GetLocalizationSet(cultureManager.GetCulture(), default);

        if (localizationSet is null)
        {
            return new LocalizedString(name, name, true);
        }

        string? value = (placeholders is null || placeholders.Length == 0)
            ? localizationSet[name]
            : localizationSet[name, placeholders];

        return new LocalizedString(name, value ?? name, value is null);
    }
}
