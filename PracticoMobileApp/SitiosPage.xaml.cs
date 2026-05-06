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

        // Guardar el sitio elegido
        Preferences.Set("sitio_id", sitio.Id);
        Preferences.Set("sitio_nombre", sitio.Nombre);

        // Navegar al Login pasando el sitio
        await Shell.Current.GoToAsync($"LoginPage?sitioId={sitio.Id}&sitioNombre={Uri.EscapeDataString(sitio.Nombre)}");
    }
}
