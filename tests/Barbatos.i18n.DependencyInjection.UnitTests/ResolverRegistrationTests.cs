// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

namespace Barbatos.i18n.DependencyInjection.UnitTests;

/// <summary>
/// AddStringLocalizer reuses an already-registered resolver by reading its ImplementationInstance. That is null
/// for a type- or factory-based registration and the wrong type for a consumer's own implementation, which used
/// to surface as NullReferenceException and InvalidCastException respectively.
/// </summary>
public sealed class ResolverRegistrationTests
{
    private static Action Register(Action<IServiceCollection> arrange) =>
        () =>
        {
            var services = new ServiceCollection();
            arrange(services);
            services.AddStringLocalizer(b => b.AddLocalization(
                new CultureInfo("en-US"),
                new Dictionary<LocalizationKey, string?> { { "greeting", "Hello" } }));
        };

    [Fact]
    public void TypeRegisteredResolver_ThrowsADescriptiveException()
    {
        Register(s => s.AddSingleton<ILocalizationProviderResolver, LocalizationProviderResolver>())
            .Should().Throw<InvalidOperationException>()
            .WithMessage("*cannot extend*");
    }

    [Fact]
    public void FactoryRegisteredResolver_ThrowsADescriptiveException()
    {
        Register(s => s.AddSingleton<ILocalizationProviderResolver>(_ => new LocalizationProviderResolver()))
            .Should().Throw<InvalidOperationException>()
            .WithMessage("*cannot extend*");
    }

    [Fact]
    public void ForeignResolverImplementation_ThrowsADescriptiveException()
    {
        Register(s => s.AddSingleton<ILocalizationProviderResolver>(new ForeignResolver()))
            .Should().Throw<InvalidOperationException>()
            .WithMessage("*cannot extend*");
    }

    [Fact]
    public void InstanceRegisteredResolver_IsReused()
    {
        var resolver = new LocalizationProviderResolver();

        Register(s => s.AddSingleton<ILocalizationProviderResolver>(resolver))
            .Should().NotThrow();

        resolver.GetProvider().Should().NotBeNull();
    }

    [Fact]
    public void NoPriorRegistration_StillWorks()
    {
        Register(_ => { }).Should().NotThrow();
    }

    [Fact]
    public void RepeatedAddStringLocalizer_StillWorks()
    {
        var services = new ServiceCollection();

        services.AddStringLocalizer(b => b.AddLocalization(
            new CultureInfo("en-US"),
            new Dictionary<LocalizationKey, string?> { { "greeting", "Hello" } }));

        Action second = () => services.AddStringLocalizer("secondary", b => b.AddLocalization(
            new CultureInfo("en-US"),
            new Dictionary<LocalizationKey, string?> { { "bonus", "Bonus" } }));

        second.Should().NotThrow();
    }

    [Fact]
    public void TypeRegisteredOptions_ThrowsADescriptiveException()
    {
        var services = new ServiceCollection();
        services.AddSingleton<LocalizationOptions>();

        Action act = () => services.ConfigureLocalizationOptions(o => o.FormatCultureBuilder = c => c);

        act.Should().Throw<InvalidOperationException>().WithMessage("*cannot read*");
    }

    [Fact]
    public void ConfigureLocalizationOptions_StillWorks()
    {
        var services = new ServiceCollection();

        Action act = () => services.ConfigureLocalizationOptions(o => o.FormatCultureBuilder = c => c);

        act.Should().NotThrow();
    }

    private sealed class ForeignResolver : ILocalizationProviderResolver
    {
        public ILocalizationProvider? GetProvider(string? key = null) => null;

        public IEnumerable<ILocalizationProvider> GetAllProviders() => [];
    }
}
