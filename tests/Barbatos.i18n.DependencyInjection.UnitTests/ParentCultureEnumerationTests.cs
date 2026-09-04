// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

namespace Barbatos.i18n.DependencyInjection.UnitTests;

// Marker type for the resource-scoped enumeration tests.
public class ReportResource { }

/// <summary>
/// GetAllStrings(includeParentCultures: true) must widen the result to the whole culture chain. The generic
/// localizer used to stop at the resource-scoped set for the exact culture, so a key defined only in the neutral
/// parent set was reported as missing by translation-coverage tooling.
/// </summary>
[Collection("Sequential")]
public sealed class ParentCultureEnumerationTests : IDisposable
{
    private static readonly CultureInfo Neutral = new("en");
    private static readonly CultureInfo Specific = new("en-US");

    private readonly CultureInfo _originalCulture = CultureInfo.CurrentCulture;
    private readonly CultureInfo _originalUiCulture = CultureInfo.CurrentUICulture;

    public void Dispose()
    {
        CultureInfo.CurrentCulture = _originalCulture;
        CultureInfo.CurrentUICulture = _originalUiCulture;
    }

    private static ServiceProvider BuildServices()
    {
        ServiceCollection services = [];

        string resourceName = typeof(ReportResource).FullName!.ToLowerInvariant();

        _ = services.AddStringLocalizer(b =>
        {
            // Shared keys live in the neutral set, the US overrides in the specific one.
            b.AddLocalization(resourceName, Neutral, new Dictionary<LocalizationKey, string?>
            {
                { "Shared", "Shared neutral" },
                { "Colour", "Colour" }
            }!);

            b.AddLocalization(resourceName, Specific, new Dictionary<LocalizationKey, string?>
            {
                { "Colour", "Color" }
            }!);

            b.SetCulture(Specific);
        });

        return services.BuildServiceProvider();
    }

    [Fact]
    public void GenericLocalizer_IncludingParents_ReportsKeysDefinedOnlyInTheParentSet()
    {
        using ServiceProvider provider = BuildServices();
        provider.GetRequiredService<ILocalizationCultureManager>().SetCulture(Specific);

        LocalizedString[] all = provider
            .GetRequiredService<ICompositeStringLocalizer<ReportResource>>()
            .GetAllStrings(includeParentCultures: true)
            .ToArray();

        all.Select(s => s.Name).Should().Contain("shared", "a key defined only in the neutral parent set is still in scope");
        all.Single(s => s.Name == "colour").Value.Should().Be("Color", "the most specific culture wins");
        all.Should().OnlyHaveUniqueItems(s => s.Name);
    }

    [Fact]
    public void GenericLocalizer_ExcludingParents_StaysOnTheExactCulture()
    {
        using ServiceProvider provider = BuildServices();
        provider.GetRequiredService<ILocalizationCultureManager>().SetCulture(Specific);

        LocalizedString[] all = provider
            .GetRequiredService<ICompositeStringLocalizer<ReportResource>>()
            .GetAllStrings(includeParentCultures: false)
            .ToArray();

        all.Select(s => s.Name).Should().Contain("colour").And.NotContain("shared");
    }

    [Fact]
    public void NonGenericLocalizer_IncludingParents_ReportsEachKeyOnce()
    {
        using ServiceProvider provider = BuildServices();
        provider.GetRequiredService<ILocalizationCultureManager>().SetCulture(Specific);

        LocalizedString[] all = provider
            .GetRequiredService<ICompositeStringLocalizer>()
            .GetAllStrings(includeParentCultures: true)
            .ToArray();

        all.Select(s => s.Name).Should().Contain("shared");
        all.Should().OnlyHaveUniqueItems(s => s.Name);
    }
}
