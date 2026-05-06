using Plugin.Firebase.CloudMessaging;

namespace PracticoMobileApp;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var nombre = Preferences.Get("usuario_nombre", "Usuario");
        WelcomeLabel.Text = $"¡Hola, {nombre}!";

#if ANDROID
        var status = await Permissions.CheckStatusAsync<Permissions.PostNotifications>();
        if (status != PermissionStatus.Granted)
            await Permissions.RequestAsync<Permissions.PostNotifications>();
#endif
    }
}