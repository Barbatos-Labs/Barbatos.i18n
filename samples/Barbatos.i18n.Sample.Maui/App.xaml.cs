using CommunityToolkit.Mvvm.Messaging;

namespace Barbatos.i18n.Sample.Maui;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        WeakReferenceMessenger.Default.Register<CultureChangedMessage>(this, (r, m) =>
        {
            // We MUST wait for the current UI event loop (like the Picker's selection event) 
            // to finish completely before pulling the rug out and replacing the root page.
            // Otherwise, Android will cleanly exit (Code 0) or crash the FragmentManager.
            Current?.Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(100), () =>
            {
                if (Current?.Windows.Count > 0)
                {
#if ANDROID
                    // MAUI .NET 8 on Android is very strict about replacing the root page.
                    // The standard Android way to apply locale changes across the entire app
                    // without killing the process or detaching the debugger is Activity.Recreate()
                    var activity = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity;
                    activity?.Recreate();
#else
                    Current.Windows[0].Page = new AppShell();
#endif
                }
            });
        });

        MainPage = new AppShell();
    }
}
