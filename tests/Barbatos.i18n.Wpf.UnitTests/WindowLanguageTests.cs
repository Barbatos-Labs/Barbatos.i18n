// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Markup;
using Barbatos.i18n.Wpf;
using AwesomeAssertions;

namespace Barbatos.i18n.Wpf.UnitTests;

/// <summary>
/// FrameworkElement.LanguageProperty can only have its metadata overridden once per process, so the default is
/// frozen at whatever culture was current the first time a culture was applied. A window opened after a later
/// culture change therefore started from that stale default, and since WPF derives the culture it hands to
/// IValueConverter.Convert and to XAML StringFormat from the element's Language, such a window formatted dates
/// and numbers with the first culture the user ever picked.
/// </summary>
/// <remarks>
/// No <see cref="Application"/> is created here on purpose. Application.Current is process-wide and cannot be
/// reset, and its dispatcher dies with the thread that made it, which would strand every later test that
/// marshals through LocalizationSource.
/// </remarks>
[Collection("Sequential")]
public sealed class WindowLanguageTests
{
    [Fact]
    public void WindowOpenedAfterTheLastCultureChange_UsesThatCulture_NotTheFrozenDefault()
    {
        (string frozenMetadataDefault, string windowLanguage) = RunOnStaThread(() =>
        {
            // The first call is what freezes the metadata default; the second is the one a stale window missed.
            ((Application)null!).SetLocalizationCulture(new CultureInfo("vi-VN"));
            ((Application)null!).SetLocalizationCulture(new CultureInfo("ko-KR"));

            var window = new Window();

            // Show() would need a message pump, so the load is raised directly to exercise the class handler.
            window.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent, window));

            string metadataDefault = ((XmlLanguage)FrameworkElement.LanguageProperty
                .GetMetadata(typeof(FrameworkElement)).DefaultValue).IetfLanguageTag;

            return (metadataDefault, window.Language.IetfLanguageTag);
        });

        frozenMetadataDefault.Should().Be(
            "vi-vn",
            "WPF only permits OverrideMetadata once, so the default genuinely stays on the first culture");

        windowLanguage.Should().Be(
            "ko-kr",
            "the window must pick up the current culture rather than the frozen metadata default");
    }


    [Fact]
    public void WindowWithAnApplicationSetLanguage_IsLeftAlone()
    {
        string language = RunOnStaThread(() =>
        {
            ((Application)null!).SetLocalizationCulture(new CultureInfo("ko-KR"));

            // An application can pin one window's formatting independently of the UI language - a report
            // preview, say - and that choice has to survive.
            var window = new Window { Language = XmlLanguage.GetLanguage("de-DE") };
            window.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent, window));

            return window.Language.IetfLanguageTag;
        });

        language.Should().Be("de-de", "a language the application set itself must not be overwritten");
    }

    [Fact]
    public void WindowAdoptedByTheLibrary_KeepsFollowingLaterCultureChanges()
    {
        (string first, string second) = RunOnStaThread(() =>
        {
            ((Application)null!).SetLocalizationCulture(new CultureInfo("vi-VN"));

            var window = new Window();
            window.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent, window));
            string afterAdoption = window.Language.IetfLanguageTag;

            // Adopting the window gives it a local value; that must not make it look application-owned.
            ((Application)null!).SetLocalizationCulture(new CultureInfo("ko-KR"));
            window.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent, window));

            return (afterAdoption, window.Language.IetfLanguageTag);
        });

        first.Should().Be("vi-vn");
        second.Should().Be("ko-kr", "a window the library adopted stays in step with later culture changes");
    }
    private static T RunOnStaThread<T>(Func<T> body)
    {
        T result = default!;
        Exception? failure = null;

        var thread = new Thread(() =>
        {
            try
            {
                result = body();
            }
            catch (Exception ex)
            {
                failure = ex;
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        if (failure is not null)
        {
            throw new Xunit.Sdk.XunitException($"The STA body threw: {failure}");
        }

        return result;
    }
}
