// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

namespace Barbatos.i18n.DependencyInjection;

/// <summary>
/// A composite <see cref="IStringLocalizer"/> that aggregates and searches across all registered
/// localization sets (JSON, YAML, INI, CSV, RESX, etc.) to resolve localized strings.
/// </summary>
/// <remarks>
/// <para>
/// Unlike <see cref="IStringLocalizer"/>, which requires you to know the exact resource name
/// or use <see cref="IStringLocalizerFactory"/> to create scoped localizers,
/// <see cref="ICompositeStringLocalizer"/> automatically searches across all registered
/// localization sets to find the requested key.
/// </para>
/// <para>
/// This makes it ideal for scenarios where localization data comes from multiple file formats
/// and you want a single injection point in your code.
/// </para>
/// <example>
/// <code>
/// public class MyService(ICompositeStringLocalizer localizer)
/// {
///     public string GetTitle() => localizer["Title"];
///     public string GetError(string code) => localizer["Error_{0}", code];
/// }
/// </code>
/// </example>
/// </remarks>
public interface ICompositeStringLocalizer : IStringLocalizer
{
}

/// <summary>
/// A composite <see cref="IStringLocalizer{TResource}"/> that first searches the localization set
/// scoped to <typeparamref name="TResource"/>, then falls back to searching all registered
/// localization sets.
/// </summary>
/// <typeparam name="TResource">
/// The resource type used to scope the localization lookup. This is typically a marker class
/// (e.g., <c>Strings</c>, <c>SharedResource</c>) whose full name is used as the namespace key.
/// </typeparam>
/// <remarks>
/// <para>
/// When resolving a key, <see cref="ICompositeStringLocalizer{TResource}"/> first attempts to find the key
/// in the localization set matching <typeparamref name="TResource"/>'s full name. If not found,
/// it falls back to searching all registered localization sets.
/// </para>
/// <example>
/// <code>
/// public class MyService(ICompositeStringLocalizer&lt;SharedResource&gt; localizer)
/// {
///     public string GetTitle() => localizer["Title"];
/// }
/// </code>
/// </example>
/// </remarks>
public interface ICompositeStringLocalizer<TResource> : IStringLocalizer<TResource>, ICompositeStringLocalizer
{
}
