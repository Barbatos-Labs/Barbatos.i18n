// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;
using Barbatos.i18n;
using Barbatos.i18n.Wpf;
using AwesomeAssertions;

namespace Barbatos.i18n.Wpf.UnitTests;

/// <summary>
/// Drives the whole chain end to end on a real WPF element: the markup extension produces a binding, the
/// culture manager announces a change, and the already-rendered text re-translates without the view being rebuilt.
/// </summary>
[Collection("Sequential")]
public sealed class LiveBindingIntegrationTests
{
    [Fact]
    public void Text_ShouldRetranslateInPlace_WhenTheCultureChanges()
    {
        var (before, after) = RunOnStaThread(() =>
        {
            var block = new TextBlock();
            var extension = new StringLocalizerExtension { Text = "greeting", Arg = "Hùng" };

            Attach(block, extension);

            string first = block.Text;
            SwitchCultureTo("en-US");

            return (first, block.Text);
        });

        before.Should().Be("Xin chào Hùng");
        after.Should().Be("Hello Hùng");
    }

    [Fact]
    public void BoundKey_ShouldRetranslateInPlace_WhenTheCultureChanges()
    {
        var (before, after) = RunOnStaThread(() =>
        {
            var block = new TextBlock { DataContext = new ProductRow("greeting") };
            var extension = new StringLocalizerExtension
            {
                BindText = new System.Windows.Data.Binding(nameof(ProductRow.StatusKey)),
                Arg = "Hùng"
            };

            Attach(block, extension);

            string first = block.Text;
            SwitchCultureTo("en-US");

            return (first, block.Text);
        });

        before.Should().Be("Xin chào Hùng");
        after.Should().Be("Hello Hùng");
    }

    [Fact]
    public void Setter_ShouldStillReceiveAPlainString_WhenTheTargetCannotHoldABinding()
    {
        object? value = RunOnStaThread(() =>
        {
            var setter = new Setter { Property = TextBlock.TextProperty };
            var extension = new StringLocalizerExtension { Text = "greeting", Arg = "Hùng" };

            return extension.ProvideValue(new TargetStub(setter, typeof(Setter).GetProperty(nameof(Setter.Value))!));
        });

        value.Should().BeOfType<string>();
        value.Should().Be("Xin chào Hùng");
    }

    [Fact]
    public void ItemsControl_ShouldTranslateEveryItem_AndRetranslateThemAll()
    {
        var (before, after) = RunOnStaThread(() =>
        {
            var items = new ItemsControl
            {
                Template = BuildItemsControlTemplate(),
                ItemTemplate = BuildKeyBoundItemTemplate(),
                ItemsSource = new[] { "hello", "bye" }
            };

            Render(items);
            string[] first = [.. Descendants<TextBlock>(items).Select(t => t.Text)];

            SwitchCultureTo("en-US");
            Render(items);

            return (first, Descendants<TextBlock>(items).Select(t => t.Text).ToArray());
        });

        before.Should().Equal("Xin chào", "Tạm biệt");
        after.Should().Equal("Hello", "Goodbye");
    }

    [Fact]
    public void EnumItems_ShouldUseTheMemberNameAsTheKey_AndRetranslate()
    {
        var (before, after) = RunOnStaThread(() =>
        {
            var items = new ItemsControl
            {
                Template = BuildItemsControlTemplate(),
                ItemTemplate = BuildKeyBoundItemTemplate(),
                ItemsSource = Enum.GetValues<SampleStatus>()
            };

            Render(items);
            string[] first = [.. Descendants<TextBlock>(items).Select(t => t.Text)];

            SwitchCultureTo("en-US");
            Render(items);

            return (first, Descendants<TextBlock>(items).Select(t => t.Text).ToArray());
        });

        before.Should().Equal("Đang bán", "Đã lưu trữ");
        after.Should().Equal("Active", "Archived");
    }

    [Fact]
    public void LocalizeConverter_ShouldGoStale_WhenTheCultureChanges()
    {
        var (before, after) = RunOnStaThread(() =>
        {
            var block = new TextBlock { DataContext = new ProductRow("hello") };
            var binding = new System.Windows.Data.Binding(nameof(ProductRow.StatusKey))
            {
                Converter = new LocalizeConverter()
            };

            System.Windows.Data.BindingOperations.SetBinding(block, TextBlock.TextProperty, binding);

            string first = block.Text;
            SwitchCultureTo("en-US");

            return (first, block.Text);
        });

        before.Should().Be("Xin chào");
        after.Should().Be(
            "Xin chào",
            "a plain IValueConverter has no source change to re-trigger it - this is the documented reason to prefer BindText");
    }

    [Fact]
    public void LiveBinding_ShouldNotKeepItsTargetAlive()
    {
        WeakReference reference = RunOnStaThread(() =>
        {
            var block = new TextBlock();
            Attach(block, new StringLocalizerExtension { Text = "hello" });

            // Prove the binding is live before measuring, otherwise the test could pass for the wrong reason.
            block.Text.Should().Be("Xin chào");
            SwitchCultureTo("en-US");
            block.Text.Should().Be("Hello");

            return new WeakReference(block);
        });

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        reference.IsAlive.Should().BeFalse(
            "the shared LocalizationSource must observe its bindings weakly, or every element ever bound would leak");
    }

    /// <summary>
    /// The item template used by the list tests, parsed through the real XAML parser so the extension is driven
    /// exactly as an application would drive it.
    /// </summary>
    private static DataTemplate BuildKeyBoundItemTemplate() =>
        (DataTemplate)XamlReader.Parse(
            """
            <DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
                          xmlns:i18n='http://schemas.barbatos.co/i18n/2026/xaml'>
                <TextBlock Text="{i18n:StringLocalizer BindText={Binding}}" />
            </DataTemplate>
            """);

    /// <summary>
    /// Supplied explicitly: with no Application there are no theme resources to style the control from.
    /// </summary>
    private static ControlTemplate BuildItemsControlTemplate() =>
        (ControlTemplate)XamlReader.Parse(
            """
            <ControlTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
                             TargetType='ItemsControl'>
                <ItemsPresenter />
            </ControlTemplate>
            """);

    /// <summary>
    /// Forces the element through a layout pass so item containers are generated.
    /// </summary>
    private static void Render(FrameworkElement element)
    {
        element.ApplyTemplate();
        element.Measure(new Size(500, 500));
        element.Arrange(new Rect(0, 0, 500, 500));
        element.UpdateLayout();
    }

    /// <summary>
    /// Walks the visual tree and yields every descendant of the requested type, in visual order.
    /// </summary>
    private static IEnumerable<T> Descendants<T>(DependencyObject root)
        where T : DependencyObject
    {
        int count = VisualTreeHelper.GetChildrenCount(root);

        for (int i = 0; i < count; i++)
        {
            DependencyObject child = VisualTreeHelper.GetChild(root, i);

            if (child is T match)
            {
                yield return match;
            }

            foreach (T descendant in Descendants<T>(child))
            {
                yield return descendant;
            }
        }
    }

    /// <summary>
    /// Asks the extension for its value the way the XAML parser does, then assigns it to the target property.
    /// </summary>
    private static void Attach(TextBlock block, StringLocalizerExtension extension)
    {
        object? value = extension.ProvideValue(new TargetStub(block, TextBlock.TextProperty));
        block.SetValue(TextBlock.TextProperty, value);
    }

    /// <summary>
    /// Switches the culture and lets the binding engine drain its queued updates.
    /// </summary>
    private static void SwitchCultureTo(string cultureName)
    {
        new LocalizationCultureManager().SetCulture(cultureName);

        // Target updates are queued at DataBind priority; invoking at a lower one drains them first.
        Dispatcher.CurrentDispatcher.Invoke(() => { }, DispatcherPriority.SystemIdle);
    }

    /// <summary>
    /// Runs the body on a dedicated STA thread with a clean provider registered, then restores global state.
    /// </summary>
    private static T RunOnStaThread<T>(Func<T> body)
    {
        T result = default!;
        Exception? failure = null;

        var thread = new Thread(() =>
        {
            CultureInfo originalCulture = CultureInfo.CurrentCulture;
            CultureInfo originalUICulture = CultureInfo.CurrentUICulture;

            try
            {
                var builder = new LocalizationBuilder();
                builder.AddLocalization(new LocalizationSet(null, new CultureInfo("vi-VN"), new[]
                {
                    new KeyValuePair<LocalizationKey, string?>("greeting", "Xin chào {0}"),
                    new KeyValuePair<LocalizationKey, string?>("hello", "Xin chào"),
                    new KeyValuePair<LocalizationKey, string?>("bye", "Tạm biệt"),
                    new KeyValuePair<LocalizationKey, string?>("active", "Đang bán"),
                    new KeyValuePair<LocalizationKey, string?>("archived", "Đã lưu trữ")
                }));
                builder.AddLocalization(new LocalizationSet(null, new CultureInfo("en-US"), new[]
                {
                    new KeyValuePair<LocalizationKey, string?>("greeting", "Hello {0}"),
                    new KeyValuePair<LocalizationKey, string?>("hello", "Hello"),
                    new KeyValuePair<LocalizationKey, string?>("bye", "Goodbye"),
                    new KeyValuePair<LocalizationKey, string?>("active", "Active"),
                    new KeyValuePair<LocalizationKey, string?>("archived", "Archived")
                }));
                builder.SetCulture(new CultureInfo("vi-VN"));

                LocalizationProviderFactory.SetInstance(builder.Build(), "");
                WpfLocalization.Initialize(null!);

                new LocalizationCultureManager().SetCulture("vi-VN");

                result = body();
            }
            catch (Exception ex)
            {
                failure = ex;
            }
            finally
            {
                LocalizationProviderFactory.SetInstance(null!, "");
                CultureInfo.CurrentCulture = originalCulture;
                CultureInfo.CurrentUICulture = originalUICulture;
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

    /// <summary>
    /// The minimal slice of the XAML parser's service provider that the extensions consult.
    /// </summary>
    private sealed class TargetStub(object targetObject, object targetProperty) : IServiceProvider, IProvideValueTarget
    {
        public object TargetObject { get; } = targetObject;

        public object TargetProperty { get; } = targetProperty;

        public object? GetService(Type serviceType) =>
            serviceType == typeof(IProvideValueTarget) ? this : null;
    }

    private sealed record ProductRow(string StatusKey);

    /// <summary>
    /// Bound directly as list items: the member name is the localization key, no converter in between.
    /// </summary>
    private enum SampleStatus
    {
        Active,
        Archived
    }
}
