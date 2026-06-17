using PracticoMobileApp.Models;
using PracticoMobileApp.Services;

namespace PracticoMobileApp;

public partial class PrediccionesPage : ContentPage
{
    private readonly ApiService _apiService;
    private readonly int _participacionId;
    private List<PartidoConPrediccionMobile> _partidos = new();

    public PrediccionesPage(int participacionId)
    {
        InitializeComponent();
        _apiService = new ApiService();
        _participacionId = participacionId;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CargarPartidosAsync();
    }

    private async Task CargarPartidosAsync()
    {
        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;
        PartidosCollection.ItemsSource = null;

        var todosLosPartidos = await _apiService.ObtenerPartidosYPrediccionesAsync(_participacionId);

        // Filtramos solo los que puede predecir (no jugados Y que no empezaron)
        _partidos = todosLosPartidos
            .Where(p => p.PuedePredecir)
            .OrderBy(p => p.Fecha)
            .ToList();

        // Pre-popular los Entry con las predicciones existentes
        foreach (var partido in _partidos)
        {
            partido.GolesLocalInput = partido.Prediccion?.GolesEquipoLocal.ToString() ?? "";
            partido.GolesVisitanteInput = partido.Prediccion?.GolesEquipoVisitante.ToString() ?? "";
        }

        PartidosCollection.ItemsSource = _partidos;

        LoadingIndicator.IsVisible = false;
        LoadingIndicator.IsRunning = false;
        RefreshPartidos.IsRefreshing = false;
    }

    private async void OnGuardarTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is not PartidoConPrediccionMobile partido)
            return;

        // 1. Parsear los inputs
        if (!int.TryParse(partido.GolesLocalInput?.Trim(), out int golesLocal) || golesLocal < 0)
        {
            await DisplayAlert("Validación", $"Goles de {partido.Local.Nombre} inválidos.", "OK");
            return;
        }

        if (!int.TryParse(partido.GolesVisitanteInput?.Trim(), out int golesVisitante) || golesVisitante < 0)
        {
            await DisplayAlert("Validación", $"Goles de {partido.Visitante.Nombre} inválidos.", "OK");
            return;
        }

        // 2. Verificar que no haya empezado (validacion cliente, mas la del server)
        if (!partido.PuedePredecir)
        {
            await DisplayAlert("Predicción cerrada",
                "El partido ya comenzó. No se pueden hacer más predicciones.", "OK");
            await CargarPartidosAsync();
            return;
        }

        // 3. Enviar al servidor (upsert)
        // Si tiene prediccion existente -> mandar su Id
        // Si no -> mandar 0 para que cree una nueva
        int prediccionIdActual = partido.Prediccion?.Id ?? 0;

        var (exito, error) = await _apiService.CrearOActualizarPrediccionAsync(
            prediccionId: prediccionIdActual,
            participacionId: _participacionId,
            partidoId: partido.Id,
            golesLocal: golesLocal,
            golesVisitante: golesVisitante);

        if (!exito)
        {
            await DisplayAlert("Error", error ?? "No se pudo guardar la predicción.", "OK");
            return;
        }

        // 4. Recargar la lista para reflejar los cambios
        // (el endpoint del compañero no devuelve el ID de la prediccion creada,
        //  por eso hay que hacer un GET para refrescar)
        await CargarPartidosAsync();

        await DisplayAlert("✓",
            prediccionIdActual == 0 ? "Predicción creada correctamente." : "Predicción actualizada correctamente.",
            "OK");
    }

    private async void OnRefreshing(object sender, EventArgs e)
    {
        await CargarPartidosAsync();
    }
}
