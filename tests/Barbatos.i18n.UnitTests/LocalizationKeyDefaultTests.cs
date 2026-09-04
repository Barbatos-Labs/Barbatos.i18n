// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

namespace Barbatos.i18n.UnitTests;

/// <summary>
/// A default-initialised <see cref="LocalizationKey"/> has no backing string. It is reachable through
/// FirstOrDefault over a key/value sequence, so every member has to tolerate it.
/// </summary>
public sealed class LocalizationKeyDefaultTests
{
    [Fact]
    public void GetHashCode_DoesNotThrow_ForADefaultInstance()
    {
        LocalizationKey key = default;

        Action act = () => key.GetHashCode();

        act.Should().NotThrow();
    }

    [Fact]
    public void ToString_ReturnsEmpty_ForADefaultInstance()
    {
        LocalizationKey key = default;

        key.ToString().Should().Be(string.Empty);
    }

    [Fact]
    public void ImplicitStringConversion_ReturnsEmpty_ForADefaultInstance()
    {
        LocalizationKey key = default;

        string converted = key;

        converted.Should().NotBeNull();
        converted.Should().Be(string.Empty);
    }

    [Fact]
    public void DefaultInstances_AreEqualAndUsableAsDictionaryKeys()
    {
        LocalizationKey first = default;
        LocalizationKey second = default;

        (first == second).Should().BeTrue();

        Dictionary<LocalizationKey, string> map = new() { [first] = "value" };

        map[second].Should().Be("value");
    }

    [Fact]
    public void DefaultInstance_DoesNotEqualARealKey()
    {
        LocalizationKey real = new("greeting");

        (real == default).Should().BeFalse();
    }
}
