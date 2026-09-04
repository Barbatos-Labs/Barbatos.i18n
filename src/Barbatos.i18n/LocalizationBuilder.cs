// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

using Barbatos.i18n.IO;

namespace Barbatos.i18n;

/// <summary>
/// Provides functionality to build a collection of localized strings for different cultures.
/// </summary>
public class LocalizationBuilder
{
    // A list, not a set: duplicates are already rejected by AddLocalization, and lookups resolve a key by
    // searching sets in registration order, so that order has to be the one the caller wrote.
    private readonly List<LocalizationSet> _localizations = [];

    private CultureInfo? _selectedCulture;

    /// <summary>
    /// Builds an <see cref="ILocalizationProvider"/> using the current culture and localizations.
    /// </summary>
    /// <returns>An <see cref="ILocalizationProvider"/> with the current culture and localizations.</returns>
    public virtual ILocalizationProvider Build()
    {
        return new LocalizationProvider(
            _selectedCulture ?? CultureInfo.CurrentCulture,
            _localizations
        );
    }

    /// <summary>
    /// Sets the culture for the <see cref="LocalizationBuilder"/>.
    /// </summary>
    /// <param name="culture">The culture to set.</param>
    public virtual void SetCulture(CultureInfo culture)
    {
        _selectedCulture = culture;
    }

    /// <summary>
    /// Adds a localization set to the collection.
    /// </summary>
    /// <param name="localization">The localization set to add.</param>
    /// <exception cref="InvalidOperationException">Thrown when a localization set for the same culture already exists in the collection.</exception>
    public virtual void AddLocalization(LocalizationSet localization)
    {
        if (
            _localizations.Any(x =>
                x.Name == localization.Name && x.Culture.Equals(localization.Culture)
            )
        )
        {
            // NOTE: Consider adding merging of multiple collections for one culture
            throw new InvalidOperationException(
                $"Localization \"{localization.Name}\" for culture {localization.Culture} already exists."
            );
        }

        _localizations.Add(localization);
    }

    /// <summary>
    /// Adds localized strings from a resource in the calling assembly to the <see cref="LocalizationBuilder"/>.
    /// </summary>
    /// <typeparam name="TResource">The type of the resource.</typeparam>
    /// <param name="culture">The culture for which the localized strings are provided.</param>
    public virtual void FromResource<TResource>(CultureInfo culture)
    {
        Type resourceType = typeof(TResource);
        string? resourceName = resourceType.FullName;

        if (resourceName is null)
        {
            return;
        }

        FromResource(resourceType.Assembly, resourceName, culture);
    }

    /// <summary>
    /// Adds localized strings from a resource in the calling assembly to the <see cref="LocalizationBuilder"/>
    /// and registers them under a custom <paramref name="name"/> instead of the default fully-qualified type name.
    /// </summary>
    /// <typeparam name="TResource">The type of the resource.</typeparam>
    /// <param name="culture">The culture for which the localized strings are provided.</param>
    /// <param name="name">
    /// The namespace name to register this localization set under (e.g. <c>nameof(Strings)</c> → <c>"Strings"</c>).
    /// Use this to avoid hardcoding the full type name in XAML <c>Namespace</c> arguments.
    /// </param>
    public virtual void FromResource<TResource>(CultureInfo culture, string name)
    {
        Type resourceType = typeof(TResource);
        string? resourceName = resourceType.FullName;

        if (resourceName is null)
        {
            return;
        }

        LocalizationSet? localizationSet = IO.LocalizationSetResourceParser.Parse(
            resourceType.Assembly,
            resourceName,
            culture
        );

        if (localizationSet is not null)
        {
            AddLocalization(localizationSet with { Name = name.ToLowerInvariant() });
        }
    }

    /// <summary>
    /// Adds localized strings from a resource with the specified base name in the specified assembly to the <see cref="LocalizationBuilder"/>.
    /// </summary>
    /// <param name="assembly">The assembly that contains the resource.</param>
    /// <param name="baseName">The base name of the resource.</param>
    /// <param name="culture">The culture for which the localized strings are provided.</param>
    /// <exception cref="LocalizationBuilderException">Thrown when the resource cannot be found.</exception>
    public virtual void FromResource(Assembly assembly, string baseName, CultureInfo culture)
    {
        LocalizationSet? localizationSet = LocalizationSetResourceParser.Parse(
            assembly,
            baseName,
            culture
        );

        if (localizationSet is not null)
        {
            AddLocalization(localizationSet);
        }
    }
}
