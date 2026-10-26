// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

using Barbatos.i18n.Yaml;

namespace Barbatos.i18n.DependencyInjection.UnitTests;

/// <summary>
/// The WPF sample's layout: a file registered first that defines Title, then a YAML file whose Settings header
/// defines Title too. The XAML lookup takes the first file's Title by registration order. The C# localizers read
/// the default set first, and the YAML loader used to copy every key under a header into it, so they answered
/// with a YAML Title instead - the last header's, at that. Both paths now agree.
/// </summary>
public sealed class YamlNamespacePrecedenceTests
{
    private static readonly CultureInfo Culture = new("en-US");

    private static ServiceProvider Build() =>
        new ServiceCollection()
            .AddStringLocalizer(builder =>
            {
                builder.AddLocalization("locales", Culture, new Dictionary<LocalizationKey, string?> { { "Title", "Barbatos Demo" } });
                builder.FromYaml(typeof(YamlNamespacePrecedenceTests).Assembly, "Resources.Settings-en-US.yaml", Culture);
                builder.SetCulture(Culture);
            })
            .BuildServiceProvider();

    [Fact]
    public void TheCompositeLocalizerFollowsRegistrationOrderForAKeyUnderAHeader()
    {
        ICompositeStringLocalizer localizer = Build().GetRequiredService<ICompositeStringLocalizer>();

        localizer["Title"].Value.Should().Be("Barbatos Demo", "the file registered first wins, as it does in XAML");
    }

    [Fact]
    public void TheCompositeLocalizerStillFindsKeysThatOnlyAHeaderDefines()
    {
        ICompositeStringLocalizer localizer = Build().GetRequiredService<ICompositeStringLocalizer>();

        localizer["Theme"].Value.Should().Be("Dark");
        localizer["Theme"].ResourceNotFound.Should().BeFalse();
    }

    [Fact]
    public void TheDefaultSetHoldsTheRootLevelKeys()
    {
        IStringLocalizer localizer = Build().GetRequiredService<IStringLocalizer>();

        localizer["AppName"].Value.Should().Be("Barbatos");
        localizer["Footer"].Value.Should().Be("Made with Barbatos");
    }
}
