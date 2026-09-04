// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

namespace Barbatos.i18n;

/// <summary>
/// Represents a normalized key used for localization string lookups.
/// Automatically converts colons (:) to dots (.) for unified path access.
/// </summary>
public readonly struct LocalizationKey : IEquatable<LocalizationKey>
{
    private readonly string? _normalizedKey;

    /// <summary>
    /// Gets the normalized key text. A default-initialised instance carries no string, and is reported as empty
    /// rather than null so that hashing, comparison and conversion stay safe.
    /// </summary>
    private string Value => _normalizedKey ?? string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalizationKey"/> struct.
    /// </summary>
    /// <param name="key">The key as written by the caller.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="key"/> is null.</exception>
    /// <remarks>
    /// The key is normalized: ':' becomes '.' and the whole key is lower-cased with the invariant culture, so
    /// <c>Header:Title</c>, <c>header.TITLE</c> and <c>Header.Title</c> all address the same entry, and an enum
    /// member name works as a key regardless of how it is cased.
    /// </remarks>
    public LocalizationKey(string key)
    {
        if (key is null)
        {
            throw new ArgumentNullException(nameof(key));
        }

        _normalizedKey = key.Replace(':', '.').ToLowerInvariant();
    }

    /// <summary>
    /// Converts a string into a normalized <see cref="LocalizationKey"/>.
    /// </summary>
    /// <param name="key">The key as written by the caller.</param>
    public static implicit operator LocalizationKey(string key) => new(key);

    /// <summary>
    /// Converts a <see cref="LocalizationKey"/> back to its normalized text.
    /// </summary>
    /// <param name="key">The key to convert.</param>
    public static implicit operator string(LocalizationKey key) => key.Value;

    /// <summary>
    /// Determines whether this key addresses the same entry as another.
    /// </summary>
    /// <param name="other">The key to compare with.</param>
    /// <returns>True when both normalize to the same text.</returns>
    public bool Equals(LocalizationKey other)
    {
        return Value == other.Value;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is LocalizationKey other && Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    /// <summary>
    /// Determines whether two keys address the same entry.
    /// </summary>
    /// <param name="left">The first key.</param>
    /// <param name="right">The second key.</param>
    /// <returns>True when both normalize to the same text.</returns>
    public static bool operator ==(LocalizationKey left, LocalizationKey right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two keys address different entries.
    /// </summary>
    /// <param name="left">The first key.</param>
    /// <param name="right">The second key.</param>
    /// <returns>True when they normalize to different text.</returns>
    public static bool operator !=(LocalizationKey left, LocalizationKey right)
    {
        return !left.Equals(right);
    }

    /// <summary>
    /// Returns the normalized key text.
    /// </summary>
    /// <returns>The normalized key, or an empty string for a default-initialised instance.</returns>
    public override string ToString()
    {
        return Value;
    }
}
