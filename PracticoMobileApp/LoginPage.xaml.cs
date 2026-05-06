using Auth0.OidcClient;
using PracticoMobileApp.Services;

namespace PracticoMobileApp;

[QueryProperty(nameof(SitioId), "sitioId")]
[QueryProperty(nameof(SitioNombre), "sitioNombre")]
public partial class LoginPage : ContentPage
{
    private readonly Auth0Client _auth0Client;
    private readonly ApiService _apiService;

    public string SitioId { get; set; } = string.Empty;
    public string SitioNombre { get; set; } = string.Empty;

    public LoginPage()
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

        // Mostrar el nombre del sitio elegido
        if (!string.IsNullOrEmpty(SitioNombre))
            SitioLabel.Text = $"Ingresando a: {Uri.UnescapeDataString(SitioNombre)}";
    }

    private async void OnGoogleLoginTapped(object sender, EventArgs e)
    {
        if (!int.TryParse(SitioId, out int sitioId) || sitioId <= 0)
        {
            MostrarError("No se seleccionó un sitio válido.");
            return;
        }

        SetLoading(true);

        try
        {
            // Paso 1: Login con Google via Auth0
            var loginResult = await _auth0Client.LoginAsync(new
            {
                connection = "google-oauth2"
            });

            if (loginResult.IsError)
            {
                MostrarError($"Error de autenticación: {loginResult.Error}");
                return;
            }

            // Paso 2: Enviar token a la API y obtener JWT propio
            var auth0Token = loginResult.AccessToken;
            var apiResponse = await _apiService.LoginSocialAsync(auth0Token, sitioId);

            if (apiResponse == null)
            {
                MostrarError("No se pudo conectar con el servidor.");
                return;
            }

            // Paso 3: Guardar JWT y datos del usuario
            await SecureStorage.SetAsync("jwt_token", apiResponse.Jwt);
            Preferences.Set("usuario_id", apiResponse.UsuarioSitioId);
            Preferences.Set("usuario_nombre", apiResponse.Nombre);
            Preferences.Set("usuario_email", apiResponse.Email);
            Preferences.Set("sitio_id", apiResponse.SitioId);

            System.Diagnostics.Debug.WriteLine($"[LOGIN OK] {apiResponse.Nombre} en sitio {apiResponse.SitioId}");

            // Paso 4: Enviar token FCM a la API
            var fcmToken = Preferences.Get("fcm_token", string.Empty);
            if (!string.IsNullOrEmpty(fcmToken))
                await _apiService.GuardarFcmTokenAsync(fcmToken);

            // Paso 5: Navegar a MainPage
            await Shell.Current.GoToAsync("//MainPage");
        }
        catch (Exception ex)
        {
            string mensaje = ex.Message.Contains("cancel") || ex.Message.Contains("Cancel")
                ? "Inicio de sesión cancelado."
                : "Error al conectar. Revisá tu conexión.";

            MostrarError(mensaje);
            System.Diagnostics.Debug.WriteLine($"[LOGIN EXCEPTION] {ex}");
        }
        finally
        {
            SetLoading(false);
        }
    }

    private void SetLoading(bool isLoading)
    {
        LoadingIndicator.IsVisible = isLoading;
        LoadingIndicator.IsRunning = isLoading;
        GoogleButton.IsEnabled = !isLoading;
        GoogleButton.Opacity = isLoading ? 0.6 : 1.0;
        ErrorLabel.IsVisible = false;
    }

    private void MostrarError(string mensaje)
    {
        ErrorLabel.Text = mensaje;
        ErrorLabel.IsVisible = true;
    }
}




