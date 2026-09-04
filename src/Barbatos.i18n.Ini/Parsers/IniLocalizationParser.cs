// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

namespace Barbatos.i18n.Ini.Parsers;

internal static class IniLocalizationParser
{
    public static IEnumerable<KeyValuePair<LocalizationKey, string?>> Parse(string contents)
    {
        Dictionary<LocalizationKey, string?> localizations = new();
        using StringReader reader = new(contents);
        
        string currentSection = string.Empty;
        
        while (reader.ReadLine() is string line)
        {
            line = line.Trim();
            
            // Ignore blank lines and comments
            if (string.IsNullOrEmpty(line) || line.StartsWith(';') || line.StartsWith('#'))
            {
                continue;
            }
            
            // Check for section
            if (line.StartsWith('['))
            {
                int endBracket = line.IndexOf(']');
                if (endBracket > 0)
                {
                    currentSection = line.Substring(1, endBracket - 1).Trim();
                    continue;
                }
            }
            
            // Check for key-value pair
            int separatorIndex = line.IndexOf('=');
            if (separatorIndex > 0)
            {
                string key = line.Substring(0, separatorIndex).Trim();
                string value = line.Substring(separatorIndex + 1).Trim();
                
                bool isQuoted = false;
                if (value.StartsWith('"'))
                {
                    int endQuote = value.IndexOf('"', 1);
                    if (endQuote > 0)
                    {
                        isQuoted = true;
                        value = value.Substring(1, endQuote - 1);
                    }
                }
                
                if (!isQuoted)
                {
                    int commentIndex = FindInlineCommentIndex(value);

                    if (commentIndex >= 0)
                    {
                        value = value.Substring(0, commentIndex).Trim();
                    }
                }
                
                string fullKey = string.IsNullOrEmpty(currentSection) ? key : $"{currentSection}.{key}";
                
                LocalizationKey locKey = new(fullKey);
                if (localizations.ContainsKey(locKey))
                {
                    throw new LocalizationBuilderException($"The contents of the INI file contains duplicate \"{fullKey}\" keys.");
                }
                localizations.Add(locKey, value);
            }
        }

        return localizations;
    }

    /// <summary>
    /// Finds where an inline comment starts in an unquoted value.
    /// </summary>
    /// <param name="value">The trimmed value to scan.</param>
    /// <returns>The index the comment starts at, or -1 when the value carries none.</returns>
    /// <remarks>
    /// An inline comment must be preceded by whitespace. Without that rule every unquoted value containing a
    /// '#' or ';' was truncated at it - "#FF0000" collapsed to an empty string, and "Barbatos; all rights
    /// reserved" lost everything after the semicolon.
    /// </remarks>
    private static int FindInlineCommentIndex(string value)
    {
        for (int i = 1; i < value.Length; i++)
        {
            if ((value[i] == ';' || value[i] == '#') && char.IsWhiteSpace(value[i - 1]))
            {
                return i;
            }
        }

        return -1;
    }
}
