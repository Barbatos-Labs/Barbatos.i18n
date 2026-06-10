// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

using System.Globalization;
using Barbatos.i18n;
using Barbatos.i18n.Json;
using Barbatos.i18n.Maui;

namespace Barbatos.i18n.Sample.Maui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });
        try
        {
            builder
                // Default Provider: main locale + errors namespace (demonstrates Namespace feature)
                // Paths use dot notation matching EmbeddedResource naming: {Folder}.{FileName}
                .UseBarbatosLocalization(locBuilder =>
                {
                    locBuilder.FromJson("Locales.Locales-en-US.json", new CultureInfo("en-US"));
                    locBuilder.FromJson("Locales.Locales-vi-VN.json", new CultureInfo("vi-VN"));
                    locBuilder.FromJson("Locales.Locales-ko-KR.json", new CultureInfo("ko-KR"));

                    // Load Errors namespace (demonstrates Namespace / LocalizationSet feature)
                    locBuilder.FromJson("Locales.Errors-en-US.json", new CultureInfo("en-US"));
                    locBuilder.FromJson("Locales.Errors-vi-VN.json", new CultureInfo("vi-VN"));
                    locBuilder.FromJson("Locales.Errors-ko-KR.json", new CultureInfo("ko-KR"));
                });

            // Secondary Provider: demonstrates Multiple Providers (ProviderKey) feature
            var secondaryBuilder = new LocalizationBuilder();
            secondaryBuilder.FromJson("Locales.Extra-en-US.json", new CultureInfo("en-US"));
            secondaryBuilder.FromJson("Locales.Extra-vi-VN.json", new CultureInfo("vi-VN"));
            secondaryBuilder.FromJson("Locales.Extra-ko-KR.json", new CultureInfo("ko-KR"));
            var secondaryProvider = secondaryBuilder.Build();
            LocalizationProviderFactory.SetInstance(secondaryProvider, "SecondaryProvider");
        }
        catch (Exception ex)
        {
            System.IO.File.WriteAllText(@"C:\Users\phamt\Desktop\maui_crash.txt", ex.ToString());
            throw;
        }

        return builder.Build();
    }
}
