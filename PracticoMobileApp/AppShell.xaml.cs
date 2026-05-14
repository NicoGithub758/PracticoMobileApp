namespace PracticoMobileApp;

public partial class AppShell : Shell
{
    private bool _navegacionIniciada = false;

    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute("LoginPage", typeof(LoginPage));

        _ = VerificarSesionAsync();
    }

    private async Task VerificarSesionAsync()
    {
        if (_navegacionIniciada) return;
        _navegacionIniciada = true;

        await Task.Delay(300);

        var jwt = await SecureStorage.GetAsync("jwt_token");
        var sitioId = Preferences.Get("sitio_id", 0);

        if (!string.IsNullOrEmpty(jwt) && sitioId > 0)
            await GoToAsync("//MainPage");
        else
            await GoToAsync("//SitiosPage");
    }
}

