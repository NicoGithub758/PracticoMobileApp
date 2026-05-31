using PracticoMobileApp.Models;
using PracticoMobileApp.Services;

namespace PracticoMobileApp;

public partial class TablaPosicionesPage : ContentPage
{
    private readonly int _pencaInstanciaId;
    private readonly ApiService _apiService;

    public TablaPosicionesPage(int pencaInstanciaId)
    {
        InitializeComponent();
        _pencaInstanciaId = pencaInstanciaId;
        _apiService = new ApiService();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CargarTablaPosiciones();
    }

    private async Task CargarTablaPosiciones()
    {
        try
        {
            // Mostrar indicador de carga
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;
            ActualizarButton.IsEnabled = false;

            // Obtener tabla de posiciones
            var posiciones = await _apiService.ObtenerTablaPosicionesAsync(_pencaInstanciaId);

            // Actualizar CollectionView
            PosicionesCollectionView.ItemsSource = posiciones;

            // Obtener y mostrar mi posición destacada
            var miPosicion = await _apiService.ObtenerMiPosicionAsync(_pencaInstanciaId);
            if (miPosicion != null)
            {
                MiPosicionBorder.IsVisible = true;
                MiPosicionLabel.Text = $"{miPosicion.Posicion}°";
                MisPuntosLabel.Text = $"{miPosicion.Puntos} pts";
            }
            else
            {
                MiPosicionBorder.IsVisible = false;
            }

            System.Diagnostics.Debug.WriteLine($"[Posiciones] Cargadas {posiciones.Count} posiciones");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Posiciones] Error: {ex.Message}");
            await DisplayAlert("Error", "No se pudo cargar la tabla de posiciones", "OK");
        }
        finally
        {
            // Ocultar indicador de carga
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
            ActualizarButton.IsEnabled = true;
        }
    }

    private async void OnActualizarClicked(object sender, EventArgs e)
    {
        await CargarTablaPosiciones();
    }
}
