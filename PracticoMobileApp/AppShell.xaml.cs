namespace PracticoMobileApp;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Registrar rutas que NO estan en el Shell pero a las que vamos a navegar
        Routing.RegisterRoute(nameof(AuthOptionsPage), typeof(AuthOptionsPage));
        Routing.RegisterRoute(nameof(LoginInternoPage), typeof(LoginInternoPage));
        Routing.RegisterRoute(nameof(RegistroPage), typeof(RegistroPage));
        Routing.RegisterRoute(nameof(SolicitudPendientePage), typeof(SolicitudPendientePage));
        Routing.RegisterRoute(nameof(PreferenciasNotificacionesPage), typeof(PreferenciasNotificacionesPage));

        VerificarSesionAsync();
    }

    private async void VerificarSesionAsync()
    {
        // Pequeña espera para que el Shell termine de inicializar antes de navegar
        await Task.Delay(50);

        var jwt = await SecureStorage.GetAsync("jwt_token");

        if (!string.IsNullOrEmpty(jwt))
        {
            // Hay sesion guardada -> ir a MainPage
            await Shell.Current.GoToAsync("//MainPage");
        }
        // Si no hay JWT, queda en SitiosPage (que es la ShellContent default)
    }
}

