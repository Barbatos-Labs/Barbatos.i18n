// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

namespace Barbatos.i18n.DependencyInjection.UnitTests;

public sealed class StringLocalizerBuilderExtensionsTests
{
    [Fact]
    public void AddStringLocalizer_ShouldAddLocalizations_ToServiceCollection()
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
        IStringLocalizer localizer = serviceProvider.GetRequiredService<IStringLocalizer>();
        ILocalizationCultureManager manager =
            serviceProvider.GetRequiredService<ILocalizationCultureManager>();

        manager.SetCulture("en-US");

        _ = localizer["Test"].Value.Should().Be("Test in english");

        manager.SetCulture("ko-KR");

        _ = localizer["Test"].Value.Should().Be("영어로 테스트");
    }

    [Fact]
    public void AddStringLocalizer_ShouldRegisterEmptySet()
    {
        ServiceCollection services = [];

        _ = services.AddStringLocalizer(b =>
        {
            b.AddLocalization(
                new LocalizationSet(
                    "Test",
                    new CultureInfo("cz-CZ"),
                    new Dictionary<LocalizationKey, string?>()
                )
            );
        });

        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IStringLocalizer localizer = serviceProvider.GetRequiredService<IStringLocalizer>();
        ILocalizationCultureManager manager =
            serviceProvider.GetRequiredService<ILocalizationCultureManager>();

        manager.SetCulture("cz-CZ");

        _ = localizer["Test"].Value.Should().Be("Test");
    }

    [Fact]
    public void AddStringLocalizer_FromResource_RegistersProviderBasedStringLocalizer()
    {
        ServiceCollection services = [];

        Action act = () => services.AddStringLocalizer(b => {
            b.FromResource<TestResource>(new CultureInfo("en-US"));
        });
        
        act.Should().Throw<LocalizationBuilderException>();
    }

    private class MockResource { }

    [Fact]
    public void IStringLocalizerOfT_ShouldResolveAndLocalizeCaseInsensitively()
    {
        ServiceCollection services = [];

        _ = services.AddStringLocalizer(b =>
        {
            b.AddLocalization(
                typeof(MockResource).FullName!.ToLowerInvariant(),
                new CultureInfo("en-US"),
                new Dictionary<LocalizationKey, string?>
                {
                    { "MessageTitle", "Hello English" },
                    { "GreetingWithName", "Hello {0}" }
                }!
            );
        });

        _ = services.AddSingleton<IStringLocalizer<MockResource>, ProviderBasedStringLocalizer<MockResource>>();

        ServiceProvider serviceProvider = services.BuildServiceProvider();

        // Set the culture to en-US
        ILocalizationCultureManager manager = serviceProvider.GetRequiredService<ILocalizationCultureManager>();
        manager.SetCulture("en-US");

        // 1. Verify resolving ILocalizationProvider directly from DI works
        ILocalizationProvider provider = serviceProvider.GetRequiredService<ILocalizationProvider>();
        provider.Should().NotBeNull();

        // 2. Verify resolving IStringLocalizer<MockResource> works
        IStringLocalizer<MockResource> localizer = serviceProvider.GetRequiredService<IStringLocalizer<MockResource>>();
        localizer.Should().NotBeNull();

        // 3. Verify case-insensitive key resolution
        localizer["MessageTitle"].Value.Should().Be("Hello English");
        localizer["messagetitle"].Value.Should().Be("Hello English");
        localizer["MESSAGETITLE"].Value.Should().Be("Hello English");

        // 4. Verify formatting / placeholders
        localizer["GreetingWithName", "John"].Value.Should().Be("Hello John");
    }

    [Fact]
    public void IStringLocalizerFactory_ShouldResolveOtherFormatsCaseInsensitively()
    {
        ServiceCollection services = [];

        _ = services.AddStringLocalizer(b =>
        {
            // Register mock JSON set
            b.AddLocalization(
                "validation",
                new CultureInfo("en-US"),
                new Dictionary<LocalizationKey, string?>
                {
                    { "Required", "Field is required" }
                }!
            );

            // Register mock INI set
            b.AddLocalization(
                "en-us",
                new CultureInfo("en-US"),
                new Dictionary<LocalizationKey, string?>
                {
                    { "Title", "My App" }
                }!
            );

            // Register mock CSV set
            b.AddLocalization(
                "errors",
                new CultureInfo("en-US"),
                new Dictionary<LocalizationKey, string?>
                {
                    { "500", "Internal Server Error" }
                }!
            );
        });

        ServiceProvider serviceProvider = services.BuildServiceProvider();

        ILocalizationCultureManager manager = serviceProvider.GetRequiredService<ILocalizationCultureManager>();
        manager.SetCulture("en-US");

        IStringLocalizerFactory factory = serviceProvider.GetRequiredService<IStringLocalizerFactory>();

        // 1. Verify JSON localizer (resolved with case-insensitive name)
        IStringLocalizer jsonLocalizer = factory.Create("Validation", null);
        jsonLocalizer["Required"].Value.Should().Be("Field is required");
        jsonLocalizer["required"].Value.Should().Be("Field is required");

        // 2. Verify INI localizer
        IStringLocalizer iniLocalizer = factory.Create("en-us", null);
        iniLocalizer["Title"].Value.Should().Be("My App");

        // 3. Verify CSV localizer
        IStringLocalizer csvLocalizer = factory.Create("Errors", null);
        csvLocalizer["500"].Value.Should().Be("Internal Server Error");
    }
}
