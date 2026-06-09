// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

namespace Barbatos.i18n.DependencyInjection;

/// <summary>
/// Provides a provider-based implementation of the <see cref="IStringLocalizerFactory"/> interface.
/// </summary>
/// <remarks>
/// This class uses an <see cref="ILocalizationProvider"/> to retrieve localization sets,
/// and an <see cref="ILocalizationCultureManager"/> to manage the current culture.
/// </remarks>
public class ProviderBasedStringLocalizerFactory(
    ILocalizationProviderResolver _resolver,
    ILocalizationCultureManager _cultureManager
) : IStringLocalizerFactory
{
    /// <inheritdoc />
    public IStringLocalizer Create(Type resourceSource)
    {
        string? baseName = resourceSource.FullName?.ToLower().Trim();

        return Create(baseName ?? default, default);
    }

    /// <inheritdoc />
    public IStringLocalizer Create(string? baseName, string? location)
    {
        var defaultProvider = _resolver.GetProvider() ?? throw new InvalidOperationException("Default localization provider not found.");
        return new ProviderBasedStringLocalizer(defaultProvider, _cultureManager, baseName);
    }
}
