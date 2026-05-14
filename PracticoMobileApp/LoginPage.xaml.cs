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
            var loginResult = await _auth0Client.LoginAsync(new
            {
                connection = "google-oauth2"
            });

            if (loginResult.IsError)
            {
                MostrarError($"Auth0 error: {loginResult.Error}");
                return;
            }

            MostrarError($"Auth0 OK. Llamando API...");

            var auth0Token = loginResult.AccessToken;
            var apiResponse = await _apiService.LoginSocialAsync(auth0Token, sitioId);

            if (apiResponse == null)
            {
                MostrarError("API devolvió null");
                return;
            }

            MostrarError($"API OK: {apiResponse.Nombre}");

            await SecureStorage.SetAsync("jwt_token", apiResponse.Jwt);
            Preferences.Set("usuario_id", apiResponse.UsuarioSitioId);
            Preferences.Set("usuario_nombre", apiResponse.Nombre);
            Preferences.Set("usuario_email", apiResponse.Email);
            Preferences.Set("sitio_id", apiResponse.SitioId);

            var fcmToken = Preferences.Get("fcm_token", string.Empty);
            if (!string.IsNullOrEmpty(fcmToken))
                await _apiService.GuardarFcmTokenAsync(fcmToken);

            await Shell.Current.GoToAsync("//MainPage");
        }
        catch (System.Exception ex)
        {
            MostrarError($"Exception: {ex.Message}");
        }
        finally
        {
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
            GoogleButton.IsEnabled = true;
            GoogleButton.Opacity = 1.0;
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
        GoogleButton.IsEnabled = !isLoading;
        GoogleButton.Opacity = isLoading ? 0.6 : 1.0;
    }

    private void MostrarError(string mensaje)
    {
        ErrorLabel.Text = mensaje;
        ErrorLabel.IsVisible = true;
    }
}




