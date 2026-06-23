using PracticoMobileApp.Models;
using PracticoMobileApp.Services;

namespace PracticoMobileApp;

public partial class DetallePencaPage : ContentPage
{
    private readonly PencaInstanciaMobile _penca;
    private readonly ApiService _apiService;

    public DetallePencaPage(PencaInstanciaMobile penca)
    {
        InitializeComponent();
        _penca = penca;
        _apiService = new ApiService();
        CargarDatos();
    }

    /// <summary>
    /// Se ejecuta cada vez que la pagina aparece (incluyendo cuando volves desde PagoPage).
    /// Recargamos los datos de la penca desde el API para reflejar el estado actualizado.
    /// </summary>
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await RecargarPenca();
    }

    /// <summary>
    /// Pide al API la lista de pencas y encuentra la nuestra para actualizar el estado.
    /// </summary>
    private async Task RecargarPenca()
    {
        try
        {
            var pencas = await _apiService.ObtenerPencasDelSitioAsync();
            var pencaActualizada = pencas.FirstOrDefault(p => p.PencaInstanciaId == _penca.PencaInstanciaId);

            if (pencaActualizada != null)
            {
                // Actualizamos los campos mutables (los inmutables como Nombre, Deporte no cambian)
                _penca.Participa = pencaActualizada.Participa;
                _penca.EstaPagado = pencaActualizada.EstaPagado;
                _penca.ParticipacionId = pencaActualizada.ParticipacionId;
                _penca.PuntajeTotal = pencaActualizada.PuntajeTotal;
                _penca.Finalizada = pencaActualizada.Finalizada;

                CargarDatos();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[DetallePencaPage] Error recargando: {ex.Message}");
            // No mostramos alerta para no molestar al usuario si solo falla el refresh
        }
    }

    private void CargarDatos()
    {
        // Info de la penca
        NombrePencaLabel.Text = _penca.NombrePenca;
        DeporteLabel.Text = $"Deporte: {_penca.Deporte}";
        EquiposLabel.Text = $"Equipos: {_penca.CantidadEquipos}";
        CostoLabel.Text = $"Costo: {_penca.CostoTexto}";
        EstadoLabel.Text = $"Estado: {_penca.EstadoTexto}";

        if (_penca.Participa)
        {
            // El usuario participa: mostrar su info
            ParticipacionBorder.IsVisible = true;
            NoParticipaBorder.IsVisible = false;
            MisPuntosLabel.Text = $"Puntos: {_penca.PuntajeTotal}";

            if (_penca.EstaPagado)
            {
                // Pago confirmado: habilitar todos los botones
                BtnPosiciones.IsEnabled = true;
                BtnPredicciones.IsEnabled = true;
                BtnPosiciones.Text = "Ver Tabla de Posiciones";
                BtnPredicciones.Text = "Hacer Predicciones";
            }
            else
            {
                // Participa pero no pago: deshabilitar acciones
                BtnPosiciones.IsEnabled = false;
                BtnPredicciones.IsEnabled = false;
                BtnPosiciones.Text = "Ver Posiciones (requiere pago)";
                BtnPredicciones.Text = "Predicciones (requiere pago)";
            }
        }
        else
        {
            // El usuario NO participa: mostrar tarjeta amarilla con botón Pagar
            ParticipacionBorder.IsVisible = false;
            NoParticipaBorder.IsVisible = true;
            BtnPosiciones.IsEnabled = false;
            BtnPredicciones.IsEnabled = false;
        }
    }

    private async void OnVerPosicionesClicked(object sender, EventArgs e)
    {
        // Navegar a la tabla de posiciones con el PencaInstanciaId
        await Navigation.PushAsync(new TablaPosicionesPage(_penca.PencaInstanciaId));
    }

    private async void OnHacerPrediccionesClicked(object sender, EventArgs e)
    {
        // Validar que tenga ParticipacionId
        if (_penca.ParticipacionId == null || _penca.ParticipacionId == 0)
        {
            await DisplayAlert("Error", "No se encontró tu participación", "OK");
            return;
        }

        // Navegar a la pagina de predicciones
        await Navigation.PushAsync(new PrediccionesPage(_penca.ParticipacionId.Value));
    }

    /// <summary>
    /// Se ejecuta cuando el usuario tap el boton "Participar con PayPal" en la tarjeta amarilla.
    /// </summary>
    private async void OnParticiparClicked(object sender, EventArgs e)
    {
        // Validar que la penca tenga costo
        if (_penca.Costo <= 0)
        {
            await DisplayAlert("Error", "Esta penca no tiene costo configurado.", "OK");
            return;
        }

        // Confirmar antes de redirigir
        bool confirma = await DisplayAlert(
            "Pagar con PayPal",
            $"Vas a pagar USD {_penca.Costo:F2} para participar en \"{_penca.NombrePenca}\".\n\n¿Continuar?",
            "Sí, pagar",
            "Cancelar"
        );

        if (!confirma) return;

        // Navegar a PagoPage pasando los datos necesarios
        await Navigation.PushAsync(new PagoPage(
            pencaInstanciaId: _penca.PencaInstanciaId,
            nombrePenca: _penca.NombrePenca,
            costo: _penca.Costo
        ));
    }
}