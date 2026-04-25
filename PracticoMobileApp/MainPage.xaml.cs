using PracticoMobileApp.Models;
using System.Net.Http.Json;
using Plugin.Firebase.CloudMessaging;

namespace PracticoMobileApp;




public partial class MainPage : ContentPage
{
    // 1. Declaramos el cliente HTTP
    private readonly HttpClient _httpClient;

    public MainPage()
    {
        InitializeComponent();
        System.Diagnostics.Debug.WriteLine("PRUEBA DEBUG");
        Android.Util.Log.Debug("TEST", "PRUEBA LOGCAT");
        Console.WriteLine("PRUEBA CONSOLA");

        ObtenerToken();

        // 2. Configuramos el cliente para que use el manejador de seguridad (solo en modo desarrollo)
#if DEBUG
        HttpClientHandler insecureHandler = GetInsecureHandler();
        _httpClient = new HttpClient(insecureHandler);
#else
        _httpClient = new HttpClient();
#endif
    }

    async void ObtenerToken()
    {
        try
        {
            await Task.Delay(3000); // 🔥 clave: esperar a Firebase

            var token = await CrossFirebaseCloudMessaging.Current.GetTokenAsync();

            string msg = $"TOKEN: {token ?? "NULL"}";

            System.Diagnostics.Debug.WriteLine(msg);
            Android.Util.Log.Debug("FCM_TOKEN", msg);

            await DisplayAlert("TOKEN", msg, "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("ERROR", ex.ToString(), "OK");
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // 1. Pedir permiso de notificaciones (VITAL para Android 13+)
#if ANDROID
        var status = await Permissions.CheckStatusAsync<Permissions.PostNotifications>();
        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<Permissions.PostNotifications>();
        }

        if (status != PermissionStatus.Granted)
        {
            await DisplayAlert("Permiso denegado", "No podrás ver las notificaciones push.", "OK");
        }
#endif

        // 2. Intentar obtener el token
        await ObtenerYMostrarToken();
    }

    private async Task ObtenerYMostrarToken()
    {
        try
        {
            // Verificamos si Firebase está bien configurado antes de pedir el token
            await CrossFirebaseCloudMessaging.Current.CheckIfValidAsync();

            var token = await CrossFirebaseCloudMessaging.Current.GetTokenAsync();

            if (string.IsNullOrEmpty(token))
            {
                await DisplayAlert("Error", "El token está vacío. Revisa el archivo google-services.json", "OK");
                return;
            }

            // Mostramos el token en un Alert para poder copiarlo a mano si es necesario
            await DisplayAlert("FCM TOKEN", token, "Cerrar");

            // Log en la consola de Visual Studio (Ventana de Salida)
            System.Diagnostics.Debug.WriteLine("========================================");
            System.Diagnostics.Debug.WriteLine($"MI_TOKEN_FIREBASE: {token}");
            System.Diagnostics.Debug.WriteLine("========================================");
        }
        catch (Exception ex)
        {
            // Si entra aquí, suele ser porque el google-services.json no tiene la "Acción de Compilación" correcta
            await DisplayAlert("Error de Firebase", ex.Message, "OK");
        }
    }

    // 3. Este es el método que "salta" la seguridad del certificado local para Android
    public HttpClientHandler GetInsecureHandler()
    {
        HttpClientHandler handler = new HttpClientHandler();
        handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
        {
            // Si el certificado es el de localhost de desarrollo, decimos que es válido
            if (cert.Issuer.Equals("CN=localhost"))
                return true;

            // Para cualquier otro, seguimos la validación normal
            return errors == System.Net.Security.SslPolicyErrors.None;
        };
        return handler;
    }

    // 4. El evento de búsqueda cuando escribes en el cuadrito (Entry)
    private async void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        string texto = e.NewTextValue;

        // Solo buscamos si escribió más de 1 letra
        if (!string.IsNullOrWhiteSpace(texto) && texto.Length > 1)
        {
            try
            {
                // IMPORTANTE: 
                // - 10.0.2.2 es la dirección para que el emulador de Android vea tu PC.
                // - REEMPLAZA "7123" por el puerto HTTPS que sale en tu Swagger.
                string puerto = "7277";
                string url = $"https://10.0.2.2:{puerto}/api/countriesapi?search={texto}";

                var paises = await _httpClient.GetFromJsonAsync<List<Country>>(url);

                // Asignamos los resultados a la grilla (CollectionView)
                CountriesCollection.ItemsSource = paises;
            }
            catch (Exception ex)
            {
                // Si hay un error de conexión, podrías mostrar una alerta
                // await DisplayAlert("Error", "No se pudo conectar con la API: " + ex.Message, "OK");
            }
        }
        else
        {
            // Si el buscador está vacío, limpiamos la grilla
            CountriesCollection.ItemsSource = null;
        }
    }
}