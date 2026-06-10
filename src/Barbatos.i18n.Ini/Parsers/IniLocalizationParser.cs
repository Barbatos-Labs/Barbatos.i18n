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
                    int semicolonIndex = value.IndexOf(';');
                    int hashIndex = value.IndexOf('#');
                    
                    int commentIndex = -1;
                    if (semicolonIndex >= 0 && hashIndex >= 0) commentIndex = Math.Min(semicolonIndex, hashIndex);
                    else if (semicolonIndex >= 0) commentIndex = semicolonIndex;
                    else if (hashIndex >= 0) commentIndex = hashIndex;
                    
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
}
