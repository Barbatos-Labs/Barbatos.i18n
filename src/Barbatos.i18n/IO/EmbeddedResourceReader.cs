// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

using System.Text.RegularExpressions;

namespace Barbatos.i18n.IO;

/// <summary>
/// Provides a method to read resources from an assembly.
/// </summary>
public static class EmbeddedResourceReader
{
    private static readonly Regex MultipleDotRegex = new(@"\.{2,}", RegexOptions.Compiled);

    /// <summary>
    /// Reads the content of a resource from the specified assembly.
    /// </summary>
    /// <param name="path">The path to the resource in the assembly.</param>
    /// <param name="assembly">The assembly that contains the resource.</param>
    /// <returns>The content of the resource as a string, or null if the resource could not be found.</returns>
    public static string? ReadToEnd(string path, Assembly assembly)
    {
        string assemblyName = assembly.GetName().Name;
        string resourceName = assemblyName + ".";

        resourceName += path.Replace(
            assemblyName,
            string.Empty,
            StringComparison.InvariantCultureIgnoreCase
        );

        resourceName = MultipleDotRegex.Replace(
            resourceName.Replace("\\", ".").Replace("/", "."),
            "."
        );

        using Stream? stream = assembly.GetManifestResourceStream(resourceName);

        if (stream is null)
        {
            return null;
        }

        using StreamReader reader = new(stream);

        return reader.ReadToEnd();
    }
}
