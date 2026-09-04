// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

using Barbatos.i18n.Yaml;

namespace Barbatos.i18n.UnitTests;

/// <summary>
/// A set's strings can arrive as a deferred query - the YAML loader handed over a LINQ projection - and the
/// indexer then re-ran it for every key read, on a path that repeats for every live binding on every culture
/// change. The provider copies them into a dictionary once instead.
/// </summary>
public sealed class ProviderMaterializationTests
{
    private static readonly CultureInfo Culture = new("en-US");

    [Fact]
    public void ADeferredProjection_IsCopiedIntoADictionary()
    {
        int evaluations = 0;

        IEnumerable<KeyValuePair<LocalizationKey, string?>> deferred =
            new[] { "greeting" }.Select(key =>
            {
                evaluations++;
                return new KeyValuePair<LocalizationKey, string?>(key, "Hello");
            });

        LocalizationBuilder builder = new();
        builder.AddLocalization(new LocalizationSet("y", Culture, deferred));
        builder.SetCulture(Culture);

        LocalizationSet? set = builder.Build().GetLocalizationSet(Culture, "y");
        int afterBuild = evaluations;

        _ = set!["greeting"];
        _ = set["greeting"];
        _ = set["missing"];

        set.Strings.Should().BeAssignableTo<IReadOnlyDictionary<LocalizationKey, string?>>();
        set["greeting"].Should().Be("Hello");
        evaluations.Should().Be(afterBuild, "reading keys must not re-run the source query");
    }

    [Fact]
    public void ADictionaryBackedSet_IsLeftAlone()
    {
        Dictionary<LocalizationKey, string?> strings = new() { { "greeting", "Hello" } };

        LocalizationBuilder builder = new();
        builder.AddLocalization(new LocalizationSet("y", Culture, strings));
        builder.SetCulture(Culture);

        builder.Build().GetLocalizationSet(Culture, "y")!.Strings.Should().BeSameAs(strings);
    }

    [Fact]
    public void ADuplicateKeyInOneSet_KeepsTheFirstEntry()
    {
        LocalizationBuilder builder = new();
        builder.AddLocalization(new LocalizationSet("y", Culture, new[]
        {
            new KeyValuePair<LocalizationKey, string?>("greeting", "First"),
            new KeyValuePair<LocalizationKey, string?>("greeting", "Second")
        }));
        builder.SetCulture(Culture);

        builder.Build().GetLocalizationSet(Culture, "y")!["greeting"].Should().Be("First");
    }

    [Theory]
    [InlineData("Resources.Translations-en-US.YAML")]
    [InlineData("Resources.Translations-en-US.yaml")]
    public void TheYamlExtensionCheckIgnoresCase(string path)
    {
        // Ini and Csv have always accepted an upper-cased extension; Yaml and Json rejected one.
        LocalizationBuilder builder = new();

        FluentActions.Invoking(() => builder.FromYaml(path, Culture))
            .Should().NotThrow<ArgumentException>();
    }
}
