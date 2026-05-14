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
        var sitioNombre = Preferences.Get("sitio_nombre", "");

        WelcomeLabel.Text = $"¡Hola, {nombre}!";
        SitioLabel.Text = sitioNombre;

        // Intentar enviar FCM token si no se envió antes
        var fcmToken = Preferences.Get("fcm_token", string.Empty);
        var jwt = await SecureStorage.GetAsync("jwt_token");
        if (!string.IsNullOrEmpty(fcmToken) && !string.IsNullOrEmpty(jwt))
        {
            var apiService = new Services.ApiService();
            var resultado = await apiService.GuardarFcmTokenAsync(fcmToken);
            System.Diagnostics.Debug.WriteLine($"[FCM] Token: {fcmToken.Substring(0, 20)}...");
            System.Diagnostics.Debug.WriteLine($"[FCM] Envío a API: {(resultado ? "OK" : "FALLÓ")}");
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"[FCM] No envió - fcmToken vacío: {string.IsNullOrEmpty(fcmToken)} - jwt vacío: {string.IsNullOrEmpty(jwt)}");
        }

#if ANDROID
        var status = await Permissions.CheckStatusAsync<Permissions.PostNotifications>();
        if (status != PermissionStatus.Granted)
            await Permissions.RequestAsync<Permissions.PostNotifications>();
#endif
    }

    private async void OnCerrarSesionTapped(object sender, EventArgs e)
    {
        SecureStorage.Remove("jwt_token");
        Preferences.Remove("usuario_id");
        Preferences.Remove("usuario_nombre");
        Preferences.Remove("usuario_email");
        Preferences.Remove("sitio_id");
        Preferences.Remove("sitio_nombre");

        await Shell.Current.GoToAsync("//SitiosPage");
    }
}
