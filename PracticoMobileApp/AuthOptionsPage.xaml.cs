using Android.Gms.Extensions;
using Auth0.OidcClient;
using PracticoMobileApp.Services;

namespace PracticoMobileApp;

[QueryProperty(nameof(SitioId), "sitioId")]
[QueryProperty(nameof(SitioNombre), "sitioNombre")]
[QueryProperty(nameof(TipoRegistro), "tipoRegistro")]
[QueryProperty(nameof(SitioLogo), "sitioLogo")]
public partial class AuthOptionsPage : ContentPage
{
    private readonly Auth0Client _auth0Client;
    private readonly ApiService _apiService;

    public string SitioId { get; set; } = string.Empty;
    public string SitioNombre { get; set; } = string.Empty;
    public string TipoRegistro { get; set; } = string.Empty;
    public string SitioLogo { get; set; } = string.Empty;

    public AuthOptionsPage()
    {
        InitializeComponent();

        _auth0Client = new Auth0Client(new Auth0ClientOptions
        {
            Domain = "dev-tohysoy6fqmar1v7.us.auth0.com",
            ClientId = "5Kv0vRTwoYKaKFoJYDhDEvnj1DFHAFi4",
            RedirectUri = "com.companyname.practicomobileapp://dev-tohysoy6fqmar1v7.us.auth0.com/android/com.companyname.practicomobileapp/callback",
            PostLogoutRedirectUri = "com.companyname.practicomobileapp://dev-tohysoy6fqmar1v7.us.auth0.com/android/com.companyname.practicomobileapp/callback",
            Scope = "openid profile email"
        });

        _apiService = new ApiService();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (!string.IsNullOrEmpty(SitioNombre))
            SitioLabel.Text = Uri.UnescapeDataString(SitioNombre);

        // Cargar el logo del sitio si lo tenemos
        AplicarLogoSitio();

        // Mostrar/ocultar botones segun el tipo de registro del sitio
        ConfigurarBotonesSegunTipoRegistro();
    }

    /// <summary>
    /// Si recibimos un LogoUrl valido, lo usamos. Si no, dejamos el icono de PencaUY por defecto.
    /// </summary>
    private void AplicarLogoSitio()
    {
        try
        {
            if (string.IsNullOrEmpty(SitioLogo)) return;

            var logoUrl = Uri.UnescapeDataString(SitioLogo);
            if (string.IsNullOrWhiteSpace(logoUrl)) return;

            // Validar que sea una URL valida antes de asignar
            if (Uri.TryCreate(logoUrl, UriKind.Absolute, out var uri))
            {
                SitioLogoImage.Source = ImageSource.FromUri(uri);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Logo] Error cargando logo del sitio: {ex.Message}");
            // Si falla, queda el logo de PencaUY por defecto (no hacemos nada)
        }
    }

    /// <summary>
    /// Ajusta que botones mostrar y avisos al usuario segun TipoRegistro del sitio.
    /// </summary>
    private void ConfigurarBotonesSegunTipoRegistro()
    {
        // Defaults
        RegistrarseButton.IsVisible = true;
        GoogleButton.IsVisible = true;
        OrLabel.IsVisible = true;
        AvisoLabel.IsVisible = false;

        switch (TipoRegistro)
        {
            case "Abierta":
                break;

            case "AbiertaConAutorizacion":
                AvisoLabel.Text = "ℹ️ El registro requiere aprobación del administrador.";
                AvisoLabel.IsVisible = true;
                break;

            case "SoloConInvitacion":
                AvisoLabel.Text = "ℹ️ Necesitás un código de invitación para registrarte.";
                AvisoLabel.IsVisible = true;
                GoogleButton.IsVisible = false;
                OrLabel.IsVisible = false;
                break;

            case "Cerrada":
                AvisoLabel.Text = "ℹ️ Este sitio no admite nuevos registros.";
                AvisoLabel.IsVisible = true;
                RegistrarseButton.IsVisible = false;
                GoogleButton.IsVisible = false;
                OrLabel.IsVisible = false;
                break;
        }
    }

    private async void OnLoginInternoTapped(object sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync(
            $"LoginInternoPage?sitioId={SitioId}" +
            $"&sitioNombre={Uri.EscapeDataString(SitioNombre)}" +
            $"&sitioLogo={SitioLogo}");
    }

    private async void OnRegistrarseTapped(object sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync(
            $"RegistroPage?sitioId={SitioId}" +
            $"&sitioNombre={Uri.EscapeDataString(SitioNombre)}" +
            $"&tipoRegistro={TipoRegistro}" +
            $"&sitioLogo={SitioLogo}");
    }

    private async void OnGoogleLoginTapped(object sender, TappedEventArgs e)
    {
        if (!int.TryParse(SitioId, out int sitioId) || sitioId <= 0)
        {
            MostrarError("No se seleccionó un sitio válido.");
            return;
        }

        SetLoading(true);

        try
        {
            var loginResult = await _auth0Client.LoginAsync(new
            {
                connection = "google-oauth2"
            });

            if (loginResult.IsError)
            {
                MostrarError($"Error de Google: {loginResult.Error}");
                return;
            }

            var auth0Token = loginResult.AccessToken;
            var (apiResponse, error) = await _apiService.LoginSocialAsync(auth0Token, sitioId);

            if (apiResponse == null)
            {
                MostrarError(error ?? "No se pudo completar el login.");
                return;
            }

            // Guardar datos del usuario
            await SecureStorage.SetAsync("jwt_token", apiResponse.Jwt);
            Preferences.Set("usuario_id", apiResponse.UsuarioSitioId);
            Preferences.Set("usuario_nombre", apiResponse.Nombre);
            Preferences.Set("usuario_email", apiResponse.Email);
            Preferences.Set("sitio_id", apiResponse.SitioId);
            Preferences.Set("sitio_nombre", Uri.UnescapeDataString(SitioNombre));
            Preferences.Set("sitio_logo", string.IsNullOrEmpty(SitioLogo) ? "" : Uri.UnescapeDataString(SitioLogo));

            // Enviar FCM token si esta disponible
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
        await Shell.Current.GoToAsync("//SitiosPage");
    }

    private void SetLoading(bool isLoading)
    {
        LoadingIndicator.IsVisible = isLoading;
        LoadingIndicator.IsRunning = isLoading;
        LoginInternoButton.IsEnabled = !isLoading;
        RegistrarseButton.IsEnabled = !isLoading;
        GoogleButton.IsEnabled = !isLoading;
        GoogleButton.Opacity = isLoading ? 0.6 : 1.0;

        if (isLoading)
            ErrorLabel.IsVisible = false;
    }

    private void MostrarError(string mensaje)
    {
        ErrorLabel.Text = mensaje;
        ErrorLabel.IsVisible = true;
    }
}
