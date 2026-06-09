// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

namespace Barbatos.i18n.DependencyInjection.UnitTests;

// Marker types for ICompositeStringLocalizer<T> tests
public class SharedResource { }
public class ValidationResource { }

public sealed class CompositeStringLocalizerTests
{
    #region ICompositeStringLocalizer (non-generic) tests

    [Fact]
    public void ICompositeStringLocalizer_ShouldResolve_FromDI()
    {
        ServiceCollection services = [];

        _ = services.AddStringLocalizer(b =>
        {
            b.AddLocalization(
                "messages",
                new CultureInfo("en-US"),
                new Dictionary<LocalizationKey, string?> { { "Hello", "Hello World" } }!
            );
        });

        ServiceProvider serviceProvider = services.BuildServiceProvider();
        ICompositeStringLocalizer localizer = serviceProvider.GetRequiredService<ICompositeStringLocalizer>();

        localizer.Should().NotBeNull();
        localizer.Should().BeAssignableTo<IStringLocalizer>();
    }

    [Fact]
    public void ICompositeStringLocalizer_ShouldFindKey_FromDefaultSet()
    {
        ServiceCollection services = [];

        _ = services.AddStringLocalizer(b =>
        {
            b.AddLocalization(
                new CultureInfo("en-US"),
                new Dictionary<LocalizationKey, string?> { { "Title", "My App" } }!
            );
        });

        ServiceProvider serviceProvider = services.BuildServiceProvider();
        ILocalizationCultureManager manager = serviceProvider.GetRequiredService<ILocalizationCultureManager>();
        manager.SetCulture("en-US");

        ICompositeStringLocalizer localizer = serviceProvider.GetRequiredService<ICompositeStringLocalizer>();

        localizer["Title"].Value.Should().Be("My App");
        localizer["Title"].ResourceNotFound.Should().BeFalse();
    }

    [Fact]
    public void ICompositeStringLocalizer_ShouldFindKey_AcrossMultipleNamedSets()
    {
        ServiceCollection services = [];

        _ = services.AddStringLocalizer(b =>
        {
            // Simulate JSON file
            b.AddLocalization(
                "validation",
                new CultureInfo("en-US"),
                new Dictionary<LocalizationKey, string?> { { "Required", "This field is required" } }!
            );

            // Simulate CSV file
            b.AddLocalization(
                "errors",
                new CultureInfo("en-US"),
                new Dictionary<LocalizationKey, string?> { { "500", "Internal Server Error" } }!
            );

            // Simulate INI file
            b.AddLocalization(
                "settings",
                new CultureInfo("en-US"),
                new Dictionary<LocalizationKey, string?> { { "AppName", "Barbatos" } }!
            );
        });

        ServiceProvider serviceProvider = services.BuildServiceProvider();
        ILocalizationCultureManager manager = serviceProvider.GetRequiredService<ILocalizationCultureManager>();
        manager.SetCulture("en-US");

        ICompositeStringLocalizer localizer = serviceProvider.GetRequiredService<ICompositeStringLocalizer>();

        // All keys should be resolvable via the single ICompositeStringLocalizer
        localizer["Required"].Value.Should().Be("This field is required");
        localizer["500"].Value.Should().Be("Internal Server Error");
        localizer["AppName"].Value.Should().Be("Barbatos");
    }

    [Fact]
    public void ICompositeStringLocalizer_ShouldReturnKeyAsValue_WhenNotFound()
    {
        ServiceCollection services = [];

        _ = services.AddStringLocalizer(b =>
        {
            b.AddLocalization(
                "test",
                new CultureInfo("en-US"),
                new Dictionary<LocalizationKey, string?>()
            );
        });

        ServiceProvider serviceProvider = services.BuildServiceProvider();
        ILocalizationCultureManager manager = serviceProvider.GetRequiredService<ILocalizationCultureManager>();
        manager.SetCulture("en-US");

        ICompositeStringLocalizer localizer = serviceProvider.GetRequiredService<ICompositeStringLocalizer>();

        var result = localizer["NonExistent"];
        result.Value.Should().Be("NonExistent");
        result.ResourceNotFound.Should().BeTrue();
    }

    [Fact]
    public void ICompositeStringLocalizer_ShouldFormatPlaceholders()
    {
        ServiceCollection services = [];

        _ = services.AddStringLocalizer(b =>
        {
            b.AddLocalization(
                "messages",
                new CultureInfo("en-US"),
                new Dictionary<LocalizationKey, string?> { { "Greeting", "Hello {0}, welcome to {1}!" } }!
            );
        });

        ServiceProvider serviceProvider = services.BuildServiceProvider();
        ILocalizationCultureManager manager = serviceProvider.GetRequiredService<ILocalizationCultureManager>();
        manager.SetCulture("en-US");

        ICompositeStringLocalizer localizer = serviceProvider.GetRequiredService<ICompositeStringLocalizer>();

        localizer["Greeting", "John", "Barbatos"].Value.Should().Be("Hello John, welcome to Barbatos!");
    }

    [Fact]
    public void ICompositeStringLocalizer_ShouldSwitchCulture()
    {
        ServiceCollection services = [];

        _ = services.AddStringLocalizer(b =>
        {
            b.AddLocalization(
                "messages",
                new CultureInfo("en-US"),
                new Dictionary<LocalizationKey, string?> { { "Hello", "Hello" } }!
            );
            b.AddLocalization(
                "messages",
                new CultureInfo("vi-VN"),
                new Dictionary<LocalizationKey, string?> { { "Hello", "Xin chÃ o" } }!
            );
        });

        ServiceProvider serviceProvider = services.BuildServiceProvider();
        ILocalizationCultureManager manager = serviceProvider.GetRequiredService<ILocalizationCultureManager>();
        ICompositeStringLocalizer localizer = serviceProvider.GetRequiredService<ICompositeStringLocalizer>();

        manager.SetCulture("en-US");
        localizer["Hello"].Value.Should().Be("Hello");

        manager.SetCulture("vi-VN");
        localizer["Hello"].Value.Should().Be("Xin chÃ o");
    }

    [Fact]
    public void ICompositeStringLocalizer_GetAllStrings_ShouldReturnFromAllSets()
    {
        ServiceCollection services = [];

        _ = services.AddStringLocalizer(b =>
        {
            b.AddLocalization(
                "set1",
                new CultureInfo("en-US"),
                new Dictionary<LocalizationKey, string?> { { "Key1", "Value1" } }!
            );
            b.AddLocalization(
                "set2",
                new CultureInfo("en-US"),
                new Dictionary<LocalizationKey, string?> { { "Key2", "Value2" } }!
            );
        });

        ServiceProvider serviceProvider = services.BuildServiceProvider();
        ILocalizationCultureManager manager = serviceProvider.GetRequiredService<ILocalizationCultureManager>();
        manager.SetCulture("en-US");

        ICompositeStringLocalizer localizer = serviceProvider.GetRequiredService<ICompositeStringLocalizer>();

        var allStrings = localizer.GetAllStrings(false).ToList();
        allStrings.Should().Contain(x => x.Name == "key1" && x.Value == "Value1");
        allStrings.Should().Contain(x => x.Name == "key2" && x.Value == "Value2");
    }

    #endregion

    #region ICompositeStringLocalizer<TResource> (generic) tests

    [Fact]
    public void ICompositeStringLocalizerOfT_ShouldResolve_FromDI()
    {
        ServiceCollection services = [];

        _ = services.AddStringLocalizer(b =>
        {
            b.AddLocalization(
                typeof(SharedResource).FullName!.ToLowerInvariant(),
                new CultureInfo("en-US"),
                new Dictionary<LocalizationKey, string?> { { "Title", "Shared Title" } }!
            );
        });

        ServiceProvider serviceProvider = services.BuildServiceProvider();
        ICompositeStringLocalizer<SharedResource> localizer = serviceProvider.GetRequiredService<ICompositeStringLocalizer<SharedResource>>();

        localizer.Should().NotBeNull();
        localizer.Should().BeAssignableTo<IStringLocalizer<SharedResource>>();
        localizer.Should().BeAssignableTo<ICompositeStringLocalizer>();
    }

    [Fact]
    public void ICompositeStringLocalizerOfT_ShouldFindKey_FromScopedSet()
    {
        ServiceCollection services = [];

        _ = services.AddStringLocalizer(b =>
        {
            b.AddLocalization(
                typeof(SharedResource).FullName!.ToLowerInvariant(),
                new CultureInfo("en-US"),
                new Dictionary<LocalizationKey, string?> { { "Title", "Shared Title" } }!
            );
            b.AddLocalization(
                "other",
                new CultureInfo("en-US"),
                new Dictionary<LocalizationKey, string?> { { "Title", "Other Title" } }!
            );
        });

        ServiceProvider serviceProvider = services.BuildServiceProvider();
        ILocalizationCultureManager manager = serviceProvider.GetRequiredService<ILocalizationCultureManager>();
        manager.SetCulture("en-US");

        ICompositeStringLocalizer<SharedResource> localizer = serviceProvider.GetRequiredService<ICompositeStringLocalizer<SharedResource>>();

        // Should prefer the scoped set matching SharedResource's type name
        localizer["Title"].Value.Should().Be("Shared Title");
    }

    [Fact]
    public void ICompositeStringLocalizerOfT_ShouldFallback_ToOtherSets_WhenKeyNotInScopedSet()
    {
        ServiceCollection services = [];

        _ = services.AddStringLocalizer(b =>
        {
            // SharedResource set only has "Title"
            b.AddLocalization(
                typeof(SharedResource).FullName!.ToLowerInvariant(),
                new CultureInfo("en-US"),
                new Dictionary<LocalizationKey, string?> { { "Title", "Shared Title" } }!
            );

            // "validation" set has "Required" key
            b.AddLocalization(
                "validation",
                new CultureInfo("en-US"),
                new Dictionary<LocalizationKey, string?> { { "Required", "Field is required" } }!
            );

            // "errors" set has "500" key
            b.AddLocalization(
                "errors",
                new CultureInfo("en-US"),
                new Dictionary<LocalizationKey, string?> { { "500", "Server Error" } }!
            );
        });

        ServiceProvider serviceProvider = services.BuildServiceProvider();
        ILocalizationCultureManager manager = serviceProvider.GetRequiredService<ILocalizationCultureManager>();
        manager.SetCulture("en-US");

        ICompositeStringLocalizer<SharedResource> localizer = serviceProvider.GetRequiredService<ICompositeStringLocalizer<SharedResource>>();

        // From scoped set
        localizer["Title"].Value.Should().Be("Shared Title");
        localizer["Title"].ResourceNotFound.Should().BeFalse();

        // Fallback to "validation" set
        localizer["Required"].Value.Should().Be("Field is required");
        localizer["Required"].ResourceNotFound.Should().BeFalse();

        // Fallback to "errors" set
        localizer["500"].Value.Should().Be("Server Error");
        localizer["500"].ResourceNotFound.Should().BeFalse();
    }

    [Fact]
    public void ICompositeStringLocalizerOfT_ShouldReturnKey_WhenNotFoundAnywhere()
    {
        ServiceCollection services = [];

        _ = services.AddStringLocalizer(b =>
        {
            b.AddLocalization(
                typeof(SharedResource).FullName!.ToLowerInvariant(),
                new CultureInfo("en-US"),
                new Dictionary<LocalizationKey, string?> { { "Title", "Test" } }!
            );
        });

        ServiceProvider serviceProvider = services.BuildServiceProvider();
        ILocalizationCultureManager manager = serviceProvider.GetRequiredService<ILocalizationCultureManager>();
        manager.SetCulture("en-US");

        ICompositeStringLocalizer<SharedResource> localizer = serviceProvider.GetRequiredService<ICompositeStringLocalizer<SharedResource>>();

        var result = localizer["NonExistentKey"];
        result.Value.Should().Be("NonExistentKey");
        result.ResourceNotFound.Should().BeTrue();
    }

    [Fact]
    public void ICompositeStringLocalizerOfT_ShouldFormatPlaceholders_FromFallbackSet()
    {
        ServiceCollection services = [];

        _ = services.AddStringLocalizer(b =>
        {
            b.AddLocalization(
                typeof(SharedResource).FullName!.ToLowerInvariant(),
                new CultureInfo("en-US"),
                new Dictionary<LocalizationKey, string?> { { "Title", "App" } }!
            );

            b.AddLocalization(
                "messages",
                new CultureInfo("en-US"),
                new Dictionary<LocalizationKey, string?> { { "Welcome", "Welcome {0} to {1}" } }!
            );
        });

        ServiceProvider serviceProvider = services.BuildServiceProvider();
        ILocalizationCultureManager manager = serviceProvider.GetRequiredService<ILocalizationCultureManager>();
        manager.SetCulture("en-US");

        ICompositeStringLocalizer<SharedResource> localizer = serviceProvider.GetRequiredService<ICompositeStringLocalizer<SharedResource>>();

        localizer["Welcome", "User", "App"].Value.Should().Be("Welcome User to App");
    }

    [Fact]
    public void ICompositeStringLocalizerOfT_ShouldSwitchCulture()
    {
        ServiceCollection services = [];

        _ = services.AddStringLocalizer(b =>
        {
            b.AddLocalization(
                typeof(SharedResource).FullName!.ToLowerInvariant(),
                new CultureInfo("en-US"),
                new Dictionary<LocalizationKey, string?> { { "Hello", "Hello" } }!
            );
            b.AddLocalization(
                typeof(SharedResource).FullName!.ToLowerInvariant(),
                new CultureInfo("ko-KR"),
                new Dictionary<LocalizationKey, string?> { { "Hello", "ì•ˆë…•í•˜ì„¸ìš”" } }!
            );
        });

        ServiceProvider serviceProvider = services.BuildServiceProvider();
        ILocalizationCultureManager manager = serviceProvider.GetRequiredService<ILocalizationCultureManager>();
        ICompositeStringLocalizer<SharedResource> localizer = serviceProvider.GetRequiredService<ICompositeStringLocalizer<SharedResource>>();

        manager.SetCulture("en-US");
        localizer["Hello"].Value.Should().Be("Hello");

        manager.SetCulture("ko-KR");
        localizer["Hello"].Value.Should().Be("ì•ˆë…•í•˜ì„¸ìš”");
    }

    [Fact]
    public void ICompositeStringLocalizerOfT_TwoDifferentResources_ShouldResolveScopedCorrectly()
    {
        ServiceCollection services = [];

        _ = services.AddStringLocalizer(b =>
        {
            b.AddLocalization(
                typeof(SharedResource).FullName!.ToLowerInvariant(),
                new CultureInfo("en-US"),
                new Dictionary<LocalizationKey, string?> { { "Title", "Shared Title" } }!
            );
            b.AddLocalization(
                typeof(ValidationResource).FullName!.ToLowerInvariant(),
                new CultureInfo("en-US"),
                new Dictionary<LocalizationKey, string?> { { "Title", "Validation Title" } }!
            );
        });

        ServiceProvider serviceProvider = services.BuildServiceProvider();
        ILocalizationCultureManager manager = serviceProvider.GetRequiredService<ILocalizationCultureManager>();
        manager.SetCulture("en-US");

        ICompositeStringLocalizer<SharedResource> sharedLocalizer = serviceProvider.GetRequiredService<ICompositeStringLocalizer<SharedResource>>();
        ICompositeStringLocalizer<ValidationResource> validationLocalizer = serviceProvider.GetRequiredService<ICompositeStringLocalizer<ValidationResource>>();

        sharedLocalizer["Title"].Value.Should().Be("Shared Title");
        validationLocalizer["Title"].Value.Should().Be("Validation Title");
    }

    [Fact]
    public void ICompositeStringLocalizerOfT_GetAllStrings_ShouldReturnScopedSetStrings()
    {
        ServiceCollection services = [];

        _ = services.AddStringLocalizer(b =>
        {
            b.AddLocalization(
                typeof(SharedResource).FullName!.ToLowerInvariant(),
                new CultureInfo("en-US"),
                new Dictionary<LocalizationKey, string?>
                {
                    { "Key1", "Value1" },
                    { "Key2", "Value2" }
                }!
            );
        });

        ServiceProvider serviceProvider = services.BuildServiceProvider();
        ILocalizationCultureManager manager = serviceProvider.GetRequiredService<ILocalizationCultureManager>();
        manager.SetCulture("en-US");

        ICompositeStringLocalizer<SharedResource> localizer = serviceProvider.GetRequiredService<ICompositeStringLocalizer<SharedResource>>();

        var allStrings = localizer.GetAllStrings(false).ToList();
        allStrings.Should().HaveCount(2);
        allStrings.Should().Contain(x => x.Name == "key1" && x.Value == "Value1");
        allStrings.Should().Contain(x => x.Name == "key2" && x.Value == "Value2");
    }

    #endregion

    #region Backwards compatibility tests

    [Fact]
    public void ICompositeStringLocalizer_ShouldBeAssignableTo_IStringLocalizer()
    {
        ServiceCollection services = [];

        _ = services.AddStringLocalizer(b =>
        {
            b.AddLocalization(
                new CultureInfo("en-US"),
                new Dictionary<LocalizationKey, string?> { { "Test", "Value" } }!
            );
        });

        ServiceProvider serviceProvider = services.BuildServiceProvider();
        ICompositeStringLocalizer localizer = serviceProvider.GetRequiredService<ICompositeStringLocalizer>();

        // ICompositeStringLocalizer IS an IStringLocalizer
        IStringLocalizer asStringLocalizer = localizer;
        asStringLocalizer.Should().NotBeNull();
    }

    [Fact]
    public void ExistingIStringLocalizer_ShouldStillWork()
    {
        ServiceCollection services = [];

        _ = services.AddStringLocalizer(b =>
        {
            b.AddLocalization(
                "test",
                new CultureInfo("en-US"),
                new Dictionary<LocalizationKey, string?> { { "Hello", "World" } }!
            );
        });

        ServiceProvider serviceProvider = services.BuildServiceProvider();
        ILocalizationCultureManager manager = serviceProvider.GetRequiredService<ILocalizationCultureManager>();
        manager.SetCulture("en-US");

        // The old IStringLocalizer should still work as before
        IStringLocalizer oldLocalizer = serviceProvider.GetRequiredService<IStringLocalizer>();
        oldLocalizer.Should().NotBeNull();

        // The new ICompositeStringLocalizer should also be available
        ICompositeStringLocalizer newLocalizer = serviceProvider.GetRequiredService<ICompositeStringLocalizer>();
        newLocalizer.Should().NotBeNull();

        // They should be different types (ProviderBasedStringLocalizer vs CompositeStringLocalizer)
        oldLocalizer.GetType().Should().NotBe(newLocalizer.GetType());
    }

    #endregion
}
