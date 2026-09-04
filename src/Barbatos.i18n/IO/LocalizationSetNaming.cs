// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

namespace Barbatos.i18n.IO;

/// <summary>
/// Derives the name a file-backed <see cref="LocalizationSet"/> is registered under.
/// </summary>
/// <remarks>
/// The Ini, Csv and Json packages all name a set after its file, and each carried its own copy of these rules.
/// </remarks>
public static class LocalizationSetNaming
{
    /// <summary>
    /// Derives the set name from a resource path.
    /// </summary>
    /// <param name="path">The dot-notated resource path, for example "Locales.Translations-en-US.ini".</param>
    /// <param name="culture">The culture the file is registered for.</param>
    /// <returns>The lower-cased set name, or null when no name can be derived.</returns>
    /// <remarks>
    /// The name is the file name with its extension, its folders and any "-{culture}" suffix removed, so
    /// "Locales.Translations-en-US.ini" becomes "translations". When nothing but the culture is left -
    /// "Locales.en-US.ini", a natural way to lay locales out - the containing folder is used instead, so
    /// "locales" names the set for every language. Taking the culture as the name put each language in a
    /// differently named set, which left Namespace='...' with no name that spans them and made a
    /// baseName-scoped IStringLocalizer resolve for one language only.
    /// </remarks>
    public static string? DeriveName(string path, CultureInfo culture)
    {
        if (path is null)
        {
            throw new ArgumentNullException(nameof(path));
        }

        if (culture is null)
        {
            throw new ArgumentNullException(nameof(culture));
        }

        string withoutExtension = path;

        int lastDotIndex = withoutExtension.LastIndexOf('.');
        if (lastDotIndex > 0)
        {
            // Remove the extension, e.g. "Locales.Translations-en-US.ini" -> "Locales.Translations-en-US".
            withoutExtension = withoutExtension.Substring(0, lastDotIndex);
        }

        // Split the folders from the file name, e.g. "Resources.v1.Translations-en-US" -> "v1" + "Translations-en-US".
        string fileName = withoutExtension;
        string? folder = null;

        int lastFolderSeparator = withoutExtension.LastIndexOf('.');
        if (lastFolderSeparator >= 0)
        {
            fileName = withoutExtension.Substring(lastFolderSeparator + 1);
            folder = withoutExtension.Substring(0, lastFolderSeparator);

            int previousSeparator = folder.LastIndexOf('.');
            if (previousSeparator >= 0)
            {
                folder = folder.Substring(previousSeparator + 1);
            }
        }

        // Remove the culture suffix, e.g. "Translations-en-US" -> "Translations". The invariant culture has an
        // empty name, and searching for a bare "-" would truncate a legitimate name such as "My-Errors".
        if (culture.Name.Length > 0)
        {
            int cultureIndex = fileName.IndexOf("-" + culture.Name, StringComparison.OrdinalIgnoreCase);
            if (cultureIndex > 0)
            {
                fileName = fileName.Substring(0, cultureIndex);
            }
        }

        if (!IsCultureName(fileName, culture))
        {
            return fileName.ToLowerInvariant();
        }

        // The file is named after nothing but its culture, so the folder is what identifies the set. Without a
        // folder there is nothing better to fall back on than the name as written.
        return folder is { Length: > 0 } && !IsCultureName(folder, culture)
            ? folder.ToLowerInvariant()
            : fileName.ToLowerInvariant();
    }

    /// <summary>
    /// Determines whether a derived name is nothing but the culture the file was registered for.
    /// </summary>
    /// <param name="name">The derived name.</param>
    /// <param name="culture">The culture the file is registered for.</param>
    /// <returns>True when the name carries no meaning of its own.</returns>
    private static bool IsCultureName(string name, CultureInfo culture)
    {
        if (name.Length == 0)
        {
            return true;
        }

        return string.Equals(name, culture.Name, StringComparison.OrdinalIgnoreCase)
            || string.Equals(name, culture.TwoLetterISOLanguageName, StringComparison.OrdinalIgnoreCase);
    }
}
