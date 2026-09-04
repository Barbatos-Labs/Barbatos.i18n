// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

using System.ComponentModel;
using System.Windows.Threading;

namespace Barbatos.i18n.Wpf;

/// <summary>
/// A process-wide observable that reports the active localization culture to the XAML binding engine.
/// </summary>
/// <remarks>
/// <para>
/// A <see cref="MarkupExtension"/> is evaluated once, when the XAML is loaded, so a translation it produces
/// cannot follow a later culture change on its own. The markup extensions in this package therefore emit a
/// <see cref="MultiBinding"/> that carries <see cref="Culture"/> as an extra, unused value. Changing the culture
/// raises <see cref="INotifyPropertyChanged.PropertyChanged"/> here, the binding engine re-runs the converter,
/// and the translation is refreshed in place - no window reload and no page navigation.
/// </para>
/// <para>
/// Bindings observe this object through WPF's weak event manager, so binding to <see cref="Instance"/> does not
/// keep any element alive.
/// </para>
/// </remarks>
public sealed class LocalizationSource : INotifyPropertyChanged
{
    private CultureInfo _culture = CultureInfo.CurrentUICulture;

    private LocalizationSource()
    {
        LocalizationNotifier.CultureChanged += OnCultureChanged;
    }

    /// <summary>
    /// Gets the shared instance observed by every live localization binding.
    /// </summary>
    public static LocalizationSource Instance { get; } = new();

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Gets the culture that the most recent localization change switched to.
    /// </summary>
    public CultureInfo Culture => _culture;

    /// <summary>
    /// Forces every live localization binding to re-evaluate against the ambient UI culture.
    /// </summary>
    /// <remarks>
    /// Call this after mutating localization data outside of a culture switch, for example when translations are
    /// downloaded and the provider is rebuilt while the current culture stays the same.
    /// </remarks>
    public void Refresh() => Dispatch(null);

    private void OnCultureChanged(object? sender, LocalizationChangedEventArgs e) => Dispatch(e);

    /// <summary>
    /// Runs <see cref="Apply"/> on the dispatcher thread.
    /// </summary>
    /// <param name="change">The culture change, or null for a data-only refresh.</param>
    private void Dispatch(LocalizationChangedEventArgs? change)
    {
        Dispatcher? dispatcher = Application.Current?.Dispatcher;

        if (dispatcher is not null && !dispatcher.CheckAccess())
        {
            _ = dispatcher.BeginInvoke(new Action<LocalizationChangedEventArgs?>(Apply), change);
            return;
        }

        Apply(change);
    }

    /// <summary>
    /// Adopts the new culture on the dispatcher thread and invalidates every live binding.
    /// </summary>
    /// <param name="change">The culture change, or null for a data-only refresh.</param>
    /// <remarks>
    /// CultureInfo.CurrentCulture and CurrentUICulture are per-thread, and ILocalizationCultureManager.SetCulture
    /// can only set them on whichever thread called it. Applying them here - this method always runs on the
    /// dispatcher thread, which is where the converters later read them - keeps translation and number, date and
    /// currency formatting on the same culture even when the switch was requested from a background thread.
    /// </remarks>
    private void Apply(LocalizationChangedEventArgs? change)
    {
        if (change is not null)
        {
            CultureInfo.CurrentUICulture = change.Culture;
            CultureInfo.CurrentCulture = change.FormatCulture;
            _culture = change.Culture;
        }

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Culture)));
    }
}
