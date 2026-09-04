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
    public void Refresh() => Refresh(CultureInfo.CurrentUICulture);

    private void OnCultureChanged(object? sender, LocalizationChangedEventArgs e) => Refresh(e.Culture);

    private void Refresh(CultureInfo culture)
    {
        Dispatcher? dispatcher = Application.Current?.Dispatcher;

        if (dispatcher is not null && !dispatcher.CheckAccess())
        {
            _ = dispatcher.BeginInvoke(new Action<CultureInfo>(Apply), culture);
            return;
        }

        Apply(culture);
    }

    private void Apply(CultureInfo culture)
    {
        _culture = culture;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Culture)));
    }
}
