namespace Barbatos.i18n.Sample.Maui;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        // Switching language no longer rebuilds the AppShell: LocalizationNotifier tells LocalizationSource,
        // and every {i18n:...} binding re-translates in place.
        MainPage = new AppShell();
    }
}
