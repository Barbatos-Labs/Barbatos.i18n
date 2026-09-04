// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

using Barbatos.i18n.UnitTests.Resources;

namespace Barbatos.i18n.UnitTests;

public sealed class LocalizationBuilderTests
{
    [Fact]
    public void FromResource_ShouldThrowException_WhenResourceSetIsMissing()
    {
        LocalizationBuilder builder = new();

        Action action = () =>
            builder.FromResource(
                Assembly.GetExecutingAssembly(),
                "Barbatos.i18n.UnitTests.Resources.Invalid",
                new CultureInfo("en-US")
            );

        _ = action.Should().Throw<LocalizationBuilderException>();
    }

    [Fact]
    public void FromResource_ShouldLoadResourceFromNamespace()
    {
        LocalizationBuilder builder = new();

        builder.FromResource<TestResource>(new CultureInfo("en-US"));

        ILocalizationProvider provider = builder.Build();

        provider.SetCulture(new CultureInfo("en-US"));
        LocalizationSet? localizationSet = provider.GetLocalizationSet("en-US");

        _ = localizationSet!["Test"].Should().Be("Test in english");
    }

    [Fact]
    public void FromResource_ShouldLoadResource()
    {
        LocalizationBuilder builder = new();

        builder.FromResource(
            Assembly.GetExecutingAssembly(),
            "Barbatos.i18n.UnitTests.Resources.TestResource",
            new CultureInfo("en-US")
        );

        builder.FromResource(
            Assembly.GetExecutingAssembly(),
            "Barbatos.i18n.UnitTests.Resources.TestResource",
            new CultureInfo("ko-KR")
        );

        ILocalizationProvider provider = builder.Build();

        LocalizationSet? enSet = provider.GetLocalizationSet("en-US");
        _ = enSet!["Test"].Should().Be("Test in english");

        LocalizationSet? koSet = provider.GetLocalizationSet("ko-KR");
        _ = koSet!["Test"].Should().Be("안녕하세요");
    }

    [Fact]
    public void AddLocalization_WithNamespace_ShouldAddSet()
    {
        LocalizationBuilder builder = new();
        builder.AddLocalization("testns", new CultureInfo("en-US"), new Dictionary<LocalizationKey, string?> { { "key", "val" } });
        
        var provider = builder.Build();
        var set = provider.GetLocalizationSet("en-US", "testns");
        
        set.Should().NotBeNull();
        set!["key"].Should().Be("val");
    }

    [Fact]
    public void AddLocalization_WithoutNamespace_ShouldAddSet()
    {
        LocalizationBuilder builder = new();
        builder.AddLocalization(new CultureInfo("en-US"), new Dictionary<LocalizationKey, string?> { { "key", "val" } });
        
        var provider = builder.Build();
        var set = provider.GetLocalizationSet("en-US");
        
        set.Should().NotBeNull();
        set!["key"].Should().Be("val");
    }

    [Fact]
    public void SetCulture_ShouldSetSelectedCulture()
    {
        LocalizationBuilder builder = new();
        builder.SetCulture("fr-FR");
        
        var provider = builder.Build();
        provider.GetCulture().Name.Should().Be("fr-FR");
    }

    [Fact]
    public void FromResourceString_ShouldLoadResource()
    {
        LocalizationBuilder builder = new();
        
        // Use string overload
        builder.FromResource<TestResource>("en-US");
        
        var provider = builder.Build();
        var set = provider.GetLocalizationSet("en-US");
        
        set.Should().NotBeNull();
        set!["Test"].Should().Be("Test in english");
    }

    [Fact]
    public void FromResourceStringBaseName_ShouldLoadResource()
    {
        LocalizationBuilder builder = new();
        
        // Use string basename overload
        builder.FromResource("Barbatos.i18n.UnitTests.Resources.TestResource", new CultureInfo("en-US"));
        
        var provider = builder.Build();
        var set = provider.GetLocalizationSet("en-US");
        
        set.Should().NotBeNull();
        set!["Test"].Should().Be("Test in english");
    }

    [Fact]
    public void AddLocalization_DuplicateShouldMerge()
    {
        // Registering two files into one namespace used to abort startup. Two files can legitimately belong
        // there - a YAML file's root-level keys and an INI file named after nothing but its culture are both
        // in the default namespace - so they are merged instead.
        LocalizationBuilder builder = new();
        builder.AddLocalization(new CultureInfo("en-US"), new Dictionary<LocalizationKey, string?> { { "key", "val" } });
        builder.AddLocalization(new CultureInfo("en-US"), new Dictionary<LocalizationKey, string?> { { "key2", "val2" } });

        builder.SetCulture(new CultureInfo("en-US"));
        LocalizationSet? set = builder.Build().GetLocalizationSet(new CultureInfo("en-US"), null);

        set!["key"].Should().Be("val");
        set["key2"].Should().Be("val2");
    }

    [Fact]
    public void FromResource_ShouldAllowLookupUsingTestResourcePropertyDirectly()
    {
        LocalizationBuilder builder = new();

        builder.FromResource<TestResource>(new CultureInfo("ko-KR"));

        ILocalizationProvider provider = builder.Build();

        provider.SetCulture(new CultureInfo("ko-KR"));
        LocalizationSet? localizationSet = provider.GetLocalizationSet("ko-KR");

        localizationSet.Should().NotBeNull();

        // 1. Direct property lookup: TestResource.Test evaluates to "hello" (default value) at runtime,
        // but the CallerArgumentExpression captures "TestResource.Test" at compile-time and extracts "Test".
        localizationSet![TestResource.Test].Should().Be("안녕하세요");

        // 2. Nameof lookup: nameof(TestResource.Test) evaluates to "Test", which works.
        localizationSet![nameof(TestResource.Test)].Should().Be("안녕하세요");

        // 3. String literal lookup: "Test" works.
        localizationSet!["Test"].Should().Be("안녕하세요");

        // 4. A translated value is not a key. TestResource is a generated RESX designer, so TestResource.Test
        // returns the translation ("hello" under the neutral culture), not the key name. Looking a translation
        // up finds nothing, which is exactly why a resource meant for {x:Static} is hand-written to return
        // nameof - see Strings.cs in the WPF sample and the "Why nameof and not the resource value?" note in
        // the README. Assertion 1 works despite the runtime value because CallerArgumentExpression reads the
        // source text "TestResource.Test", not what the property evaluates to.
        localizationSet!["hello"].Should().BeNull();
    }
}
