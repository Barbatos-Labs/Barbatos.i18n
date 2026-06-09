// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

namespace Barbatos.i18n.UnitTests;

public sealed class LocalizationKeyTests
{
    [Fact]
    public void Constructor_ShouldNormalizeKey()
    {
        var key = new LocalizationKey("Namespace:Key.SubKey");
        key.ToString().Should().Be("namespace.key.subkey");
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_IfKeyIsNull()
    {
        Action act = () => new LocalizationKey(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ImplicitConversionFromString_ShouldCreateKey()
    {
        LocalizationKey key = "Test:Key";
        key.ToString().Should().Be("test.key");
    }

    [Fact]
    public void ImplicitConversionToString_ShouldReturnNormalizedKey()
    {
        LocalizationKey key = new LocalizationKey("Test:Key");
        string str = key;
        str.Should().Be("test.key");
    }

    [Fact]
    public void Equals_ShouldReturnTrueForIdenticalKeys()
    {
        var key1 = new LocalizationKey("Test:Key");
        var key2 = new LocalizationKey("test.key");

        key1.Equals(key2).Should().BeTrue();
        key1.Equals((object)key2).Should().BeTrue();
        (key1 == key2).Should().BeTrue();
        (key1 != key2).Should().BeFalse();
    }

    [Fact]
    public void Equals_ShouldReturnFalseForDifferentKeys()
    {
        var key1 = new LocalizationKey("Test:Key1");
        var key2 = new LocalizationKey("Test:Key2");

        key1.Equals(key2).Should().BeFalse();
        key1.Equals((object)key2).Should().BeFalse();
        (key1 == key2).Should().BeFalse();
        (key1 != key2).Should().BeTrue();
    }

    [Fact]
    public void GetHashCode_ShouldBeEqualForIdenticalKeys()
    {
        var key1 = new LocalizationKey("Test:Key");
        var key2 = new LocalizationKey("test.key");

        key1.GetHashCode().Should().Be(key2.GetHashCode());
    }
}
