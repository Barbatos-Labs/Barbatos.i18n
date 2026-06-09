// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

using System.Text;
using Barbatos.i18n;

namespace Barbatos.i18n.Csv.Parsers;

internal static class CsvLocalizationParser
{
    public static Dictionary<string, IEnumerable<KeyValuePair<LocalizationKey, string?>>> Parse(string contents)
    {
        Dictionary<string, List<KeyValuePair<LocalizationKey, string?>>> results = new();
        List<List<string>> rows = ParseCsv(contents);
        
        bool isFirstRow = true;
        List<string> cultures = new(); // Column index -> Culture string (0 is Key)
        
        foreach (var row in rows)
        {
            if (row.Count == 0 || (row.Count == 1 && string.IsNullOrWhiteSpace(row[0])))
            {
                continue;
            }

            if (isFirstRow)
            {
                isFirstRow = false;
                
                // Read header to determine if it's single culture or multi culture
                cultures.Add(""); // index 0 is Key
                for (int i = 1; i < row.Count; i++)
                {
                    string colName = row[i].Trim();
                    if (i == 1 && colName.Equals("Value", StringComparison.OrdinalIgnoreCase))
                    {
                        cultures.Add(""); // Single culture
                        results[""] = new();
                        break; // Only need one column
                    }
                    else
                    {
                        cultures.Add(colName);
                        results[colName] = new();
                    }
                }
                
                if (row[0].Trim().Equals("Key", StringComparison.OrdinalIgnoreCase))
                {
                    continue; // Skip header row
                }
            }

            string key = row[0].Trim();
            if (string.IsNullOrWhiteSpace(key))
            {
                continue;
            }

            LocalizationKey locKey = new(key);

            // Iterate over each column mapping to a culture
            for (int i = 1; i < row.Count && i < cultures.Count; i++)
            {
                string cultureName = cultures[i];
                string value = row[i];
                
                // Add to result if the dictionary has the list (it should)
                if (results.TryGetValue(cultureName, out var list))
                {
                    // Check for duplicate keys in the specific culture
                    if (list.Any(k => k.Key.Equals(locKey)))
                    {
                        throw new LocalizationBuilderException($"The contents of the CSV file contains duplicate \"{key}\" keys.");
                    }
                    list.Add(new KeyValuePair<LocalizationKey, string?>(locKey, value));
                }
            }
        }
        
        // Convert List to IEnumerable for the return type
        Dictionary<string, IEnumerable<KeyValuePair<LocalizationKey, string?>>> finalResults = new();
        foreach (var kvp in results)
        {
            finalResults[kvp.Key] = kvp.Value;
        }
        return finalResults;
    }

    private static List<List<string>> ParseCsv(string contents)
    {
        List<List<string>> rows = new();
        List<string> currentRow = new();
        StringBuilder currentCell = new();
        bool inQuotes = false;
        
        for (int i = 0; i < contents.Length; i++)
        {
            char c = contents[i];
            
            if (c == '"')
            {
                // Escaped quote ""
                if (inQuotes && i + 1 < contents.Length && contents[i + 1] == '"')
                {
                    currentCell.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                currentRow.Add(currentCell.ToString());
                currentCell.Clear();
            }
            else if ((c == '\r' || c == '\n') && !inQuotes)
            {
                // Handle CRLF
                if (c == '\r' && i + 1 < contents.Length && contents[i + 1] == '\n')
                {
                    i++;
                }
                
                currentRow.Add(currentCell.ToString());
                rows.Add(currentRow);
                currentRow = new();
                currentCell.Clear();
            }
            else
            {
                currentCell.Append(c);
            }
        }
        
        if (currentCell.Length > 0 || currentRow.Count > 0)
        {
            currentRow.Add(currentCell.ToString());
            rows.Add(currentRow);
        }
        
        return rows;
    }
}
