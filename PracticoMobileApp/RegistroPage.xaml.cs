using PracticoMobileApp.Services;

namespace PracticoMobileApp;

[QueryProperty(nameof(SitioId), "sitioId")]
[QueryProperty(nameof(SitioNombre), "sitioNombre")]
[QueryProperty(nameof(TipoRegistro), "tipoRegistro")]
public partial class RegistroPage : ContentPage
{
    private readonly ApiService _apiService;

    public string SitioId { get; set; } = string.Empty;
    public string SitioNombre { get; set; } = string.Empty;
    public string TipoRegistro { get; set; } = string.Empty;

    public RegistroPage()
    {
        InitializeComponent();
        _apiService = new ApiService();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (!string.IsNullOrEmpty(SitioNombre))
            SitioLabel.Text = Uri.UnescapeDataString(SitioNombre);

        // Configurar UI segun el tipo de registro del sitio
        ConfigurarSegunTipoRegistro();
    }

    private void ConfigurarSegunTipoRegistro()
    {
        switch (TipoRegistro)
        {
            case "Abierta":
                // Default: sin avisos, sin campo de invitacion
                AvisoLabel.IsVisible = false;
                InvitacionStack.IsVisible = false;
                break;

            case "AbiertaConAutorizacion":
                AvisoLabel.Text = "ℹ️ Tu solicitud quedará pendiente hasta que un administrador la apruebe.";
                AvisoLabel.IsVisible = true;
                InvitacionStack.IsVisible = false;
                break;

            case "SoloConInvitacion":
                AvisoLabel.Text = "ℹ️ Necesitás un código de invitación válido.\nTu solicitud quedará pendiente hasta ser aprobada.";
                AvisoLabel.IsVisible = true;
                InvitacionStack.IsVisible = true;
                break;

            case "Cerrada":
                // Este caso no debería llegar acá (el boton de registro debería estar oculto)
                AvisoLabel.Text = "Este sitio no admite nuevos registros.";
                AvisoLabel.IsVisible = true;
                RegistrarButton.IsEnabled = false;
                RegistrarButton.Opacity = 0.5;
                break;
        }
    }

    private async void OnRegistrarTapped(object sender, TappedEventArgs e)
    {
        var nombre = NombreEntry.Text?.Trim() ?? "";
        var email = EmailEntry.Text?.Trim() ?? "";
        var password = PasswordEntry.Text ?? "";
        var passwordConfirm = PasswordConfirmEntry.Text ?? "";
        var tokenInvitacion = TokenInvitacionEntry.Text?.Trim();

        // Validaciones basicas
        if (string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            MostrarError("Completá todos los campos.");
            return;
        }

        if (password.Length < 6)
        {
            MostrarError("La contraseña debe tener al menos 6 caracteres.");
            return;
        }

        if (password != passwordConfirm)
        {
            MostrarError("Las contraseñas no coinciden.");
            return;
        }

        if (TipoRegistro == "SoloConInvitacion" && string.IsNullOrEmpty(tokenInvitacion))
        {
            MostrarError("Necesitás un código de invitación.");
            return;
        }

        if (!int.TryParse(SitioId, out int sitioId) || sitioId <= 0)
        {
            MostrarError("Sitio inválido.");
            return;
        }

        SetLoading(true);

        try
        {
            var (apiResponse, error) = await _apiService.RegistrarAsync(
                nombre, email, password, sitioId, tokenInvitacion);

            if (apiResponse == null)
            {
                MostrarError(error ?? "No se pudo completar el registro.");
                return;
            }

            // Si quedo pendiente de aprobacion (sitios con autorizacion o invitacion)
            if (apiResponse.EstadoSolicitud == "Pendiente")
            {
                await Shell.Current.GoToAsync(
                    $"SolicitudPendientePage?email={Uri.EscapeDataString(apiResponse.Email)}&sitioNombre={SitioNombre}");
                return;
            }

            // Registro abierto: ya tenemos JWT, loguear directamente
            await SecureStorage.SetAsync("jwt_token", apiResponse.Jwt);
            Preferences.Set("usuario_id", apiResponse.UsuarioSitioId);
            Preferences.Set("usuario_nombre", apiResponse.Nombre);
            Preferences.Set("usuario_email", apiResponse.Email);
            Preferences.Set("sitio_id", apiResponse.SitioId);
            Preferences.Set("sitio_nombre", Uri.UnescapeDataString(SitioNombre));

            var fcmToken = Preferences.Get("fcm_token", string.Empty);
            if (!string.IsNullOrEmpty(fcmToken))
                await _apiService.GuardarFcmTokenAsync(fcmToken);

            await Shell.Current.GoToAsync("//MainPage");
        }
        catch (Exception ex)
        {
            MostrarError($"Error: {ex.Message}");
        }
        finally
        {
            SetLoading(false);
        }
    }

    private async void OnVolverTapped(object sender, TappedEventArgs e)
    {
        await Shell.Current.Navigation.PopAsync();
    }

    private void SetLoading(bool isLoading)
    {
        LoadingIndicator.IsVisible = isLoading;
        LoadingIndicator.IsRunning = isLoading;
        RegistrarButton.IsEnabled = !isLoading;
        RegistrarButton.Opacity = isLoading ? 0.6 : 1.0;
        NombreEntry.IsEnabled = !isLoading;
        EmailEntry.IsEnabled = !isLoading;
        PasswordEntry.IsEnabled = !isLoading;
        PasswordConfirmEntry.IsEnabled = !isLoading;
        TokenInvitacionEntry.IsEnabled = !isLoading;

        if (isLoading)
            ErrorLabel.IsVisible = false;
    }

    private void MostrarError(string mensaje)
    {
        ErrorLabel.Text = mensaje;
        ErrorLabel.IsVisible = true;
    }
}
