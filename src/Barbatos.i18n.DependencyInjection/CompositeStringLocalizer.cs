// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

namespace Barbatos.i18n.DependencyInjection;

/// <summary>
/// Default implementation of <see cref="ICompositeStringLocalizer"/> that searches all registered
/// localization sets to find the requested key.
/// </summary>
/// <remarks>
/// <para>
/// When resolving a key, <see cref="CompositeStringLocalizer"/> first attempts to find the key
/// in the default (unnamed) localization set. If not found, it falls back to searching
/// all localization sets registered with the provider for the current culture.
/// </para>
/// </remarks>
public class CompositeStringLocalizer(
    ILocalizationProvider localizations,
    ILocalizationCultureManager cultureManager
) : ICompositeStringLocalizer
{
    /// <inheritdoc />
    public LocalizedString this[string name] => this[name, []];

    /// <inheritdoc />
    public LocalizedString this[string name, params object[] arguments] =>
        LocalizeString(name, arguments);

    /// <inheritdoc />
    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
    {
        CultureInfo culture = cultureManager.GetCulture();

        return localizations.GetLocalizationSets(culture)
            .SelectMany(s => s.Strings)
            .Select(x => new LocalizedString(x.Key, x.Value ?? x.Key));
    }

    /// <summary>
    /// Resolves the localized string for the given key by searching across all registered
    /// localization sets for the current culture.
    /// </summary>
    private LocalizedString LocalizeString(string name, object[] placeholders)
    {
        CultureInfo culture = cultureManager.GetCulture();

        // First, try the default (unnamed) localization set
        LocalizationSet? localizationSet = localizations.GetLocalizationSet(culture, default);

        string? value = TryResolveFromSet(localizationSet, name, placeholders);

        if (value is not null)
        {
            return new LocalizedString(name, value, false);
        }

        // Fallback: search all localization sets for the current culture
        foreach (var set in localizations.GetLocalizationSets(culture))
        {
            value = TryResolveFromSet(set, name, placeholders);
            if (value is not null)
            {
                return new LocalizedString(name, value, false);
            }
        }

        return new LocalizedString(name, name, true);
    }

    private static string? TryResolveFromSet(LocalizationSet? set, string name, object[] placeholders)
    {
        if (set is null) return null;

        return (placeholders is null || placeholders.Length == 0)
            ? set[name]
            : set[name, placeholders];
    }
}

/// <summary>
/// Default implementation of <see cref="ICompositeStringLocalizer{TResource}"/> that first searches the
/// localization set matching <typeparamref name="TResource"/>'s full name, then falls back
/// to all registered localization sets.
/// </summary>
/// <typeparam name="TResource">The resource type used to scope the localization lookup.</typeparam>
/// <remarks>
/// <para>
/// Resolution order:
/// <list type="number">
///   <item><description>Localization set matching <c>typeof(TResource).FullName</c> (case-insensitive)</description></item>
///   <item><description>Default (unnamed) localization set</description></item>
///   <item><description>All other localization sets for the current culture</description></item>
/// </list>
/// </para>
/// </remarks>
public class CompositeStringLocalizer<TResource>(
    ILocalizationProvider localizations,
    ILocalizationCultureManager cultureManager
) : ICompositeStringLocalizer<TResource>
{
    private static readonly string? ResourceName = typeof(TResource).FullName?.ToLower();

    /// <inheritdoc />
    public LocalizedString this[string name] => this[name, []];

    /// <inheritdoc />
    public LocalizedString this[string name, params object[] arguments] =>
        LocalizeString(name, arguments);

    /// <inheritdoc />
    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
    {
        CultureInfo culture = cultureManager.GetCulture();

        // Start with the resource-scoped set
        LocalizationSet? primarySet = localizations.GetLocalizationSet(culture, ResourceName);
        if (primarySet is not null)
        {
            return primarySet.Strings.Select(x => new LocalizedString(x.Key, x.Value ?? x.Key));
        }

        // Fallback: aggregate all sets for the culture
        return localizations.GetLocalizationSets(culture)
            .SelectMany(s => s.Strings)
            .Select(x => new LocalizedString(x.Key, x.Value ?? x.Key));
    }

    /// <summary>
    /// Resolves the localized string using the priority: TResource-scoped set → default set → all sets.
    /// </summary>
    private LocalizedString LocalizeString(string name, object[] placeholders)
    {
        CultureInfo culture = cultureManager.GetCulture();

        // 1. Try the resource-scoped localization set first
        LocalizationSet? resourceSet = localizations.GetLocalizationSet(culture, ResourceName);
        string? value = TryResolveFromSet(resourceSet, name, placeholders);

        if (value is not null)
        {
            return new LocalizedString(name, value, false);
        }

        // 2. Try the default (unnamed) localization set
        LocalizationSet? defaultSet = localizations.GetLocalizationSet(culture, default);
        value = TryResolveFromSet(defaultSet, name, placeholders);

        if (value is not null)
        {
            return new LocalizedString(name, value, false);
        }

        // 3. Fallback: search all localization sets for the current culture
        foreach (var set in localizations.GetLocalizationSets(culture))
        {
            value = TryResolveFromSet(set, name, placeholders);
            if (value is not null)
            {
                return new LocalizedString(name, value, false);
            }
        }

        return new LocalizedString(name, name, true);
    }

    private static string? TryResolveFromSet(LocalizationSet? set, string name, object[] placeholders)
    {
        if (set is null) return null;

        return (placeholders is null || placeholders.Length == 0)
            ? set[name]
            : set[name, placeholders];
    }
}
