using PracticoMobileApp.Services;

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

        AplicarLogoSitio();

        // Intentar enviar FCM token si no se envio antes
        var fcmToken = Preferences.Get("fcm_token", string.Empty);
        var jwt = await SecureStorage.GetAsync("jwt_token");
        if (!string.IsNullOrEmpty(fcmToken) && !string.IsNullOrEmpty(jwt))
        {
            var apiService = new ApiService();
            var resultado = await apiService.GuardarFcmTokenAsync(fcmToken);
            System.Diagnostics.Debug.WriteLine($"[FCM] Token: {fcmToken.Substring(0, 20)}...");
            System.Diagnostics.Debug.WriteLine($"[FCM] Envio a API: {(resultado ? "OK" : "FALLO")}");
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"[FCM] No envio - fcmToken vacio: {string.IsNullOrEmpty(fcmToken)} - jwt vacio: {string.IsNullOrEmpty(jwt)}");
        }

#if ANDROID
        var status = await Permissions.CheckStatusAsync<Permissions.PostNotifications>();
        if (status != PermissionStatus.Granted)
            await Permissions.RequestAsync<Permissions.PostNotifications>();
#endif
    }

    private async void OnCerrarSesionTapped(object sender, EventArgs e)
    {
        // Limpiar el FCM token en la BD antes de cerrar sesion
        // (asi no llegan notificaciones a este dispositivo si otro usuario loguea)
        try
        {
            var apiService = new ApiService();
            await apiService.LimpiarFcmTokenAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Logout] Error al limpiar FCM: {ex.Message}");
        }

        // Limpiar almacenamiento local
        SecureStorage.Remove("jwt_token");
        Preferences.Remove("usuario_id");
        Preferences.Remove("usuario_nombre");
        Preferences.Remove("usuario_email");
        Preferences.Remove("sitio_id");
        Preferences.Remove("sitio_nombre");
        Preferences.Remove("sitio_logo");
        // Nota: NO borramos fcm_token de Preferences porque es del dispositivo,
        // no del usuario. Si el usuario vuelve a loguear, lo reutilizamos.

        await Shell.Current.GoToAsync("//SitiosPage");
    }

    private async void OnVerPencasClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new PencasPage());
    }

    private async void OnConfiguracionTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new PreferenciasNotificacionesPage());
    }
    /// <summary>
    /// Carga el logo del sitio elegido. Si no hay, deja el logo PencaUY por defecto.
    /// </summary>
    private void AplicarLogoSitio()
    {
        try
        {
            var sitioLogo = Preferences.Get("sitio_logo", string.Empty);
            if (string.IsNullOrWhiteSpace(sitioLogo)) return;

            if (Uri.TryCreate(sitioLogo, UriKind.Absolute, out var uri))
            {
                SitioLogoImage.Source = ImageSource.FromUri(uri);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Logo] Error cargando logo del sitio en MainPage: {ex.Message}");
        }
    }
}
