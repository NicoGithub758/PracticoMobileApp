using PracticoMobileApp.Models;
using PracticoMobileApp.Services;

namespace PracticoMobileApp;

public partial class PencasPage : ContentPage
{
    private readonly ApiService _apiService;
    private List<PencaInstanciaMobile> _todasLasPencas = new();
    private bool _mostrandoMisPencas = false;

    public PencasPage()
    {
        InitializeComponent();
        _apiService = new ApiService();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CargarPencas();
    }

    private async Task CargarPencas()
    {
        try
        {
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;

            // Traer todas las pencas del sitio
            _todasLasPencas = await _apiService.ObtenerPencasDelSitioAsync();

            // Aplicar el filtro actual
            AplicarFiltro();

            System.Diagnostics.Debug.WriteLine($"[Pencas] Cargadas {_todasLasPencas.Count} pencas");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Pencas] Error: {ex.Message}");
            await DisplayAlert("Error", "No se pudieron cargar las pencas", "OK");
        }
        finally
        {
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
            RefreshPencas.IsRefreshing = false;
        }
    }

    private void AplicarFiltro()
    {
        if (_mostrandoMisPencas)
        {
            // Solo las pencas donde el usuario participa
            var misPencas = _todasLasPencas.Where(p => p.Participa).ToList();
            PencasCollectionView.ItemsSource = misPencas;
            EmptyLabel.Text = "No participas en ninguna penca todavia";
        }
        else
        {
            // Todas las pencas del sitio
            PencasCollectionView.ItemsSource = _todasLasPencas;
            EmptyLabel.Text = "No hay pencas disponibles";
        }
    }

    private void OnFiltroTodasClicked(object sender, EventArgs e)
    {
        _mostrandoMisPencas = false;

        // Actualizar estilo de botones
        BtnTodas.BackgroundColor = Color.FromArgb("#2196F3");
        BtnTodas.TextColor = Colors.White;
        BtnMisPencas.BackgroundColor = Color.FromArgb("#E0E0E0");
        BtnMisPencas.TextColor = Colors.Black;

        AplicarFiltro();
    }

    private void OnFiltroMisPencasClicked(object sender, EventArgs e)
    {
        _mostrandoMisPencas = true;

        // Actualizar estilo de botones
        BtnMisPencas.BackgroundColor = Color.FromArgb("#2196F3");
        BtnMisPencas.TextColor = Colors.White;
        BtnTodas.BackgroundColor = Color.FromArgb("#E0E0E0");
        BtnTodas.TextColor = Colors.Black;

        AplicarFiltro();
    }

    private async void OnVerDetalleClicked(object sender, EventArgs e)
    {
        if (sender is Button boton && boton.CommandParameter is PencaInstanciaMobile penca)
        {
            await Navigation.PushAsync(new DetallePencaPage(penca));
        }
    }

    private async void OnRefreshing(object sender, EventArgs e)
    {
        await CargarPencas();
    }
}
