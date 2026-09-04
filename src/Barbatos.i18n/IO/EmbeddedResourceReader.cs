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
        // AssemblyName.Name is nullable: a dynamic assembly, or one loaded from a stream, can have none.
        // Treating it as non-null used to make string.Replace throw ArgumentNullException on its oldValue.
        string? assemblyName = assembly.GetName().Name;

        if (string.IsNullOrEmpty(assemblyName))
        {
            return ReadResource(assembly, NormalizeSeparators(path));
        }

        // Only a leading assembly name is stripped, once, and only when it ends on a segment boundary. Removing
        // every occurrence corrupted any path in which the name appears again, and matching a bare prefix turned
        // a folder that merely starts with it - "AppData" under an assembly called "App" - into "Data". Either
        // way the resource was reported missing.
        string relativePath = StartsWithSegment(path, assemblyName)
            ? path.Substring(assemblyName.Length)
            : path;

        return ReadResource(assembly, NormalizeSeparators(assemblyName + "." + relativePath));
    }

    /// <summary>
    /// Determines whether a path begins with the assembly name as a whole segment.
    /// </summary>
    /// <param name="path">The resource path.</param>
    /// <param name="assemblyName">The assembly name.</param>
    /// <returns>True when the path is the assembly name, or the assembly name followed by a separator.</returns>
    private static bool StartsWithSegment(string path, string assemblyName)
    {
        if (!path.StartsWith(assemblyName, StringComparison.InvariantCultureIgnoreCase))
        {
            return false;
        }

        return path.Length == assemblyName.Length
            || path[assemblyName.Length] is '.' or '/' or '\\';
    }

    /// <summary>
    /// Turns path separators into the dots used by manifest resource names and collapses repeated dots.
    /// </summary>
    /// <param name="value">The resource path.</param>
    /// <returns>The normalized manifest resource name.</returns>
    private static string NormalizeSeparators(string value) =>
        MultipleDotRegex.Replace(value.Replace("\\", ".").Replace("/", "."), ".");

    /// <summary>
    /// Reads a manifest resource to the end.
    /// </summary>
    /// <param name="assembly">The assembly that contains the resource.</param>
    /// <param name="resourceName">The manifest resource name.</param>
    /// <returns>The content of the resource, or null when the assembly has no such resource.</returns>
    private static string? ReadResource(Assembly assembly, string resourceName)
    {
        using Stream? stream = assembly.GetManifestResourceStream(resourceName);

        if (stream is null)
        {
            return null;
        }

        using StreamReader reader = new(stream);

        return reader.ReadToEnd();
    }
}
