// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

namespace Barbatos.i18n.DependencyInjection.UnitTests;

/// <summary>
/// A provider's culture is process-wide, which does not fit a server. Turning each request's language into a
/// SetCulture call let concurrent requests overwrite each other: measured over 400 parallel lookups, 160 were
/// answered in the other request's language. UseAmbientCulture makes lookups follow CurrentUICulture, which
/// ASP.NET Core establishes per request and flows with the async context.
/// </summary>
[Collection("Sequential")]
public sealed class AmbientCultureTests : IDisposable
{
    private readonly CultureInfo _originalUiCulture = CultureInfo.CurrentUICulture;
    private readonly CultureInfo _originalCulture = CultureInfo.CurrentCulture;

    public void Dispose()
    {
        CultureInfo.CurrentUICulture = _originalUiCulture;
        CultureInfo.CurrentCulture = _originalCulture;
    }

    private static ServiceProvider Build(bool ambient)
    {
        ServiceCollection services = [];

        if (ambient)
        {
            _ = services.ConfigureLocalizationOptions(o => o.UseAmbientCulture = true);
        }

        _ = services.AddStringLocalizer(b =>
        {
            b.AddLocalization(new CultureInfo("en-US"),
                new Dictionary<LocalizationKey, string?> { { "greeting", "Hello" } }!);
            b.AddLocalization(new CultureInfo("vi-VN"),
                new Dictionary<LocalizationKey, string?> { { "greeting", "Xin chao" } }!);
            b.SetCulture(new CultureInfo("en-US"));
        });

        return services.BuildServiceProvider();
    }

    [Fact]
    public void ConcurrentLookups_EachSeeTheirOwnCulture()
    {
        using ServiceProvider provider = Build(ambient: true);
        int wrong = 0;

        Parallel.For(0, 400, i =>
        {
            bool english = i % 2 == 0;
            CultureInfo.CurrentUICulture = new CultureInfo(english ? "en-US" : "vi-VN");

            string actual = provider.GetRequiredService<ICompositeStringLocalizer>()["greeting"].Value;

            if (actual != (english ? "Hello" : "Xin chao"))
            {
                _ = Interlocked.Increment(ref wrong);
            }
        });

        wrong.Should().Be(0, "a lookup must not be answered in a concurrent caller's language");
    }

    [Fact]
    public void AmbientMode_FollowsTheCurrentUiCulture_WithoutSetCulture()
    {
        using ServiceProvider provider = Build(ambient: true);

        CultureInfo.CurrentUICulture = new CultureInfo("vi-VN");

        provider.GetRequiredService<ICompositeStringLocalizer>()["greeting"].Value.Should().Be("Xin chao");
    }

    [Fact]
    public void TheDefaultStaysOnTheProviderCulture()
    {
        using ServiceProvider provider = Build(ambient: false);

        // Off by default, so an application that configures the builder's culture and never calls SetCulture
        // keeps resolving against it no matter what the ambient culture happens to be.
        CultureInfo.CurrentUICulture = new CultureInfo("vi-VN");

        provider.GetRequiredService<ILocalizationCultureManager>().GetCulture().Name.Should().Be("en-US");
    }

    [Fact]
    public void SetCultureStillWorksInAmbientMode()
    {
        using ServiceProvider provider = Build(ambient: true);

        provider.GetRequiredService<ILocalizationCultureManager>().SetCulture(new CultureInfo("vi-VN"));

        provider.GetRequiredService<ICompositeStringLocalizer>()["greeting"].Value.Should().Be("Xin chao");
    }
}
