// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

using System.Globalization;
using System.Reflection;
using Barbatos.i18n.Ini.Parsers;
using Barbatos.i18n.IO;

namespace Barbatos.i18n.Ini;

/// <summary>
/// Provides extension methods for the <see cref="LocalizationBuilder"/> class to support INI configuration files.
/// </summary>
public static class LocalizationBuilderExtensions
{
    /// <summary>
    /// Adds localized strings from an INI file in the calling assembly to the <see cref="LocalizationBuilder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="LocalizationBuilder"/> to add the localized strings to.</param>
    /// <param name="path">The path to the INI file in the assembly.</param>
    /// <param name="culture">The culture for which the localized strings are provided.</param>
    /// <returns>The <see cref="LocalizationBuilder"/> with the added localized strings.</returns>
    public static LocalizationBuilder FromIni(
        this LocalizationBuilder builder,
        string path,
        CultureInfo culture
    )
    {
        return builder.FromIni(Assembly.GetCallingAssembly(), path, culture);
    }

    /// <summary>
    /// Adds localized strings from an INI file in the specified assembly to the <see cref="LocalizationBuilder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="LocalizationBuilder"/> to add the localized strings to.</param>
    /// <param name="assembly">The assembly that contains the INI file.</param>
    /// <param name="path">The path to the INI file in the assembly.</param>
    /// <param name="culture">The culture for which the localized strings are provided.</param>
    /// <returns>The <see cref="LocalizationBuilder"/> with the added localized strings.</returns>
    /// <exception cref="ArgumentException">Thrown when the path does not point to an INI file.</exception>
    /// <exception cref="LocalizationBuilderException">Thrown when the INI file cannot be found in the assembly.</exception>
    public static LocalizationBuilder FromIni(
        this LocalizationBuilder builder,
        Assembly assembly,
        string path,
        CultureInfo culture
    )
    {
        if (!path.EndsWith(".ini", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                $"Parameter {nameof(path)} in {nameof(FromIni)} must be path to the INI file."
            );
        }

        string? contents = EmbeddedResourceReader.ReadToEnd(path, assembly);

        if (contents is null)
        {
            throw new LocalizationBuilderException(
                $"Resource {path} not found in assembly {assembly.FullName}."
            );
        }

        IEnumerable<KeyValuePair<LocalizationKey, string?>> localizations = IniLocalizationParser.Parse(contents);

        string fileName = path;
        int lastDotIndex = fileName.LastIndexOf('.');
        if (lastDotIndex > 0)
        {
            // Remove extension, e.g. "Translations-en-US.ini" -> "Translations-en-US"
            fileName = fileName.Substring(0, lastDotIndex);
            
            // Extract base name, e.g. "Resources.v1.Translations-en-US" -> "Translations-en-US"
            int lastDotBeforeExtension = fileName.LastIndexOf('.');
            if (lastDotBeforeExtension >= 0)
            {
                fileName = fileName.Substring(lastDotBeforeExtension + 1);
            }
        }
        
        string name = fileName;
        // Typically remove culture code from name, like "Translations-en-US" -> "Translations"
        int cultureIndex = name.IndexOf("-" + culture.Name, StringComparison.OrdinalIgnoreCase);
        if (cultureIndex > 0)
        {
            name = name.Substring(0, cultureIndex);
        }

        builder.AddLocalization(name.ToLowerInvariant(), culture, localizations);

        return builder;
    }

    /// <summary>
    /// Adds localized strings from an INI string to the <see cref="LocalizationBuilder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="LocalizationBuilder"/> to add the localized strings to.</param>
    /// <param name="name">The base name of the resource.</param>
    /// <param name="culture">The culture for which the localized strings are provided.</param>
    /// <param name="contents">The contents of the INI configuration.</param>
    /// <returns>The <see cref="LocalizationBuilder"/> with the added localized strings.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the contents is null.</exception>
    public static LocalizationBuilder FromIniString(
        this LocalizationBuilder builder,
        string name,
        CultureInfo culture,
        string? contents
    )
    {
        if (contents is null)
        {
            throw new ArgumentNullException(nameof(contents));
        }

        IEnumerable<KeyValuePair<LocalizationKey, string?>> localizations = IniLocalizationParser.Parse(contents);

        builder.AddLocalization(name.ToLowerInvariant(), culture, localizations);

        return builder;
    }
}
