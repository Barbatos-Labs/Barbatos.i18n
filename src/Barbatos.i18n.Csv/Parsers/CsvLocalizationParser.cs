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
        // One dictionary per culture, filled directly: it detects a duplicate key and is what LocalizationSet's O(1)
        // lookup needs. A list, a set to detect duplicates and a final copy into a dictionary used to hold every
        // entry three times over.
        Dictionary<string, Dictionary<LocalizationKey, string?>> results = new();
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
                        // An empty header cell (a trailing comma, for instance) is not a culture. Registering
                        // it would collide with the empty-string key that marks a single-culture file and make
                        // the multi-culture overload reject the file with a misleading message.
                        if (colName.Length == 0)
                        {
                            cultures.Add(colName);
                            continue;
                        }

                        cultures.Add(colName);
                        results[colName] = new();
                    }
                }

                // The first row always defines the columns, so it is never data - regardless of whether its
                // first cell happens to be spelled "Key". Falling through here used to insert the header
                // itself as a translation entry.
                continue;
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
                
                // An empty header cell maps to no culture, so its column has no dictionary and is skipped.
                if (results.TryGetValue(cultureName, out Dictionary<LocalizationKey, string?>? strings)
                    && !strings.TryAdd(locKey, value))
                {
                    throw new LocalizationBuilderException($"The contents of the CSV file contains duplicate \"{key}\" keys.");
                }
            }
        }
        
        // Dictionary-backed, as LocalizationSet's O(1) lookup path requires; a CSV-backed set used to be alone in
        // scanning all of its entries per key.
        Dictionary<string, IEnumerable<KeyValuePair<LocalizationKey, string?>>> finalResults = new(results.Count);
        foreach (KeyValuePair<string, Dictionary<LocalizationKey, string?>> culture in results)
        {
            finalResults[culture.Key] = culture.Value;
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
