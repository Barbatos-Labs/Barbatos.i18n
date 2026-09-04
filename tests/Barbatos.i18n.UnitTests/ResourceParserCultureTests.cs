// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

using Barbatos.i18n.UnitTests.Resources;

namespace Barbatos.i18n.UnitTests;

/// <summary>
/// Reading a RESX switches the thread's culture so that satellite assemblies resolve, and has to put back
/// exactly what it found. Saving only CurrentCulture and restoring both from it replaced the application's UI
/// culture with its formatting culture - silently changing the language of everything translated afterwards.
/// </summary>
[Collection("Sequential")]
public sealed class ResourceParserCultureTests : IDisposable
{
    private readonly CultureInfo _originalCulture = CultureInfo.CurrentCulture;
    private readonly CultureInfo _originalUiCulture = CultureInfo.CurrentUICulture;

    public void Dispose()
    {
        CultureInfo.CurrentCulture = _originalCulture;
        CultureInfo.CurrentUICulture = _originalUiCulture;
    }

    [Fact]
    public void FromResource_LeavesBothThreadCulturesExactlyAsItFoundThem()
    {
        // The standard .NET split: translations in one culture, number and date formatting in another.
        CultureInfo.CurrentUICulture = new CultureInfo("vi-VN");
        CultureInfo.CurrentCulture = new CultureInfo("en-US");

        LocalizationBuilder builder = new();
        builder.FromResource<TestResource>(new CultureInfo("ko-KR"));

        CultureInfo.CurrentUICulture.Name.Should().Be("vi-VN", "the UI culture is not the parser's to change");
        CultureInfo.CurrentCulture.Name.Should().Be("en-US");
    }

    [Fact]
    public void FromResource_RestoresTheCultures_EvenWhenTheResourceIsMissing()
    {
        CultureInfo.CurrentUICulture = new CultureInfo("vi-VN");
        CultureInfo.CurrentCulture = new CultureInfo("en-US");

        LocalizationBuilder builder = new();
        _ = Record.Exception(() => builder.FromResource(typeof(TestResource).Assembly, "No.Such.Resource", new CultureInfo("ko-KR")));

        CultureInfo.CurrentUICulture.Name.Should().Be("vi-VN");
        CultureInfo.CurrentCulture.Name.Should().Be("en-US");
    }
}
