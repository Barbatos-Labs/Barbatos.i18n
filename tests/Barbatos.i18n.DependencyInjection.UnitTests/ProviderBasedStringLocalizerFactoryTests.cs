// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

namespace Barbatos.i18n.DependencyInjection.UnitTests;

public sealed class ProviderBasedStringLocalizerFactoryTests
{
    [Fact]
    public void Create_ShouldReturnCorrectIStringLocalizer()
    {
        ServiceCollection services = [];

        _ = services.AddStringLocalizer(b =>
        {
            b.AddLocalization(
                new LocalizationSet(
                    "Test",
                    new CultureInfo("en-US"),
                    new Dictionary<LocalizationKey, string?> { { "Test", "Test in english" } }!
                )
            );
            b.AddLocalization(
                new LocalizationSet(
                    "Test",
                    new CultureInfo("ko-KR"),
                    new Dictionary<LocalizationKey, string?> { { "Test", "영어로 테스트" } }!
                )
            );
        });

        ServiceProvider serviceProvider = services.BuildServiceProvider();
        ILocalizationCultureManager manager =
            serviceProvider.GetRequiredService<ILocalizationCultureManager>();

        manager.SetCulture("ko-KR");

        IStringLocalizerFactory factory =
            serviceProvider.GetRequiredService<IStringLocalizerFactory>();
        IStringLocalizer localizer = factory.Create("Test", default!);

        _ = localizer.Should().NotBeNull();
        _ = localizer["Test"].Value.Should().Be("영어로 테스트");
    }

    [Fact]
    public void Create_ByType_ShouldReturnCorrectIStringLocalizer()
    {
        ServiceCollection services = [];

        _ = services.AddStringLocalizer(b =>
        {
            b.AddLocalization(
                new LocalizationSet(
                    typeof(TestResource).FullName!.ToLower(),
                    new CultureInfo("en-US"),
                    new Dictionary<LocalizationKey, string?> { { "key1", "value1" } }!
                )
            );
        });

        ServiceProvider serviceProvider = services.BuildServiceProvider();
        ILocalizationCultureManager manager = serviceProvider.GetRequiredService<ILocalizationCultureManager>();
        manager.SetCulture("en-US");

        IStringLocalizerFactory factory = serviceProvider.GetRequiredService<IStringLocalizerFactory>();
        IStringLocalizer localizer = factory.Create(typeof(TestResource));

        localizer.Should().NotBeNull();
        localizer["key1"].Value.Should().Be("value1");
    }
}
