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

    public LocalizationKey(string key)
    {
        if (key is null)
        {
            throw new ArgumentNullException(nameof(key));
        }

        _normalizedKey = key.Replace(':', '.').ToLowerInvariant();
    }

    public static implicit operator LocalizationKey(string key) => new(key);

    public static implicit operator string(LocalizationKey key) => key.Value;

    public bool Equals(LocalizationKey other)
    {
        return Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        return obj is LocalizationKey other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public static bool operator ==(LocalizationKey left, LocalizationKey right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(LocalizationKey left, LocalizationKey right)
    {
        return !left.Equals(right);
    }

    public override string ToString()
    {
        return Value;
    }
}
