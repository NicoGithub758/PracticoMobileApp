
namespace PracticoMobileApp;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute("SitiosPage", typeof(SitiosPage));
        Routing.RegisterRoute("LoginPage", typeof(LoginPage));
        Routing.RegisterRoute("MainPage", typeof(MainPage));

        _ = VerificarSesionAsync();
    }

    private async Task VerificarSesionAsync()
    {
        await Task.Delay(300);

        // Verificar si hay sesión guardada
        var jwt = await SecureStorage.GetAsync("jwt_token");
        var sitioId = Preferences.Get("sitio_id", 0);

        if (!string.IsNullOrEmpty(jwt) && sitioId > 0)
        {
            // Ya tiene sesión → ir directo a MainPage
            await GoToAsync("//MainPage");
        }
        else
        {
            // Sin sesión → elegir sitio primero
            await GoToAsync("//SitiosPage");
        }
    }
}

