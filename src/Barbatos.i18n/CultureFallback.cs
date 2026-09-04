// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

namespace Barbatos.i18n;

/// <summary>
/// Walks the culture fallback chain used when a translation is not registered for an exact culture.
/// </summary>
/// <remarks>
/// Mirrors what <see cref="ResourceManager"/> does for satellite assemblies: a specific culture falls back to
/// its neutral parent and finally to the invariant culture, so translations registered for "en" also serve
/// "en-GB" and "en-US".
/// </remarks>
public static class CultureFallback
{
    /// <summary>
    /// Enumerates <paramref name="culture"/> followed by each of its parents, ending with the invariant culture.
    /// </summary>
    /// <param name="culture">The culture to start from.</param>
    /// <returns>The culture itself first, then progressively less specific ones.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="culture"/> is null.</exception>
    /// <remarks>
    /// <see cref="CultureInfo.InvariantCulture"/> is its own parent, so the walk stops there rather than looping.
    /// </remarks>
    public static IEnumerable<CultureInfo> EnumerateChain(CultureInfo culture)
    {
        if (culture is null)
        {
            throw new ArgumentNullException(nameof(culture));
        }

        CultureInfo current = culture;

        while (true)
        {
            yield return current;

            // The invariant culture has an empty name and is its own parent; it is always the last step.
            if (string.IsNullOrEmpty(current.Name))
            {
                yield break;
            }

            CultureInfo parent = current.Parent;

            if (parent is null || parent.Equals(current))
            {
                yield break;
            }

            current = parent;
        }
    }
}
