using PracticoMobileApp.Services;

namespace PracticoMobileApp;

[QueryProperty(nameof(SitioId), "sitioId")]
[QueryProperty(nameof(SitioNombre), "sitioNombre")]
[QueryProperty(nameof(SitioLogo), "sitioLogo")]
public partial class LoginInternoPage : ContentPage
{
    private readonly ApiService _apiService;

    public string SitioId { get; set; } = string.Empty;
    public string SitioNombre { get; set; } = string.Empty;
    public string SitioLogo { get; set; } = string.Empty;

    public LoginInternoPage()
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

    private async void OnLoginTapped(object sender, TappedEventArgs e)
    {
        var email = EmailEntry.Text?.Trim() ?? "";
        var password = PasswordEntry.Text ?? "";

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            MostrarError("Completá email y contraseña.");
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
            var (apiResponse, error) = await _apiService.LoginInternoAsync(email, password, sitioId);

            if (apiResponse == null)
            {
                MostrarError(error ?? "No se pudo iniciar sesión.");
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
        LoginButton.IsEnabled = !isLoading;
        LoginButton.Opacity = isLoading ? 0.6 : 1.0;
        EmailEntry.IsEnabled = !isLoading;
        PasswordEntry.IsEnabled = !isLoading;

        if (isLoading)
            ErrorLabel.IsVisible = false;
    }

    private void MostrarError(string mensaje)
    {
        ErrorLabel.Text = mensaje;
        ErrorLabel.IsVisible = true;
    }
}
