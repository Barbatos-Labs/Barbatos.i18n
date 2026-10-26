// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

namespace Barbatos.i18n.Yaml;

/// <summary>
/// Deserializes the small subset of YAML that localization files use into namespaced dictionaries.
/// </summary>
public static class YamlDictionariesDeserializer
{
    private const string DefaultNamespace = "default";

    /// <summary>
    /// Reads YAML contents into one dictionary per namespace.
    /// </summary>
    /// <param name="input">The YAML contents.</param>
    /// <returns>
    /// The namespaces found in the file, keyed by name. Root-level keys are collected under "default"; a key under a
    /// header belongs to that header's namespace only.
    /// </returns>
    public static IDictionary<string, IDictionary<string, string>> FromString(string input)
    {
        Dictionary<string, IDictionary<string, string>> result = new();
        string[] lines = input.Split(
            new Char[] { '\n', '\r' },
            StringSplitOptions.RemoveEmptyEntries
        );
        string currentNamespace = DefaultNamespace;
        int currentIndentation = 0;

        foreach (string line in lines)
        {
            string trimmedLine = line.Trim();

            // Ignore comments
            if (trimmedLine.StartsWith("#"))
            {
                continue;
            }

            // Check for namespace. A namespace declaration carries nothing after its colon, so the test has to
            // be "nothing follows the FIRST colon" rather than "the line ends with a colon" - otherwise a
            // key whose value itself ends in a colon ("EnterName: Enter Name:") is swallowed as a namespace.
            int firstColon = trimmedLine.IndexOf(':');

            if (firstColon >= 0 && trimmedLine.AsSpan(firstColon + 1).Trim().IsEmpty)
            {
                currentNamespace = trimmedLine.Substring(0, firstColon).Trim();
                currentIndentation = line.IndexOf(trimmedLine, StringComparison.Ordinal);
                continue;
            }

            // Check for nested namespace
            if (trimmedLine.StartsWith("- "))
            {
                currentNamespace += "." + trimmedLine.TrimStart('-').Trim();
                continue;
            }

            // Check for end of namespace. A key in column 0 is a root-level key, and so is one indented less than the
            // header it follows. Testing only the second meant a header in column 0 never ended, and a root key
            // written after its block was filed under that header.
            int indentation = line.IndexOf(trimmedLine, StringComparison.Ordinal);

            if (indentation == 0 || indentation < currentIndentation)
            {
                currentNamespace = DefaultNamespace;
                currentIndentation = 0;
            }

            // Split key and value
            string[] parts = trimmedLine.Split(new[] { ':' }, 2).Select(p => p.Trim()).ToArray();

            if (parts.Length != 2)
            {
                continue;
            }

            // Remove start and end quotes from value, but keep quotes within the string
            string value = RemoveStartEndQuotes(parts[1]);

            // Add to dictionary
            AddToDictionary(result, currentNamespace, parts[0], value);
        }

        return result;
    }

    private static string RemoveStartEndQuotes(string value)
    {
        // A single quote character satisfies both StartsWith and EndsWith, so the length guard is what keeps
        // Substring from being handed a negative length.
        if (value.Length < 2)
        {
            return value;
        }

        if (
            (value.StartsWith("'") && value.EndsWith("'"))
            || (value.StartsWith("\"") && value.EndsWith("\""))
        )
        {
            value = value.Substring(1, value.Length - 2);
        }

        return value;
    }

    /// <summary>
    /// Adds a key to the dictionary of its namespace, creating that dictionary on first use.
    /// </summary>
    /// <param name="dict">The namespaces read so far.</param>
    /// <param name="namespaceKey">The namespace the key belongs to.</param>
    /// <param name="key">The key.</param>
    /// <param name="value">The translation.</param>
    /// <remarks>
    /// A key goes to its own namespace only. Keys under a header used to be copied into the default namespace as
    /// well, so a lookup that reads the default set first - the C# localizers do - answered from a YAML header
    /// where the XAML lookup, which follows registration order, answered from another file; and two headers
    /// sharing a key left the default namespace holding whichever came last.
    /// </remarks>
    private static void AddToDictionary(
        IDictionary<string, IDictionary<string, string>> dict,
        string namespaceKey,
        string key,
        string value
    )
    {
        if (!dict.TryGetValue(namespaceKey, out IDictionary<string, string>? strings))
        {
            strings = new Dictionary<string, string>();
            dict[namespaceKey] = strings;
        }

        strings[key] = value;
    }
}
