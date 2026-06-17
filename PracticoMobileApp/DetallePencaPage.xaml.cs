using PracticoMobileApp.Models;

namespace PracticoMobileApp;

public partial class DetallePencaPage : ContentPage
{
    private readonly PencaInstanciaMobile _penca;

    public DetallePencaPage(PencaInstanciaMobile penca)
    {
        InitializeComponent();
        _penca = penca;
        CargarDatos();
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
            // El usuario NO participa
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
}
