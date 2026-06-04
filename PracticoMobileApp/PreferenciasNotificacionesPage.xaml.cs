using PracticoMobileApp.Models;
using PracticoMobileApp.Services;

namespace PracticoMobileApp;

public partial class PreferenciasNotificacionesPage : ContentPage
{
    private readonly ApiService _apiService;

    public PreferenciasNotificacionesPage()
    {
        InitializeComponent();
        _apiService = new ApiService();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CargarPreferenciasAsync();
    }

    private async Task CargarPreferenciasAsync()
    {
        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;
        OpcionesStack.IsVisible = false;

        var prefs = await _apiService.ObtenerPreferenciasAsync();

        LoadingIndicator.IsVisible = false;
        LoadingIndicator.IsRunning = false;

        if (prefs == null)
        {
            MostrarFeedback("No se pudieron cargar las preferencias.", esError: true);
            // Mostramos el stack con valores default por si quiere intentar guardar
            SwitchResultados.IsToggled = true;
            SwitchPartidos.IsToggled = true;
            SwitchGenerales.IsToggled = true;
            SwitchRanking.IsToggled = true;
        }
        else
        {
            SwitchResultados.IsToggled = prefs.RecibirResultados;
            SwitchPartidos.IsToggled = prefs.RecibirPartidos;
            SwitchGenerales.IsToggled = prefs.RecibirGenerales;
            SwitchRanking.IsToggled = prefs.RecibirRanking;
        }

        OpcionesStack.IsVisible = true;
    }

    private async void OnGuardarTapped(object sender, TappedEventArgs e)
    {
        SetGuardando(true);

        var prefs = new PreferenciasNotificacionDTO
        {
            RecibirResultados = SwitchResultados.IsToggled,
            RecibirPartidos = SwitchPartidos.IsToggled,
            RecibirGenerales = SwitchGenerales.IsToggled,
            RecibirRanking = SwitchRanking.IsToggled
        };

        var ok = await _apiService.GuardarPreferenciasAsync(prefs);

        SetGuardando(false);

        if (ok)
        {
            MostrarFeedback("✓ Preferencias guardadas correctamente.", esError: false);
        }
        else
        {
            MostrarFeedback("No se pudieron guardar los cambios. Intentá de nuevo.", esError: true);
        }
    }

    private void SetGuardando(bool guardando)
    {
        GuardarButton.IsEnabled = !guardando;
        GuardarButton.Opacity = guardando ? 0.6 : 1.0;
        SwitchResultados.IsEnabled = !guardando;
        SwitchPartidos.IsEnabled = !guardando;
        SwitchGenerales.IsEnabled = !guardando;
        SwitchRanking.IsEnabled = !guardando;

        if (guardando)
            FeedbackLabel.IsVisible = false;
    }

    private void MostrarFeedback(string mensaje, bool esError)
    {
        FeedbackLabel.Text = mensaje;
        FeedbackLabel.TextColor = esError ? Color.FromArgb("#FF6B6B") : Color.FromArgb("#4CAF50");
        FeedbackLabel.IsVisible = true;
    }
}
