// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

using System.Windows;
using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Barbatos.i18n.DependencyInjection;
using Barbatos.i18n.Wpf;
using Barbatos.i18n.Ini;
using Barbatos.i18n.Csv;
using Barbatos.i18n.Json;
using Barbatos.i18n.Yaml;

namespace Barbatos.i18n.Sample.Wpf;

public partial class App : Application
{
    public IServiceProvider ServiceProvider { get; private set; }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();

        // Configure Options (Optional)
        //services.ConfigureLocalizationOptions(options =>
        //{
        //    options.FormatCultureBuilder = uiCulture =>
        //    {
        //        uiCulture.NumberFormat.CurrencySymbol = "đ";
        //        uiCulture.NumberFormat.CurrencyPositivePattern = 3;
        //        uiCulture.NumberFormat.CurrencyNegativePattern = 8;
        //        return uiCulture;
        //    };
        //});

        // Default Provider
        services.AddStringLocalizer(builder =>
        {
            builder.FromIni("Locales.en-US.ini", new CultureInfo("en-US"));
            builder.FromIni("Locales.vi-VN.ini", new CultureInfo("vi-VN"));
            builder.FromIni("Locales.ko-KR.ini", new CultureInfo("ko-KR"));

            // Load from YAML (Built-in)
            builder.FromYaml("Locales.Settings-en-US.yaml", new CultureInfo("en-US"));
            builder.FromYaml("Locales.Settings-vi-VN.yaml", new CultureInfo("vi-VN"));
            builder.FromYaml("Locales.Settings-ko-KR.yaml", new CultureInfo("ko-KR"));

            // Load from JSON (v2.0)
            builder.FromJson("Locales.Validation-en-US.json", new CultureInfo("en-US"));
            builder.FromJson("Locales.Validation-vi-VN.json", new CultureInfo("vi-VN"));
            builder.FromJson("Locales.Validation-ko-KR.json", new CultureInfo("ko-KR"));

            // Load from RESX
            builder.FromResource<Locales.Strings>(new CultureInfo("en-US"));
            builder.FromResource<Locales.Strings>(new CultureInfo("vi-VN"));
            builder.FromResource<Locales.Strings>(new CultureInfo("ko-KR"));

            // Load Errors namespace
            builder.FromCsv("Locales.Errors.csv");
        });

        // Setup Secondary Provider
        services.AddStringLocalizer("SecondaryProvider", builder =>
        {
            builder.FromJson("Locales.Extra-en-US.json", new CultureInfo("en-US"));
            builder.FromJson("Locales.Extra-vi-VN.json", new CultureInfo("vi-VN"));
            builder.FromJson("Locales.Extra-ko-KR.json", new CultureInfo("ko-KR"));
        });

        ServiceProvider = services.BuildServiceProvider();

        ServiceProvider.UseWpfLocalization()
                       .SetLocalizationCulture(CultureInfo.CurrentUICulture);
    }
}
