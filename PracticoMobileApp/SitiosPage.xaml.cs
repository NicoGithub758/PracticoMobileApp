using PracticoMobileApp.Services;

namespace PracticoMobileApp;

public partial class SitiosPage : ContentPage
{
    private readonly ApiService _apiService;

    public SitiosPage()
    {
        InitializeComponent();
        _apiService = new ApiService();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CargarSitiosAsync();
    }

    private async Task CargarSitiosAsync()
    {
        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;
        ErrorLabel.IsVisible = false;
        SitiosCollection.ItemsSource = null;

        var sitios = await _apiService.GetSitiosAsync();

        LoadingIndicator.IsVisible = false;
        LoadingIndicator.IsRunning = false;

        if (sitios.Count == 0)
        {
            ErrorLabel.IsVisible = true;
            return;
        }

        SitiosCollection.ItemsSource = sitios;
    }

    private async void OnSitioTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is not SitioDto sitio) return;

        Preferences.Set("sitio_id_temp", sitio.Id);
        Preferences.Set("sitio_nombre_temp", sitio.Nombre);

        // Guardamos el LogoUrl tambien por si alguna pagina lo necesita despues
        // (ademas de pasarlo por query)
        Preferences.Set("sitio_logo_temp", sitio.LogoUrl ?? "");

        // Navegar a AuthOptionsPage pasandole sitioId, nombre, TipoRegistro y LogoUrl
        var logoUrl = string.IsNullOrEmpty(sitio.LogoUrl) ? "" : Uri.EscapeDataString(sitio.LogoUrl);
        var url = $"AuthOptionsPage?sitioId={sitio.Id}" +
                  $"&sitioNombre={Uri.EscapeDataString(sitio.Nombre)}" +
                  $"&tipoRegistro={sitio.TipoRegistro}" +
                  $"&sitioLogo={logoUrl}";

        await Shell.Current.GoToAsync(url);
    }
}
