// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

namespace Barbatos.i18n.DependencyInjection.UnitTests;

/// <summary>
/// A dedicated resource type so the generic localizers' static state is initialised inside these tests'
/// culture scope. Its full name contains an upper-case "I" (in "DependencyInjection"), which is what the
/// Turkish lower-casing rule maps to a dotless "ı".
/// </summary>
public class TurkishProbeResource { }

/// <summary>
/// Localization set names are registered with <c>ToLowerInvariant()</c>, so every lookup must lower-case the
/// same way. Under tr-TR and az, culture-sensitive <c>ToLower()</c> turns "I" into "ı", which does not match
/// under <see cref="StringComparison.OrdinalIgnoreCase"/> and made resource-scoped localizers silently return
/// the key instead of the translation.
/// </summary>
public sealed class InvariantResourceNameTests : IDisposable
{
    private const string Key = "greeting";
    private const string Translation = "Merhaba";

    private readonly CultureInfo _originalCulture;
    private readonly CultureInfo _originalUICulture;

    public InvariantResourceNameTests()
    {
        _originalCulture = CultureInfo.CurrentCulture;
        _originalUICulture = CultureInfo.CurrentUICulture;

        // Every assertion below runs while Turkish casing rules are active.
        CultureInfo.CurrentCulture = new CultureInfo("tr-TR");
        CultureInfo.CurrentUICulture = new CultureInfo("tr-TR");
    }

    public void Dispose()
    {
        CultureInfo.CurrentCulture = _originalCulture;
        CultureInfo.CurrentUICulture = _originalUICulture;
    }

    [Fact]
    public void ResourceFullName_LowerCasedByTurkishRules_DoesNotMatchTheRegisteredName()
    {
        // Guards the premise: if this ever stops being true the tests below prove nothing.
        string registered = typeof(TurkishProbeResource).FullName!.ToLowerInvariant();
        string cultureSensitive = typeof(TurkishProbeResource).FullName!.ToLower();

        cultureSensitive.Should().NotBe(registered);
        string.Equals(registered, cultureSensitive, StringComparison.OrdinalIgnoreCase).Should().BeFalse();
    }

    [Fact]
    public void ProviderBasedStringLocalizerT_ResolvesUnderTurkishCulture()
    {
        var localizer = new ProviderBasedStringLocalizer<TurkishProbeResource>(BuildProvider(), BuildCultureManager());

        LocalizedString result = localizer[Key];

        result.Value.Should().Be(Translation);
        result.ResourceNotFound.Should().BeFalse();
    }

    [Fact]
    public void CompositeStringLocalizerT_ResolvesUnderTurkishCulture()
    {
        var localizer = new CompositeStringLocalizer<TurkishProbeResource>(BuildProvider(), BuildCultureManager());

        LocalizedString result = localizer[Key];

        result.Value.Should().Be(Translation);
        result.ResourceNotFound.Should().BeFalse();
    }

    [Fact]
    public void StringLocalizerFactory_ResolvesUnderTurkishCulture()
    {
        var resolver = new LocalizationProviderResolver();
        resolver.AddProvider(null, BuildProvider());

        var factory = new ProviderBasedStringLocalizerFactory(resolver, BuildCultureManager());

        IStringLocalizer localizer = factory.Create(typeof(TurkishProbeResource));

        localizer[Key].Value.Should().Be(Translation);
    }

    /// <summary>
    /// Registers the set exactly the way the RESX ingest path does, with an invariant lower-cased name.
    /// </summary>
    private static LocalizationProvider BuildProvider() =>
        new(
            new CultureInfo("tr-TR"),
            [
                new LocalizationSet(
                    typeof(TurkishProbeResource).FullName!.ToLowerInvariant(),
                    new CultureInfo("tr-TR"),
                    new Dictionary<LocalizationKey, string?> { { Key, Translation } }
                )
            ]
        );

    private static ILocalizationCultureManager BuildCultureManager()
    {
        var manager = new LocalizationCultureManager();
        manager.SetCulture(new CultureInfo("tr-TR"));
        return manager;
    }
}
