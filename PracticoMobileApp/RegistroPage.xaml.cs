using Android.Gms.Extensions;
using PracticoMobileApp.Services;

namespace PracticoMobileApp;

[QueryProperty(nameof(SitioId), "sitioId")]
[QueryProperty(nameof(SitioNombre), "sitioNombre")]
[QueryProperty(nameof(TipoRegistro), "tipoRegistro")]
[QueryProperty(nameof(SitioLogo), "sitioLogo")]
public partial class RegistroPage : ContentPage
{
    private readonly ApiService _apiService;

    public string SitioId { get; set; } = string.Empty;
    public string SitioNombre { get; set; } = string.Empty;
    public string TipoRegistro { get; set; } = string.Empty;
    public string SitioLogo { get; set; } = string.Empty;

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

        AplicarLogoSitio();
        ConfigurarSegunTipoRegistro();
    }

    /// <summary>
    /// Si recibimos un LogoUrl valido, lo usamos. Si no, dejamos el icono PencaUY por defecto.
    /// </summary>
    private void AplicarLogoSitio()
    {
        try
        {
            if (string.IsNullOrEmpty(SitioLogo)) return;

            var logoUrl = Uri.UnescapeDataString(SitioLogo);
            if (string.IsNullOrWhiteSpace(logoUrl)) return;

            if (Uri.TryCreate(logoUrl, UriKind.Absolute, out var uri))
            {
                SitioLogoImage.Source = ImageSource.FromUri(uri);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Logo] Error cargando logo del sitio: {ex.Message}");
        }
    }

    private void ConfigurarSegunTipoRegistro()
    {
        switch (TipoRegistro)
        {
            case "Abierta":
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
                // Pasamos tambien el logo del sitio a la pagina de pendiente
                await Shell.Current.GoToAsync(
                    $"SolicitudPendientePage?email={Uri.EscapeDataString(apiResponse.Email)}" +
                    $"&sitioNombre={SitioNombre}" +
                    $"&sitioLogo={SitioLogo}");
                return;
            }

            // Registro abierto: ya tenemos JWT, loguear directamente
            await SecureStorage.SetAsync("jwt_token", apiResponse.Jwt);
            Preferences.Set("usuario_id", apiResponse.UsuarioSitioId);
            Preferences.Set("usuario_nombre", apiResponse.Nombre);
            Preferences.Set("usuario_email", apiResponse.Email);
            Preferences.Set("sitio_id", apiResponse.SitioId);
            Preferences.Set("sitio_nombre", Uri.UnescapeDataString(SitioNombre));
            Preferences.Set("sitio_logo", string.IsNullOrEmpty(SitioLogo) ? "" : Uri.UnescapeDataString(SitioLogo));

            try
            {
                var tokenTask = Firebase.Messaging.FirebaseMessaging.Instance.GetToken();
                var token = await tokenTask.AsAsync<Java.Lang.String>();
                var fcmToken = token?.ToString();
                if (!string.IsNullOrEmpty(fcmToken))
                {
                    Preferences.Set("fcm_token", fcmToken);
                    await _apiService.GuardarFcmTokenAsync(fcmToken);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[FCM] Error obteniendo token: {ex.Message}");
            }

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
