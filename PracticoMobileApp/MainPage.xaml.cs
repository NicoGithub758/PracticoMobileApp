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

        // 2. Configuramos el cliente para que use el manejador de seguridad (solo en modo desarrollo)
#if DEBUG
        HttpClientHandler insecureHandler = GetInsecureHandler();
        _httpClient = new HttpClient(insecureHandler);
#else
        _httpClient = new HttpClient();
#endif
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            await Permissions.RequestAsync<Permissions.PostNotifications>();

            var token = await CrossFirebaseCloudMessaging.Current.GetTokenAsync();
            await DisplayAlert("TOKEN", token ?? "NULL", "OK");

            Console.WriteLine($"TOKEN: {token}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error obteniendo token: {ex.Message}");
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