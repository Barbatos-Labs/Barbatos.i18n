// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

namespace Barbatos.i18n;

/// <summary>
/// Provides functionality to manage the current culture for localization.
/// </summary>
public class LocalizationCultureManager : ILocalizationCultureManager
{
    /// <inheritdoc />
    public LocalizationOptions Options { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalizationCultureManager"/> class.
    /// </summary>
    /// <param name="options">Optional localization options. If null, default options are used.</param>
    public LocalizationCultureManager(LocalizationOptions? options = null)
    {
        Options = options ?? new LocalizationOptions();
    }

    /// <inheritdoc />
    public void SetCulture(string cultureName) => SetCulture(new CultureInfo(cultureName));

    /// <inheritdoc />
    public void SetCulture(CultureInfo culture)
    {
        if (culture == null)
            throw new ArgumentNullException(nameof(culture));

        CultureInfo targetCulture = culture;
        if (Options.FormatCultureBuilder is not null)
        {
            targetCulture = Options.FormatCultureBuilder.Invoke((CultureInfo)culture.Clone()) ?? culture;
        }

        CultureInfo.CurrentUICulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        CultureInfo.CurrentCulture = targetCulture;
        CultureInfo.DefaultThreadCurrentCulture = targetCulture;

        LocalizationProviderFactory.GetInstance()?.SetCulture(culture);
    }

    /// <inheritdoc />
    public CultureInfo GetCulture()
    {
        return LocalizationProviderFactory.GetInstance()?.GetCulture()
            ?? CultureInfo.CurrentCulture;
    }
}